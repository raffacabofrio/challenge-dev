﻿using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Repository;
using ShareBook.Repository.Repository;
using ShareBook.Repository.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class BookUserService : IBookUserService
    {

        private readonly IBookUserRepository _bookUserRepository;
        private readonly IBookService _bookService;
        private readonly IBookUsersEmailService _bookUsersEmailService;

        public BookUserService(IBookUserRepository bookUserRepository, IBookService bookService,
            IBookUsersEmailService bookUsersEmailService, IUnitOfWork unitOfWork)
        {
            _bookUserRepository = bookUserRepository;
            _bookService = bookService;
            _bookUsersEmailService = bookUsersEmailService;
        }

        public IList<User> GetGranteeUsersByBookId(Guid bookId) =>
            _bookUserRepository.Get().Include(x => x.User)
            .Where(x => x.BookId == bookId && x.Status == DonationStatus.WaitingAction)
            .Select(x => x.User.Cleanup()).ToList();

        // TODO: avaliar se o uso de custom sql melhora significativamente a performance. Muitos includes.
        public IList<BookUser> GetRequestersList(Guid bookId) =>
            _bookUserRepository.Get()
            .Include(x => x.User).ThenInclude(u => u.Address)
            .Include(x => x.User).ThenInclude(u => u.BookUsers)
            .Include(x => x.User).ThenInclude(u => u.BooksDonated)
            .Where(x => x.BookId == bookId)
            .OrderBy(x => x.CreationDate)
            .ToList();

        public void Insert(Guid bookId, string reason)
        {
            //obtem o livro requisitado e o doador
            var bookRequested = _bookService.GetBookWithAllUsers(bookId);
            
            var bookUser = new BookUser()
            {
                BookId = bookId,
                UserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name),
                Reason = reason,
                NickName = $"Interessado {bookRequested?.TotalInterested() + 1}"
            };

            if (!_bookService.Any(x => x.Id == bookUser.BookId))
                throw new ShareBookException(ShareBookException.Error.NotFound);

            if (_bookUserRepository.Any(x => x.UserId == bookUser.UserId && x.BookId == bookUser.BookId))
                throw new ShareBookException("O usuário já possui uma requisição para o mesmo livro.");

            _bookUserRepository.Insert(bookUser);

            _bookUsersEmailService.SendEmailBookRequested(bookUser);
            _bookUsersEmailService.SendEmailBookDonor(bookUser, bookRequested);
            _bookUsersEmailService.SendEmailBookInterested(bookUser, bookRequested);
        }

        public void DonateBook(Guid bookId, Guid userId, string note)
        {
            var book = _bookService.Find(bookId);
            if (!book.MayChooseWinner())
                throw new ShareBookException(ShareBookException.Error.BadRequest, "Aguarde a data de decisão.");

            var bookUserAccepted = _bookUserRepository.Get()
                .Include(u => u.Book).ThenInclude(b => b.UserFacilitator)
                .Include(u => u.Book).ThenInclude(b => b.User)
                .Include(u => u.User).ThenInclude(u => u.Address)
                .Where(x => x.UserId == userId
                    && x.BookId == bookId
                    && x.Status == DonationStatus.WaitingAction)
                    .FirstOrDefault();

            if (bookUserAccepted == null)
                throw new ShareBookException("Não existe a relação de usuário e livro para a doação.");

            bookUserAccepted.UpdateBookUser(DonationStatus.Donated, note);

            _bookUserRepository.Update(bookUserAccepted);

            DeniedBookUsers(bookId);

            _bookService.HideBook(bookId);

            // não usamos await nas notificações, pra serem assíncronas de verdade e retornar mais rápido.

            // avisa o ganhador
            var taskWinner = _bookUsersEmailService.SendEmailBookDonated(bookUserAccepted);

            // avisa os perdedores :/
            var taskLoosers = NotifyInterestedAboutBooksWinner(bookId);

            // avisa o doador
            var taskDonator = _bookUsersEmailService.SendEmailBookDonatedNotifyDonor(bookUserAccepted.Book, bookUserAccepted.User);
        }

        public Result<Book> Cancel(Guid bookId, bool isAdmin = false)
        {
            var book = _bookService.Find(bookId);

            if (book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            var bookUsers = _bookUserRepository.Get().Where(x => x.BookId == bookId).ToList();

            if (!isAdmin && bookUsers != null && bookUsers.Count > 0)
                throw new ShareBookException("Este livro já possui interessados");

            book.Approved = false;
            book.ChooseDate = null;
            book.Canceled = true;

            CancelBookUsersAndSendNotification(book);            

            _bookService.Update(book);
            _bookUsersEmailService.SendEmailBookCanceledToAdmins(book).Wait();

            return new Result<Book>(book);
        }

        public void DeniedBookUsers(Guid bookId)
        {
            var bookUsersDenied = _bookUserRepository.Get().Where(x => x.BookId == bookId
            && x.Status == DonationStatus.WaitingAction).ToList();
            foreach (var item in bookUsersDenied)
            {
                string note = string.Empty;
                item.UpdateBookUser(DonationStatus.Denied, note);
                _bookUserRepository.Update(item);
            }
        }

        private void CancelBookUsersAndSendNotification(Book book){
            DeniedBookUsers(book.Id);
            NotifyUsersBookCanceled(book);
        }

        public PagedList<BookUser> GetRequestsByUser()
        {
            var userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            return _bookUserRepository.Get(x => x.UserId == userId, x => x.Book, new IncludeList<BookUser>(b => b.Book));
        }

        public async Task NotifyInterestedAboutBooksWinner(Guid bookId)
        {
            //Obter todos os users do livro
            var bookUsers = _bookUserRepository.Get()
                                                .Include(u => u.Book)
                                                .Include(u => u.User)
                                                .Where(x => x.BookId == bookId).ToList();

            //obter apenas o ganhador
            var winnerBookUser = bookUsers.Where(bu => bu.Status == DonationStatus.Donated).FirstOrDefault();

            //Book
            var book =  winnerBookUser.Book;

            //usuarios que perderam a doação :(
            var losersBookUser = bookUsers.Where(bu => bu.Status == DonationStatus.Denied).ToList();

            //enviar e-mails
           await this._bookUsersEmailService.SendEmailDonationDeclined(book, winnerBookUser, losersBookUser);

        }

        public void NotifyUsersBookCanceled(Book book){

            
            List<BookUser> bookUsers = _bookUserRepository.Get()
                                            .Include(u => u.User)
                                            .Where(x => x.BookId == book.Id).ToList();

            
            this._bookUsersEmailService.SendEmailDonationCanceled(book, bookUsers).Wait();

        }

        public void InformTrackingNumber(Guid bookId, string trackingNumber)
        {
            var book = _bookService.Find(bookId);
            var winnerBookUser = _bookUserRepository
                                        .Get()
                                        .Include(u => u.User)
                                        .Where(bu => bu.BookId == bookId && bu.Status == DonationStatus.Donated)
                                        .FirstOrDefault();

            if (winnerBookUser == null)
                throw new ShareBookException("Vencedor ainda não foi escolhido");


            book.TrackingNumber = trackingNumber; 
            _bookService.Update(book);

            //Envia e-mail para avisar o ganhador do tracking number                          
            _bookUsersEmailService.SendEmailTrackingNumberInformed(winnerBookUser, book);
            
        }
    }
}

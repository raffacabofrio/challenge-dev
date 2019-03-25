﻿using DriverLib.Domain;
using DriverLib.Domain.Common;
using DriverLib.Service.Generic;
using System;
using System.Collections.Generic;

namespace DriverLib.Service
{
    public interface IUserService : IBaseService<User>
    {
        Result<User> AuthenticationByEmailAndPassword(User user);
        new Result<User> Update(User user);
        Result<User> ValidOldPasswordAndChangeUserPassword(User user, string newPassword);
        Result<User> ChangeUserPassword(User user, string newPassword);
        Result GenerateHashCodePasswordAndSendEmailToUser(string email);
        Result ConfirmHashCodePassword(string hashCodePassword);
        IList<User> GetFacilitators(Guid userIdDonator);
    }
}

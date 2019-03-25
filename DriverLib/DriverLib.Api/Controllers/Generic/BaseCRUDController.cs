﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using DriverLib.Api.Filters;
using DriverLib.Api.ViewModels;
using DriverLib.Domain.Common;
using DriverLib.Service.Generic;
using System;

namespace DriverLib.Api.Controllers
{
    public class BaseCrudController<T> : BaseCrudController<T, T, T>
        where T : BaseEntity
    {
        public BaseCrudController(IBaseService<T> service) : base(service) { }
    }

    public class BaseCrudController<T, R> : BaseDeleteController<T, R, T>
       where T : BaseEntity
       where R : BaseViewModel
    {
        public BaseCrudController(IBaseService<T> service) : base(service) { }
    }

    [GetClaimsFilter]
    [EnableCors("AllowAllHeaders")]
    public class BaseCrudController<T, R, A> : BaseDeleteController<T, R, A>
        where T : BaseEntity
        where R : IIdProperty
        where A : class
    {

        public BaseCrudController(IBaseService<T> service) : base(service) { }

        [Authorize("Bearer")]
        [HttpPost]
        public virtual Result<A> Create([FromBody] R viewModel)
        {
            if (!HasRequestViewModel)
                return Mapper.Map<Result<A>>(_service.Insert(viewModel as T));

            var entity = Mapper.Map<T>(viewModel);
            var result = _service.Insert(entity);
            var resultVM = Mapper.Map<Result<A>>(result);
            return resultVM;
        }

        [Authorize("Bearer")]
        [HttpPut("{id}")]
        public virtual Result<A> Update(Guid id, [FromBody] R viewModel)
        {
            viewModel.Id = id;

            if (!HasRequestViewModel)
                return Mapper.Map<Result<A>>(_service.Update(viewModel as T));

            var entity = Mapper.Map<T>(viewModel);
            var result = _service.Update(entity);
            var resultVM = Mapper.Map<A>(result);
            return new Result<A>(resultVM);
        }
    }
}

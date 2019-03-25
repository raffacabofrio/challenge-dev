﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Rollbar;
using ShareBook.Domain.Common;
using ShareBook.Domain.Exceptions;

namespace ShareBook.Api.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (ShareBookException ex)
            {
                var result = new Result();
                result.Messages.Add(ex.Message);
                var jsonResponse = JsonConvert.SerializeObject(result);

                httpContext.Response.Clear();
                httpContext.Response.StatusCode = (int)ex.ErrorType;
                await httpContext.Response.WriteAsync(jsonResponse);
                return;
            }
            catch (Exception ex)
            {
                throw ex;
                // TODO: rollbar está mascarando os erros. Buscar alternativa.    
                // await RollbarLocator.RollbarInstance.Error(ex);
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}

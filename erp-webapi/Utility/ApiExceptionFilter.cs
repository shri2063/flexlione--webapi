using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using flexli_erp_webapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace flexli_erp_webapi.Utility
{
    // This class is implemented to handle exceptions returned from controllers.
    // Another way to handle exception is UseExceptionHandler() in Startup.cs
    // If UseExceptionHandler() is used, then CORS header is erased, but we want
    // to keep the CORS header.
    //
    // However, using ExceptionFilterAttribute retains CORS headers
    // Ref: https://weblog.west-wind.com/posts/2016/oct/16/error-handling-and-exceptionfilter-dependency-injection-for-aspnet-core-apis
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            context.HttpContext.Response.ContentType = "application/json";
            Exception e = context.Exception;

            if (e is ExternalException)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            else if (e is ArgumentException)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            }
            else if (e is IndexOutOfRangeException || e is KeyNotFoundException)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            else if (e is InvalidOperationException)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
            else
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            context.Result = new JsonResult(new Error(e.Message, e.ToString()));

            base.OnException(context);
        }
    }
}
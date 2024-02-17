﻿using Ardalis.GuardClauses;
using Newtonsoft.Json;
using System.Net;
using Weather.API.Domain.Logging;
using WeatherApi.Domain.Http;

namespace Weather.API.Middleware
{
    public sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = Guard.Against.Null(next);
            _logger = Guard.Against.Null(logger);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception generalEx)
            {
                _logger.LogError(LogEvents.GeneralError, generalEx, "Unexpected error occurred.");
                await WriteResponseAsync(generalEx, context);
            }
        }

        private async Task WriteResponseAsync(Exception generalEx, HttpContext context)
        {
            var (responseCode, responseMessage) = ExtractFromException(generalEx);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)responseCode;
            var jsonResult = CreateResponseJson(responseMessage);
            await context.Response.WriteAsync(jsonResult);
        }

        private string CreateResponseJson(string errorMessage)
        {
            var response = new DataResponse<object>();
            response.Errors.Add(errorMessage);
            return JsonConvert.SerializeObject(response);
        }

        private (HttpStatusCode responseCode, string responseMessage) ExtractFromException(Exception generalEx)
            => generalEx switch
            {
                TaskCanceledException taskCanceledException =>(HttpStatusCode.NoContent, taskCanceledException.Message),
                _ => (HttpStatusCode.InternalServerError, "Generic error occurred on server. Check logs for more info.")
            };
    }
}

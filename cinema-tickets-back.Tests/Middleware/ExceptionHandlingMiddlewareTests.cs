using System.Net;
using System.Text.Json;
using CinemaTicketsBack.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace CinemaTicketsBack.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenExceptionIsThrown_Returns500InternalServerErrorJsonPayload()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        RequestDelegate next = (HttpContext ctx) => throw new InvalidOperationException("Test exception message");
        var middleware = new ExceptionHandlingMiddleware(next, loggerMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var jsonResponse = await reader.ReadToEndAsync();

        using var doc = JsonDocument.Parse(jsonResponse);
        var root = doc.RootElement;

        Assert.Equal(500, root.GetProperty("status").GetInt32());
        Assert.Equal("An internal server error occurred. Please try again later.", root.GetProperty("message").GetString());
        Assert.Equal("Test exception message", root.GetProperty("detail").GetString());
    }
}

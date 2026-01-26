using arna.HRMS.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace arna.HRMS.Infrastructure.Middleware;

public class TestAuthHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public TestAuthHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (string.IsNullOrEmpty(context.Request.Headers.Authorization)
            && !string.IsNullOrEmpty(TestTokenStore.Token))
        {
            context.Request.Headers.Authorization =
                $"Bearer {TestTokenStore.Token}";
        }

        await _next(context);
    }
}


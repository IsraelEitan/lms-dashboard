using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Lms.Api.Common;
using Lms.Api.Infrastructure.Attributes;
using Lms.Api.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lms.UnitTests;

public class IdempotencyMiddlewareTests
{
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<IdempotencyMiddleware>> _logger;
    private readonly IdempotencyMiddleware _middleware;
    private bool _nextCalled;

    public IdempotencyMiddlewareTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _logger = new Mock<ILogger<IdempotencyMiddleware>>();
        _nextCalled = false;
        _middleware = new IdempotencyMiddleware(
            next: (context) => 
            {
                _nextCalled = true;
                context.Response.StatusCode = 201;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"id\":\"test123\"}");
            },
            cache: _cache,
            logger: _logger.Object
        );
    }

    [Fact]
    public async Task InvokeAsync_SkipsNonPostRequests()
    {
        // Arrange
        var context = CreateHttpContext("GET");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.True(_nextCalled);
        Assert.False(context.Response.Headers.ContainsKey(Constants.Idempotency.HeaderName));
    }

    [Fact]
    public async Task InvokeAsync_SkipsPostWithoutIdempotencyAttribute()
    {
        // Arrange
        var context = CreateHttpContext("POST", hasIdempotencyAttribute: false);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.True(_nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsBadRequest_WhenIdempotencyKeyMissing()
    {
        // Arrange
        var context = CreateHttpContext("POST", hasIdempotencyAttribute: true, includeIdempotencyKey: false);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.False(_nextCalled);
        Assert.Equal(400, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsBadRequest_WhenIdempotencyKeyTooLong()
    {
        // Arrange
        var longKey = new string('x', Constants.Idempotency.MaxKeyLength + 1);
        var context = CreateHttpContext("POST", hasIdempotencyAttribute: true, idempotencyKey: longKey);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.False(_nextCalled);
        Assert.Equal(400, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ExecutesRequest_WhenIdempotencyKeyValid()
    {
        // Arrange
        var context = CreateHttpContext("POST", hasIdempotencyAttribute: true, idempotencyKey: "test-key-123");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.True(_nextCalled);
        Assert.Equal(201, context.Response.StatusCode);
        
        // Read response body
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("test123", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_CachesSuccessfulResponse()
    {
        // Arrange
        var context = CreateHttpContext("POST", hasIdempotencyAttribute: true, idempotencyKey: "cache-test");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var cacheKey = $"{Constants.Idempotency.CacheKeyPrefix}cache-test";
        Assert.True(_cache.TryGetValue(cacheKey, out var cached));
        Assert.NotNull(cached);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsCachedResponse_OnSecondRequest()
    {
        // Arrange
        var key = "duplicate-test";
        var context1 = CreateHttpContext("POST", hasIdempotencyAttribute: true, idempotencyKey: key);
        var context2 = CreateHttpContext("POST", hasIdempotencyAttribute: true, idempotencyKey: key);

        // Act - First request
        await _middleware.InvokeAsync(context1);
        
        // Reset flag
        _nextCalled = false;
        
        // Act - Second request with same key
        await _middleware.InvokeAsync(context2);

        // Assert
        Assert.False(_nextCalled); // Should not execute the actual request
        Assert.Equal(201, context2.Response.StatusCode);
        
        // Read response body
        context2.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context2.Response.Body).ReadToEndAsync();
        Assert.Contains("test123", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_AllowsOptionalIdempotency()
    {
        // Arrange
        var context = CreateHttpContext("POST", hasIdempotencyAttribute: true, 
            includeIdempotencyKey: false, idempotencyRequired: false);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.True(_nextCalled);
        Assert.Equal(201, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotCache_NonSuccessfulResponses()
    {
        // Arrange
        var errorMiddleware = new IdempotencyMiddleware(
            next: (context) => 
            {
                context.Response.StatusCode = 422; // Validation error
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\":\"validation failed\"}");
            },
            cache: _cache,
            logger: _logger.Object
        );
        var context = CreateHttpContext("POST", hasIdempotencyAttribute: true, idempotencyKey: "error-test");

        // Act
        await errorMiddleware.InvokeAsync(context);

        // Assert
        Assert.Equal(422, context.Response.StatusCode);
        var cacheKey = $"{Constants.Idempotency.CacheKeyPrefix}error-test";
        Assert.False(_cache.TryGetValue(cacheKey, out var _));
    }

    private HttpContext CreateHttpContext(
        string method, 
        bool hasIdempotencyAttribute = true,
        bool includeIdempotencyKey = true,
        string idempotencyKey = "default-key",
        bool idempotencyRequired = true)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Response.Body = new MemoryStream();

        if (method == "POST" && hasIdempotencyAttribute)
        {
            // Create a mock endpoint with IdempotencyAttribute
            var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.CreateAction));
            var actionDescriptor = new ControllerActionDescriptor
            {
                MethodInfo = methodInfo!,
                ControllerName = "Fake",
                ActionName = "CreateAction"
            };

            var endpoint = new Endpoint(
                requestDelegate: null,
                metadata: new EndpointMetadataCollection(
                    actionDescriptor,
                    new IdempotencyAttribute { Required = idempotencyRequired }
                ),
                displayName: "Test"
            );

            context.SetEndpoint(endpoint);
        }

        if (includeIdempotencyKey)
        {
            context.Request.Headers[Constants.Idempotency.HeaderName] = idempotencyKey;
        }

        return context;
    }

    // Fake controller class for testing
    private class FakeController : ControllerBase
    {
        [Idempotency]
        public IActionResult CreateAction() => Ok();
    }
}


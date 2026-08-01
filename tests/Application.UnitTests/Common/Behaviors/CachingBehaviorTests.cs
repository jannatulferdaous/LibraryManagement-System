using System.Text;
using System.Text.Json;
using Application.Common.Behaviors;
using Application.Common.Interfaces;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.UnitTests.Common.Behaviors;

public class CachingBehaviorTests
{
    private record CacheableTestQuery(string Id) : IRequest<string>, ICacheableQuery
    {
        public string CacheKey => $"test:{Id}";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
    }

    private record UncacheableTestQuery(string Id) : IRequest<string>;

    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly Mock<ILogger<CachingBehavior<CacheableTestQuery, string>>> _loggerMock = new();

    [Fact]
    public async Task Handle_CacheMiss_CallsHandlerAndPopulatesCache()
    {
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null); // GetStringAsync internally calls GetAsync

        var behavior = new CachingBehavior<CacheableTestQuery, string>(_cacheMock.Object, _loggerMock.Object);
        var handlerCalled = false;

        var result = await behavior.Handle(new CacheableTestQuery("1"), () =>
        {
            handlerCalled = true;
            return Task.FromResult("fresh-value");
        }, CancellationToken.None);

        result.Should().Be("fresh-value");
        handlerCalled.Should().BeTrue();

        _cacheMock.Verify(c => c.SetAsync(
            "test:1",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CacheHit_ReturnsCachedValueWithoutCallingHandler()
    {
        var cachedJson = JsonSerializer.Serialize("cached-value");
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(cachedJson));

        var behavior = new CachingBehavior<CacheableTestQuery, string>(_cacheMock.Object, _loggerMock.Object);
        var handlerCalled = false;

        var result = await behavior.Handle(new CacheableTestQuery("1"), () =>
        {
            handlerCalled = true;
            return Task.FromResult("should-not-be-returned");
        }, CancellationToken.None);

        result.Should().Be("cached-value");
        handlerCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_RequestNotCacheable_PassesThroughToHandlerWithoutTouchingCache()
    {
        var loggerMock = new Mock<ILogger<CachingBehavior<UncacheableTestQuery, string>>>();
        var behavior = new CachingBehavior<UncacheableTestQuery, string>(_cacheMock.Object, loggerMock.Object);

        var result = await behavior.Handle(new UncacheableTestQuery("1"),
            () => Task.FromResult("value"), CancellationToken.None);

        result.Should().Be("value");
        _cacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

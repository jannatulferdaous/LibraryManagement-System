using System.Text.Json;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(IDistributedCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only queries that explicitly opt in via ICacheableQuery go through Redis -
        // everything else passes straight through, no behavior change.
        if (request is not ICacheableQuery cacheable)
            return await next();

        try
        {
            var cached = await _cache.GetStringAsync(cacheable.CacheKey, cancellationToken);
            if (cached is not null)
            {
                _logger.LogInformation("Cache HIT for {CacheKey}", cacheable.CacheKey);
                return JsonSerializer.Deserialize<TResponse>(cached)!;
            }
        }
        catch (Exception ex)
        {
            // Redis being unavailable should degrade to "no cache", not break the request.
            _logger.LogWarning(ex, "Cache read failed for {CacheKey} - falling through to handler", cacheable.CacheKey);
        }

        _logger.LogInformation("Cache MISS for {CacheKey}", cacheable.CacheKey);
        var response = await next();

        try
        {
            var serialized = JsonSerializer.Serialize(response);
            await _cache.SetStringAsync(cacheable.CacheKey, serialized,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheable.CacheDuration },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache write failed for {CacheKey} - response still returned to caller", cacheable.CacheKey);
        }

        return response;
    }
}

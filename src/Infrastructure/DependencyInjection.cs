using Application.Borrowing.Strategies;
using Application.Common.Interfaces;
using Infrastructure.Identity;
using Infrastructure.Notifications;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // ApplicationDbContext implements IUnitOfWork directly - same scoped instance per request.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Factory pattern (Day 11): concrete channel implementations registered so the
        // factory can resolve them via DI instead of "new"-ing them up directly.
        services.AddScoped<EmailNotificationMessage>();
        services.AddScoped<InAppNotificationMessage>();
        services.AddScoped<INotificationFactory, NotificationFactory>();

        // Strategy pattern (Day 11): no state, no DI dependencies needed, safe as a singleton.
        services.AddSingleton<IFineStrategyFactory, FineStrategyFactory>();

        // Redis-backed distributed cache (bonus). CachingBehavior (Application layer)
        // uses this via IDistributedCache - any query implementing ICacheableQuery gets
        // cache-aside behavior automatically, no changes needed here per-query.
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:ConnectionString"] ?? "localhost:6379";
            options.InstanceName = "LibraryManagementSystem:";
        });

        return services;
    }
}

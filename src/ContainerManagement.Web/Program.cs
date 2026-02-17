using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Security;
using ContainerManagement.Application.Services;
using ContainerManagement.Infrastructure;
using ContainerManagement.Application;
using ContainerManagement.Infrastructure.Integrations;
using ContainerManagement.Infrastructure.Persistence;
using ContainerManagement.Infrastructure.Persistence.Repositories;
using ContainerManagement.Infrastructure.Persistence.Uow;
using ContainerManagement.Infrastructure.Seed;
using ContainerManagement.Web.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// JWT Auth
builder.Services
  .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(opt =>
{
    opt.RequireHttpsMetadata = false; // true in prod
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };

    opt.Events = new JwtBearerEvents
    {

        OnMessageReceived = ctx =>
        {
            if (string.IsNullOrWhiteSpace(ctx.Token) &&
                ctx.Request.Cookies.TryGetValue("access_token", out var cookieToken) &&
                !string.IsNullOrWhiteSpace(cookieToken))
            {
                ctx.Token = cookieToken;
            }
            return Task.CompletedTask;
        },

        OnTokenValidated = async ctx =>
        {
            var userIdStr = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            // safer: jti claim name
            var jti = ctx.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value
                      ?? ctx.Principal?.FindFirst("jti")?.Value;

            if (!Guid.TryParse(userIdStr, out var userId) || string.IsNullOrWhiteSpace(jti))
            {
                ctx.Fail("Missing userId/jti.");
                return;
            }

            var store = ctx.HttpContext.RequestServices.GetRequiredService<ITokenStore>();
            var ok = await store.IsJtiActiveAsync(userId, jti, ctx.HttpContext.RequestAborted);

            if (!ok) ctx.Fail("Token revoked/invalid.");
        }
    };
});


builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy(AppPolicies.PaymentsCreate, p => p.RequireRole(AppRoles.Admin, AppRoles.User));
    opt.AddPolicy(AppPolicies.PaymentsView, p => p.RequireRole(AppRoles.Admin, AppRoles.User));
    opt.AddPolicy(AppPolicies.ProvidersManage, p => p.RequireRole(AppRoles.Admin));
    opt.AddPolicy(AppPolicies.PaymentsProcess, p => p.RequireRole(AppRoles.Admin));
});

// Repos & UoW & Services
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped<ITokenStore, EfTokenStore>();

// JWT login services
builder.Services.AddScoped<IAuthUserRepository, AuthUserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserManagementRepository, EfUserManagementRepository>();
builder.Services.AddInfrastructure(); //Infrastructure services
builder.Services.AddApplication();  //Application services
builder.Services.AddControllersWithViews();



// FakeProvider client
builder.Services.AddHttpClient<IFakeProviderClient, FakeProviderClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["FakeProvider:BaseUrl"]!);
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ExceptionHandlingFilter>();
});


builder.Services.AddSignalR();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("Authorization") &&
        context.Request.Cookies.TryGetValue("access_token", out var token) &&
        !string.IsNullOrWhiteSpace(token))
    {
        context.Request.Headers.Authorization = $"Bearer {token}";
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);

app.MapGet("/", () => Results.Redirect("/Account/Login"));

// migrate + seed
await DbSeeder.SeedAsync(app.Services, builder.Configuration["FakeProvider:BaseUrl"]!);

// recurring retry
//RecurringJob.AddOrUpdate<ContainerManagement.Web.Jobs.FailedPaymentsRetryRunner>(
//    "failed-payments-retry-runner",
//    r => r.RunAsync(CancellationToken.None),
//    "*/2 * * * *"
//);

app.Run();

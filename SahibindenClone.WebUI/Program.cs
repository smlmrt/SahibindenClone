using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Infrastructure.Context;
using SahibindenClone.Infrastructure.Repositories;
using SahibindenClone.Infrastructure.Services;
using SahibindenClone.Application.Services;
using SahibindenClone.WebUI.Errors;
using SahibindenClone.WebUI.Hubs;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- JWT AUTHENTICATION AYARLARI ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true
    };
    // SignalR (WebSocket) için QueryString'den Token okuma ayarı
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userIdValue = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out var userId))
            {
                context.Fail("Geçersiz kullanıcı bilgisi.");
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var isActive = await db.Users.AnyAsync(u => u.Id == userId && u.IsActive);
            if (!isActive) context.Fail("Kullanıcı hesabı etkin değil.");
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new ApiErrorResponse("Kimlik doğrulaması gerekiyor.", StatusCodes.Status401Unauthorized));
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ApiErrorResponse("Bu işlem için yetkiniz yok.", StatusCodes.Status403Forbidden));
        },
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            // Eğer istek SignalR hub'ına geliyorsa ve token varsa, token'ı oradan al
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
// -----------------------------------

// SQLite Veritabanı Bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection (Bağımlılıkların Enjekte Edilmesi)
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IAdvertRepository, AdvertRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IAdminAuditLogRepository, AdminAuditLogRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAdvertService, AdvertService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<INotificationService, SahibindenClone.WebUI.Services.NotificationService>();

// ARKA PLAN SERVİSİ: İlan süresi kontrolü (builder.Build()'den ÖNCE olmalıdır)
builder.Services.AddHostedService<AdvertExpirationWorker>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();

// CORS Konfigürasyonu
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    options.AddPolicy("SignalR", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// API Controller'ları
builder.Services.AddControllers(options => options.Filters.Add<ApiErrorResultFilter>())
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var validationMessage = string.Join(" ", context.ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct());
            var message = string.IsNullOrWhiteSpace(validationMessage) ? "İstek doğrulanamadı." : validationMessage;
            return new BadRequestObjectResult(new ApiErrorResponse(message, StatusCodes.Status400BadRequest));
        };
    });

// DİKKAT: Bu satırdan sonra builder.Services değiştirilemez!
var app = builder.Build();

// --- SEED (Başlangıç Verisi) İŞLEMİ BAŞLANGICI ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        DbInitializer.Initialize(context);
        var adminEmail = app.Configuration["Admin:Email"];
        if (!string.IsNullOrWhiteSpace(adminEmail))
        {
            var adminUser = context.Users.FirstOrDefault(u => u.Email.ToLower() == adminEmail.Trim().ToLower());
            if (adminUser is not null && adminUser.Role != "Admin")
            {
                adminUser.Role = "Admin";
                context.SaveChanges();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Veritabanı seed işlemi sırasında hata oluştu: " + ex.Message);
    }
}
// --- SEED İŞLEMİ BİTİŞİ ---

// Statik HTML dosyalarının (index.html) varsayılan olarak açılmasını sağlar
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;
    if (!httpContext.Request.Path.StartsWithSegments("/api")) return;

    var statusCode = httpContext.Response.StatusCode;
    var message = statusCode switch
    {
        StatusCodes.Status404NotFound => "İstenen kaynak bulunamadı.",
        StatusCodes.Status401Unauthorized => "Kimlik doğrulaması gerekiyor.",
        StatusCodes.Status403Forbidden => "Bu işlem için yetkiniz yok.",
        _ => "İstek işlenemedi."
    };
    await httpContext.Response.WriteAsJsonAsync(new ApiErrorResponse(message, statusCode));
});
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();
app.UseRouting();

// Authentication middleware'i Routing'den sonra, Authorization'dan önce gelmeli
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // API rotalarını aktifleştirir
app.MapHub<NotificationHub>("/notificationHub")
    .RequireCors("SignalR");

app.Run();

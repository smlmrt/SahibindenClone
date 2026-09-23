using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Infrastructure.Context;
using SahibindenClone.Infrastructure.Repositories;
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

// MVC yerine sadece API Controller'ları ekliyoruz
builder.Services.AddControllers();

var app = builder.Build();

// --- SEED (Başlangıç Verisi) İŞLEMİ BAŞLANGICI ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Veritabanı seed işlemi sırasında hata oluştu: " + ex.Message);
    }
}
// --- SEED İŞLEMİ BİTİŞİ ---

// Statik HTML dosyalarının (index.html) varsayılan olarak açılmasını sağlar
app.UseDefaultFiles(); 
app.UseStaticFiles();

app.UseRouting();

// Authentication middleware'i Routing'den sonra, Authorization'dan önce gelmeli
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // API rotalarını aktifleştirir

app.Run();
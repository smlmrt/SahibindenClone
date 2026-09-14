using Microsoft.EntityFrameworkCore;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Infrastructure.Context;
using SahibindenClone.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IAdvertRepository, AdvertRepository>();

// MVC yerine sadece API Controller'ları ekliyoruz
builder.Services.AddControllers();

var app = builder.Build();

// Statik HTML dosyalarının (index.html) varsayılan olarak açılmasını sağlar
app.UseDefaultFiles(); 
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllers(); // API rotalarını aktifleştirir

app.Run();
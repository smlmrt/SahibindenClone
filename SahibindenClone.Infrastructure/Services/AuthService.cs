using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using SahibindenClone.Application.DTOs;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Application.Services;
using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Infrastructure.Services;

public sealed class AuthService(IUserRepository users, IConfiguration configuration) : IAuthService
{
    public async Task<ServiceResult<bool>> RegisterAsync(RegisterDto dto)
    {
        if (await users.GetUserByEmailAsync(dto.Email) is not null)
            return ServiceResult<bool>.Failure(ServiceError.Conflict, "Bu email adresi zaten kullanımda.");
        await users.AddAsync(new User
        {
            FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), CreatedAt = DateTime.UtcNow, IsActive = true,
            Role = string.Equals(dto.Email.Trim(), configuration["Admin:Email"]?.Trim(), StringComparison.OrdinalIgnoreCase) ? "Admin" : "User"
        });
        await users.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<AuthResult>> LoginAsync(LoginDto dto)
    {
        var user = await users.GetUserByEmailAsync(dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return ServiceResult<AuthResult>.Failure(ServiceError.Unauthorized, "Email veya şifre hatalı");
        var key = Encoding.ASCII.GetBytes(configuration["JwtSettings:SecretKey"]!);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(configuration["JwtSettings:ExpiryMinutes"]!)),
            Issuer = configuration["JwtSettings:Issuer"], Audience = configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = new JwtSecurityTokenHandler().CreateToken(descriptor);
        return ServiceResult<AuthResult>.Success(new AuthResult(new JwtSecurityTokenHandler().WriteToken(token), user.Id, $"{user.FirstName} {user.LastName}", user.Role));
    }
}

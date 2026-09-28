using SahibindenClone.Application.DTOs;

namespace SahibindenClone.Application.Services;

public sealed record AuthResult(string Token, int UserId, string UserName);

public interface IAuthService
{
    Task<ServiceResult<bool>> RegisterAsync(RegisterDto dto);
    Task<ServiceResult<AuthResult>> LoginAsync(LoginDto dto);
}

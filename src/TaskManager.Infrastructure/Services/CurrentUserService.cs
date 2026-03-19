using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TaskManager.Application.Interfaces;

namespace TaskManager.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("sub")
                ?? _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            return claim is not null ? Guid.Parse(claim.Value) : Guid.Empty;
        }
    }

    public string Email =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    public string Role =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
}

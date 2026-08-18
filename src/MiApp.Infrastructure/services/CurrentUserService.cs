namespace MiApp.Infrastructure.Services;

using System.Security.Claims;
using MiApp.Application.Interfaces;
using Microsoft.AspNetCore.Http;
public class CurrentUserService : ICurrentUserService{

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);

            if(userIdClaim == null){
                throw new UnauthorizedAccessException();
            }
            
            if(!int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException();
            }

            return userId;
        }
    }
}
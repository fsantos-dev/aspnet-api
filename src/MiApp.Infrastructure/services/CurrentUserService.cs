namespace MiApp.Infrastructure.Services;

using System.IdentityModel.Tokens.Jwt;
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
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub);

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
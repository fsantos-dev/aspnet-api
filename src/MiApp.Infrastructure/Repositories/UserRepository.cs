using MiApp.Domain.Entities;
using MiApp.Domain.Interfaces;

namespace MiApp.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private static List<User> _users = new ()
    {
       new User
       {
           Id = 1,
           Email = "admin@example.com",
           PasswordHash = "12345",
           FullName = "Admin User",
           IsActive = true,
           CreatedAt = DateTime.UtcNow,
       }
    };

    public Task<User?> GetByEmailAsync(string email)
    {
        var user = _users.FirstOrDefault(user => user.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<User> CreateAsync(User user)
    {
        user.Id = _users.Count + 1;
        _users.Add(user);
        return Task.FromResult(user);
    }
}
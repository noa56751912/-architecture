using Entities;

namespace Repositories
{
    public interface IUserRepository
    {
        
        Task<User?> GetUserById(int id);
        Task<User?> Login(string email);
        Task<User?> Register(User user);
        Task Update(int id, User updateUser);
        Task<IEnumerable<User>> GetUsers();
        Task<bool> UserWithSameEmail(string email, int id);
    }
}
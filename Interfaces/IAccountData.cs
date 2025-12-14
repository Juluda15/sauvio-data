using SauvioData.Models;

namespace SuavioData.Interfaces
{
    public interface IAccountData
    {
        Task<User?> GetByEmail(string email);
        Task<User?> GetByToken(string token);
        Task<User?> GetById(int id);
        Task<int> CreateUser(User user);
        Task ConfirmUser(int userId);
        Task UpdatePassword(int userId, string hashedPassword);
    }
}


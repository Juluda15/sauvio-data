using SauvioData.Models;
using SauvioData.Models.Transaction;

namespace SauvioData.Interfaces
{
    public interface IFinanceData
    {
        Task AddTransaction(Transaction transaction);
        Task<List<Transaction>> GetTransactions(int userId, string type);

        Task UpdateUserBalance(
            int userId,
            decimal balanceDelta,
            decimal incomeDelta,
            decimal expenseDelta
        );

        Task<User?> GetUserById(int userId);
    }
}

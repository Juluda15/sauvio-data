using SauvioData.Entities.Transaction;
using SauvioData.Entities.User;

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
        Task<decimal> CalculateTotalIncome(int userId);
        Task<decimal> CalculateTotalExpense(int userId);
        Task<decimal> CalculateBalance(int userId);
        Task DeleteTransaction(int transactionId);
        Task UpdateTransaction(Transaction t);
        Task<Transaction?> GetTransactionById(int transactionId);
    }
}

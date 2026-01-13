using Npgsql;
using SauvioData.Interfaces;
using SauvioData.Entities.Transaction;
using SauvioData.Entities.User;

namespace SauvioData.Data
{
    public class FinanceRepo : IFinanceData
    {
        private readonly DbConnectionFactory _db;

        public FinanceRepo(DbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<User?> GetUserById(int userId)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(
                "SELECT * FROM users WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", userId);

            using var r = await cmd.ExecuteReaderAsync();
            if (!r.Read()) return null;

            return new User
            {
                Id = r.GetInt32(r.GetOrdinal("id")),
                Balance = r.GetDecimal(r.GetOrdinal("balance")),
                TotalIncome = r.GetDecimal(r.GetOrdinal("totalincome")),
                TotalExpense = r.GetDecimal(r.GetOrdinal("totalexpense"))
            };
        }

        public async Task AddTransaction(Transaction t)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                INSERT INTO transactions
                (userid, amount, type, description, sourceorcategory, date)
                VALUES
                (@uid, @amount, @type, @desc, @src, NOW())", conn);

            cmd.Parameters.AddWithValue("@uid", t.UserId);
            cmd.Parameters.AddWithValue("@amount", t.Amount);
            cmd.Parameters.AddWithValue("@type", t.Type);
            cmd.Parameters.AddWithValue("@desc", t.Description);
            cmd.Parameters.AddWithValue("@src", t.SourceOrCategory);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateUserBalance(
            int userId,
            decimal balance,
            decimal totalIncome,
            decimal totalExpense)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                UPDATE users SET
                    balance = @balance,
                    totalincome = @totalIncome,
                    totalexpense = @totalExpense
                WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("@balance", balance);
            cmd.Parameters.AddWithValue("@totalIncome", totalIncome);
            cmd.Parameters.AddWithValue("@totalExpense", totalExpense);
            cmd.Parameters.AddWithValue("@id", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Transaction?> GetTransactionById(int transactionId)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(
                "SELECT * FROM transactions WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", transactionId);

            using var r = await cmd.ExecuteReaderAsync();
            if (!r.Read()) return null;

            return new Transaction
            {
                Id = r.GetInt32(r.GetOrdinal("id")),
                UserId = r.GetInt32(r.GetOrdinal("userid")),
                Amount = r.GetDecimal(r.GetOrdinal("amount")),
                Type = r["type"].ToString()!,
                Description = r["description"].ToString()!,
                SourceOrCategory = r["sourceorcategory"].ToString()!,
                Date = r.GetDateTime(r.GetOrdinal("date"))
            };
        }

        public async Task UpdateTransaction(Transaction t)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                UPDATE transactions SET
                    amount = @amount,
                    description = @desc,
                    sourceorcategory = @src
                WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("@amount", t.Amount);
            cmd.Parameters.AddWithValue("@desc", t.Description);
            cmd.Parameters.AddWithValue("@src", t.SourceOrCategory);
            cmd.Parameters.AddWithValue("@id", t.Id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteTransaction(int transactionId)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(
                "DELETE FROM transactions WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", transactionId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Transaction>> GetTransactions(int userId, string type)
        {
            var list = new List<Transaction>();

            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT * FROM transactions
                WHERE userid = @uid AND type = @type
                ORDER BY date DESC", conn);

            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@type", type);

            using var r = await cmd.ExecuteReaderAsync();
            while (r.Read())
            {
                list.Add(new Transaction
                {
                    Id = r.GetInt32(r.GetOrdinal("id")),
                    UserId = userId,
                    Amount = r.GetDecimal(r.GetOrdinal("amount")),
                    Type = r["type"].ToString()!,
                    Description = r["description"].ToString()!,
                    SourceOrCategory = r["sourceorcategory"].ToString()!,
                    Date = r.GetDateTime(r.GetOrdinal("date"))
                });
            }

            return list;
        }

        public async Task<decimal> CalculateTotalIncome(int userId)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(
                "SELECT COALESCE(SUM(amount),0) FROM transactions WHERE userid = @uid AND type = 'income'", conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToDecimal(result);
        }

        public async Task<decimal> CalculateTotalExpense(int userId)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(
                "SELECT COALESCE(SUM(amount),0) FROM transactions WHERE userid = @uid AND type = 'expense'", conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToDecimal(result);
        }

        public async Task<decimal> CalculateBalance(int userId)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(
                "SELECT COALESCE(SUM(CASE WHEN type='income' THEN amount ELSE -amount END),0) FROM transactions WHERE userid = @uid", conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToDecimal(result);
        }
    }
}

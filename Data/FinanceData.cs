using Npgsql;
using SauvioData.Interfaces;
using SauvioData.Models.Transaction;
using SauvioData.Models;

namespace SauvioData.Data
{
    public class FinanceData : IFinanceData
    {
        private readonly DbConnectionFactory _db;

        public FinanceData(DbConnectionFactory db)
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
            decimal balanceDelta,
            decimal incomeDelta,
            decimal expenseDelta)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                UPDATE users SET
                    balance = balance + @balance,
                    totalincome = totalincome + @income,
                    totalexpense = totalexpense + @expense
                WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("@balance", balanceDelta);
            cmd.Parameters.AddWithValue("@income", incomeDelta);
            cmd.Parameters.AddWithValue("@expense", expenseDelta);
            cmd.Parameters.AddWithValue("@id", userId);

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
    }
}

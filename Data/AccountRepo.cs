using Npgsql;
using SauvioData.Entities.User;
using SuavioData.Interfaces;

namespace SauvioData.Data
{
    public class AccountRepo : IAccountData
    {
        private readonly DbConnectionFactory _db;

        public AccountRepo(DbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<User?> GetByEmail(string email)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM users WHERE email = @Email", conn);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = await cmd.ExecuteReaderAsync();
            return reader.Read() ? MapUser(reader) : null;
        }

        public async Task<User?> GetByToken(string token)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(
                "SELECT * FROM users WHERE confirmationtoken = @token", conn);
            cmd.Parameters.AddWithValue("@token", token);

            using var r = await cmd.ExecuteReaderAsync();
            return r.Read() ? MapUser(r) : null;
        }

        public async Task<int> CreateUser(User user)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
            INSERT INTO users (name, email, password, confirmationtoken, isconfirmed)
            VALUES (@name, @email, @password, @token, false)
            RETURNING id", conn);

            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@password", user.Password);
            cmd.Parameters.AddWithValue("@token", user.ConfirmationToken);

            return (int)await cmd.ExecuteScalarAsync();
        }

        public async Task ConfirmUser(int userId)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(
                "UPDATE users SET isconfirmed = true, confirmationtoken = NULL WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdatePassword(int userId, string hashedPassword)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(
                "UPDATE users SET password = @pwd WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@pwd", hashedPassword);
            cmd.Parameters.AddWithValue("@id", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        private User MapUser(NpgsqlDataReader r) => new()
        {
            Id = r.GetInt32(r.GetOrdinal("id")),
            Name = r["name"] as string,
            Email = r["email"] as string,
            Password = r["password"] as string,
            ConfirmationToken = r["confirmationtoken"] as string,
            IsConfirmed = r.GetBoolean(r.GetOrdinal("isconfirmed")),
            IsAdmin = r.GetBoolean(r.GetOrdinal("isadmin")),
            Balance = r.GetDecimal(r.GetOrdinal("balance")),
            TotalIncome = r.GetDecimal(r.GetOrdinal("totalincome")),
            TotalExpense = r.GetDecimal(r.GetOrdinal("totalexpense"))
        };

        public async Task<User?> GetById(int id)
        {
            using var conn = _db.Create();
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(
                "SELECT * FROM users WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var r = await cmd.ExecuteReaderAsync();
            return r.Read() ? MapUser(r) : null;
        }
    }
}

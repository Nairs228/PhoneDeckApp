using Microsoft.Data.Sqlite;
using PhoneDeckApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PhoneDeckApp.Services;

public class DatabaseService
{
    private readonly string _dbPath;

    public DatabaseService()
    {
        _dbPath = Path.Combine(AppContext.BaseDirectory, "users.db");
        _deviceDbPath = Path.Combine(AppContext.BaseDirectory, "devices.db");
        InitBonusTables();
    }

    private void InitBonusTables()
    {
        using var conn = Connect();
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS user_bonuses (
            user_id INTEGER PRIMARY KEY,
            balance INTEGER NOT NULL DEFAULT 999
        );
        CREATE TABLE IF NOT EXISTS bonus_history (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            user_id INTEGER NOT NULL,
            amount INTEGER NOT NULL,
            description TEXT NOT NULL,
            date TEXT NOT NULL
        );
        """;
        cmd.ExecuteNonQuery();
    }

    public int GetBonusBalance(int userId)
    {
        using var conn = Connect();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT balance FROM user_bonuses WHERE user_id = @id";
        cmd.Parameters.AddWithValue("@id", userId);
        var result = cmd.ExecuteScalar();
        if (result == null)
        {
            // Первый раз — создаём запись с 999
            var insert = conn.CreateCommand();
            insert.CommandText = "INSERT INTO user_bonuses (user_id, balance) VALUES (@id, 999)";
            insert.Parameters.AddWithValue("@id", userId);
            insert.ExecuteNonQuery();
            return 999;
        }
        return Convert.ToInt32(result);
    }

    public void SetBonusBalance(int userId, int balance)
    {
        using var conn = Connect();
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
        INSERT INTO user_bonuses (user_id, balance) VALUES (@id, @b)
        ON CONFLICT(user_id) DO UPDATE SET balance = @b
        """;
        cmd.Parameters.AddWithValue("@id", userId);
        cmd.Parameters.AddWithValue("@b", balance);
        cmd.ExecuteNonQuery();
    }

    public void AddBonusHistory(int userId, int amount, string description)
    {
        using var conn = Connect();
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
        INSERT INTO bonus_history (user_id, amount, description, date)
        VALUES (@uid, @a, @d, @dt)
        """;
        cmd.Parameters.AddWithValue("@uid", userId);
        cmd.Parameters.AddWithValue("@a", amount);
        cmd.Parameters.AddWithValue("@d", description);
        cmd.Parameters.AddWithValue("@dt", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
        cmd.ExecuteNonQuery();
    }

    public List<BonusHistoryItem> GetBonusHistory(int userId)
    {
        var list = new List<BonusHistoryItem>();
        using var conn = Connect();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM bonus_history WHERE user_id = @id ORDER BY id DESC";
        cmd.Parameters.AddWithValue("@id", userId);
        using var reader = cmd.ExecuteReader();
        int code = 1;
        while (reader.Read())
        {
            list.Add(new BonusHistoryItem
            {
                Code = code++,
                Amount = Convert.ToInt32(reader["amount"]),
                Description = reader["description"].ToString()!,
                Date = reader["date"].ToString()!
            });
        }
        return list;
    }

    public bool UpdateUserByAdmin(int userId, string lastName, string firstName,
        string patronymic, string phoneModel, string username, bool isAdmin)
    {
        try
        {
            using var conn = Connect();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
            UPDATE users SET last_name=@ln, first_name=@fn,
            patronymic=@p, phone_model=@pm, username=@u, is_admin=@a
            WHERE id=@id
            """;
            cmd.Parameters.AddWithValue("@ln", lastName);
            cmd.Parameters.AddWithValue("@fn", firstName);
            cmd.Parameters.AddWithValue("@p", patronymic);
            cmd.Parameters.AddWithValue("@pm", phoneModel);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@a", isAdmin ? 1 : 0);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch { return false; }
    }

    private SqliteConnection Connect()
    {
        var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        return conn;
    }

    // Проверка werkzeug pbkdf2:sha256 хешей
    private bool VerifyWerkzeugHash(string password, string storedHash)
    {
        try
        {
            // Формат: pbkdf2:sha256:iterations$salt$hash
            var parts = storedHash.Split('$');
            if (parts.Length != 3) return false;

            var methodParts = parts[0].Split(':');
            if (methodParts.Length != 3) return false;

            int iterations = int.Parse(methodParts[2]);
            var salt = Encoding.UTF8.GetBytes(parts[1]);
            var expectedHash = Convert.FromHexString(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password, salt, iterations, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }

    private string HashPassword(string password)
    {
        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        var saltStr = Convert.ToHexString(salt).ToLower();
        int iterations = 260000;

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password, Encoding.UTF8.GetBytes(saltStr), iterations, HashAlgorithmName.SHA256);
        var hash = Convert.ToHexString(pbkdf2.GetBytes(32)).ToLower();

        return $"pbkdf2:sha256:{iterations}${saltStr}${hash}";
    }

    public User? Login(string username, string password)
    {
        using var conn = Connect();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM users WHERE username = @u";
        cmd.Parameters.AddWithValue("@u", username);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        var hash = reader["password_hash"].ToString()!;
        if (!VerifyWerkzeugHash(password, hash)) return null;

        return new User
        {
            Id = Convert.ToInt32(reader["id"]),
            Username = reader["username"].ToString()!,
            LastName = reader["last_name"].ToString()!,
            FirstName = reader["first_name"].ToString()!,
            Patronymic = reader["patronymic"].ToString()!,
            PhoneModel = reader["phone_model"].ToString()!,
            IsAdmin = Convert.ToInt32(reader["is_admin"]) == 1
        };
    }
    public User? GetUserById(int id)
    {
        using var conn = Connect();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM users WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        return new User
        {
            Id = Convert.ToInt32(reader["id"]),
            Username = reader["username"].ToString()!,
            LastName = reader["last_name"].ToString()!,
            FirstName = reader["first_name"].ToString()!,
            Patronymic = reader["patronymic"].ToString()!,
            PhoneModel = reader["phone_model"].ToString()!,
            IsAdmin = Convert.ToInt32(reader["is_admin"]) == 1
        };
    }

    public bool Register(string username, string password,
        string lastName, string firstName, string patronymic, string phoneModel)
    {
        try
        {
            var hash = HashPassword(password);
            using var conn = Connect();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO users (username, password_hash, last_name, first_name, patronymic, phone_model)
                VALUES (@u, @h, @ln, @fn, @p, @pm)
                """;
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@h", hash);
            cmd.Parameters.AddWithValue("@ln", lastName);
            cmd.Parameters.AddWithValue("@fn", firstName);
            cmd.Parameters.AddWithValue("@p", patronymic);
            cmd.Parameters.AddWithValue("@pm", phoneModel);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch
        {
            return false;
        }
    }
    private readonly string _deviceDbPath;

    // Добавь в конструктор:
    // _deviceDbPath = Path.Combine(AppContext.BaseDirectory, "devices.db");

    public List<Session> GetSessions()
    {
        var list = new List<Session>();
        try
        {
            var deviceDb = Path.Combine(AppContext.BaseDirectory, "devices.db");
            using var conn = new SqliteConnection($"Data Source={deviceDb}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM devices ORDER BY id DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Session
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Name = reader["name"].ToString()!,
                    Model = reader["model"].ToString()!,
                    Charge = reader["charge"].ToString()!,
                    ConnectionTime = reader["connection_time"].ToString()!,
                    DisconnectionTime = reader["disconnection_time"].ToString()!
                });
            }
        }
        catch { }
        return list;
    }

    public List<User> GetAllUsers()
    {
        var list = new List<User>();
        try
        {
            using var conn = Connect();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM users ORDER BY id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new User
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Username = reader["username"].ToString()!,
                    LastName = reader["last_name"].ToString()!,
                    FirstName = reader["first_name"].ToString()!,
                    Patronymic = reader["patronymic"].ToString()!,
                    PhoneModel = reader["phone_model"].ToString()!,
                    IsAdmin = Convert.ToInt32(reader["is_admin"]) == 1
                });
            }
        }
        catch { }
        return list;
    }

    public bool SetAdminRole(int userId, bool isAdmin)
    {
        try
        {
            using var conn = Connect();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE users SET is_admin = @a WHERE id = @id";
            cmd.Parameters.AddWithValue("@a", isAdmin ? 1 : 0);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch { return false; }
    }

    public bool UpdateProfile(int userId, string lastName, string firstName,
        string patronymic, string phoneModel, string username)
    {
        try
        {
            using var conn = Connect();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
            UPDATE users SET last_name=@ln, first_name=@fn,
            patronymic=@p, phone_model=@pm, username=@u WHERE id=@id
            """;
            cmd.Parameters.AddWithValue("@ln", lastName);
            cmd.Parameters.AddWithValue("@fn", firstName);
            cmd.Parameters.AddWithValue("@p", patronymic);
            cmd.Parameters.AddWithValue("@pm", phoneModel);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch { return false; }
    }

    public bool ChangePassword(int userId, string currentPassword, string newPassword)
    {
        using var conn = Connect();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT password_hash FROM users WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", userId);
        var hash = cmd.ExecuteScalar()?.ToString() ?? "";
        if (!VerifyWerkzeugHash(currentPassword, hash)) return false;

        var newHash = HashPassword(newPassword);
        var cmd2 = conn.CreateCommand();
        cmd2.CommandText = "UPDATE users SET password_hash = @h WHERE id = @id";
        cmd2.Parameters.AddWithValue("@h", newHash);
        cmd2.Parameters.AddWithValue("@id", userId);
        cmd2.ExecuteNonQuery();
        return true;
    }

    public (int users, string usersPath, int devices, string devicesPath) GetDbHealth()
    {
        int userCount = 0, deviceCount = 0;
        try
        {
            using var conn = Connect();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM users";
            userCount = Convert.ToInt32(cmd.ExecuteScalar());
        }
        catch { }

        try
        {
            var deviceDb = Path.Combine(AppContext.BaseDirectory, "devices.db");
            using var conn = new SqliteConnection($"Data Source={deviceDb}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM devices";
            deviceCount = Convert.ToInt32(cmd.ExecuteScalar());
            return (userCount, _dbPath, deviceCount, deviceDb);
        }
        catch { return (userCount, _dbPath, 0, ""); }
    }
}
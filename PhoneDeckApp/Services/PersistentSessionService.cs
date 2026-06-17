using System;
using System.IO;
using System.Text.Json;
using PhoneDeckApp.Models;

namespace PhoneDeckApp.Services;

public class PersistentSessionService
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PhoneDeck", "session.json");

    public static void Save(User user)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var data = JsonSerializer.Serialize(new { user.Id, user.Username });
        File.WriteAllText(FilePath, data);
    }

    public static int? LoadUserId()
    {
        if (!File.Exists(FilePath)) return null;
        try
        {
            var data = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(FilePath));
            return data.GetProperty("Id").GetInt32();
        }
        catch { return null; }
    }

    public static void Clear()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
    }
}
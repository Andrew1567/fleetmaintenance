using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FleetMaintenance.Infrastructure;

/// <summary>Читання й запис списків у файл JSON.</summary>
internal static class JsonFile
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    internal static List<T> Read<T>(string path)
    {
        if (!File.Exists(path))
        {
            return new List<T>();
        }

        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>();
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Файл {path} пошкоджено: {ex.Message}", ex);
        }
    }

    internal static void Write<T>(string path, IEnumerable<T> items)
    {
        string? dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(path, JsonSerializer.Serialize(items, Options));
    }
}

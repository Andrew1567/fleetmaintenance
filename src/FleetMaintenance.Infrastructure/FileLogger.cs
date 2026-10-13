using System;
using System.Globalization;
using System.IO;
using FleetMaintenance.Application;

namespace FleetMaintenance.Infrastructure;

/// <summary>Журнал у текстовому файлі: час|рівень|сценарій|повідомлення.</summary>
public sealed class FileLogger : IAppLogger
{
    private readonly string _path;
    private readonly TimeProvider _time;

    /// <summary>Створює журнал; теку буде створено під час першого запису.</summary>
    /// <param name="path">Шлях до файлу журналу.</param>
    /// <param name="time">Джерело часу для позначок.</param>
    public FileLogger(string path, TimeProvider time)
    {
        _path = path;
        _time = time;
    }

    /// <inheritdoc />
    public void Information(string scenario, string message)
    {
        Write("INFO", scenario, message);
    }

    /// <inheritdoc />
    public void Warning(string scenario, string message)
    {
        Write("WARN", scenario, message);
    }

    /// <inheritdoc />
    public void Error(string scenario, string message, Exception error)
    {
        Write("ERROR", scenario, $"{message}: {error.GetType().Name}: {error.Message}");
    }

    private void Write(string level, string scenario, string message)
    {
        DateTimeOffset now = _time.GetLocalNow();
        string stamp = now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        string? dir = Path.GetDirectoryName(Path.GetFullPath(_path));
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.AppendAllText(_path, $"{stamp}|{level}|{scenario}|{message}{Environment.NewLine}");
    }
}

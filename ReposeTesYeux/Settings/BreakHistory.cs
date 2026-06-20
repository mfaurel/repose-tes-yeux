using System.Text.Json;

namespace ReposeTesYeux.Settings;

public class BreakHistory
{
    private static readonly string DefaultPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ReposeTesYeux", "history.json");

    private readonly string _filePath;
    private Dictionary<string, int> _data = new();

    public BreakHistory(string? filePath = null)
    {
        _filePath = filePath ?? DefaultPath;
        Load();
    }

    public int GetToday() => _data.TryGetValue(TodayKey(), out var v) ? v : 0;

    public int GetWeekTotal()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return Enumerable.Range(0, 7)
            .Sum(i => _data.TryGetValue(today.AddDays(-i).ToString("yyyy-MM-dd"), out var v) ? v : 0);
    }

    public int GetMonthTotal()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return Enumerable.Range(0, 30)
            .Sum(i => _data.TryGetValue(today.AddDays(-i).ToString("yyyy-MM-dd"), out var v) ? v : 0);
    }

    public IReadOnlyDictionary<DateOnly, int> GetLastNDays(int n)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var result = new Dictionary<DateOnly, int>();
        for (int i = 0; i < n; i++)
        {
            var date = today.AddDays(-i);
            result[date] = _data.TryGetValue(date.ToString("yyyy-MM-dd"), out var v) ? v : 0;
        }
        return result;
    }

    public void Increment()
    {
        var key = TodayKey();
        _data[key] = GetToday() + 1;
        Trim();
        Save();
    }

    private static string TodayKey() => DateTime.Today.ToString("yyyy-MM-dd");

    private void Load()
    {
        try
        {
            if (File.Exists(_filePath))
                _data = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(_filePath)) ?? new();
        }
        catch { _data = new(); }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_data));
        }
        catch { }
    }

    private void Trim()
    {
        var cutoff = DateOnly.FromDateTime(DateTime.Today.AddDays(-35)).ToString("yyyy-MM-dd");
        foreach (var key in _data.Keys.Where(k => string.Compare(k, cutoff, StringComparison.Ordinal) < 0).ToList())
            _data.Remove(key);
    }
}

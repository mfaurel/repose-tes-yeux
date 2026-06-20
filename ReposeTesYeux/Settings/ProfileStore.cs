using System.Text.Json;

namespace ReposeTesYeux.Settings;

public class ProfileStore
{
    public const string DefaultProfileKey = "Default";

    private static readonly string DefaultPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ReposeTesYeux", "profiles.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _filePath;
    private Dictionary<string, AppSettings> _profiles = new();

    public ProfileStore(string? filePath = null)
    {
        _filePath = filePath ?? DefaultPath;
        Load();
        if (!_profiles.ContainsKey(DefaultProfileKey))
            _profiles[DefaultProfileKey] = new AppSettings().WithDefaults();
    }

    public IReadOnlyList<string> Names => _profiles.Keys.OrderBy(k => k).ToList();

    public AppSettings Get(string name) =>
        _profiles.TryGetValue(name, out var s) ? s : new AppSettings().WithDefaults();

    public void Save(string name, AppSettings settings)
    {
        _profiles[name] = settings;
        Persist();
    }

    public bool Delete(string name)
    {
        if (name == DefaultProfileKey) return false;
        bool removed = _profiles.Remove(name);
        if (removed) Persist();
        return removed;
    }

    public bool Contains(string name) => _profiles.ContainsKey(name);

    private void Load()
    {
        try
        {
            if (File.Exists(_filePath))
                _profiles = JsonSerializer.Deserialize<Dictionary<string, AppSettings>>(
                    File.ReadAllText(_filePath)) ?? new();
        }
        catch { _profiles = new(); }
    }

    private void Persist()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_profiles, JsonOptions));
        }
        catch { }
    }
}

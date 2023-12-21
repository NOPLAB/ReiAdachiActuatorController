using System.Text.Json;

namespace ActuatorController.Settings;

public class SettingsWrapper
{
    public Json Json { get; init; }

    public SettingsWrapper(string path)
    {
        Json = JsonSerializer.Deserialize<Json>(File.ReadAllText(path))
               ?? throw new ArgumentException("Json can't serialize!");
    }
}
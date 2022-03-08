using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace FrankenDrift.Runner
{
    internal class SettingsManager
    {
        private const string _fileName = "appsettings.json";
        private readonly string _settingsPath;
        private readonly string _settingsFile;
        private SettingsManager()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                _settingsPath = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "Adrian Welcker", "FrankenDrift");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                _settingsPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Library",
                    "Application Support", "de.diepixelecke.frankendrift");
            else
            {
                var dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                _settingsPath = !string.IsNullOrEmpty(dataHome) ? Path.Combine(dataHome, "FrankenDrift") : Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local", "share", "FrankenDrift");
            }
            _settingsFile = Path.Combine(_settingsPath, _fileName);
            if (File.Exists(_settingsFile))
            {
                var settingsText = File.ReadAllText(_settingsFile);
                Settings = JsonSerializer.Deserialize<Settings>(settingsText);
            }
            else
            {
                Settings = new Settings {
                    EnableGraphics = !RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                    EnableDevColors = true
                };
            }
        }
        private static readonly Lazy<SettingsManager> lazySmgr = new(() => new SettingsManager());
        public static SettingsManager Instance => lazySmgr.Value;

        public Settings Settings { get; }

        public void Save()
        {
            var options = new JsonSerializerOptions {WriteIndented = true};
            string jsonSettings = JsonSerializer.Serialize(Settings, options);
            Directory.CreateDirectory(_settingsPath);
            File.WriteAllText(_settingsFile, jsonSettings);
        }
    }

    public class Settings
    {
        public bool EnableGraphics { get; set; }
        public bool EnableDevColors { get; set; }
    }
}
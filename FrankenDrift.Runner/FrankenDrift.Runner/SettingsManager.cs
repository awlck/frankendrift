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
                _settings = JsonSerializer.Deserialize<Settings>(settingsText);
                if (_settings.UserFontSize < 6)
                    _settings.UserFontSize = 6;
            }
            else
            {
                _settings = new Settings {
                    EnableGraphics = true,
                    EnableDevColors = true,
                    EnableDevFont = true,
                    DefaultFontName = null,
                    UserFontSize = 10,
                    AlterFontSize = 1,
                    BanComicSans = false,
                    EnablePressAnyKey = false,
                    SuppressLocationName = false
                };
            }
        }
        private static readonly Lazy<SettingsManager> lazySmgr = new(() => new SettingsManager());
        public static SettingsManager Instance => lazySmgr.Value;
        public static Settings Settings => lazySmgr.Value._settings;

        private Settings _settings { get; }

        public void Save()
        {
            var options = new JsonSerializerOptions {WriteIndented = true};
            string jsonSettings = JsonSerializer.Serialize(_settings, options);
            Directory.CreateDirectory(_settingsPath);
            File.WriteAllText(_settingsFile, jsonSettings);
        }
    }

    public class Settings
    {
        public bool EnableGraphics { get; set; }
        public bool EnableDevColors { get; set; }
        public bool EnableDevFont { get; set; }
        public string DefaultFontName { get; set; }
        public int UserFontSize { get; set; }
        public int AlterFontSize {  get; set; }
        public bool BanComicSans { get; set; }
        public bool EnablePressAnyKey { get; set; }
        public bool SuppressLocationName { get; set; }
    }
}
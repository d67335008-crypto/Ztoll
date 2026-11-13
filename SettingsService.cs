// Ztoll - Dynamic Island style panel for Windows.
// Licensed under the GNU General Public License v3.0. See LICENSE and NOTICE.
// Derived concept from Atoll (c) Atoll Contributors, which is derived from boring.notch.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ztoll.Models;

namespace Ztoll.Services
{
    public sealed class SettingsService
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly string _filePath;

        public SettingsService() : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ztoll", "settings.json"))
        {
        }

        public SettingsService(string filePath)
        {
            _filePath = filePath;
            Current = new AppSettings();
        }

        public AppSettings Current { get; private set; }

        /// <summary>Raised after settings were changed and saved.</summary>
        public event Action? Changed;

        public static string Serialize(AppSettings settings) => JsonSerializer.Serialize(settings, JsonOptions);

        public static AppSettings Deserialize(string json)
        {
            try
            {
                AppSettings? s = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                s ??= new AppSettings();
                s.Sanitize();
                return s;
            }
            catch (JsonException)
            {
                return new AppSettings();
            }
        }

        public void Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    Current = Deserialize(File.ReadAllText(_filePath));
                    return;
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            Current = new AppSettings();
        }

        public void Save()
        {
            try
            {
                string? dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(_filePath, Serialize(Current));
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        /// <summary>Sanitizes, saves and notifies listeners.</summary>
        public void Commit()
        {
            Current.Sanitize();
            Save();
            Changed?.Invoke();
        }

        public void ResetToDefaults()
        {
            Current = new AppSettings();
            Commit();
        }
    }
}

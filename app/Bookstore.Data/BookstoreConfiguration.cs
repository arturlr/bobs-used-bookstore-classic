using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace BobsBookstoreClassic.Data
{
    public sealed class BookstoreConfiguration
    {
        private static readonly Lazy<BookstoreConfiguration> Lazy = new Lazy<BookstoreConfiguration>(() => new BookstoreConfiguration());

        private static BookstoreConfiguration Instance => Lazy.Value;

        private readonly Dictionary<string, string> _appSettings = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _connectionStrings = new Dictionary<string, string>();
        private static IConfiguration _configuration;

        private BookstoreConfiguration()
        {
        }

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
            
            // Load app settings from configuration
            foreach (var setting in configuration.AsEnumerable())
            {
                if (!string.IsNullOrEmpty(setting.Value))
                {
                    Instance._appSettings[setting.Key] = setting.Value;
                }
            }

            // Override with environment variables if present
            foreach (var key in Instance._appSettings.Keys)
            {
                var envValue = Environment.GetEnvironmentVariable(key.Replace(":", "__"));
                if (envValue != null)
                {
                    Instance._appSettings[key] = envValue;
                }
            }
        }

        public static void AddSetting(string key, string value)
        {
            Instance._appSettings[key] = value;
        }

        public static string GetSetting(string key)
        {
            if (Instance._appSettings.TryGetValue(key, out var value))
            {
                return value;
            }
            return _configuration?[key];
        }

        public static T GetSetting<T>(string key)
        {
            var value = GetSetting(key);
            if (value == null) return default(T);

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static void AddConnectionString(string key, string value)
        {
            Instance._connectionStrings[key] = value;
        }

        public static string GetConnectionString(string key)
        {
            if (Instance._connectionStrings.TryGetValue(key, out var value))
            {
                return value;
            }
            return _configuration?.GetConnectionString(key);
        }

    }
}

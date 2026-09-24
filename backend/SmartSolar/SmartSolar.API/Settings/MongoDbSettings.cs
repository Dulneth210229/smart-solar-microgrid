/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: MongoDbSettings.cs
 * Description: Stores MongoDB configuration values loaded from appsettings.json.
 */

namespace SmartSolar.API.Settings
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;
    }
}
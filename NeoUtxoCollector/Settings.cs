using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoUtxoCollector
{
    internal class Settings
    {
        public string MysqlConfig { get; }
        public string DataBaseName { get; }
        public string RpcUrl { get; }

        public static Settings Default { get; }

        public string ZoroHash { get; }

        static Settings()
        {
            IConfigurationSection section = new ConfigurationBuilder().AddJsonFile(Program.Config).Build().GetSection("Configuration");
            Default = new Settings(section);
        }

        public Settings(IConfigurationSection section)
        {
            IEnumerable<IConfigurationSection> mysql = section.GetSection("MySql").GetChildren();

            this.MysqlConfig = "";

            foreach (var item in mysql)
            {
                this.MysqlConfig += item.Key + " = " + item.Value;
                this.MysqlConfig += ";";
            }

            DataBaseName = section.GetSection("MySql").GetSection("database").Value;
            RpcUrl = section.GetSection("RPC").GetSection("url").Value;
            ZoroHash = section.GetSection("RPC").GetSection("zorohash").Value;            
        }
       
    }
}
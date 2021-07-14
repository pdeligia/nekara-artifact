using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Coyote.Tasks;

namespace Thunderstruck.Runtime
{
    public class ConnectionStringBuffer
    {
        #region Singleton

        private static ConnectionStringBuffer _instance;

        static ConnectionStringBuffer()
        {
            _instance = new ConnectionStringBuffer();
        }

        public static ConnectionStringBuffer Instance
        {
            get { return _instance; }
        }

        #endregion

        private Nekara.MockDictionary<string, ConnectionStringSettings> _buffer;

        public ConnectionStringBuffer()
        {
            _buffer = new Nekara.MockDictionary<string, ConnectionStringSettings>();
        }

        public ConnectionStringSettings Get(string connectionStringName)
        {

            if (!_buffer.ContainsKey(connectionStringName))
            {
                var css = new ConnectionStringSettings(connectionStringName, "Server=Localhost;Database=ThunderTest;Integrated Security=true;Pooling=false", "System.Data.SqlClient");
                _buffer.Add(connectionStringName, css);
            }
            return _buffer[connectionStringName];
        }

        private ConnectionStringSettings GetFromConfig(string connectionName)
        {
            var setting = ConfigurationManager.ConnectionStrings[connectionName];

            if (setting == null)
            {
                var exceptionMessage = String.Concat("ConnectionString '", connectionName ,"' not found in config file.");
                throw new ThunderException(exceptionMessage);
            }

            return setting;
        }
    }
}
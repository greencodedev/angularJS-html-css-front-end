using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Utils.Sql
{
    public class SqlReader : IDisposable
    {
        private MySqlDataReader _myReader;

        public SqlReader(MySqlDataReader reader)
        {
            _myReader = reader;
        }

        public bool HasNext()
        {
            return _myReader.Read();
        }

        public void Close()
        {
            if (!_myReader.IsClosed)
            {
                _myReader.Close();
            }
        }

        public int? GetInteger(string name)
        {
            int colIndex = _myReader.GetOrdinal(name);
            if (_myReader.IsDBNull(colIndex))
            {
                return null;
            }

            return _myReader.GetInt32(colIndex);
        }

        public int GetIntegerToZero(string name)
        {
            int? val = GetInteger(name);

            return val.HasValue ? val.Value : 0;
        }

        public int GetInt(string name)
        {
            return _myReader.GetInt32(_myReader.GetOrdinal(name));
        }

        public int GetIntOrZero(string name)
        {
            int? val = GetInteger(name);

            if (val != null)
                return val.Value;

            return 0;
        }

        public double? GetDouble(string name)
        {
            int colIndex = _myReader.GetOrdinal(name);
            if (_myReader.IsDBNull(colIndex))
            {
                return null;
            }

            return _myReader.GetDouble(colIndex);
        }

        public double GetDoubleOrZero(string name)
        {
            int colIndex = _myReader.GetOrdinal(name);
            if (_myReader.IsDBNull(colIndex))
            {
                return 0;
            }

            return _myReader.GetDouble(colIndex);
        }

        public string GetString(string name)
        {
            int colIndex = _myReader.GetOrdinal(name);
            if (_myReader.IsDBNull(colIndex))
            {
                return null;
            }

            return _myReader.GetString(colIndex).Trim();
        }

        public string GetIntegerToString(string name)
        {
            int colIndex = _myReader.GetOrdinal(name);
            if (_myReader.IsDBNull(colIndex))
            {
                return null;
            }

            return _myReader.GetInt32(colIndex).ToString();
        }

        public DateTime GetTime(string name)
        {
            return _myReader.GetDateTime(_myReader.GetOrdinal(name));
        }

        public DateTime GetUtcTimeToLocal(string name)
        {
            DateTime time = _myReader.GetDateTime(_myReader.GetOrdinal(name));
            return UtilsLib.UtcToLocalTime(time);
        }

        public DateTime GetUtcTime(string name)
        {
            DateTime time = _myReader.GetDateTime(_myReader.GetOrdinal(name));
            time = DateTime.SpecifyKind(time, DateTimeKind.Utc);
            return time;
        }

        public DateTime? GetOptionalUtcTimeToLocal(string name)
        {
            DateTime? time = GetOptionalTime(name);

            if (time.HasValue)
            {
                return UtilsLib.UtcToLocalTime(time.Value);
            }

            return null;
        }

        public DateTime? GetOptionalTime(string name)
        {
            int colIndex = _myReader.GetOrdinal(name);
            if (_myReader.IsDBNull(colIndex))
            {
                return null;
            }

            return _myReader.GetDateTime(colIndex);
        }

        public DateTime? GetOptionalUtcTime(string name)
        {
            DateTime? time = GetOptionalTime(name);

            if (time != null)
            {
                time = DateTime.SpecifyKind(time.Value, DateTimeKind.Utc);
            }

            return time;
        }

        public void PopulateTime(DateTime time, string name)
        {
            DateTime? dbTime = GetOptionalTime(name);

            if (dbTime.HasValue)
            {
                time = dbTime.Value;
            }
        }

        public bool GetBoolean(string name)
        {
            int i = _myReader.GetOrdinal(name);

            if (_myReader.IsDBNull(i))
            {
                return false;
            }

            return _myReader.GetBoolean(i);
        }

        public bool? GetOptionalBoolean(string name)
        {
            int i = _myReader.GetOrdinal(name);

            if (_myReader.IsDBNull(i))
            {
                return null;
            }

            return _myReader.GetBoolean(i);
        }

        public bool GetIntAsBoolean(string name)
        {
            return _myReader.GetInt32(_myReader.GetOrdinal(name)) == 1;
        }

        public double GetStringToDouble(string name)
        {
            int colIndex = _myReader.GetOrdinal(name);
            if (_myReader.IsDBNull(colIndex))
            {
                return 0;
            }

            return UtilsLib.ParseToDouble(_myReader.GetString(colIndex));
        }

        public T ParseEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), GetString(name));
        }

        public void Dispose()
        {
            Close();
        }
    }
}
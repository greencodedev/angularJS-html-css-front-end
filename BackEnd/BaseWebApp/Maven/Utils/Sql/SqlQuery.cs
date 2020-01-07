using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace BaseWebApp.Maven.Utils.Sql
{
    public class SqlQuery
    {
        private MySqlCommand _command;

        public SqlQuery()
        {
            _command = new MySqlCommand();
        }

        public string listToInString(List<object> list)
        {
            if (list.Count == 0)
            {
                return "NULL";
            }
            else
            {
                return "'" + String.Join("','", list.ToArray()) + "'";
            }
        }

        public string strListToInString(List<string> list, string prefix)
        {
            if (list.Count == 0)
            {
                return "NULL";
            }
            else
            {
                string val = "";
                for (int i = 0; i < list.Count; i++)
                {
                    val += "@" + prefix + i;
                    AddParam("@" + prefix + i, list[i]);

                    if (i != list.Count - 1)
                    {
                        val += ", ";
                    }
                }

                return val;
            }
        }

        public SqlQuery AddParam(string name, object value)
        {
            object val = value != null ? value : DBNull.Value;

            _command.Parameters.Add(new MySqlParameter(name, val));

            return this;
        }

        public string AddSearchTerm(string textSearch, List<string> columns)
        {
            string query = "";

            if (!string.IsNullOrEmpty(textSearch))
            {
                string[] split = textSearch.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < split.Length; i++)
                {
                    string paramName = "@TextSearch" + i;

                    query += "and ( ";
                    for (int j = 0; j < columns.Count; j++)
                    {
                        if(j != 0)
                        {
                            query += "or ";
                        }

                        query += columns[j] + " like " + paramName + " ";
                    }
                    query += ") ";
                    
                    AddParam(paramName, "%" + split[i] + "%");
                }
            }

            return query;
        }

        private void openConnection(Sql sql, string query)
        {
            if (sql.ReadUncommitted)
            {
                query = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; "
                    + query
                    + "; COMMIT; ";
            }

            _command.CommandText = query;
            _command.Connection = sql.Connection;

            if (sql.InTransaction)
            {
                _command.Transaction = sql.Transaction;
            }
        }

        public SqlReader ExecuteReader(Sql sql, string query)
        {
            openConnection(sql, query);

            Stopwatch timer = Stopwatch.StartNew();
            SqlReader sqlReader = new SqlReader(_command.ExecuteReader());
            logSql(timer.ElapsedMilliseconds);

            return sqlReader;
        }

        public int ExecuteNonQuery(Sql sql, string query)
        {
            openConnection(sql, query);

            Stopwatch timer = Stopwatch.StartNew();
            int res = _command.ExecuteNonQuery();
            logSql(timer.ElapsedMilliseconds);

            return res;
        }

        public int ExecuteScalar(Sql sql, string query)
        {
            openConnection(sql, query);

            Stopwatch timer = Stopwatch.StartNew();
            int res = (int)_command.ExecuteScalar();
            logSql(timer.ElapsedMilliseconds);

            return res;
        }

        public string ExecuteOptinalScalar(Sql sql, string query)
        {
            openConnection(sql, query);

            Stopwatch timer = Stopwatch.StartNew();
            int? res = (int?)_command.ExecuteScalar();
            logSql(timer.ElapsedMilliseconds);

            return res.HasValue ? res.Value.ToString() : null;
        }

        public long ExecuteInsert(Sql sql, string query)
        {
            openConnection(sql, query);

            Stopwatch timer = Stopwatch.StartNew();
            logSql(timer.ElapsedMilliseconds);
            _command.ExecuteNonQuery();
            logSql(timer.ElapsedMilliseconds);
            return _command.LastInsertedId;
        }

        DbType[] quotedParameterTypes = new DbType[] {
            DbType.AnsiString, DbType.Date,
            DbType.DateTime, DbType.Guid, DbType.String,
            DbType.AnsiStringFixedLength, DbType.StringFixedLength
        };

        private void logSql(long timer)
        {
            string query = _command.CommandText;
            var arrParams = new MySqlParameter[_command.Parameters.Count];
            _command.Parameters.CopyTo(arrParams, 0);

            foreach (MySqlParameter p in arrParams.OrderByDescending(p => p.ParameterName.Length))
            {
                string value = p.Value.ToString();

                if (quotedParameterTypes.Contains(p.DbType))
                    value = "'" + value + "'";

                query = query.Replace(p.ParameterName, value);
            }

            string repl = Regex.Replace(Regex.Replace(query, @"\t|\n|\r", ""), @"[ ]{2,}", " ");

            Logger.Log("SQL :: " + timer.ToString() + " :: " + repl);
        }
    }
}
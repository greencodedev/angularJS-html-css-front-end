using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Utils.Sql
{
    public class Sql : IDisposable
    {
        private MySqlConnection _conn;
        private MySqlTransaction _trans;
        private bool _inTransaction;
        public bool ReadUncommitted { get; set; }

        public MySqlConnection Connection
        {
            get
            {
                return _conn;
            }
        }

        public MySqlTransaction Transaction
        {
            get
            {
                return _trans;
            }
        }

        public bool InTransaction
        {
            get
            {
                return _inTransaction;
            }
        }

        public Sql()
        {
            _conn = new MySqlConnection(UtilsLib.getConnectionString());
            _conn.Open();
        }

        public void BeginTransaction(string transactionName)
        {
            _trans = _conn.BeginTransaction();
            _inTransaction = true;
        }

        public void Commit()
        {
            _trans.Commit();
        }

        public void Rollback()
        {
            _trans.Rollback();
        }

        public void Dispose()
        {
            _conn.Dispose();
        }
    }
}
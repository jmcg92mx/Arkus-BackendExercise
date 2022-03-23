using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Collections.Generic;

namespace BackendExercise.Data
{
    /// <summary>
    /// Class that encapsules all the database accesses along the application.
    /// </summary>
    public class DataAccess : IDisposable
    {
        private static string DB_FILE = AppContext.BaseDirectory + "Data\\base.db";
        private static int DEFAULT_CONNECTION_TIMEOUT = 40;

        private bool disposedValue;
        private SqliteConnection con;
        private int timeout;

        /// <summary>
        /// Prepares the database file if it does not exists.
        /// </summary>
        internal static void PreateDatabase()
        {
            if (!File.Exists(DB_FILE))
            {
                File.Create(DB_FILE);
                string sql = File.ReadAllText(AppContext.BaseDirectory + "Data\\base.sql");
                using (var db = new SqliteConnection($"Data Source={DB_FILE}"))
                {
                    db.Open();
                    SqliteCommand cmd = new SqliteCommand(sql, db);
                    var exe = cmd.ExecuteNonQuery();
                    Console.WriteLine(exe);
                }
            }
        }

        /// <summary>
        /// Constructor for DataAccess class.
        /// </summary>
        internal DataAccess()
        {
            timeout = DEFAULT_CONNECTION_TIMEOUT;
            con = new SqliteConnection($"Data Source={DB_FILE}");
        }

        /// <summary>
        /// Runs a query to the database and returns a collection of results.
        /// </summary>
        /// <param name="command">SQL query to run.</param>
        /// <param name="parameters">Parameters to the query.</param>
        /// <returns>Collection of data returned by the database.</returns>
        internal IEnumerable<SqliteDataReader> RunQuery(string command, params Param[] parameters)
        {
            using (var cmnd = new SqliteCommand(command, con))
            {
                if (con.State == ConnectionState.Broken)
                    con.Close();
                if (con.State == ConnectionState.Closed)
                    con.Open();
                cmnd.CommandTimeout = timeout;
                cmnd.CommandType = CommandType.Text;
                foreach (var c in parameters)
                    cmnd.Parameters.Add(c.ToSqliteParameter());
                using (var reader = cmnd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            yield return reader;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Runs a query to the database and returns a collection of results.
        /// </summary>
        /// <param name="transaction">Current transaction.</param>
        /// <param name="command">SQL query to run.</param>
        /// <param name="parameters">Parameters to the query.</param>
        /// <returns>Collection of data returned by the database.</returns>
        internal IEnumerable<SqliteDataReader> RunQuery(SqliteTransaction transaction, string command, params Param[] parameters)
        {
            using (var cmnd = new SqliteCommand(command, con, transaction))
            {
                if (con.State == ConnectionState.Broken)
                    con.Close();
                if (con.State == ConnectionState.Closed)
                    con.Open();
                cmnd.CommandTimeout = timeout;
                cmnd.CommandType = CommandType.Text;
                foreach (var c in parameters)
                    cmnd.Parameters.Add(c.ToSqliteParameter());
                using (var reader = cmnd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            yield return reader;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Runs a query to the database and returns the first value of the first row (if any).
        /// </summary>
        /// <param name="command">SQL query to run.</param>
        /// <param name="parameters">Parameters to the query.</param>
        /// <returns>The first column's value of the first row, if any, or null.</returns>
        internal dynamic RunScalar<T>(string command, params Param[] parameters)
        {
            using (var cmnd = new SqliteCommand(command, con))
            {
                if (con.State == ConnectionState.Broken)
                    con.Close();
                if (con.State == ConnectionState.Closed)
                    con.Open();
                cmnd.CommandTimeout = timeout;
                cmnd.CommandType = CommandType.Text;
                foreach (var c in parameters)
                    cmnd.Parameters.Add(c.ToSqliteParameter());
                object o = cmnd.ExecuteScalar();
                if (DBNull.Value == o)
                    o = null;
                string type = o.GetType().Name;
                if (!type.Equals(typeof(T).Name))
                {
                    if (type.Equals("String"))
                    {
                        if (typeof(T).Name.Equals("Int32"))
                            o = (int)o;
                        if (typeof(T).Name.Equals("Int64"))
                            o = (long)o;
                        if (typeof(T).Name.Equals("DateTime"))
                            o = DateTime.Parse(o.ToString());
                    }
                }
                return o;
            }
        }

        /// <summary>
        /// Runs a query to the database and returns the first value of the first row (if any).
        /// </summary>
        /// <param name="transaction">Current transaction.</param>
        /// <param name="command">SQL query to run.</param>
        /// <param name="parameters">Parameters to the query.</param>
        /// <returns>The first column's value of the first row, if any, or null.</returns>
        internal dynamic RunScalar<T>(SqliteTransaction transaction, string command, params Param[] parameters)
        {
            using (var cmnd = new SqliteCommand(command, con, transaction))
            {
                if (con.State == ConnectionState.Broken)
                    con.Close();
                if (con.State == ConnectionState.Closed)
                    con.Open();
                cmnd.CommandTimeout = timeout;
                cmnd.CommandType = CommandType.Text;
                foreach (var c in parameters)
                    cmnd.Parameters.Add(c.ToSqliteParameter());
                object o = cmnd.ExecuteScalar();
                if (DBNull.Value == o)
                    o = null;
                string type = o.GetType().Name;
                if (!type.Equals(typeof(T).Name))
                {
                    if (type.Equals("String"))
                    {
                        if (typeof(T).Name.Equals("Int32"))
                            o = (int)o;
                        if (typeof(T).Name.Equals("Int64"))
                            o = (long)o;
                        if (typeof(T).Name.Equals("DateTime"))
                            o = DateTime.Parse(o.ToString());
                    }
                }
                return o;
            }
        }

        /// <summary>
        /// Runs a command to the database and returns the number of affected rows.
        /// </summary>
        /// <param name="command">SQL command to run.</param>
        /// <param name="parameters">Parameters to the command.</param>
        /// <returns>Integer counting the affected rows.</returns>
        internal int RunCommand(string command, params Param[] parameters)
        {
            using (var cmnd = new SqliteCommand(command, con))
            {
                if (con.State == ConnectionState.Broken)
                    con.Close();
                if (con.State == ConnectionState.Closed)
                    con.Open();
                cmnd.CommandTimeout = timeout;
                foreach (var c in parameters)
                    cmnd.Parameters.Add(c.ToSqliteParameter());
                return cmnd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Runs a command to the database inside a transaction and returns the number of affected rows.
        /// </summary>
        /// <param name="transaction">Current transaction.</param>
        /// <param name="command">SQL command to run.</param>
        /// <param name="parameters">Parameters to the command.</param>
        /// <returns>Integer counting the affected rows.</returns>
        internal int RunCommand(SqliteTransaction transaction, string command, params Param[] parameters)
        {
            using (var cmnd = new SqliteCommand(command, con, transaction))
            {
                if (con.State == ConnectionState.Broken)
                    con.Close();
                if (con.State == ConnectionState.Closed)
                    con.Open();
                cmnd.CommandTimeout = timeout;
                foreach (var c in parameters)
                    cmnd.Parameters.Add(c.ToSqliteParameter());
                return cmnd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets the SqliteConnection for its use in transactions.
        /// </summary>
        /// <returns>SqliteConnection used by this instance.</returns>
        internal SqliteConnection GetConnection()
        {
            if (con.State == ConnectionState.Broken)
                con.Close();
            if (con.State == ConnectionState.Closed)
                con.Open();
            return con;
        }

        #region IDisposable support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: eliminar el estado administrado (objetos administrados)
                    con.Close();
                    con.Dispose();
                    con = null;
                }

                // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
                // TODO: establecer los campos grandes como NULL
                disposedValue = true;
            }
        }

        // // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
        // ~DataAccess()
        // {
        //     // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

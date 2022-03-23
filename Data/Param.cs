using Microsoft.Data.Sqlite;

namespace BackendExercise.Data
{
    /// <summary>
    /// Structure that represents a shortcut to use SQLite parameters.
    /// </summary>
    internal struct Param
    {
        private string paramName;
        private object value;

        /// <summary>
        /// Takes a parameter name and value to use as shortcut to SQLite parameters.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="value">Parameter value.</param>
        internal Param(string name, object value)
        {
            paramName = name;
            this.value = value;
        }

        internal SqliteParameter ToSqliteParameter() => new SqliteParameter(paramName, value);
    }
}

using BackendExercise.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BackendExercise.Models
{
    /// <summary>
    /// Class that represents a transaction.
    /// </summary>
    public class TransactionModel
    {
        public TransactionModel() { }

        public TransactionModel(int transaction)
        {
            using(var db = new DataAccess())
            {
                foreach (var row in db.RunQuery(@"SELECT ""To"",""Amount"",""Date"",""Value"",""Description"" FROM ""Transactions"" T INNER JOIN ""Transaction_Status"" S ON T.""Status"" = S.""Key"" WHERE T.""TransactionId"" = $id;",
                    new Param("id", transaction)))
                {
                    Id = transaction;
                    To = row.GetString(0);
                    Amount = row.GetDouble(1);
                    TransactionDate = row.GetDateTime(2);
                    Status = row.GetString(3);
                    Description = row.GetString(4);
                }
            }
        }

        #region Properties
        /// <summary>
        /// Identifier number for the transaction.
        /// </summary>
        [Display(Name = "Transaction ID")]
        public long Id { get; set; }
        /// <summary>
        /// Addressee of the transaction.
        /// </summary>
        public string To { get; set; }
        /// <summary>
        /// Amount of the transaction.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:C}")]
        public double Amount { get; set; }
        /// <summary>
        /// Description for the transaction.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Date and time of the transaction.
        /// </summary>
        [Display(Name = "Transaction date")]
        public DateTime TransactionDate { get; set; }
        /// <summary>
        /// Status of the transaction.
        /// </summary>
        public string Status { get; set; }
        #endregion

        #region Model methods
        /// <summary>
        /// Writes the transaction into the database.
        /// </summary>
        public void RegisterTransaction()
        {
            if (Amount <= 0)
                throw new ArgumentException("Amount has to be greater than zero.");
            using (var db = new DataAccess())
            {
                // Inserts the transaction
                db.RunCommand(@"INSERT INTO ""Transactions""(""To"", ""Amount"", ""Date"", ""Status"", ""Description"") VALUES ($to, $amount, datetime('now', 'localtime'), 1, $description);",
                    new Param("to", To),
                    new Param("amount", Amount),
                    new Param("description", Description));
                Id = db.RunScalar<long>("SELECT MAX(TransactionId) FROM \"Transactions\";");
            }
        }

        /// <summary>
        /// Edits the status of the current transaction.
        /// </summary>
        /// <param name="status">Selected status.</param>
        /// <returns>True if the transaction got updated; False otherwise.</returns>
        public bool EditStatus(int status)
        {
            using (var db = new DataAccess())
            {
                return 1 == db.RunCommand("UPDATE \"Transactions\" SET \"Status\" = $status WHERE TransactionId = $id;", new Param("status", status), new Param("id", this.Id));
            }
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Gets a list of all transactions on the database.
        /// </summary>
        /// <returns>List of transactions.</returns>
        public static List<TransactionModel> GetTransactions()
        {
            var l = new List<TransactionModel>();
            using (var db = new DataAccess())
            {
                foreach (var row in db.RunQuery(@"SELECT ""TransactionId"",""To"",""Amount"",""Date"",""Description"",""Value"" AS ""Status"" FROM ""Transactions"" T INNER JOIN ""Transaction_Status"" S ON T.Status = S.Key;"))
                {
                    l.Add(new TransactionModel()
                    {
                        Id = row.GetInt64(0),
                        To = row.GetString(1),
                        Amount = row.GetDouble(2),
                        TransactionDate = row.GetDateTime(3),
                        Description = row.GetString(4),
                        Status = row.GetString(5)
                    });
                }
            }
            return l;
        }

        /// <summary>
        /// Gets a list of the transactions on the database between the specified dates.
        /// </summary>
        /// <param name="start">Lower limit of dates.</param>
        /// <param name="end">Higher limit of dates.</param>
        /// <returns>List of transactions.</returns>
        public static List<TransactionModel> GetTransactions(DateTime start, DateTime end)
        {
            var l = new List<TransactionModel>();
            using (var db = new DataAccess())
            {
                foreach (var row in db.RunQuery(@"SELECT ""TransactionId"",""To"",""Amount"",""Date"",""Description"",""Value"" AS ""Status"""
                    + @"FROM ""Transactions"" T INNER JOIN ""Transaction_Status"" S ON T.Status = S.Key WHERE ""Date"" BETWEEN $start AND $end AND T.""Status"" = 1;",
                    new Param("start", start.ToString("yyyy-MM-dd")), new Param("end", end.AddDays(1).ToString("yyyy-MM-dd"))))
                {
                    l.Add(new TransactionModel()
                    {
                        Id = row.GetInt64(0),
                        To = row.GetString(1),
                        Amount = row.GetDouble(2),
                        TransactionDate = row.GetDateTime(3),
                        Description = row.GetString(4),
                        Status = row.GetString(5)
                    });
                }
            }
            return l;
        }

        /// <summary>
        /// Gets the collection of available status in the database.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetStatus()
        {
            using (var db = new DataAccess())
            {
                var d = new Dictionary<int, string>();
                foreach (var row in db.RunQuery("SELECT \"Key\", \"Value\" FROM \"Transaction_Status\";"))
                {
                    d.Add(row.GetInt32(0), row.GetString(1));
                }
                return d;
            }
        }
        #endregion
    }
}

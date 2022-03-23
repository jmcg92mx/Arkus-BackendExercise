using BackendExercise.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackendExercise.Models
{
    /// <summary>
    /// Class that represents an invoice.
    /// </summary>
    public class InvoiceModel
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        internal InvoiceModel() { }
        /// <summary>
        /// Constructor for the invoice.
        /// </summary>
        public InvoiceModel(int id)
        {
            using (var db = new DataAccess()) {
                foreach (var row in db.RunQuery(@"SELECT T.""TransactionId"", I.""Date"" AS ""Invoice_Date"", T.""To"", T.""Amount"", T.""Date"" AS ""Transact_Date"", T.""Description"", S.""Value"" AS ""Status""
  FROM ""Invoices"" I INNER JOIN ""Transactions"" T ON I.""Transaction"" = T.""TransactionId"" INNER JOIN ""Transaction_Status"" S ON T.""Status"" = S.""Key"" WHERE I.""InvoiceId"" = $id;",
                            new Param("id", id)))
                {
                    Id = id;
                    InvoiceDate = row.GetDateTime(1);
                    Amount = row.GetDouble(3);
                    Transaction = new TransactionModel()
                    {
                        Id = row.GetInt64(0),
                        To = row.GetString(2),
                        Amount = row.GetDouble(3),
                        TransactionDate = row.GetDateTime(4),
                        Description = row.GetString(5),
                        Status = row.GetString(6)
                    };
                }
            }
        }

        /// <summary>
        /// Id for the invoice.
        /// </summary>
        [Display(Name = "Invoice ID")]
        public long Id { get; set; }
        /// <summary>
        /// Related transaction.
        /// </summary>
        public TransactionModel Transaction { get; set; }
        /// <summary>
        /// Date for the invoice.
        /// </summary>
        [Display(Name = "Invoice date")]
        public DateTime InvoiceDate { get; set; }
        /// <summary>
        /// Amount for the invoice.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:C}")]
        public double Amount { get; set; }

        /// <summary>
        /// Marks an invoice as "Paid".
        /// </summary>
        /// <returns>True if the transaction gets paid, False otherwise.</returns>
        public bool Pay()
        {
            using (var db = new DataAccess())
            {
                long transactionId = db.RunScalar<long>(@"SELECT ""Transaction"" FROM ""Invoices"" WHERE ""InvoiceId"" = $inv;", new Param("inv", Id));
                using (var transact = db.GetConnection().BeginTransaction())
                {
                    try
                    {
                        db.RunCommand(transact, @"INSERT INTO ""Payments""(""Invoice"", ""PaymentDate"") VALUES ($inv, datetime('now','localtime'));", new Param("inv", Id));
                        db.RunCommand(transact, @"UPDATE ""Invoices"" SET ""IsPaid"" = 1 WHERE ""InvoiceId"" = $inv;", new Param("inv", Id));
                        db.RunCommand(transact, @"UPDATE ""Transactions"" SET ""Status"" = 3 WHERE ""TransactionId"" = $tran;", new Param("tran", transactionId));
                        transact.Commit();
                        return true;
                    }
                    catch(Exception e)
                    {
                        transact.Rollback();
                        return false;
                    }
                }
            }
        }

        #region Static methods
        /// <summary>
        /// Generates a new invoice from a transaction, and updates the status of the transaction to "Billed".
        /// </summary>
        /// <param name="transaction">Id for the transaction to bill.</param>
        /// <returns>The invoice generated.</returns>
        public static InvoiceModel GenerateInvoiceFromTransaction(int transaction)
        {
            InvoiceModel invoice = new InvoiceModel();
            invoice.Transaction = new TransactionModel(transaction);
            using (var db = new DataAccess())
            {
                if ((int)db.RunScalar<int>("SELECT COUNT(*) FROM \"Invoices\" WHERE \"Transaction\" = $tran;", new Param("tran", transaction)) == 0)
                    using (var transact = db.GetConnection().BeginTransaction())
                    {
                        try
                        {
                            db.RunCommand(transact, @"INSERT INTO ""Invoices"" (""Transaction"", ""Date"") VALUES ($transact, datetime('now', 'localtime'));",
                                new Param("transact", transaction));
                            db.RunCommand(transact, @"UPDATE ""Transactions"" SET ""Status"" = 2 WHERE ""TransactionId"" = $id;", new Param("id", transaction));
                            transact.Commit();

                            invoice.Id = db.RunScalar<long>("SELECT MAX(\"InvoiceId\") FROM \"Invoices\";");
                            invoice.Amount = invoice.Transaction.Amount;
                            object obj = db.RunScalar<DateTime>("SELECT \"Date\" FROM \"Invoices\" WHERE \"InvoiceId\" = $id;", new Param("id", invoice.Id));
                        }
                        catch (Exception e)
                        {
                            transact.Rollback();
                        }
                    }
                else
                    throw new Exception("Transaction already has an invoice.");
            }
            return invoice;
        }
        /// <summary>
        /// Gets the list of all available invoices.
        /// </summary>
        /// <returns>List of invoices.</returns>
        public static List<InvoiceModel> GetInvoices()
        {
            var l = new List<InvoiceModel>();
            using (var db = new DataAccess())
            {
                foreach (var row in db.RunQuery(@"SELECT I.""InvoiceId"", I.""Date"" AS ""Invoice_Date"", T.""To"", T.""Amount"", T.""Date"" AS ""Transact_Date"", T.""Description"", S.""Value"" AS ""Status""
  FROM ""Invoices"" I INNER JOIN ""Transactions"" T ON I.""Transaction"" = T.""TransactionId"" INNER JOIN ""Transaction_Status"" S ON T.""Status"" = S.""Key"";"))
                {
                    l.Add(new InvoiceModel()
                    {
                        Id = row.GetInt32(0),
                        InvoiceDate = row.GetDateTime(1),
                        Amount = row.GetDouble(3),
                        Transaction = new TransactionModel()
                        {
                            To = row.GetString(2),
                            TransactionDate = row.GetDateTime(4),
                            Description = row.GetString(5),
                            Status = row.GetString(6)
                        }
                    });
                }

                return l;
            }
        } 
        #endregion
    }
}

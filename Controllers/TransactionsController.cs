using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace BackendExercise.Controllers
{
    public class TransactionsController : Controller
    {
        // GET: Transactions
        public ActionResult Index()
        {
            return View(Models.TransactionModel.GetTransactions());
        }

        // GET: Transactions/Details/5
        public ActionResult Details(int id)
        {
            return View(new Models.TransactionModel(id));
        }

        // GET: Transactions/Create
        public ActionResult Create()
        {
            return View(new Models.TransactionModel());
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                double amount = double.Parse(collection["Amount"]);
                var transact = new Models.TransactionModel()
                {
                    To = collection["To"],
                    Amount = amount,
                    Description = collection["Description"]
                };
                transact.RegisterTransaction();

                return RedirectToAction(nameof(Details), new { id = transact.Id });
            }
            catch (Exception e)
            {
                return View();
            }
        }

        // GET: Transactions/Delete/5
        public ActionResult Delete(int id)
        {
            return View(new Models.TransactionModel(id));
        }

        // POST: Transactions/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}

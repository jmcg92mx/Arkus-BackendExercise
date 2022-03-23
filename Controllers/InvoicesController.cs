using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendExercise.Controllers
{
    public class InvoicesController : Controller
    {
        // GET: Invoices
        public ActionResult Index()
        {
            return View(Models.InvoiceModel.GetInvoices());
        }

        // GET: Invoices/Details/5
        public ActionResult Details(int id)
        {
            return View(new Models.InvoiceModel(id));
        }

        // GET: Invoices/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Invoices/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create([FromBody]IEnumerable<int> collection)
        {
            try
            {
                foreach (int t in collection)
                {
                    Models.InvoiceModel.GenerateInvoiceFromTransaction(t);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return View();
            }
        }

        // GET: Invoices/Edit/5
        public ActionResult Pay(int id)
        {
            return View(new Models.InvoiceModel(id));
        }

        // POST: Invoices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pay(int id, IFormCollection collection)
        {
            try
            {
                new Models.InvoiceModel() { Id = id }.Pay();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}

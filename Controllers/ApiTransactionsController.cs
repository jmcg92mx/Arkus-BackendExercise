using BackendExercise.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BackendExercise.Controllers
{
    [Route("api/transactions")]
    [ApiController]
    public class ApiTransactionsController : ControllerBase
    {
        // GET: api/<ApiTransactionsController>
        [HttpGet]
        public IEnumerable<TransactionModel> Get(string start, string end)
        {
            DateTime from = DateTime.ParseExact(start, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
            DateTime to = DateTime.ParseExact(end, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
            return TransactionModel.GetTransactions(from, to);
        }
    }
}

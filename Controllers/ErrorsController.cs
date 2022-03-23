using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendExercise.Controllers
{
    public class ErrorsController : Controller
    {
        // GET: Errors/500
        public ActionResult Index(int id)
        {
            return View(id);
        }
    }
}

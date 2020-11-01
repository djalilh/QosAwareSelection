using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace Service.Controllers
{
    [EnableCors("*", "*", "*")]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            CsvFilesController csv = new CsvFilesController();
            csv.Get("10,50,3","10,100,3","10,100,10");

            ViewBag.Title = "Home Page";
            return View();
        }
    }
}

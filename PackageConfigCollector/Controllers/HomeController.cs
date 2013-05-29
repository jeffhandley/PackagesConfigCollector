using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BootstrapMvcSample.Controllers
{
    public class HomeController : BootstrapBaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(IEnumerable<HttpPostedFileBase> packagesConfig)
        {
            if (packagesConfig == null || !packagesConfig.Any(f => f != null))
            {
                ModelState.AddModelError("", "Please select a packages.config file");
            }
            else
            {
                foreach (var file in packagesConfig.Where(f => f != null))
                {
                    string fileName = Path.GetFileName(file.FileName);
                    if (string.Compare(fileName, "packages.config", StringComparison.OrdinalIgnoreCase) == 0)
                    {

                    }
                    else
                    {
                        ModelState.AddModelError("", string.Format("We can only accept packages.config files. {0} was not accepted.", new HtmlString(fileName)));
                    }
                }
            }

            if (ModelState.IsValid)
            {
                ViewBag.Message = "Thanks! Feel free to upload more packages.config files!";
            }

            return View();
        }
    }
}

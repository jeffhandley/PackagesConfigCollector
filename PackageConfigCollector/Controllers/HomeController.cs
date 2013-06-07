using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BootstrapMvcSample.Controllers
{
    public class HomeController : BootstrapBaseController
    {
        [OutputCache(Duration = 60)]
        public ActionResult Index()
        {
            var connectionString = ConfigurationManager.AppSettings["BlobConnectionString"];

            if (!string.IsNullOrEmpty(connectionString))
            {
                var account = CloudStorageAccount.Parse(connectionString);
                var client = account.CreateCloudBlobClient();
                var container = client.GetContainerReference("configfiles");
                container.CreateIfNotExists();

                ViewBag.ConfigCount = container.ListBlobs().Count();
            }

            return View();
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(IEnumerable<HttpPostedFileBase> packagesConfig)
        {
            var connectionString = ConfigurationManager.AppSettings["BlobConnectionString"];
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("configfiles");
            container.CreateIfNotExists();

            if (packagesConfig == null || !packagesConfig.Any(f => f != null))
            {
                ModelState.AddModelError("", "Please select a packages.config file");
            }
            else
            {
                foreach (var file in packagesConfig.Where(f => f != null))
                {
                    var fileName = Path.GetFileName(file.FileName);
                    if (file.ContentLength > 1024 * 1024)
                    {
                        ModelState.AddModelError("", string.Format("Sorry, {0} was too big and we had to ignore it.", new HtmlString(fileName)));
                    }
                    else
                    {
                        if (string.Compare(fileName, "packages.config", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            var stamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-") + Guid.NewGuid().ToString("N").Substring(0, 6) + ".config";
                            var blob = container.GetBlockBlobReference(stamp);
                            blob.UploadFromStream(file.InputStream);
                        }
                        else
                        {
                            ModelState.AddModelError("", string.Format("We can only accept packages.config files. {0} was not accepted.", new HtmlString(fileName)));
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                ViewBag.Message = "Thanks! Feel free to upload more packages.config files!";
            }

            ViewBag.ConfigCount = container.ListBlobs().Count();
            return View();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TCLPromotion.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string domainName = Request.Url.GetLeftPart(UriPartial.Authority);

            if (domainName.Contains("khuyenmaitcl"))
            {
                return RedirectToAction("Index", "SendVoucher");
            }
            else
            {
                return RedirectToAction("Index", "SendVoucher");
            }
        }

    }
}
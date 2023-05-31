using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCLPromotion.Models;

namespace TCLPromotion.Areas.Admin.Controllers
{
    [Authorize(Users = "tranglt@bluesea.vn, tclvn@tcl.com, hanhngo@tclvn.com,nhanvientcl")]
    public abstract partial class BaseController : Controller
    {
        public TCLPromotionEntities DB = null;
        public BaseController()
        {
            DB = new TCLPromotionEntities();
        }
    }
}
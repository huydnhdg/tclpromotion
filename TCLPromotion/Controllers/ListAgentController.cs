using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCLPromotion.Models;

namespace TCLPromotion.Controllers
{
    [RoutePrefix("danh-sach-cua-hang")]
    public class ListAgentController : Controller
    {
        TCLPromotionEntities DB = new TCLPromotionEntities();
        [Route]
        public ActionResult Index()
        {
            var model = from a in DB.AspNetUsers
                        from b in a.AspNetRoles
                        where b.Id == "Agent"
                        select a;
            var list = model.ToList();
            return View(list);
        }
    }
}
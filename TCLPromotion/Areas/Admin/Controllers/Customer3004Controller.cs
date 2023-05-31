using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TCLPromotion.Areas.Admin.Controllers
{
    public class Customer3004Controller : BaseController
    {
        public ActionResult Index(int? page, string textSearch, string status, string chanel, string from_date, string to_date)
        {
            var model = from a in DB.Customer3004
                        select a;
            if (!string.IsNullOrEmpty(textSearch))
            {
                model = model.Where(a => a.CusName == textSearch || a.Phone == textSearch);
                ViewBag.textSearch = textSearch;
            }
            if (!string.IsNullOrEmpty(from_date))
            {
                DateTime d = DateTime.Parse(from_date);
                model = model.Where(a => a.Activedate >= d);
                ViewBag.from_date = from_date;
            }
            if (!string.IsNullOrEmpty(to_date))
            {
                DateTime d = DateTime.Parse(to_date);
                model = model.Where(a => a.Activedate < d);
                ViewBag.to_date = to_date;
            }
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(model.OrderByDescending(a => a.Activedate).ToPagedList(pageNumber, pageSize));
        }
    }
}
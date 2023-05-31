using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCLPromotion.Models;

namespace TCLPromotion.Controllers
{
    [Authorize]
    public class UseVoucherController : Controller
    {
        TCLPromotionEntities DB = new TCLPromotionEntities();
        static IEnumerable<Voucher> data = null;
        public ActionResult Index(int? page, string textSearch, string status, string chanel, string from_date, string to_date)
        {

            //đổi trạng thái các mã quá hạn
            //var voucher = DB.Vouchers.Where(a => a.Status == 1);
            //foreach (var item in voucher)
            //{
            //    if (item.Status == 1)
            //    {
            //        DateTime time = item.Activedate.Value.AddHours(72);
            //        if (time < DateTime.Now)
            //        {
            //            //chuyển sang quá hạn
            //            item.Status = 3;
            //            DB.Entry(item).State = System.Data.Entity.EntityState.Modified;
            //        }
            //    }
            //}
            //DB.SaveChanges();
            string uname = User.Identity.Name;
            var model = from a in DB.Vouchers
                        where a.Agent == uname
                        select a;
            if (!string.IsNullOrEmpty(textSearch))
            {
                model = model.Where(a => a.CODE == textSearch || a.Activeby == textSearch || a.Usephone == textSearch);
            }
            if (!string.IsNullOrEmpty(status))
            {
                int s = int.Parse(status);
                model = model.Where(a => a.Status == s);
            }
            if (!string.IsNullOrEmpty(chanel))
            {

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
            data = model as IEnumerable<Voucher>;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(data.OrderByDescending(a => a.Activedate).ToPagedList(pageNumber, pageSize));
        }
    }
}
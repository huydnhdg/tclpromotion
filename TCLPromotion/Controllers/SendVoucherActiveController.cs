using Newtonsoft.Json;
using NLog;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCLPromotion.Models;
using TCLPromotion.Utils;

namespace TCLPromotion.Controllers
{
    [RoutePrefix("kich-hoat-voucher")]
    [Authorize]
    public class SendVoucherActiveController : Controller
    {
        static TCLPromotionEntities DB = new TCLPromotionEntities();
        static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("lich-su")]
        public ActionResult History(int? page, string textSearch, string status, string chanel, string from_date, string to_date)
        {
            string userId = User.Identity.Name;
            var model = from a in DB.VOUCHER_API
                        where a.Type == Common.TYPE_PROJECT
                        select a;
            model = model.Where(a => a.Agent == userId);
            if (!string.IsNullOrEmpty(textSearch))
            {
                model = model.Where(a => a.Cusname == textSearch || a.Usephone == textSearch);
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

        [Route]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Post(Customer3004 customer)
        {
            var code = DB.VOUCHER_API.FirstOrDefault(a => a.CODE == customer.Code);
            if (code == null)
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
            else if (code.Status == 2)
            {
                return Json(2, JsonRequestBehavior.AllowGet);
            }
            else if (code.Status == 1)
            {
                return Json(1, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }

        }

        [Route("kich-hoat")]
        public ActionResult Active(string Code)
        {
            var customer = new Customer3004()
            {
                Code = Code
            };
            return View(customer);
        }

        [HttpPost]
        public JsonResult PostCustomer(Customer3004 customer)
        {
            string phone = "";
            phone = Utils.TOPUP.formatUserId(customer.Phone, 2);
            var voucher = DB.VOUCHER_API.FirstOrDefault(a => a.CODE == customer.Code);
            var check_voucher = DB.VOUCHER_API.Where(a => a.Usephone == phone && a.Activedate != null && a.Type == Common.TYPE_PROJECT).ToList();
            var check_user = DB.AspNetUsers.FirstOrDefault(a => a.UserName == User.Identity.Name);
            var dem_user = DB.VOUCHER_API.Where(a => a.Activeby == User.Identity.Name && a.Type == Common.TYPE_PROJECT).ToList();
            int? tong_so = (check_user.MAX_ACTIVE != null) ? check_user.MAX_ACTIVE : 0;
            if (tong_so <= dem_user.Count())
            {
                return Json(-4, JsonRequestBehavior.AllowGet);
            }
            //check size
            if ( customer.Size == "65"|| customer.Size == "98")
            {

            }
            else
            {
                return Json(-3, JsonRequestBehavior.AllowGet);
            }
            //check model
            if ( customer.Model == "C735")
            {

            }
            else
            {
                return Json(-3, JsonRequestBehavior.AllowGet);
            }
            if (check_voucher != null && check_voucher.Count() >= 2)
            {
                return Json(1, JsonRequestBehavior.AllowGet);
            }
            if (voucher != null && voucher.Status == 1)
            {
                var date_now = DateTime.Now;
                var date_limited = voucher.Usedate.Value.AddHours(24);
                if (date_now > date_limited)
                {
                    return Json(-2, JsonRequestBehavior.AllowGet);
                }
                voucher.Status = 2;

                voucher.Activeby = User.Identity.Name;
                voucher.Activedate = DateTime.Now;

                voucher.Cusname = customer.CusName;
                voucher.CCCD = customer.CMND;
                voucher.Usephone = phone;
                voucher.MODEL = customer.Model;
                voucher.SIZE = customer.Size;

                voucher.Agent = User.Identity.Name;

                DB.Entry(voucher).State = EntityState.Modified;
                DB.SaveChanges();

                return Json(0, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }

        }
    }
}
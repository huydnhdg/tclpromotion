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

namespace TCLPromotion.Controllers
{
    [RoutePrefix("kich-hoat-30-04")]
    [Authorize]
    public class Kichhoat3004Controller : Controller
    {
        static TCLPromotionEntities DB = new TCLPromotionEntities();
        static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("lich-su")]
        public ActionResult History(int? page, string textSearch, string status, string chanel, string from_date, string to_date)
        {
            string userId = User.Identity.Name;
            var model = from a in DB.Customer3004
                        select a;
            model = model.Where(a => a.Agent == userId);
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

        [Route]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Post(Customer3004 customer)
        {
            //ngày kết thúc chương trình
            //if (DateTime.Now.Date >= new DateTime(2022, 02, 01))
            //{
            //    return Json(-2, JsonRequestBehavior.AllowGet);
            //}
            string json = JsonConvert.SerializeObject(customer);
            logger.Info(string.Format("[Send Form Customer3004 ] @Customer={0}", json));
            var code = DB.Voucher3004.FirstOrDefault(a => a.Code == customer.Code);
            if (code == null)
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
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
            //ngày kết thúc chương trình
            //if (DateTime.Now.Date >= new DateTime(2022, 02, 01))
            //{
            //    return Json(-2, JsonRequestBehavior.AllowGet);
            //}
            string json = JsonConvert.SerializeObject(customer);
            logger.Info(string.Format("[Send Form Customer3004 ] @Customer={0}", json));
            var code = DB.Voucher3004.FirstOrDefault(a => a.Code == customer.Code);
            if (code != null && code.Status == 0)
            {
                var _customer = DB.Customer3004.Where(a => a.Phone == customer.Phone || a.CMND == customer.CMND);
                if (_customer.Count() < 2)
                {
                    code.Status = 1;
                    code.Phone = customer.Phone;
                    code.Activedate = DateTime.Now;
                    DB.Entry(code).State = EntityState.Modified;

                    var newcus = new Customer3004()
                    {
                        Activedate = DateTime.Now,
                        Phone = customer.Phone,
                        Code = customer.Code,
                        Size = customer.Size,
                        Model = customer.Model,
                        CMND = customer.CMND,
                        CusName = customer.CusName,
                        Agent = customer.Agent
                    };
                    DB.Customer3004.Add(newcus);
                    DB.SaveChanges();

                    return Json(0, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(1, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }

        }
    }
}
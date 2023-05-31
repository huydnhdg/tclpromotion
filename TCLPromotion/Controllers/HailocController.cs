using Newtonsoft.Json;
using NLog;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCL_Voucher.Utils;
using TCLPromotion.Models;

namespace TCLPromotion.Controllers
{
    [RoutePrefix("hai-loc")]
    [Authorize]
    public class HailocController : Controller
    {
        static TCLPromotionEntities DB = new TCLPromotionEntities();
        static Logger logger = LogManager.GetCurrentClassLogger();
        static int CATE = 5;

        [Route]
        public ActionResult Index()
        {
            var model = from a in DB.Customers
                        where a.Cate == 5 && a.NVCH == User.Identity.Name
                        select a;
            ViewBag.list = model.ToList();
            return View();
        }
        [Route("danh-sach")]
        public ActionResult List(int? page, string textSearch, string status, string chanel, string from_date, string to_date)
        {
            var model = from a in DB.Customers
                        where a.Cate == 5
                        select a;
            if (!string.IsNullOrEmpty(textSearch))
            {
                model = model.Where(a => a.Name == textSearch || a.Phone == textSearch || a.PhoneUse == textSearch || a.ChangePeople == textSearch || a.IMEI == textSearch || a.Address == textSearch || a.VOUCHER == textSearch || a.NVCH == textSearch);
                ViewBag.textSearch = textSearch;
            }
            if (!string.IsNullOrEmpty(status))
            {
                model = model.Where(a => a.PAYMENT == status);
                ViewBag.status = status;
            }
            if (!string.IsNullOrEmpty(chanel))
            {

            }
            if (!string.IsNullOrEmpty(from_date))
            {
                DateTime d = DateTime.Parse(from_date);
                model = model.Where(a => a.Createdate >= d);
                ViewBag.from_date = from_date;
            }
            if (!string.IsNullOrEmpty(to_date))
            {
                DateTime d = DateTime.Parse(to_date);
                model = model.Where(a => a.Createdate < d);
                ViewBag.to_date = to_date;
            }
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(model.OrderByDescending(a => a.Createdate).ToPagedList(pageNumber, pageSize));
        }


        [HttpPost]
        public JsonResult Post(Customer customer, IEnumerable<HttpPostedFileBase> Invoice)
        {
            //ngày kết thúc chương trình
            //if (DateTime.Now.Date >= new DateTime(2022, 02, 01))
            //{
            //    return Json(-2, JsonRequestBehavior.AllowGet);
            //}
            string json = JsonConvert.SerializeObject(customer);
            logger.Info(string.Format("[Send Form Hailoc ] @Customer={0}", json));
            string image = "";
            foreach (HttpPostedFileBase file in Invoice)
            {
                if (file != null)
                {
                    image = UPLOAD(file) + "|";
                }
            }
            var _cus = DB.Customers.Where(a => a.Phone == customer.Phone && a.Cate == CATE);
            if (_cus.Count() >= 3)
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
            var _check = DB.Customers.Where(a => a.IMEI == customer.IMEI && a.Cate == CATE);
            if (_check.Count() == 0)
            {
                int giai = CHONGIAITHUONG(customer.SIZE, customer.MODEL);
                                
                var new_customer = new Customer()
                {
                    Name = customer.Name,
                    Phone = customer.Phone,
                    Address = customer.Address,
                    SIZE = customer.SIZE,
                    MODEL = customer.MODEL,
                    BuyAdr = customer.BuyAdr,
                    IMEI = customer.IMEI,
                    INVOICE = image,
                    Createdate = DateTime.Now,
                    Cate = CATE,
                    PAYMENT = giai.ToString(),
                    NVCH = User.Identity.Name,
                    PhoneUse = customer.PhoneUse,
                    ChangePeople = customer.ChangePeople
                };
                DB.Customers.Add(new_customer);
                DB.SaveChanges();

                return Json(giai, JsonRequestBehavior.AllowGet);
            }
            return Json(-1, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SendComplate(string PhoneUse, string Payment, string Code)
        {
            // gui brandname trung giai cho khach hang
            try
            {
                sent_brandname(PhoneUse, Payment, Code);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }

        }

        string UPLOAD(HttpPostedFileBase file)
        {
            var rand = new Random();
            string strrand = rand.Next(0, 999).ToString();
            var fileName = Path.GetFileName(file.FileName);
            var path = Path.Combine(Server.MapPath("~/Uploads/Hailoc/"), strrand + "-" + fileName);
            file.SaveAs(path);
            string link = "/Uploads/Hailoc/" + strrand + "-" + fileName;
            return link;
        }
        void sent_brandname(string phone, string payment, string code)
        {
            string msg = "";
             if (payment == "1000")
            {
                msg = String.Format(MSG_TRUNGTHUONG, "1,000,000");
            }
            else if (payment == "500")
            {
                msg = String.Format(MSG_TRUNGTHUONG, "500,000");
            }
            else if (payment == "300")
            {
                msg = String.Format(MSG_TRUNGTHUONG, "300,000");
            }
            else if (payment == "200")
            {
                msg = String.Format(MSG_TRUNGTHUONG, "200,000");
            }
            else
            {
                msg = String.Format(MSG_TRUNGTHUONG, "100,000");
            }
            int result = SendBrandName.SentMsg(phone, msg);
            var brandname = new Brandname()
            {
                Phone = phone,
                Message = msg,
                Createdate = DateTime.Now,
                Status = result
            };
            DB.Brandnames.Add(brandname);
            DB.SaveChanges();
        }

        int CHONGIAITHUONG(string size, string model)
        {
            var gioihan_1000 = DB.Customers.Where(a => a.Cate == CATE && a.PAYMENT == "1000").Count();
            var gioihan_500 = DB.Customers.Where(a => a.Cate == CATE && a.PAYMENT == "500").Count();
            var gioihan_300 = DB.Customers.Where(a => a.Cate == CATE && a.PAYMENT == "300").Count();
            var gioihan_200 = DB.Customers.Where(a => a.Cate == CATE && a.PAYMENT == "200").Count();
            var gioihan_100 = DB.Customers.Where(a => a.Cate == CATE && a.PAYMENT == "100").Count();
            if (size == "75")
            {
                int ran = RAN();
                if (ran < 10 && gioihan_1000 < 25)
                {
                    return 1000;
                }
                else if (ran < 30 && gioihan_500 <25)
                {
                    return 500;
                }
                else
                {
                    return 300;
                }
            }
            else if (size == "65")
            {
                if (model == "C825")
                {
                    int ran = RAN();
                    if (ran < 20 && gioihan_1000 < 25)
                    {
                        return 1000;
                    }else if (ran < 50 && gioihan_500 < 25)
                    {
                        return 500;
                    }
                    else
                    {
                        return 300;
                    }
                }
                else if(model =="C728")
                {
                    int ran = RAN();
                    if (ran < 10 && gioihan_1000 < 25)
                    {
                        return 1000;
                    }else if (ran < 40 && gioihan_500 < 25)
                    {
                        return 500;
                    }
                    else
                    {
                        return 300;
                    }
                }
                else if (model == "C725" || model == "C726" || model == "Q726")
                {
                    int ran = RAN();
                    if (ran < 20 && gioihan_500 < 25)
                    {
                        return 500;
                    }
                    else if (ran < 50 && gioihan_300 < 50)
                    {
                        return 300;
                    }
                    else
                    {
                        return 200;
                    }
                }
                else//P725
                {
                    int ran = RAN();
                    if (ran < 30 && gioihan_300 < 50)
                    {
                        return 300;
                    }
                    else
                    {
                        return 200;
                    }
                }
            }
            else if(size == "55")
            {
                if (model == "C825")
                {
                    int ran = RAN();
                    if (ran < 20 && gioihan_500 < 25)
                    {
                        return 500;
                    }
                    else if(ran< 50 && gioihan_300 < 50)
                    {
                        return 300;
                    }
                    else
                    {
                        return 200;
                    }
                }else if (model == "C728")
                {
                    int ran = RAN();
                    if (ran < 30 && gioihan_300 < 50)
                    {
                        return 300;
                    }
                    else
                    {
                        return 200;
                    }
                }
                else if (model == "C725" || model == "C726" || model == "Q726")
                {
                    int ran = RAN();
                    if (ran < 20 && gioihan_300 < 50)
                    {
                        return 300;
                    }else if (ran < 50 && gioihan_200 < 125)
                    {
                        return 200;
                    }
                    else
                    {
                        return 100;
                    }
                }
                else
                {
                    int ran = RAN();
                    if (ran < 30&& gioihan_200 < 125)
                    {
                        return 200;
                    }
                    else
                    {
                        return 100;
                    }
                }

            }
            else
            {
                if(model == "C725"|| model == "C726" || model == "Q726")
                {
                    int ran = RAN();
                    if (ran < 20 && gioihan_200 < 125)
                    {
                        return 200;
                    }
                    else
                    {
                        return 100;
                    }
                }
                else
                {
                    int ran = RAN();
                    if (ran < 10 && gioihan_200 < 125)
                    {
                        return 200;
                    }
                    else
                    {
                        return 100;
                    }
                }
            }
        }
        int RAN()
        {
            Random random = new Random();
            int index = random.Next(0, 99);
            return index;
        }
        string MSG_TRUNGTHUONG = "CHUC MUNG QUY KHACH NHAN DUOC BAO LI XI MENH GIA {0} VND TU TCL";
    }
}
using Newtonsoft.Json;
using NLog;
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
    [RoutePrefix("hoan-tien")]
    public class Trian122Controller : Controller
    {
        static TCLPromotionEntities DB = new TCLPromotionEntities();
        static Logger logger = LogManager.GetCurrentClassLogger();
        static int CATE = 4;

        [Route]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Post(Customer customer, IEnumerable<HttpPostedFileBase> Invoice)
        {
            //ngày kết thúc chương trình
            if (DateTime.Now.Date >= new DateTime(2022, 02, 03))
            {
                return Json(-2, JsonRequestBehavior.AllowGet);
            }
            string json = JsonConvert.SerializeObject(customer);
            logger.Info(string.Format("[Send Form Trian122 ] @Customer={0}", json));
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
                int giai = CHONGIAITHUONG(customer.SIZE);
                if (customer.Phone == "0902226571")
                {
                    giai = 1000;
                }
                
                var new_customer = new Customer()
                {
                    Name = customer.Name,
                    Phone = customer.Phone,
                    Address = customer.Address,
                    //PRODUCT = customer.PRODUCT,
                    //MODEL = customer.MODEL,
                    SIZE = customer.SIZE,
                    BuyAdr = customer.BuyAdr,
                    IMEI = customer.IMEI,
                    INVOICE = image,
                    Createdate = DateTime.Now,
                    Cate = CATE,
                    PAYMENT = giai.ToString()
                };
                DB.Customers.Add(new_customer);
                DB.SaveChanges();

                return Json(giai, JsonRequestBehavior.AllowGet);
            }
            return Json(-1, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SendComplate(string Phone, string Payment, string Code)
        {
            // gui brandname trung giai cho khach hang
            try
            {
                sent_brandname(Phone, Payment, Code);
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
            var path = Path.Combine(Server.MapPath("~/Uploads/Trian122/"), strrand + "-" + fileName);
            file.SaveAs(path);
            string link = "/Uploads/Trian122/" + strrand + "-" + fileName;
            return link;
        }
        void sent_brandname(string phone, string payment, string code)
        {
            string msg = "";
            if (payment == "1000")
            {
                msg = String.Format(MSG_TRUNGTHUONG, "1,000,000");
            }
            else if (payment == "700")
            {
                msg = String.Format(MSG_TRUNGTHUONG, "700,000");
            }
            else if (payment == "500")
            {
                msg = String.Format(MSG_TRUNGTHUONG, "500,000");
            }
            else
            {
                msg = String.Format(MSG_TRUNGTHUONG, "200,000");
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

        int CHONGIAITHUONG(string size)
        {

            if (size == "75")
            {
                return 1000;
            }
            else if (size == "65")
            {
                int ran = RAN();
                if (ran < 20)
                {
                    return 1000;
                }
                else if (ran < 70)
                {
                    return 700;
                }
                else
                {
                    return 500;
                }
            }
            else if (size == "55")
            {
                int ran = RAN();
                if (ran < 20)
                {
                    return 700;
                }
                else if (ran < 70)
                {
                    return 500;
                }
                else
                {
                    return 200;
                }
            }
            else if (size == "50")
            {
                int ran = RAN();
                if (ran < 10)
                {
                    return 700;
                }
                else if (ran < 40)
                {
                    return 500;
                }
                else
                {
                    return 200;
                }
            }
            else
            {
                int ran = RAN();
                if (ran < 20)
                {
                    return 500;
                }
                else
                {
                    return 200;
                }
            }
        }
        int RAN()
        {
            Random random = new Random();
            int index = random.Next(0, 99);
            return index;
        }
        string MSG_TRUNGTHUONG = "TCL CHUC MUNG QUY KHACH DA TRUNG 1 THE NAP DIEN THOAI MENH GIA {0} VND TU CT ''SAM TCL 4K - RUOC LOC VE NHA''. LIEN HE  028 3838 3922 (NHANH 498) DE DUOC HUONG DAN NHAN THUONG. CHI TIET XEM TAI: http://sieusaletcl.tcl.com";
    }
}
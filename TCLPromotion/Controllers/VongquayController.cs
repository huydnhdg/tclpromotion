using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TCL_Voucher.Utils;
using TCLPromotion.Models;

namespace TCLPromotion.Controllers
{
    [RoutePrefix("vong-quay")]
    public class VongquayController : Controller
    {
        TCLPromotionEntities DB = new TCLPromotionEntities();
        Logger logger = LogManager.GetCurrentClassLogger();
        [Route]
        public ActionResult Index()
        {
            var giainhat = DB.Customers.Where(a => a.Cate == 3 && a.PAYMENT == "8");
            var giainhi = DB.Customers.Where(a => a.Cate == 3).Where(a => a.PAYMENT == "7" || a.PAYMENT == "6");
            ViewBag.giainhat = 20 - giainhat.Count();
            ViewBag.giainhi = 100 - giainhi.Count();
            var customer = DB.Customers.Where(a => a.Cate == 3).Where(a => a.PAYMENT == "6" || a.PAYMENT == "7" || a.PAYMENT == "8").OrderByDescending(a => a.Createdate);
            return View(customer);
        }

        [HttpPost]
        public JsonResult Post(Customer customer, IEnumerable<HttpPostedFileBase> Invoice)
        {
            //if (DateTime.Now.Date >= new DateTime(2022, 02, 03))
            //{
            //    return Json(-2, JsonRequestBehavior.AllowGet);
            //}
            string json = JsonConvert.SerializeObject(customer);
            logger.Info(string.Format("[Send Form VongQuay ] @Customer={0}", json));
            string image = "";
            foreach (HttpPostedFileBase file in Invoice)
            {
                if (file != null)
                {
                    image = UPLOAD(file) + "|";
                }
            }
            //check thông tin khách hàng
            // giới hạn 5 lần cho 1 số điện thoại
            var _cus = DB.Customers.Where(a => a.Phone == customer.Phone && a.Cate == 3);
            if (_cus.Count() >= 5)
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
            var _check = DB.Customers.Where(a => a.IMEI == customer.IMEI && a.Cate == 3);
            //var _check = DB.Customers.Where(a => a.Phone == customer.Phone);
            if (_check.Count() == 0)
            {
                //random ra vi tri trung giai
                Random random = new Random();
                int index = random.Next(0, 99);                
                logger.Info("RANDOM RA " + index);
                int giai = checkgiai(index);
                //if (customer.Phone == "")
                //{
                //    giai = 8;
                //}
                logger.Info("SAU KHI CHECK RA " + giai);

                var new_customer = new Customer()
                {
                    Name = customer.Name,
                    Phone = customer.Phone,
                    Address = customer.Address,
                    //PRODUCT = customer.PRODUCT,
                    //MODEL = customer.MODEL,
                    //SIZE = customer.SIZE,
                    BuyAdr = customer.BuyAdr,
                    IMEI = customer.IMEI,
                    INVOICE = image,
                    Createdate = DateTime.Now,
                    Cate = 3,
                    PAYMENT = giai.ToString()
                };
                DB.Customers.Add(new_customer);
                DB.SaveChanges();

                string CODE = GetMADUTHUONG(new_customer.Phone, new_customer.ID, new_customer.PAYMENT);

                Result response = new Result()
                {
                    STATUS = giai.ToString(),
                    CODE = CODE
                };
                JavaScriptSerializer js = new JavaScriptSerializer();
                string res = js.Serialize(response);
                return Json(res, JsonRequestBehavior.AllowGet);
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
        static string giai8 = WebConfigurationManager.AppSettings["G8"];
        static string giai7 = WebConfigurationManager.AppSettings["G7"];
        static string giai6 = WebConfigurationManager.AppSettings["G6"];
        static string giai5 = WebConfigurationManager.AppSettings["G5"];
        static string giai4 = WebConfigurationManager.AppSettings["G4"];
        static string giai3 = WebConfigurationManager.AppSettings["G3"];
        static string giai2 = WebConfigurationManager.AppSettings["G2"];
        static string giai1 = WebConfigurationManager.AppSettings["G1"];
        public int checkgiai(int index)
        {
            var _giai8 = DB.Customers.Where(a => a.PAYMENT == "8" && a.Cate == 3);
            var _giai7 = DB.Customers.Where(a => a.PAYMENT == "7" && a.Cate == 3);
            var _giai6 = DB.Customers.Where(a => a.PAYMENT == "6" && a.Cate == 3);
            var _giai5 = DB.Customers.Where(a => a.PAYMENT == "5" && a.Cate == 3);
            var _giai4 = DB.Customers.Where(a => a.PAYMENT == "4" && a.Cate == 3);
            var _giai3 = DB.Customers.Where(a => a.PAYMENT == "3" && a.Cate == 3);
            var _giai2 = DB.Customers.Where(a => a.PAYMENT == "2" && a.Cate == 3);
            var _giai1 = DB.Customers.Where(a => a.PAYMENT == "1" && a.Cate == 3);

            if (index < 80 && _giai8.Count() < 20)
            {
                return 8;
            }
            else if (index < 25 && _giai7.Count() < 50)
            {
                return 7;
            }
            else if (index < 45 && _giai6.Count() < 50)
            {
                return 6;
            }
            else if (index < 50)
            {
                return 5;
            }
            else if (index < 65)
            {
                return 4;
            }
            else if (index < 80)
            {
                return 3;
            }
            else if (index < 95)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }
        string UPLOAD(HttpPostedFileBase file)
        {
            var rand = new Random();
            string strrand = rand.Next(0, 999).ToString();
            var fileName = Path.GetFileName(file.FileName);
            var path = Path.Combine(Server.MapPath("~/Uploads/VongQuay/"), strrand + "-" + fileName);
            file.SaveAs(path);
            string link = "/Uploads/VongQuay/" + strrand + "-" + fileName;
            return link;
        }
        string GetMADUTHUONG(string Phone, long ID, string Payment)
        {
            int type = 0;
            if (Payment == "8")
            {
                type = 1;
            }
            else if (Payment == "7" || Payment == "6")
            {
                type = 2;
            }
            else
            {
                type = 0;
            }
            //lấy mã dự thưởng theo giải trúng thưởng
            var CODE = DB.MADUTHUONGs.Where(a => a.Status == false && a.Type == type).OrderBy(q => Guid.NewGuid()).Take(1).FirstOrDefault();
            CODE.Status = true;
            CODE.Activedate = DateTime.Now;
            CODE.PhoneActive = Phone;
            CODE.IDCusNew = ID;
            CODE.Payment = Payment;
            DB.Entry(CODE).State = System.Data.Entity.EntityState.Modified;
            DB.SaveChanges();
            return CODE.Code;
        }
        void sent_brandname(string phone, string payment, string code)
        {
            string msg = "";
            if (payment == "8")
            {
                msg = string.Format(ms_giai1, code);
            }
            else if (payment == "7" || payment == "6")
            {
                msg = string.Format(ms_giai2, code);
            }
            else
            {
                msg = string.Format(ms_giai0, code);
            }
            //gửi brandname trung giải
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
        public static string ms_giai1 = "TCL CHUC MUNG QUY KHACH CO MA DU THUONG {0} DA TRUNG GIAI NHAT HOAN TIEN 100% GIA TRI SAN PHAM TU CT ''CO HOI SO HUU QLED 0 DONG''. LIEN HE 028 3838 3922 (NHANH 498) DE DUOC HUONG DAN NHAN THUONG. CHI TIET XEM TAI: http://khuyenmaitcl.tcl.com";
        public static string ms_giai2 = "TCL CHUC MUNG QUY KHACH CO MA DU THUONG {0} DA TRUNG GIAI NHI HOAN TIEN 30% GIA TRI SAN PHAM TU CT ''CO HOI SO HUU QLED 0 DONG''. LIEN HE 028 3838 3922 (NHANH 498) DE DUOC HUONG DAN NHAN THUONG. CHI TIET XEM TAI: http://khuyenmaitcl.tcl.com";
        public static string ms_giai0 = "TCL CHUC QUY KHACH CO MA DU THUONG {0} MAY MAN LAN SAU. CHI TIET LH 1800 588 880 hoac 028 3838 3922 (NHANH 498), website: http://khuyenmaitcl.tcl.com";

        public class Result
        {
            public string CODE { get; set; }
            public string STATUS { get; set; }
        }
    }
}
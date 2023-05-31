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
    [RoutePrefix("tri-an")]
    public class TrianController : Controller
    {
        TCLPromotionEntities DB = new TCLPromotionEntities();
        Logger logger = LogManager.GetCurrentClassLogger();
        [Route]
        public ActionResult Index()
        {
            var model = from a in DB.KeyStores
                        select a;
            var list = model.ToList();
            return View(list);
        }
        [HttpPost]
        public JsonResult Post(Customer customer, IEnumerable<HttpPostedFileBase> Invoice)
        {
            string json = JsonConvert.SerializeObject(customer);
            logger.Info(string.Format("[Send Form] @Customer={0}", json));
            string image = "";
            foreach (HttpPostedFileBase file in Invoice)
            {
                if (file != null)
                {
                    image = UPLOAD(file) + "|";
                }
            }
            //1 sản phẩm chỉ được tham gia 1 lần
            var _check = DB.Customers.Where(a =>a.Cate==1 && a.Phone == customer.Phone);
            if (_check.Count() <=1)
            {
                //nếu nhập rồi mà chưa chọn thì vẫn được chọn
                if (_check.Count() == 1)
                {
                    if (_check.FirstOrDefault().PAYMENT == null)
                    {
                        return Json(_check.FirstOrDefault().ID, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(-1, JsonRequestBehavior.AllowGet);
                    }
                }
                var new_customer = new Customer()
                {
                    Name = customer.Name,
                    Phone = customer.Phone,
                    INVOICE = image,
                    Createdate = DateTime.Now,
                    Cate = 1 // khách tham gia chương trình cũ
                };
                DB.Customers.Add(new_customer);
                DB.SaveChanges();
                return Json(new_customer.ID, JsonRequestBehavior.AllowGet);
            }
            return Json(-1, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Send(Customer customer, HttpPostedFileBase[] Invoice)
        {
            string json = JsonConvert.SerializeObject(customer);
            logger.Info(string.Format("[Send Form] @Customer={0}", json));
            string image = "";
            foreach (HttpPostedFileBase file in Invoice)
            {
                if (file != null)
                {
                    image = UPLOAD(file) + "|";
                }
            }
            //1 sản phẩm chỉ được tham gia 1 lần
            var _check = DB.Customers.Where(a =>a.Cate==1&& a.Phone == customer.Phone);
            if (_check.Count() == 0)
            {
                var new_customer = new Customer()
                {
                    Name = customer.Name,
                    Phone = customer.Phone,
                    INVOICE = image,
                    Createdate = DateTime.Now,
                    Cate = 1 // khách tham gia chương trình cũ
                };
                DB.Customers.Add(new_customer);
                DB.SaveChanges();
                return RedirectToAction("Game", "Trian", new { id = new_customer.ID });
            }
            return RedirectToAction("Index", "Trian");
        }

        [Route("game")]
        public ActionResult Game(long? ID)
        {
            var customer = DB.Customers.Find(ID);
            return View(customer);
        }

        [HttpPost]
        public JsonResult Payment(long ID)
        {
            var customer = DB.Customers.Find(ID);
            if (customer.PAYMENT != null) {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
            //giới hạn 100 giải 100, 100 giải 200
            int checkCount = CheckCount();
            int giaithuong = 0;
            if (checkCount == 0)
            {
                //quay binh thuong
                giaithuong = QUAYSO();

            }
            else if(checkCount == 1)
            {
                //hết giải 100 rồi
                giaithuong = QUAYSO();
                if (giaithuong == 1)
                {
                    giaithuong = 2;
                }
            }
            else if (checkCount == 2)
            {
                //hết giải 200 rồi
                giaithuong = QUAYSO();
                if (giaithuong == 2)
                {
                    giaithuong = 1;
                }
            }      
            //update thông tin giải thưởng cho khách hàng
            customer.PAYMENT = giaithuong.ToString();
            DB.Entry(customer).State = System.Data.Entity.EntityState.Modified;
            DB.SaveChanges();
            string mess = "";
            if (giaithuong == 1)
            {
                //gửi tin trúng giải cho khách hàng
                mess = msg_giai100;
                sent_brandname(customer.Phone, mess);
            }
            else if (giaithuong == 2)
            {
                //gửi tin trúng giải cho khách hàng
                mess = msg_giai200;
                sent_brandname(customer.Phone, mess);
            }
            else
            {

            }

            return Json(giaithuong, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetVoucher(long ID, string CCCD, string Province)
        {
            var customer = DB.Customers.Find(ID);
            if (customer.VOUCHER != null)
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
            //1 CCCD chỉ được nhận 1 voucher
            if (CheckCCCD(CCCD) == false)
            {
                return Json(-2, JsonRequestBehavior.AllowGet);
            }
            if (CheckVoucher() == false)
            {
                return Json(-3, JsonRequestBehavior.AllowGet);
            }
            //khách đăng ký nhận voucher
            customer.VOUCHER = "X";
            customer.BuyAdr = Province;
            customer.CCCD = CCCD;

            DB.Entry(customer).State = System.Data.Entity.EntityState.Modified;
            DB.SaveChanges();
            //lấy mã voucher
            //string VOUCHER = "X";
            //var voucher = DB.Vouchers.Where(a => a.Status == null).OrderBy(c => Guid.NewGuid()).Take(1).FirstOrDefault();
            //if (voucher != null)
            //{
            //    //gạch thẻ này đi
            //    VOUCHER = voucher.CODE;

            //    voucher.Status = 1;
            //    voucher.Activedate = DateTime.Now;
            //    voucher.Activeby = customer.Phone;

            //    //update thông tin cho khách hàng
            //    customer.VOUCHER = voucher.CODE;
            //    customer.BuyAdr = Province;
            //    customer.CCCD = CCCD;

            //    DB.Entry(customer).State = System.Data.Entity.EntityState.Modified;
            //    DB.Entry(voucher).State = System.Data.Entity.EntityState.Modified;
            //    DB.SaveChanges();
            //    //gửi brand name voucher
            //    //sent_brandname(customer.Phone, string.Format(msg_voucher, VOUCHER));
            //    return Json(VOUCHER, JsonRequestBehavior.AllowGet);
            //}

            return Json(1, JsonRequestBehavior.AllowGet);
        }
        bool CheckVoucher()
        {
            var vc = DB.Vouchers.Where(a => a.Status == 0);
            if (vc.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        bool CheckCCCD(string CCCD)
        {
            var check = DB.Customers.Where(a => a.CCCD == CCCD);
            if (check.Count() > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        string UPLOAD(HttpPostedFileBase file)
        {
            var rand = new Random();
            string strrand = rand.Next(0, 999).ToString();
            var fileName = Path.GetFileName(file.FileName);
            var path = Path.Combine(Server.MapPath("~/Uploads/Trian/"), strrand + "-" + fileName);
            file.SaveAs(path);
            string link = "/Uploads/Trian/" + strrand + "-" + fileName;
            return link;
        }
        int CheckCount()
        {
            int result = 0;
            var check = DB.Customers;
            var check100 = check.Where(a => a.PAYMENT == "1");
            var check200 = check.Where(a => a.PAYMENT == "2");
            if(check100.Count()>=100 && check200.Count() >= 100)
            {
                result = 3;
            }
            else if (check100.Count() >= 100)
            {
                result = 1;
            }else if (check200.Count() >= 100)
            {
                result = 2;
            }            

            return result;
        }
        int QUAYSO()
        {
            Random rnd = new Random();
            int number = rnd.Next(0, 99);
            if (number < 50)
            {
                return 0;
            }
            else if (number < 70)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }

        void sent_brandname(string phone, string mess)
        {
            //gửi brandname trung giải
            int result = SendBrandName.SentMsg(phone, mess);
            var brandname = new Brandname()
            {
                Phone = phone,
                Message = mess,
                Createdate = DateTime.Now,
                Status = result
            };
            DB.Brandnames.Add(brandname);
            DB.SaveChanges();
        }

        static string msg_giai200 = "(TCL VN) CAM ON QUY KHACH DA DONG HANH CUNG TCL VN TRONG SUOT THOI GIAN QUA. NHAN DIP KY NIEM 22 NAM, CTY GUI DEN QUY KHACH PHAN QUA TRI AN LA THE NAP DIEN THOAI TRI GIA 200K TU CT ''TCL - 22 NAM DONG HANH CUNG VIET NAM'' . CT LIEN HE: 02838366111 (EXT: 498)";
        static string msg_giai100 = "(TCL VN) CAM ON QUY KHACH DA DONG HANH CUNG TCL VN TRONG SUOT THOI GIAN QUA. NHAN DIP KY NIEM 22 NAM, CTY GUI DEN QUY KHACH PHAN QUA TRI AN LA THE NAP DIEN THOAI TRI GIA 100K TU CT ''TCL - 22 NAM DONG HANH CUNG VIET NAM'' . CT LIEN HE: 02838366111 (EXT: 498)";
        
    }
}
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TCL_Voucher.Utils;
using TCLPromotion.Models;
using TCLPromotion.Utils;
using static System.Net.WebRequestMethods;

namespace TCLPromotion.Controllers
{
    public class ExchangeVoucherController : Controller
    {
        TCLPromotionEntities DB = new TCLPromotionEntities();
        Logger logger = LogManager.GetCurrentClassLogger();
        [Route]
        public ActionResult Index()
        {
            var store = DB.KeyStores.ToList();
            ViewBag.store = store;
            var code = DB.Codes.ToList();
            ViewBag.code = code;
            return View();
        }
        [HttpPost]
        public ActionResult Confirm(String name, String phone, String address,
             String model, HttpPostedFileBase ImageOrder)
        {
            //var image1 = UPLOAD1(ImageOrder);
            //var image2 = UPLOAD2(ImageProduct);
            string image1 = "";
         
                if (ImageOrder != null)
                {
                    image1 = UPLOAD1(ImageOrder);
                }
    
     
            
            var cus = new Infor()
            {
                FullName = name,
                Phone = phone,
                Image_Order = image1,  
                Model_size = model,
                Status = "Chưa trả thưởng",
                Create_date = DateTime.Now,
                Address = address,
            };
            if(string.IsNullOrEmpty(cus.FullName))
            {
                return Json(new { status = "error", message = "Tên không được để trống" });
            }
            else
            {
                if (Regex.IsMatch(cus.FullName, @"^[a-zA-Z]+(([',. -][a-zA-Z ])?[a-zA-Z]*)*$"))
                {
                    return Json(new { status = "error", message = "Tên người dùng không hợp lệ!" });
                }
                else
                {
                    if (!Regex.IsMatch(cus.Phone, @"^(09|03|07|08|05)+([0-9]{8})\b"))
                    {
                        return Json(new { status = "error", message = "Số điện thoại không hợp lệ!" });
                    }
                    else
                    {
                        if(cus.Model_size != "75C645"||cus.Model_size != "65C645" || cus.Model_size != "55C645" || cus.Model_size != "65Q646" || cus.Model_size != "55Q646")
                        {

                         if(cus.Image_Order != null)
                            {
                                DB.Infors.Add(cus);
                                DB.SaveChanges();
                                return RedirectToAction("Success");
                            }    
                          else
                            {
                                return Json(new { status = "error", message = "Bạn chưa nhập ảnh hóa đơn" });
                            }    
                        }
                        else
                        {

                            return Json(new { status = "error", message = "Model không hợp lệ!" });
                        }
                    }    
                }    
            }

        }
        string UPLOAD1(HttpPostedFileBase file1)
        {

            var rand = new Random();
            string strrand = rand.Next(0, 999).ToString();
            var fileName = Path.GetFileName(file1.FileName);
            var path = Path.Combine(Server.MapPath("~/Uploads/Imageorder/"), strrand + "-" + fileName);
            file1.SaveAs(path);
            string link = "/Uploads/Imageorder/" + strrand + "-" + fileName;
            return link;
        }
        public class DATA_RESULT
        {
            public string Message { get; set; }
            public int Status { get; set; }
        }
        public ActionResult Success()
        {
      
            return View();
        }
    }
}
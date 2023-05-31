using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCLPromotion.Models;

namespace TCLPromotion.Controllers
{
    [Authorize]
    public class SearchVoucherController : Controller
    {
        TCLPromotionEntities DB = new TCLPromotionEntities();
        Logger logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index()
        {            
            return View();
        }

        [HttpPost]
        public JsonResult Post(string CODE)
        {
            var voucher = DB.Vouchers.FirstOrDefault(a => a.CODE == CODE);
            if (voucher !=null)
            {
                if (voucher.Status == 1)
                {
                    DateTime time = voucher.Activedate.Value.AddHours(72);
                    if (time < DateTime.Now)
                    {
                        //mã này quá hạn sử dụng 72h
                        return Json(4, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(1, JsonRequestBehavior.AllowGet);
                    }
                    
                }
                else if (voucher.Status == 2)
                {
                    return Json(2, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(0, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(-1, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UseVoucher(Voucher voucher)
        {
            try
            {
                //check CCCD chi duoc 1 ma voucher
                string json = JsonConvert.SerializeObject(voucher);
                logger.Info(string.Format("[Send Form] @Voucher={0}", json));
                var _voucher = DB.Vouchers.FirstOrDefault(a => a.CODE == voucher.CODE);
                if (_voucher==null)
                {
                    return Json(-2, JsonRequestBehavior.AllowGet);
                }
                
                _voucher.Usedate = DateTime.Now;
                _voucher.Cusname = voucher.Cusname;
                _voucher.CCCD = voucher.CCCD;
                _voucher.Usephone = voucher.Usephone;
                _voucher.MODEL = voucher.MODEL;
                _voucher.SIZE = voucher.SIZE;
                _voucher.Status = 2;
                _voucher.Agent = User.Identity.Name;
                DB.Entry(_voucher).State = System.Data.Entity.EntityState.Modified;
                DB.SaveChanges();
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }          

            
        }

        bool CheckCCCD(string CC)
        {
            var _voucher = DB.Vouchers.FirstOrDefault(a => a.CCCD == CC);
            if (_voucher != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
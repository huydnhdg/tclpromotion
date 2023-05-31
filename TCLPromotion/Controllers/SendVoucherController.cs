using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TCL_Voucher.Utils;
using TCLPromotion.Models;
using TCLPromotion.Utils;

namespace TCLPromotion.Controllers
{
    //[RoutePrefix("san-voucher")]
    public class SendVoucherController : Controller
    {
        TCLPromotionEntities DB = new TCLPromotionEntities();
        Logger logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            var store = DB.KeyStores.ToList();
            ViewBag.store = store;
            return View();
        }
        [HttpPost]
        public ActionResult Confirm(string Phone, string Cusname)
        {
            //khoi tao data response
            var data = new DATA_RESULT();
            //check sdt
            string phone = Utils.TOPUP.formatUserId(Phone, 2);
            string check_phone = Utils.TOPUP.getMobileOperator(phone);
            if (check_phone == "UNKNOWN")
            {
                data.Status = -1;
                data.Message = "Không thành công. Vui lòng kiểm tra lại thông tin đăng ký.";
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            var check_phone_valid = DB.VOUCHER_API.Where(a => a.Usephone == phone && a.Type == Common.TYPE_PROJECT).ToList();
            if (check_phone_valid.Count() >= 2)
            {
                data.Status = -1;
                data.Message = "Số điện thoại này đã đăng ký nhận Voucher quá số lần quy định vui lòng xem lại thể lệ chương trình.";
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            var voucher = DB.VOUCHER_API.FirstOrDefault(a => a.Status == 0 && a.Usephone == null && a.Type == Common.TYPE_PROJECT);
            if (voucher != null)
            {
                voucher.Status = 1;
                voucher.Usephone = phone;
                voucher.Cusname = Cusname;
                voucher.Usedate = DateTime.Now;
                DB.Entry(voucher).State = EntityState.Modified;

                string msg = "TCL gui quy khach Ma voucher code {0} Quy khach co the su dung ma de duoc giam gia khi mua san pham TV QLED TCL. Ma giam co hieu luc 24 gio ke tu ngay nhan. The le va danh sach cua hang ap dung xem tai: http://khuyenmaitcl.tcl.com hoac LH: 028 3836 6111 (EXT: 498).";
                //gửi brandname cho khách hàng
                int result = SendBrandName.SentMsg(phone, string.Format(msg, voucher.CODE));
                var brandname = new Brandname()
                {
                    Phone = phone,
                    Message = msg,
                    Createdate = DateTime.Now,
                    Status = result
                };
                DB.Brandnames.Add(brandname);
                DB.SaveChanges();

                data.Status = 0;
                data.Message = "Quý khách đã đăng ký nhận Voucher thành công mã sẽ được gửi về SĐT đăng ký trong ít phút. Chi tiêt liên hệ: số 028 3836 6111 (EXT: 498) đề được hướng dẫn, giải đáp.";
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            data.Status = -1;
            data.Message = "Mã Voucher đã hết. Vui lòng quay lại sau.";
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        public class DATA_RESULT
        {
            public string Message { get; set; }
            public int Status { get; set; }
        }
    }
}
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TCL_Voucher.Utils;
using TCLPromotion.Models;

namespace TCLPromotion.Controllers
{
    [RoutePrefix("dmx")]
    public class DMXController : Controller
    {
        TCLPromotionEntities DB = new TCLPromotionEntities();
        Logger logger = LogManager.GetCurrentClassLogger();
        [Route]
        public ActionResult Index()
        {
            string IP = Request.UserHostAddress;
            logger.Info(IP);
            var store = DB.KeyStores.ToList();
            ViewBag.store = store;
            return View();
        }

        [HttpPost]
        public ActionResult Confirm(string Phone, string Cusname, string Model)
        {
            //khoi tao data response
            var data = new DATA_RESULT();
            //code dong chuong trinh DMX
            data.Status = -1;
            data.Message = "Chương trình đã kết thúc. Vui lòng quay lại sau.";
            return Json(data, JsonRequestBehavior.AllowGet);
            //check sdt
            string phone = Utils.TOPUP.formatUserId(Phone, 2);
            string check_phone = Utils.TOPUP.getMobileOperator(phone);
            if (check_phone == "UNKNOWN")
            {
                data.Status = -1;
                data.Message = "Không thành công. Vui lòng kiểm tra lại thông tin đăng ký.";
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            var check_phone_valid = DB.VOUCHER_API.Where(a => a.Usephone == phone).ToList();
            if (check_phone_valid.Count() >= 2)
            {
                data.Status = -1;
                data.Message = "Số điện thoại này đã đăng ký nhận Voucher quá số lần quy định vui lòng xem lại thể lệ chương trình.";
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(Model))
            {
                data.Status = -1;
                data.Message = "Model sản phẩm không đúng.";
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            var voucher = DB.VOUCHER_API.FirstOrDefault(a => a.Status == 0 && a.Usephone == null && a.Type == 3);
            if (voucher != null)
            {
                //sent code DMX
                string authen = Login_DMX();
                if (string.IsNullOrEmpty(authen))
                {
                    data.Status = -1;
                    data.Message = "Không đăng nhập được với DMX";
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                var size = Model.Substring(0, 2);
                string amount = "";
                string model = Model.Substring(2, 4);
                string model_to_dmx = "";
                if (Model == "50Q636"|| Model== "43Q636")
                {
                    amount = "1,000,000";
                    model_to_dmx = "T636";
                }
                else if (Model == "55Q636" || Model == "65Q636"||Model== "55C835"||Model== "65C835")
                {
                    amount = "2,000,000";
                    model_to_dmx = "T835";
                }
                var insert_model = new InsertCoupon()
                {
                    Authen = authen,
                    DiscountCode = voucher.CODE,
                    SMSProductCode = model_to_dmx,
                    StoreID = 0,
                    //CustomerPhone = phone
                };
                int insert_status = ActiveCode_DMX(insert_model);
                if (insert_status == -1)
                {
                    data.Status = -1;
                    data.Message = "Không gửi được CODE sang DMX";
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                voucher.SIZE = size;
                voucher.MODEL = model;
                voucher.Status = 1;
                voucher.Usephone = phone;
                voucher.Cusname = Cusname;
                voucher.Agent = "DMX";
                voucher.Usedate = DateTime.Now;
                DB.Entry(voucher).State = EntityState.Modified;

                string msg = "TCL gui quy khach Ma voucher code {0} tri gia {1} Quy khach co the su dung ma code tai cua hang DMX toan quoc de duoc giam gia khi mua QLED {2}. Ma giam co hieu luc 07 ngay ke tu ngay nhan. The le xem: https://sieusaletcl.tcl.com/dmx hoac LH: 028 3838 3922 (EXT: 498)";
                //gửi brandname cho khách hàng
                int result = SendBrandName.SentMsg(phone, string.Format(msg, voucher.CODE, amount, voucher.MODEL));
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
                data.Message = "Quý khách đã đăng ký nhận Voucher thành công mã sẽ được gửi về SĐT đăng ký trong ít phút. Chi tiêt liên hệ: số 028 3838 3922 (nhánh: 498) đề được hướng dẫn, giải đáp.";
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

        public string Login_DMX()
        {
            try
            {
                //var url = "https://betaapipartner.thegioididong.com/api/Login/Login";
                var url = "https://apipartner.thegioididong.com/api/Login/Login";
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                httpRequest.Method = "POST";
                httpRequest.ContentType = "application/json";
                var data = @"{""userName"":""tcl"",""password"":""2@22m^y!#tcl""}";
                //var data = "userName=tcl&password=YRY6HFDuwH5jMGHt@";

                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                var response = new Response();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    logger.Info(string.Format("LOGIN DMX {0}", result));
                    response = JsonConvert.DeserializeObject<Response>(result);
                }
                Console.WriteLine(httpResponse.StatusCode);
                if (response.IsError == false)
                {
                    return response.Certificatekey;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return "";
            }
        }
        public int ActiveCode_DMX(InsertCoupon model)
        {
            try
            {
                //var url = "https://betaapipartner.thegioididong.com/api/TCL/InsertCoupon";
                var url = "https://apipartner.thegioididong.com/api/TCL/InsertCoupon";
                logger.Info(string.Format("URL REQUEST {0}", url));

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                httpRequest.Method = "POST";
                httpRequest.ContentType = "application/json";

                var data = JsonConvert.SerializeObject(model);
                logger.Info(data);

                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                var response = new Response();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    response = JsonConvert.DeserializeObject<Response>(result);
                    logger.Info(string.Format("INSERT COUPON {0}", result));
                }
                Console.WriteLine(httpResponse.StatusCode);
                if (response.IsError == false)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return -1;
            }
        }
        [Serializable]
        public class InsertCoupon
        {
            public string Authen { get; set; }
            public string DiscountCode { get; set; }
            public string SMSProductCode { get; set; }
            public int StoreID { get; set; }
            public string CustomerPhone { get; set; }
        }
        public class Response
        {
            public bool IsError { get; set; }
            public string Status { get; set; }
            public string Message { get; set; }
            public string Certificatekey { get; set; }
            public int ExpiredToken { get; set; }
        }
    }
}
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using TCLPromotion.Models;
using TCLPromotion.Utils;

namespace TCLPromotion.API
{
    [RoutePrefix("api/voucher")]
    public class VoucherController : ApiController
    {
        public TCLPromotionEntities DB = null;
        Logger logger = LogManager.GetCurrentClassLogger();
        private string CODE_DMCL = "caophong";
        public VoucherController()
        {
            DB = new TCLPromotionEntities();
        }
        //https://localhost:44302/api/voucher/getcode?KEY=DMCL&code=AAAAAA1
        [Route("getcode")]
        [HttpGet]
        public HttpResponseMessage GetCode(String KEY, String Code, String Model, String Size)
        {
            logger.Info(string.Format("[API Get Code] @Customer={0} {1} {2} {3}", KEY, Code, Model, Size));
            var result = new Result();
            var user = DB.AspNetUsers.FirstOrDefault(a => a.UserName == KEY);
            var check_user = DB.AspNetUsers.FirstOrDefault(a => a.UserName == CODE_DMCL);
            var dem_user = DB.VOUCHER_API.Where(a => a.Activeby == User.Identity.Name && a.Type == Common.TYPE_PROJECT).ToList();
            int? tong_so = (check_user.MAX_ACTIVE != null) ? check_user.MAX_ACTIVE : 0;
            if (tong_so <= dem_user.Count())
            {
                result.status = -1;
                result.message = "Đại lý đã hết lượt kích hoạt ";
                result.data = new List<string>();
                return ResponseMessage(result);
            }
            if (string.IsNullOrEmpty(KEY))
            {
                result.status = -1;
                result.message = "KEY không đúng";
                result.data = new List<string>();
                return ResponseMessage(result);
            }
            var voucher = DB.VOUCHER_API.FirstOrDefault(a => a.CODE == Code && a.Type == Common.TYPE_PROJECT);
            if (voucher != null)
            {
                if (voucher.Status == 1)
                {
                    var date_now = DateTime.Now;
                    var date_limited = voucher.Usedate.Value.AddHours(24);
                    if (date_now > date_limited)
                    {
                        result.status = -1;
                        result.message = "mã đã quá hạn sử dụng";
                        result.data = new List<string>();
                        return ResponseMessage(result);
                    }
                    if(Model== "C735")
                    {
                        if (Size.Contains("65"))
                        {
                            var model = new ResponseCode()
                            {
                                Code = voucher.CODE,
                                Model = Model,
                                Size = Size,
                                Price = 1000000
                            };
                            var response = new ResultGetCode();
                            response.status = 1;
                            response.message = "mã hợp lệ giá 1,000,000 đ";
                            response.data = new List<ResponseCode>() { model };
                            return ResponseMessage(response);
                        }else if (Size.Contains("98"))
                        {
                            var model = new ResponseCode()
                            {
                                Code = voucher.CODE,
                                Model = Model,
                                Size = Size,
                                Price = 5000000
                            };
                            var response = new ResultGetCode();
                            response.status = 1;
                            response.message = "mã hợp lệ giá 5,000,000 đ";
                            response.data = new List<ResponseCode>() { model };
                            return ResponseMessage(response);
                        }
                        else
                        {
                            result.status = 0;
                            result.message = "size không đúng";
                            result.data = new List<string>();
                        }
                    }
                    else
                    {
                        result.status = 0;
                        result.message = "model không đúng";
                        result.data = new List<string>();
                    }
                }
                else if (voucher.Status == 2)
                {
                    result.status = 2;
                    result.message = "mã đã được sử dụng";
                    result.data = new List<string>();
                }
                else if (voucher.Status == 3)
                {
                    result.status = 3;
                    result.message = "mã đã hết hạn";
                    result.data = new List<string>();
                }
                else
                {
                    //mã chưa được gửi đi
                    result.status = 0;
                    result.message = "mã chưa được gửi đi";
                    result.data = new List<string>();

                }

            }
            else
            {
                result.status = -1;
                result.message = "mã không tồn tại trong hệ thống";
                result.data = new List<string>();
            }
            return ResponseMessage(result);

        }
        [Route("activecode")]
        [HttpPost]
        public HttpResponseMessage ActiveCode(String KEY, Request request)
        {
            string json = JsonConvert.SerializeObject(request);
            logger.Info(string.Format("[API Active] @Customer={0} {1}", KEY, json));
            var user = DB.AspNetUsers.FirstOrDefault(a => a.UserName == KEY);

            var result = new Result();
            var check_user = DB.AspNetUsers.FirstOrDefault(a => a.UserName == CODE_DMCL);
            var dem_user = DB.VOUCHER_API.Where(a => a.Activeby == User.Identity.Name && a.Type == Common.TYPE_PROJECT).ToList();
            int? tong_so = (check_user.MAX_ACTIVE != null) ? check_user.MAX_ACTIVE : 0;
            if (tong_so <= dem_user.Count())
            {
                result.status = -1;
                result.message = "Đại lý đã hết lượt kích hoạt ";
                result.data = new List<string>();
                return ResponseMessage(result);
            }
            if (string.IsNullOrEmpty(KEY))
            {
                result.status = -1;
                result.message = "KEY không đúng";
                result.data = new List<string>();
                return ResponseMessage(result);
            }            
            if (request.Phone.Length != 10)
            {
                result.status = -1;
                result.message = "số điện thoại không đúng";
                result.data = new List<string>();
                return ResponseMessage(result);
            }
            if (string.IsNullOrEmpty(request.CusName))
            {
                result.status = -1;
                result.message = "nhập tên khách hàng";
                result.data = new List<string>();
                return ResponseMessage(result);
            }
            if (string.IsNullOrEmpty(request.MODEL))
            {
                result.status = -1;
                result.message = "nhập MODEL sản phẩm";
                result.data = new List<string>();
                return ResponseMessage(result);
            }
            if (string.IsNullOrEmpty(request.SIZE))
            {
                result.status = -1;
                result.message = "nhập size sản phẩm";
                result.data = new List<string>();
                return ResponseMessage(result);
            }
            var voucher = DB.VOUCHER_API.FirstOrDefault(a => a.CODE == request.Code && a.Type == Common.TYPE_PROJECT);
            if (voucher == null)
            {
                result.status = -1;
                result.message = "mã không tồn tại trong hệ thống";
                result.data = new List<string>();
                return ResponseMessage(result);
            }

            if (voucher.Status == 1)
            {
                var date_now = DateTime.Now;
                var date_limited = voucher.Usedate.Value.AddHours(48);
                if (date_now > date_limited)
                {
                    result.status = -1;
                    result.message = "mã đã quá hạn sử dụng";
                    result.data = new List<string>();
                    return ResponseMessage(result);
                }
                if (request.MODEL == "C735")
                {
                    if (request.SIZE.Contains("65"))
                    {
                        //gạch mã này đi
                        voucher.Status = 2;

                        voucher.Activedate = DateTime.Now;
                        voucher.Activeby = KEY;
                        voucher.Cusname = request.CusName;
                        voucher.CCCD = request.CCCD;
                        voucher.Usephone = request.Phone;
                        voucher.MODEL = request.MODEL;
                        voucher.SIZE = request.SIZE;

                        voucher.Agent = CODE_DMCL;
                        DB.Entry(voucher).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();
                        result.status = 1;
                        result.message = "mã giảm giá 1,000,000 đ";
                        result.data = new List<string>();
                    }
                    else if (request.SIZE.Contains("98"))
                    {
                        //gạch mã này đi
                        voucher.Status = 2;

                        voucher.Activedate = DateTime.Now;
                        voucher.Activeby = KEY;
                        voucher.Cusname = request.CusName;
                        voucher.CCCD = request.CCCD;
                        voucher.Usephone = request.Phone;
                        voucher.MODEL = request.MODEL;
                        voucher.SIZE = request.SIZE;

                        voucher.Agent = CODE_DMCL;
                        DB.Entry(voucher).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();
                        result.status = 1;
                        result.message = "mã giảm giá 5,000,000 đ";
                        result.data = new List<string>();
                    }
                    else
                    {
                        result.status = 0;
                        result.message = "size không đúng";
                        result.data = new List<string>();
                    }
                }
                else
                {
                    result.status = 0;
                    result.message = "model không đúng";
                    result.data = new List<string>();
                }

            }
            else if (voucher.Status == 2)
            {
                result.status = 2;
                result.message = "mã đã được sử dụng";
                result.data = new List<string>();
            }
            else if (voucher.Status == 3)
            {
                result.status = 3;
                result.message = "mã đã hết hạn";
                result.data = new List<string>();
            }
            else
            {
                //mã chưa được gửi đi
                result.status = 0;
                result.message = "mã chưa được gửi đi";
                result.data = new List<string>();
            }
            return ResponseMessage(result);

        }
        [Route("activenewcode")]
        [HttpPost]
        public HttpResponseMessage ActiveNewCode(String KEY, Request request)
        {
            string json = JsonConvert.SerializeObject(request);
            logger.Info(string.Format("[API Active New] @Customer={0} {1}", KEY, json));
            var user = DB.AspNetUsers.FirstOrDefault(a => a.UserName == KEY);
            var result = new Result();
            result.status = -1;
            result.message = "chức năng này dùng kích hoạt khách hàng mới";
            result.data = new List<string>();
            return ResponseMessage(result);
            if (!string.IsNullOrEmpty(request.Code))
            {
                result.status = -1;
                result.message = "chức năng này dùng kích hoạt khách hàng mới";
                result.data = new List<string>();
                return ResponseMessage(result);
            }
            if (request.Phone.Length != 10)
            {
                result.status = -1;
                result.message = "số điện thoại không đúng";
                result.data = new List<string>();
                return ResponseMessage(result);
            }
            if (string.IsNullOrEmpty(request.CusName))
            {
                result.status = -1;
                result.message = "nhập tên khách hàng";
                result.data = new List<string>();
                return ResponseMessage(result);
            }
            if (string.IsNullOrEmpty(request.MODEL))
            {
                result.status = -1;
                result.message = "nhập MODEL sản phẩm";
                result.data = new List<string>();
                return ResponseMessage(result);
            }
            if (string.IsNullOrEmpty(request.SIZE))
            {
                result.status = -1;
                result.message = "nhập size sản phẩm";
                result.data = new List<string>();
                return ResponseMessage(result);
            }
            var voucher = DB.VOUCHER_API.FirstOrDefault(a => a.Status == 0 && a.Usephone == null && a.Type == Common.TYPE_PROJECT);
            if (voucher == null)
            {
                result.status = -1;
                result.message = "đã hết voucher hợp lệ trong hệ thống";
                result.data = new List<string>();
                return ResponseMessage(result);
            }
            else
            {
                if (request.MODEL == "C735")
                {
                    if (request.SIZE.Contains("65"))
                    {
                        //gạch mã này đi
                        voucher.Status = 2;

                        voucher.Activedate = DateTime.Now;
                        voucher.Activeby = KEY;
                        voucher.Sendby = KEY;

                        voucher.Usedate = DateTime.Now;
                        voucher.Cusname = request.CusName;
                        voucher.CCCD = request.CCCD;
                        voucher.Usephone = request.Phone;
                        voucher.MODEL = request.MODEL;
                        voucher.SIZE = request.SIZE;
                        voucher.Agent = KEY;

                        DB.Entry(voucher).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();
                        result.status = 1;
                        result.message = "mã giảm giá 1,000,000 đ";
                        result.data = new List<string>();
                    }
                    else if (request.SIZE.Contains("98"))
                    {
                        //gạch mã này đi
                        voucher.Status = 2;

                        voucher.Activedate = DateTime.Now;
                        voucher.Activeby = KEY;
                        voucher.Sendby = KEY;

                        voucher.Usedate = DateTime.Now;
                        voucher.Cusname = request.CusName;
                        voucher.CCCD = request.CCCD;
                        voucher.Usephone = request.Phone;
                        voucher.MODEL = request.MODEL;
                        voucher.SIZE = request.SIZE;
                        voucher.Agent = KEY;

                        DB.Entry(voucher).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();
                        result.status = 1;
                        result.message = "mã giảm giá 5,000,000 đ";
                        result.data = new List<string>();
                    }
                    else
                    {
                        result.status = 0;
                        result.message = "size không đúng";
                        result.data = new List<string>();
                    }
                }
                else
                {
                    result.status = 0;
                    result.message = "model không đúng";
                    result.data = new List<string>();
                }
            }
            return ResponseMessage(result);

        }
        private HttpResponseMessage ResponseMessage(Result result)
        {
            string json = JsonConvert.SerializeObject(result);
            var response = new HttpResponseMessage();
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            logger.Info(json);
            return response;
        }
        private HttpResponseMessage ResponseMessage(ResultGetCode result)
        {
            string json = JsonConvert.SerializeObject(result);
            var response = new HttpResponseMessage();
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");

            logger.Info(json);
            return response;
        }
    }
    public class Request
    {
        public string CusName { get; set; }
        public string Phone { get; set; }
        public string CCCD { get; set; }
        public string Code { get; set; }
        public string Product { get; set; }
        public string MODEL { get; set; }
        public string SIZE { get; set; }
    }
    public class Result
    {
        public int status { get; set; }
        public string message { get; set; }
        public List<String> data { get; set; }
    }
    public class ResultGetCode
    {
        public int status { get; set; }
        public string message { get; set; }
        public List<ResponseCode> data { get; set; }
    }
    public class ResponseCode
    {
        public string Code { get; set; }
        public string Model { get; set; }
        public string Size { get; set; }
        public int Price { get; set; }
    }

}
using NLog;
using OfficeOpenXml;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCLPromotion.Models;

namespace TCLPromotion.Areas.Admin.Controllers
{
    public class CustomerHailocController : BaseController
    {
        static IEnumerable<Customer> data = null;
        static Logger logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(int? page, string textSearch, string status, string chanel, string from_date, string to_date)
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
            data = model as IEnumerable<Customer>;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(data.OrderByDescending(a => a.Createdate).ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Topup(long? Id)
        {
            var customer = DB.Customers.Find(Id);
            if (customer == null)
            {
                return View();
            }
            string provider = Utils.TOPUP.getMobileOperator(customer.PhoneUse);
            ViewBag.provider = provider;
            return View(customer);
        }

        [HttpPost]
        public ActionResult Topup(long? Id, string PhoneUse, string Provider, int Amount)
        {
            string msg = "Topup không thành công ";
            var customer = DB.Customers.Find(Id);
            try
            {
                logger.Info(string.Format("TOPUP @ID{0} @Phone{1} @Provider{2} @Amount{3} @User{4}", Id, PhoneUse, Provider, Amount, User.Identity.Name));
                int amount = 0;
                if (Amount == 100)
                {
                    amount = 100000;
                }
                else if (Amount == 200)
                {
                    amount = 200000;
                }
                else if (Amount == 300)
                {
                    amount = 300000;
                }
                else if (Amount == 500)
                {
                    amount = 500000;
                }
                else if (Amount == 1000)
                {
                    amount = 1000000;
                }
                else
                {
                    amount = 0;
                }
                string TOPUP = Utils.TOPUP.TopuptoUserId(PhoneUse, amount.ToString(), Provider);
                logger.Info(TOPUP);

                var log_topup = new LogTopup()
                {
                    CusID = Id,
                    Amount = amount,
                    Phone = PhoneUse,
                    Provider = Provider,
                    Createdate = DateTime.Now,
                    Result = TOPUP
                };
                DB.LogTopups.Add(log_topup);
                DB.SaveChanges();

                string[] words = TOPUP.Split('|');
                string data = words[0];
                if (data == "0")
                {
                    customer.TOPUP = true;
                    DB.Entry(customer).State = EntityState.Modified;
                    DB.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    msg = msg + TOPUP;
                }


            }
            catch (Exception ex)
            {
                msg = msg + ex.Message;
            }

            string provider = Utils.TOPUP.getMobileOperator(customer.PhoneUse);
            ViewBag.provider = provider;
            ViewBag.msg = msg;
            return View(customer);
        }
        public void TCL_KHACHHANG()
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
            Sheet.Cells["A1"].Value = "stt";
            Sheet.Cells["B1"].Value = "họ tên";
            Sheet.Cells["C1"].Value = "số điện thoại";
            Sheet.Cells["D1"].Value = "số nhận thưởng";
            Sheet.Cells["E1"].Value = "quan hệ";
            Sheet.Cells["F1"].Value = "ngày tạo";
            Sheet.Cells["G1"].Value = "IMEI";
            Sheet.Cells["H1"].Value = "hóa đơn";
            Sheet.Cells["I1"].Value = "giải thưởng";
            Sheet.Cells["J1"].Value = "model";
            Sheet.Cells["K1"].Value = "size";
            Sheet.Cells["L1"].Value = "nvbh";

            int index = 1;
            int row = 2;
            var list = data.ToList();
            foreach (var item in data.ToList())
            {
                item.INVOICE = item.INVOICE.Replace('|', '\n');

                Sheet.Cells[string.Format("A{0}", row)].Value = index++;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.Name;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.Phone;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.PhoneUse;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.ChangePeople;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.Createdate.ToString();
                Sheet.Cells[string.Format("G{0}", row)].Value = item.IMEI;
                Sheet.Cells[string.Format("H{0}", row)].Value = item.INVOICE;
                Sheet.Cells[string.Format("I{0}", row)].Value = item.PAYMENT;
                Sheet.Cells[string.Format("J{0}", row)].Value = item.MODEL;
                Sheet.Cells[string.Format("K{0}", row)].Value = item.SIZE;
                Sheet.Cells[string.Format("L{0}", row)].Value = item.NVCH;
                row++;
            }


            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
    }
}
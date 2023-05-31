using OfficeOpenXml;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCLPromotion.Areas.Admin.Data;
using TCLPromotion.Models;

namespace TCLPromotion.Areas.Admin.Controllers
{
    public class CustomerNewController : BaseController
    {
        static IEnumerable<CustomerVoucher> data = null;
        public ActionResult Index(int? page, string textSearch, string status, string chanel, string from_date, string to_date)
        {
            var model = from a in DB.Customers
                        where a.Cate == 3
                        join b in DB.MADUTHUONGs on a.ID equals b.IDCusNew
                        select new CustomerVoucher()
                        {
                            ID = a.ID,
                            Phone = a.Phone,
                            PAYMENT = a.PAYMENT,
                            PRODUCT = a.PRODUCT,
                            BuyAdr = a.BuyAdr,
                            Createdate = a.Createdate,
                            Name = a.Name,
                            Address = a.Address,
                            IMEI = a.IMEI,
                            INVOICE = a.INVOICE,
                            VOUCHER = a.VOUCHER,
                            Voucher = b.Code
                        };
            if (!string.IsNullOrEmpty(textSearch))
            {
                model = model.Where(a => a.Name == textSearch || a.Phone == textSearch || a.Address == textSearch || a.VOUCHER == textSearch);
                ViewBag.textSearch = textSearch;
            }
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "1")
                {
                    model = model.Where(a => a.PAYMENT == "8");
                }
                else if(status == "2")
                {
                    model = model.Where(a => a.PAYMENT == "7" || a.PAYMENT == "6");
                }
                else
                {
                    model = model.Where(a => a.PAYMENT == "1" || a.PAYMENT == "2" || a.PAYMENT == "3" || a.PAYMENT == "4" || a.PAYMENT == "5");
                }
                
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
            data = model as IEnumerable<CustomerVoucher>;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(data.OrderByDescending(a => a.Createdate).ToPagedList(pageNumber, pageSize));
        }
        public void TCL_KHACHHANG()
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
            Sheet.Cells["A1"].Value = "stt";
            Sheet.Cells["B1"].Value = "họ tên";
            Sheet.Cells["C1"].Value = "số điện thoại";
            Sheet.Cells["D1"].Value = "địa chỉ";
            Sheet.Cells["E1"].Value = "ngày tạo";
            Sheet.Cells["F1"].Value = "IMEI";
            Sheet.Cells["G1"].Value = "hóa đơn";
            Sheet.Cells["H1"].Value = "giải thưởng";
            Sheet.Cells["I1"].Value = "mã dự thưởng";

            int index = 1;
            int row = 2;
            var list = data.ToList();
            foreach (var item in data.ToList())
            {
                item.INVOICE = item.INVOICE.Replace('|', '\n');

                Sheet.Cells[string.Format("A{0}", row)].Value = index++;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.Name;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.Phone;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.Address;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.Createdate.ToString();
                Sheet.Cells[string.Format("F{0}", row)].Value = item.IMEI;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.INVOICE;
                Sheet.Cells[string.Format("H{0}", row)].Value = GetPayment(item.PAYMENT);
                Sheet.Cells[string.Format("I{0}", row)].Value = item.Voucher;
                row++;
            }


            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }

        string GetPayment(string pay)
        {
            if (pay == "8")
            {
                return "Giải Nhất";
            }
            else if (pay == "7")
            {
                return "Giải Nhì";
            }
            else if (pay == "6")
            {
                return "Giải Nhì";
            }
            else
            {
                return "Không trúng";
            }
        }
    }
}
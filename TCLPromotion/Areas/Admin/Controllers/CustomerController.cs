
using OfficeOpenXml;
using PagedList;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCLPromotion.Models;

namespace TCLPromotion.Areas.Admin.Controllers
{
    public class CustomerController : BaseController
    {
        static IEnumerable<Customer> data = null;
        public ActionResult Index(int? page, string textSearch, string status, string chanel, string from_date, string to_date)
        {
            var model = from a in DB.Customers
                        where a.Cate == 1
                        select a;
            if (!string.IsNullOrEmpty(textSearch))
            {
                model = model.Where(a => a.Name == textSearch || a.Phone == textSearch || a.Address == textSearch || a.VOUCHER == textSearch);
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

        public void TCL_KHACHHANG()
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
            Sheet.Cells["A1"].Value = "stt";
            Sheet.Cells["B1"].Value = "họ tên";
            Sheet.Cells["C1"].Value = "số điện thoại";
            Sheet.Cells["D1"].Value = "địa chỉ";
            Sheet.Cells["E1"].Value = "cmnd/cccd";
            Sheet.Cells["F1"].Value = "ngày tạo";
            Sheet.Cells["G1"].Value = "thành phố";
            Sheet.Cells["H1"].Value = "sản phẩm";
            Sheet.Cells["I1"].Value = "model";
            Sheet.Cells["J1"].Value = "";
            Sheet.Cells["K1"].Value = "hóa đơn";
            Sheet.Cells["L1"].Value = "size";
            Sheet.Cells["M1"].Value = "giải thưởng";
            Sheet.Cells["N1"].Value = "mã voucher";

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
                Sheet.Cells[string.Format("E{0}", row)].Value = item.CCCD;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.Createdate.ToString();
                Sheet.Cells[string.Format("G{0}", row)].Value = item.BuyAdr;
                Sheet.Cells[string.Format("H{0}", row)].Value = item.PRODUCT;
                Sheet.Cells[string.Format("I{0}", row)].Value = item.MODEL;
                Sheet.Cells[string.Format("J{0}", row)].Value = "";
                Sheet.Cells[string.Format("K{0}", row)].Value = item.INVOICE;
                Sheet.Cells[string.Format("L{0}", row)].Value = item.SIZE;
                Sheet.Cells[string.Format("M{0}", row)].Value = GetPayment(item.PAYMENT);
                Sheet.Cells[string.Format("N{0}", row)].Value = item.VOUCHER;
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
            if (pay == "2")
            {
                return "Giải 200K";
            }
            else if (pay == "1")
            {
                return "Giải 100K";
            }
            else
            {
                return "May mắn";
            }
        }
    }
}
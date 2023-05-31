using OfficeOpenXml;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCLPromotion.Models;

namespace TCLPromotion.Areas.Admin.Controllers
{
    public class CustomerNew1Controller : BaseController
    {
        static IEnumerable<Customer> data = null;
        public ActionResult Index(int? page, string textSearch, string status, string chanel, string from_date, string to_date)
        {
            var model = from a in DB.Customers
                        where a.Cate == 2
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
            Sheet.Cells["E1"].Value = "ngày tạo";
            Sheet.Cells["F1"].Value = "IMEI";
            Sheet.Cells["G1"].Value = "hóa đơn";
            Sheet.Cells["H1"].Value = "giải thưởng";

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
                Sheet.Cells[string.Format("H{0}", row)].Value = item.PAYMENT;
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
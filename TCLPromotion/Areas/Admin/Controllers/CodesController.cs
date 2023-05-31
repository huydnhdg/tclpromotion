using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TCLPromotion.Models;

namespace TCLPromotion.Areas.Admin.Controllers
{
    public class CodesController : Controller
    {
      
        private TCLPromotionEntities db = new TCLPromotionEntities();

        // GET: Admin/Codes
        public ActionResult Index(int? page, string textSearch, string status, string chanel, string from_date, string to_date)
        {
            var model = from a in db.Codes
                        select a;
            if (!string.IsNullOrEmpty(textSearch))
            {
                model = model.Where(a => a.Model_size == textSearch || a.Price_topup == textSearch );
                ViewBag.textSearch = textSearch;
            }
            if (!string.IsNullOrEmpty(status))
            {
                model = model.Where(a => a.Price_topup == status);
                ViewBag.status = status;
            }
            if (!string.IsNullOrEmpty(chanel))
            {

            }

          
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(model.OrderByDescending(a => a.Price_topup).ToPagedList(pageNumber, pageSize));
        }

        public void TCL_MA()
        {
            var model = from a in db.Codes
                        select a;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
            Sheet.Cells["A1"].Value = "STT";
            Sheet.Cells["B1"].Value = "MÃ TV";
            Sheet.Cells["C1"].Value = "SỐ TIỀN";
         

            int index = 1;
            int row = 2;
            var list = model.ToList();
            foreach (var item in model.ToList())
            {
                

                Sheet.Cells[string.Format("A{0}", row)].Value = index++;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.Model_size;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.Price_topup;
     
                row++;
            }


            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

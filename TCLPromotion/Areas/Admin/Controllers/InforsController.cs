using Newtonsoft.Json;
using NLog;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TCL_Voucher.Utils;
using TCLPromotion.Models;

namespace TCLPromotion.Areas.Admin.Controllers
{
    public class InforsController : Controller
    {
        private TCLPromotionEntities db = new TCLPromotionEntities();
        static IEnumerable<Infor> data = null;
        public ActionResult Index(int? page, string textSearch, string status, string chanel, string from_date, string to_date)
        {
            var model = from a in db.Infors
                       
                        select a;
            if (!string.IsNullOrEmpty(textSearch))
            {
                model = model.Where(a => a.FullName == textSearch || a.Phone == textSearch || a.Model_size == textSearch );
                ViewBag.textSearch = textSearch;
            }
            if (!string.IsNullOrEmpty(status))
            {
                model = model.Where(a => a.Status == status);
                ViewBag.status = status;
            }
            if (!string.IsNullOrEmpty(chanel))
            {

            }
            if (!string.IsNullOrEmpty(from_date))
            {
                DateTime d = DateTime.Parse(from_date);
                model = model.Where(a => a.Create_date >= d);
                ViewBag.from_date = from_date;
            }
            if (!string.IsNullOrEmpty(to_date))
            {
                DateTime d = DateTime.Parse(to_date);
                model = model.Where(a => a.Create_date < d);
                ViewBag.to_date = to_date;
            }
            data = model as IEnumerable<Infor>;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(data.OrderByDescending(a => a.Create_date).ToPagedList(pageNumber, pageSize));
        }
        public void TCL_KHACHHANG()
        {
            var model = from a in db.Infors

                        select a;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
            Sheet.Cells["A1"].Value = "STT";
            Sheet.Cells["B1"].Value = "Họ tên";
            Sheet.Cells["C1"].Value = "Số điện thoại";
            Sheet.Cells["D1"].Value = "Mã TV";
            Sheet.Cells["E1"].Value = "Tình trạng";
            Sheet.Cells["F1"].Value = "Ngày tạo";
            Sheet.Cells["G1"].Value = "Ảnh hóa đơn";
            Sheet.Cells["H1"].Value = "Địa chỉ";


            int index = 1;
            int row = 2;
            var list = model.ToList();
            foreach (var item in model.ToList())
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = index++;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.FullName;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.Phone;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.Model_size;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.Status;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.Create_date.ToString();
                Sheet.Cells[string.Format("G{0}", row)].Value = item.Image_Order;
                Sheet.Cells[string.Format("H{0}", row)].Value = item.Address;
            
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


        public ActionResult EditStatus(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Infor infor = db.Infors.Find(Id);
            if (infor == null)
            {
                return HttpNotFound();
            }
            return View(infor);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStatusConfirm([Bind(Include = "")] Infor infor)
        {
           
            if (ModelState.IsValid)
            {
                string msg_tc = "Thong tin dang ky qua tang loa S522W cua quy khach hop le. TCL tien hanh gui qua tang den dia chi dang ky nhan thuong trong vong 7-15 ngay lam viec. Moi thac mac lien he 028 3836 6111 (nhanh: 498) de duoc huong dan, giai dap.";
                string msg_er = "Thong tin dang ky qua tang loa S522W cua quy khach khong hop le. Han chot bo sung ho so: 10/05/2023. Moi thac mac lien he 028 3836 6111 (nhanh: 498) de duoc huong dan, giai dap.";
                var _infor  = db.Infors.Find(infor.Id);
                _infor.Status = infor.Status;
                db.Entry(_infor).State = EntityState.Modified;
                if (_infor.Status == "Đã trả thưởng")
                {
                    SendBrandName.SentMsg(infor.Phone, msg_tc);
                    _infor.Message = msg_tc;
                    db.Entry(_infor).State = EntityState.Modified;
                }    else if(_infor.Status =="Lỗi thông tin")
                {
                    SendBrandName.SentMsg(infor.Phone, msg_er);
                    _infor.Message = msg_er;
                    db.Entry(_infor).State = EntityState.Modified;
                }    else
                {
                    _infor.Message = "";
                    db.Entry(_infor).State = EntityState.Modified;
                }    
                db.SaveChanges();
           
                return RedirectToAction("Index");
            }
          
            return RedirectToAction("Index");

        }
    }
}

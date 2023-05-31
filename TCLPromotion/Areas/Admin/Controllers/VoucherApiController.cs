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
    public class VoucherApiController : BaseController
    {

        static IEnumerable<VOUCHER_API> data = null;
        public ActionResult Index(int? page, string textSearch, string status, string chanel, string from_date, string to_date)
        {
            var model = from a in DB.VOUCHER_API
                        where a.Type != 23
                        select a;
            if (!string.IsNullOrEmpty(textSearch))
            {
                model = model.Where(a => a.CODE == textSearch || a.Activeby == textSearch || a.Usephone == textSearch || a.Cusname == textSearch);
                ViewBag.textSearch = textSearch;
            }
            if (!string.IsNullOrEmpty(status))
            {
                int s = int.Parse(status);
                model = model.Where(a => a.Status == s);
                ViewBag.status = status;
            }
            if (!string.IsNullOrEmpty(chanel))
            {
                model = model.Where(a => a.Agent == chanel);
                ViewBag.chanel = chanel;
            }
            if (!string.IsNullOrEmpty(from_date))
            {
                DateTime d = DateTime.Parse(from_date);
                model = model.Where(a => a.Usedate >= d);
                ViewBag.from_date = from_date;
            }
            if (!string.IsNullOrEmpty(to_date))
            {
                DateTime d = DateTime.Parse(to_date);
                var dend = d.AddDays(1);
                model = model.Where(a => a.Usedate < dend);
                ViewBag.to_date = to_date;
            }
            ViewBag.agent = from a in DB.AspNetUsers
                            from b in a.AspNetRoles
                            where b.Id == "Agent"
                            select a;
            data = model as IEnumerable<VOUCHER_API>;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(data.OrderByDescending(a => a.Activedate).ToPagedList(pageNumber, pageSize));
        }
        public void TCL_VOUCHER()
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
            Sheet.Cells["A1"].Value = "stt";
            Sheet.Cells["B1"].Value = "ngày tạo";
            Sheet.Cells["C1"].Value = "mã voucher";
            Sheet.Cells["D1"].Value = "trạng thái";
            Sheet.Cells["E1"].Value = "kích hoạt";
            Sheet.Cells["F1"].Value = "gửi đến";
            Sheet.Cells["G1"].Value = "ngày sử dụng";
            Sheet.Cells["H1"].Value = "khách hàng";
            Sheet.Cells["I1"].Value = "sản phẩm";
            Sheet.Cells["J1"].Value = "đại lý";

            int index = 1;
            int row = 2;
            var list = data.ToList();
            foreach (var item in data.ToList())
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = index++;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.Createdate.ToString();
                Sheet.Cells[string.Format("C{0}", row)].Value = item.CODE;
                Sheet.Cells[string.Format("D{0}", row)].Value = GetStatus(item.Status);
                Sheet.Cells[string.Format("E{0}", row)].Value = item.Activedate.ToString();
                Sheet.Cells[string.Format("F{0}", row)].Value = item.Activeby;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.Usedate.ToString();
                Sheet.Cells[string.Format("H{0}", row)].Value = item.Usephone + "\r\n" + item.Cusname + "\r\n" + item.CCCD;
                Sheet.Cells[string.Format("H{0}", row)].Style.WrapText = true;
                Sheet.Cells[string.Format("I{0}", row)].Value = item.MODEL + "\r\n" + item.SIZE;
                Sheet.Cells[string.Format("I{0}", row)].Style.WrapText = true;
                Sheet.Cells[string.Format("J{0}", row)].Value = item.Agent;
                row++;
            }


            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
        string GetStatus(int? status)
        {
            if (status == 0)
            {
                return "chưa active";

            }
            else if (status == 1)
            {
                return "đã gửi";
            }
            else if (status == 2)
            {
                return "đã sử dụng";
            }
            else if (status == 3)
            {
                return "quá hạn";
            }
            else
            {
                return "";
            }
        }

        public ActionResult UploadFile()
        {
            List<VOUCHER_API> list_product = new List<VOUCHER_API>();
            return View(list_product);
        }
        [HttpPost]
        public ActionResult UploadFile(FormCollection collection)
        {
            List<VOUCHER_API> list_product = new List<VOUCHER_API>();
            try
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;
                        for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                        {
                            string code;
                            string type;

                            try { code = workSheet.Cells[rowIterator, 1].Value.ToString(); } catch (Exception) { code = ""; }
                            try { type = workSheet.Cells[rowIterator, 2].Value.ToString(); } catch (Exception) { type = ""; }

                            //add thong tin rows vao product
                            var cate = new VOUCHER_API()
                            {
                                CODE = code,
                                Createdate = DateTime.Now,
                                Status = 0,
                                Type = int.Parse(type)
                            };
                            //check trung serial code
                            if (!string.IsNullOrEmpty(code))
                            {
                                var _cate = DB.VOUCHER_API.Where(a => a.CODE == code);
                                if (_cate.Count() == 0)
                                {
                                    DB.VOUCHER_API.Add(cate);
                                    DB.SaveChanges();
                                }
                            }
                            list_product.Add(cate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return View(list_product);
        }
    }
}
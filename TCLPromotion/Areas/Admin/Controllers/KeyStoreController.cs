using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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
    public class KeyStoreController : Controller
    {
        TCLPromotionEntities DB = new TCLPromotionEntities();
        public ActionResult UploadFile()
        {
            List<KeyStore> list_product = new List<KeyStore>();
            return View(list_product);
        }
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;


        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public ActionResult Index(int? page, string textSearch, string status, string chanel, string from_date, string to_date)
        {
            var model = from a in DB.KeyStores
                        select a;
            if (!string.IsNullOrEmpty(textSearch))
            {
                model = model.Where(a => a.Store == textSearch || a.Province == textSearch);
                ViewBag.textSearch = textSearch;
            }
            
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(model.OrderByDescending(a => a.Store).ToPagedList(pageNumber, pageSize));
        }
        [HttpPost]
        public ActionResult UploadFile(FormCollection collection)
        {
            List<KeyStore> list_product = new List<KeyStore>();
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
                            string province;
                            string store;
                            string address;

                            try { province = workSheet.Cells[rowIterator, 1].Value.ToString(); } catch (Exception) { province = ""; }
                            try { store = workSheet.Cells[rowIterator, 2].Value.ToString(); } catch (Exception) { store = ""; }
                            try { address = workSheet.Cells[rowIterator, 3].Value.ToString(); } catch (Exception) { address = ""; }

                            var voucher = new KeyStore()
                            {
                                Province = province,
                                Store = store,
                                Address = address
                            };
                            DB.KeyStores.Add(voucher);
                            DB.SaveChanges();
                            list_product.Add(voucher);
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
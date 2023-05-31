using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TCLPromotion.Models;

namespace TCLPromotion.Areas.Admin.Controllers
{
    public class UserController : BaseController
    {
        static IEnumerable<AspNetUser> data = null;
        public ActionResult Index(int? page, string textSearch, string status, string chanel, string from_date, string to_date)
        {
            var model = from a in DB.AspNetUsers
                        where a.AspNetRoles.FirstOrDefault().Id == "Agent"
                        select a;
            if (!string.IsNullOrEmpty(textSearch))
            {
                model = model.Where(a => a.UserName == textSearch || a.PhoneNumber == textSearch || a.MAX_ACTIVE.ToString() == textSearch);
            }
            if (!string.IsNullOrEmpty(status))
            {

            }
            if (!string.IsNullOrEmpty(chanel))
            {

            }
            if (!string.IsNullOrEmpty(from_date))
            {

            }
            if (!string.IsNullOrEmpty(to_date))
            {

            }
            data = model as IEnumerable<AspNetUser>;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(data.OrderBy(a => a.UserName).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Delete(string Id)
        {
            if (ModelState.IsValid)
            {
                var model = DB.AspNetUsers.Find(Id);
                DB.AspNetUsers.Remove(model);
                DB.SaveChanges();
            }
            // If we got this far, something failed, redisplay form
            return RedirectToAction("Index", "User");
        }
        public ActionResult Edit(string Id)
        {
            var model = DB.AspNetUsers.Find(Id);
            return View(model);
        }
        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] AspNetUser user)
        {
            try
            {
                var _user = DB.AspNetUsers.Find(user.Id);
                _user.MAX_ACTIVE = user.MAX_ACTIVE;
                DB.Entry(_user).State = System.Data.Entity.EntityState.Modified;
                DB.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View(user);
            }
        }
    }
}
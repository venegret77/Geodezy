using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using vmz.Models;

namespace vmz.Controllers
{
    public class HomeController : Controller
    {
        vmzEntities db = new vmzEntities();

        public ActionResult Index()
        {
            var cookie = Request.Cookies["user"];
            if (cookie != null)
            {
                int id = Convert.ToInt32(cookie.Value);
                var user = db.User.Where(u => u.id == id).ToList().FirstOrDefault();
                ViewBag.UserName = user.name;
                ViewBag.UserProfession = db.Profession.Where(p => p.id == user.professionid).ToList().FirstOrDefault().name;
                ViewBag.UserDescription = user.description;
                ViewBag.UserErrID = null;
                //Для админа
                if (ViewBag.UserProfession == "Администратор")
                    AdminContent();
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        [HttpPost]
        public ActionResult Index(AdmModel model)
        {
            var cookie = Request.Cookies["user"];
            if (cookie != null)
            {
                int id = Convert.ToInt32(cookie.Value);
                var _user = db.User.FirstOrDefault(u => u.id == id);
                ViewBag.UserName = _user.name;
                ViewBag.UserProfession = db.Profession.FirstOrDefault(p => p.id == _user.professionid).name;
                ViewBag.UserDescription = _user.description;
                ViewBag.UserErrID = null;
            }
            using (vmzEntities db = new vmzEntities())
            {
                int _uid = Convert.ToInt32(model.uid);
                var user = db.User.Find(_uid);
                var oldprof = db.Profession.Find(user.professionid);
                if (model.ConfProf == null)
                    model.ConfProf = db.Profession.FirstOrDefault(p => p.id == user.confprofid).name;
                var newprof = db.Profession.FirstOrDefault(p => p.name == model.ConfProf);
                if (model.submitButton == "1")
                {
                    if (newprof.name == "Администратор")
                    {
                        user.professionid = newprof.id;
                        user.confprofid = newprof.id;
                    }
                    else
                    {
                        if (oldprof.name == "Администратор")
                        {
                            var admid = db.Profession.FirstOrDefault(p => p.name == "Администратор").id;
                            var useradm = db.User.Where(u => u.professionid == admid).ToList();
                            if (useradm.Count() > 1)
                            {
                                user.professionid = newprof.id;
                                user.confprofid = newprof.id;
                            }
                            else
                            {
                                ViewBag.UserErrID = model.uid;
                                ModelState.AddModelError("Err", "Должен быть как минимум 1 администратор!");
                            }
                        }
                        else
                        {
                            user.professionid = newprof.id;
                            user.confprofid = newprof.id;
                        }
                    }
                }
                else if (model.submitButton == "0")
                {
                    user.confprofid = db.Profession.FirstOrDefault(p => p.name == "Не подтвержден").id;
                }
                db.SaveChanges();
            }
            if (ViewBag.UserProfession == "Администратор")
                AdminContent();
            return View(model);
        }
        private void AdminContent()
        {
            ViewBag.Prof = db.Profession.Where(p => p.name != "Не подтвержден").ToList();
            ViewBag.AllUsers = db.ListOfUsersAdm();
            ViewBag._AllUsers = db.ListOfUsersAdm();
        }
        public new ActionResult Profile()
        {
            var cookie = Request.Cookies["user"];
            if (cookie != null)
            {
                int id = Convert.ToInt32(cookie.Value);
                var user = db.User.FirstOrDefault(u => u.id == id);
                ViewBag.UserName = user.name;
                ViewBag.UserProfession = db.Profession.FirstOrDefault(p => p.id == user.professionid).name;
                ViewBag.UserDescription = user.description;
                return View();
            }
            else
                return RedirectToAction("Login", "Account");
        }
    }
}
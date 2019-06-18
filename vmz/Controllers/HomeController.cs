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
        Entities db = new Entities();
        #region Профиль
        public new ActionResult Profile()
        {
            if (FillViewBag(true))
                return View();
            else
                return RedirectToAction("Login", "Account");
        }
        #endregion
        #region Администратор
        public ActionResult Index()
        {
            bool test = FillViewBag();
            if (test & ViewBag.UserProfession == "Администратор")
                return View();
            else
                return RedirectToAction("Profile", "Home");
        }
        [HttpPost]
        public ActionResult Index(AdmModel model)
        {
            using (Entities db = new Entities())
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
            FillViewBag();
            return View(model);
        }
        #endregion
        #region Клиент
        public ActionResult Client()
        {
            if (FillViewBag() & ViewBag.UserProfession == "Клиент")
                return View();
            else
                return RedirectToAction("Profile", "Account");
        }
        #endregion
        private bool FillViewBag()
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
                switch(ViewBag.UserProfession)
                {
                    case "Администратор":
                        ViewBag.Prof = db.Profession.Where(p => p.name != "Не подтвержден").ToList();
                        ViewBag.AllUsers = db.ListOfUsersAdm().ToList();
                        ViewBag._AllUsers = db.ListOfUsersAdm().ToList();
                        break;
                    case "Клиент":
                        var Orders = db.Order.LeftJoin(
                            
                            );
                        var Orders = from o in db.Order
                                     join list in db.ListOfTasks on o.listoftasksid equals list.orderid
                                     join t in db.Task on list.taskid equals t.id
                                     where o.clientid == id
                                     select new
                                     {
                                         oid = o.id,
                                         oname = o.name,
                                         odes = o.description,
                                         osd = o.datestart,
                                         oed = o.datestart,
                                         opr = o.priority,
                                         tid = t.id,
                                         tname = t.name,
                                         tdes = t.description,
                                         tsd = t.datestart,
                                         ted = t.datestart
                                     };
                        ViewBag.Orders = Orders;
                        //ViewBag.Orders = db.Order.Where(o => o.clientid == id).ToList();
                        break;
                    default:
                        break;
                }
                return true;
            }
            return false;
        }
        private bool FillViewBag(bool b)
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
                return true;
            }
            return false;
        }
    }
}
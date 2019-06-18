﻿using System;
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
            if (FillViewBag(true) != -1)
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
                                FillViewBag();
                                return View(model);
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
                return RedirectToAction("Index", "Home");
            }
        }
        #endregion
        #region Клиент
        public ActionResult Client()
        {
            bool test = FillViewBag();
            if (test & ViewBag.UserProfession == "Клиент")
                return View();
            else
                return RedirectToAction("Profile", "Account");
        }
        [HttpPost]
        public ActionResult Client(CreateOrderModel model)
        {
            int id = FillViewBag(true);
            if (id != -1)
            {
                using (Entities db = new Entities())
                {
                    try
                    {
                        if (model.oid != null)
                        {
                            int oid = Convert.ToInt32(model.oid);
                            db.Order.Remove(db.Order.FirstOrDefault(o => o.id == oid));
                        }
                        else
                        {
                            if (model.datestart == null)
                                model.datestart = DateTime.Now;
                            db.Order.Add(new Order
                            {
                                datestart = model.datestart,
                                name = model.name,
                                description = model.description,
                                clientid = id,
                                priority = model.priority,
                            });
                        }
                        db.SaveChanges();
                        return RedirectToAction("Client", "Home");
                    }
                    catch
                    {
                        return RedirectToAction("Client", "Home");
                    }
                }
            }
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
                        var Orders = db.Order.Where(o => o.clientid == id).ToList();
                        List<OrderTask> _Orders = new List<OrderTask>();
                        foreach (var o in Orders)
                            _Orders.Add(new OrderTask(o));
                       
                        for (int i = 0; i < _Orders.Count; i++)
                        {
                            var task = from ord in db.Order
                                       join l in db.ListOfTasks on ord.listoftasksid equals l.orderid
                                       select l.Task;
                            foreach(var t in task)
                                _Orders[i].Tasks.Add(t);
                        }
                        _Orders = _Orders.OrderByDescending(o => o.Order.datestart).ToList();
                        ViewBag.Orders = _Orders;
                        break;
                    default:
                        break;
                }
                return true;
            }
            return false;
        }
        private int FillViewBag(bool b)
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
                return id;
            }
            return -1;
        }
    }
    public struct OrderTask
    {
        public OrderTask(Order order) : this()
        {
            this.Order = order;
        }

        public Order Order { get; set; }
        public List<Task> Tasks { get; set; }
    }
}
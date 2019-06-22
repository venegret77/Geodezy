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
        public new ActionResult Index()
        {
            if (FillViewBag(true) != -1)
                return View();
            else
                return RedirectToAction("Login", "Account");
        }
        [HttpPost]
        public new ActionResult Index(string description, string pass, string action, string uid)
        {
            int id = Convert.ToInt32(uid);
            if (action == "changedesc")
            {
                db.User.FirstOrDefault(u => u.id == id).description = description;
            }
            else if (action == "changepass")
            {
                var user = db.User.FirstOrDefault(u => u.id == id);
                using (MD5 md5Hash = MD5.Create())
                {
                    string source = "login:" + user.login + "###password:" + pass;
                    string hash = AccountController.GetMd5Hash(md5Hash, source);
                    user.md5 = hash;
                }
            }
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        #endregion
        #region Администратор
        public ActionResult Admin()
        {
            bool test = FillViewBag();
            if (test & ViewBag.UserProfession == "Администратор")
                return View();
            else
                return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public ActionResult Admin(AdmModel model)
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
                return RedirectToAction("Admin", "Home");
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
                return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public ActionResult Client(ClientModel model)
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
            return RedirectToAction("Index", "Account");
        }
        #endregion
        #region Секретарь
        public ActionResult Secretary()
        {
            bool test = FillViewBag();
            if (test & ViewBag.UserProfession == "Секретарь")
                return View();
            else
                return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public ActionResult Secretary(string orderid)
        {
            return RedirectToAction("Order", "Home", new { @id = orderid });
        }
        #endregion
        #region Бригадир
        public ActionResult Brigadier()
        {
            bool test = FillViewBag();
            if (test & ViewBag.UserProfession != null)
                return View();
            else
                return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public ActionResult Brigadier(BrigadeModel model)
        {
            int id = FillViewBag(true);
            if (id != -1)
            {
                using (Entities db = new Entities())
                {
                    /*try
                    {*/
                    switch (model.action)
                    {
                        case "addserv":
                            db.Service.Add(new Service
                            {
                                name = model.servicen,
                                description = model.serviced,
                            });
                            break;
                        case "deleteserv":
                            {
                                int idserv = Convert.ToInt32(model.slist[0]);
                                var test = db.JoinService.Where(j => j.serviceid == idserv).ToList();
                                if (test.Count() > 0)
                                {
                                    ModelState.AddModelError("Err", "Невозможно удалить услугу, т.к. она используется");
                                    FillViewBag();
                                    return View(model);
                                }
                                else
                                {
                                    db.Service.Remove(db.Service.FirstOrDefault(s => s.id == idserv));
                                }
                            }
                            break;
                        case "createbr":
                            db.Brigade.Add(new Brigade
                            {
                                brigadierid = id,
                                name = model.name,
                                description = model.description
                            });
                            db.SaveChanges();
                            var brigade = db.Brigade.FirstOrDefault(b => b.brigadierid == id);
                            foreach (var u in model.ulist)
                            {
                                int uid = Convert.ToInt32(u);
                                db.ListOfUsers.Add(new ListOfUsers
                                {
                                    brigadeid = brigade.id,
                                    userid = uid
                                });
                            }
                            foreach (var s in model.slist)
                            {
                                int sid = Convert.ToInt32(s);
                                JoinService js = new JoinService
                                {
                                    serviceid = sid
                                };
                                db.JoinService.Add(js);
                                db.SaveChanges();
                                int jsid = js.idjs;
                                db.JoinBrorTask.Add(new JoinBrorTask
                                {
                                    idbrortask = brigade.id,
                                    idjs = jsid
                                });
                            }
                            break;
                        case "deletebr":
                            {
                                var br = db.Brigade.FirstOrDefault(b => b.brigadierid == id);
                                var tasks = db.TaskToBrigade.Where(t => t.brigade == br.id).ToList();
                                //Доделать
                                if (tasks.Count() > 0)
                                {
                                    ModelState.AddModelError("Err", "Невозможно удалить бригаду");
                                    FillViewBag();
                                    return View(model);
                                }
                                else
                                {
                                    var jbt = db.JoinBrorTask.Where(j => j.idbrortask == br.id).ToList();
                                    foreach (var j in jbt)
                                    {
                                        db.JoinService.Remove(db.JoinService.FirstOrDefault(js => js.idjs == j.idjs));
                                        db.SaveChanges();
                                    }
                                    db.JoinBrorTask.RemoveRange(jbt);
                                    db.ListOfUsers.RemoveRange(db.ListOfUsers.Where(l => l.brigadeid == br.id).ToList());
                                    db.Brigade.Remove(br);
                                }
                            }
                            break;
                        case "changebr":
                            {
                                var br = db.Brigade.FirstOrDefault(b => b.brigadierid == id);
                                var jbt = db.JoinBrorTask.Where(j => j.idbrortask == br.id).ToList();
                                foreach (var j in jbt)
                                {
                                    db.JoinService.Remove(db.JoinService.FirstOrDefault(js => js.idjs == j.idjs));
                                    db.SaveChanges();
                                }
                                db.JoinBrorTask.RemoveRange(jbt);
                                db.ListOfUsers.RemoveRange(db.ListOfUsers.Where(l => l.brigadeid == br.id).ToList());
                                br.name = model.name;
                                br.description = model.description;
                                db.SaveChanges();
                                foreach (var u in model.ulist)
                                {
                                    int uid = Convert.ToInt32(u);
                                    db.ListOfUsers.Add(new ListOfUsers
                                    {
                                        brigadeid = br.id,
                                        userid = uid
                                    });
                                }
                                foreach (var s in model.slist)
                                {
                                    int sid = Convert.ToInt32(s);
                                    JoinService js = new JoinService
                                    {
                                        serviceid = sid
                                    };
                                    db.JoinService.Add(js);
                                    db.SaveChanges();
                                    int jsid = js.idjs;
                                    db.JoinBrorTask.Add(new JoinBrorTask
                                    {
                                        idbrortask = br.id,
                                        idjs = jsid
                                    });
                                }
                            }
                            break;
                        case "readytask":
                            {
                                int tid = Convert.ToInt32(model.tid);
                                var Task = db.Task.FirstOrDefault(t => t.id == tid);
                                Task.dateend = DateTime.Now;
                                Task.note = model.note;
                            }
                            break;
                    }
                    db.SaveChanges();
                    return RedirectToAction("Brigadier", "Home");
                    /*}
                    catch(Exception e)
                    {
                        return RedirectToAction("Brigadier", "Home");
                    }*/
                }
            }
            return RedirectToAction("Index", "Account");
        }
        #endregion
        #region Директор
        public ActionResult Director(DateTime? datestart, DateTime? dateend)
        {
            bool test = FillViewBag(datestart, dateend);
            if (test & ViewBag.UserProfession != null)
                return View();
            else
                return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Director(DateTime datestart, DateTime dateend)
        {
            return RedirectToAction("Director", "Home", new { @datestart = datestart, @dateend = dateend });
        }
        #endregion
        #region Заказ
        public ActionResult Order(string id)
        {
            if (id == null)
                return RedirectToAction("Index", "Home");
            bool test = FillViewBag(id);
            if (test & ViewBag.UserProfession == "Секретарь")
                return View();
            else
                return RedirectToAction("Secretary", "Home");
        }
        [HttpPost]
        public ActionResult Order(OrderModel model)
        {
            using (Entities db = new Entities())
            {
                switch (model.action)
                {
                    case "endorder":
                        {
                            int oid = Convert.ToInt32(model.oid);
                            db.Order.FirstOrDefault(o => o.id == oid).dateend = DateTime.Now;
                            db.SaveChanges();
                            return RedirectToAction("Secretary", "Home");
                        }
                    case "addtask":
                        {
                            int oid = Convert.ToInt32(model.oid);
                            Task task = new Task
                            {
                                datestart = model.datestart,
                                name = model.name,
                                description = model.description
                            };
                            db.Task.Add(task);
                            db.SaveChanges();
                            foreach (var s in model.slist)
                            {
                                int sid = Convert.ToInt32(s);
                                JoinService js = new JoinService
                                {
                                    serviceid = sid
                                };
                                db.JoinService.Add(js);
                                db.SaveChanges();
                                int jsid = js.idjs;
                                db.JoinBrorTask.Add(new JoinBrorTask
                                {
                                    idbrortask = task.id,
                                    idjs = jsid
                                });
                            }
                            db.ListOfTasks.Add(new ListOfTasks
                            {
                                orderid = oid,
                                taskid = task.id
                            });
                        }
                        break;
                    case "endtask":
                        {
                            int tid = Convert.ToInt32(model.tid);
                            var task = db.Task.FirstOrDefault(b => b.id == tid);
                            var jbt = db.JoinBrorTask.Where(j => j.idbrortask == task.id).ToList();
                            foreach (var j in jbt)
                            {
                                db.JoinService.Remove(db.JoinService.FirstOrDefault(js => js.idjs == j.idjs));
                                db.SaveChanges();
                            }
                            db.JoinBrorTask.RemoveRange(jbt);
                            db.ListOfTasks.Remove(task.ListOfTasks);
                            db.Task.Remove(task);
                        }
                        break;
                    case "setbr":
                        {
                            int tid = Convert.ToInt32(model.tid);
                            int bid = Convert.ToInt32(model.slist[0]);
                            var task = db.Task.FirstOrDefault(b => b.id == tid);
                            task.TaskToBrigade = new TaskToBrigade()
                            {
                                taskid = tid,
                                brigade = bid
                            };
                        }
                        break;
                }
                db.SaveChanges();
                return RedirectToAction("Order", "Home", new { @id = model.oid });
            }
        }
        #endregion
        #region Прочее
        private bool FillViewBag()
        {
            var cookie = Request.Cookies["user"];
            if (cookie != null)
            {
                int id = Convert.ToInt32(cookie.Value);
                var _user = db.User.FirstOrDefault(u => u.id == id);
                ViewBag.UserID = id;
                ViewBag.UserName = _user.name;
                ViewBag.UserProfession = db.Profession.FirstOrDefault(p => p.id == _user.professionid).name;
                ViewBag.UserDescription = _user.description;
                ViewBag.UserErrID = null;
                switch (ViewBag.UserProfession)
                {
                    case "Администратор":
                        {
                            ViewBag.Prof = db.Profession.ToList();
                            ViewBag.AllUsers = db.GetListOfUsersAdm().ToList();
                            ViewBag._AllUsers = db.GetListOfUsersAdm().ToList();
                        }
                        break;
                    case "Клиент":
                        {
                            var Orders = db.Order.Where(o => o.clientid == id).ToList();
                            List<OrderTask> _Orders = new List<OrderTask>();
                            foreach (var o in Orders)
                                _Orders.Add(new OrderTask(o));
                            for (int i = 0; i < _Orders.Count; i++)
                            {
                                int oid = _Orders[i].Order.id;
                                var tasks = db.ListOfTasks.Where(l => l.orderid == oid).ToList();
                                if (tasks.Count() != 0)
                                {
                                    foreach (var t in tasks)
                                        _Orders[i].Tasks.Add(t.Task);
                                }
                            }
                            ViewBag.Orders = _Orders.OrderByDescending(o => o.Order.datestart).ToList();
                        }
                        break;
                    case "Бригадир":
                        {
                            var Brigade = db.Brigade.FirstOrDefault(b => b.brigadierid == id);
                            if (Brigade != null)
                            {
                                int idbr = Brigade.id;
                                var ListOfUsers = db.ListOfUsers.Where(l => l.brigadeid == idbr).ToList();
                                ViewBag.Brigade = Brigade;
                                ViewBag.ListOfUsers = ListOfUsers;
                                ViewBag.ListOfServices = db.GetServicesOnBorT(idbr);
                                ViewBag.Tasks = db.TaskToBrigade.Where(t => t.brigade == Brigade.id).ToList().OrderByDescending(o => o.Task.dateend).ToList();
                            }
                            ViewBag.Workers = db.User.Where(u => (u.Profession.name == "Рабочий") & (u.ListOfUsers == null)).ToList();
                            ViewBag.Services = db.Service.ToList();
                        }
                        break;
                    case "Секретарь":
                        {
                            var Orders = db.Order.ToList();
                            List<OrderTask> _Orders = new List<OrderTask>();
                            foreach (var o in Orders)
                                _Orders.Add(new OrderTask(o));
                            for (int i = 0; i < _Orders.Count; i++)
                            {
                                int oid = _Orders[i].Order.id;
                                var tasks = db.ListOfTasks.Where(l => l.orderid == oid).ToList();
                                if (tasks.Count() != 0)
                                {
                                    foreach (var t in tasks)
                                        _Orders[i].Tasks.Add(t.Task);
                                }
                            }
                            ViewBag.Orders = _Orders.OrderByDescending(o => o.Order.datestart).ToList();
                        }
                        break;
                    default:
                        return false;
                }
                return true;
            }
            return false;
        }
        private int FillViewBag(bool b)
        {
            //Сделать список всех услуг и список свободных рабочих + тех кто уже есть в бригаде выделенным
            //При изменении бригады. Также подгружать действующие данные в элементы формы
            //row justify-content-center - элементы по центру
            var cookie = Request.Cookies["user"];
            if (cookie != null)
            {
                int id = Convert.ToInt32(cookie.Value);
                var _user = db.User.FirstOrDefault(u => u.id == id);
                ViewBag.UserName = _user.name;
                ViewBag.UserID = id;
                ViewBag.UserProfession = db.Profession.FirstOrDefault(p => p.id == _user.professionid).name;
                ViewBag.UserDescription = _user.description;
                ViewBag.UserErrID = null;
                return id;
            }
            return -1;
        }
        //Для заказа
        private bool FillViewBag(string orderid)
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
                int oid = Convert.ToInt32(orderid);
                OrderTask _Order = new OrderTask();
                _Order.Tasks = new List<Task>();
                if (db.Order.FirstOrDefault(o => o.id == oid) == null)
                    return false;
                _Order.Order = db.Order.FirstOrDefault(o => o.id == oid);
                List<List<GetServicesOnBorT_Result>> Services = new List<List<GetServicesOnBorT_Result>>();
                var tasks = db.ListOfTasks.Where(l => l.orderid == oid).ToList();
                List<bool> isBr = new List<bool>();
                List<Brigade> _brigades = new List<Brigade>();
                bool isReady = true;
                if (tasks.Count() != 0)
                {
                    _Order.Tasks = new List<Task>();
                    foreach (var t in tasks)
                    {
                        _Order.Tasks.Add(t.Task);
                        var ls = db.GetServicesOnBorT(t.taskid).ToList();
                        Services.Add(ls);
                        if (t.Task.TaskToBrigade == null)
                        {
                            isBr.Add(false);
                            _brigades.Add(new Brigade());
                        }
                        else
                        {
                            _brigades.Add(db.Brigade.FirstOrDefault(b => b.id == t.Task.TaskToBrigade.brigade));
                            isBr.Add(true);
                        }
                    }

                    foreach (var t in tasks)
                    {
                        if (t.Task.dateend == null)
                        {
                            isReady = false;
                            break;
                        }
                        else
                        {
                            isReady = true;
                        }
                    }
                }
                List<List<Service>> _Services = new List<List<Service>>();
                for (int i = 0; i < Services.Count; i++)
                {
                    _Services.Add(new List<Service>());
                    foreach (var s in Services[i])
                        _Services[i].Add(db.Service.FirstOrDefault(se => se.id == s.id));
                }
                List<List<Brigade>> _Brigades = new List<List<Brigade>>();
                var brigades = db.Brigade.ToList();
                for (int i = 0; i < _Order.Tasks.Count; i++)
                {
                    _Brigades.Add(new List<Brigade>());
                    _Brigades[i] = new List<Brigade>();
                    if (!isBr[i])
                    {
                        foreach (var b in brigades)
                        {
                            bool isgood = false;
                            foreach (var s in Services[i])
                            {
                                var sbr = db.GetServicesOnBorT(b.id).ToList();
                                if (sbr.Where(_sbr => _sbr.id == s.id).ToList().Count == 0)
                                {
                                    isgood = false;
                                    break;
                                }
                                else
                                {
                                    isgood = true;
                                }
                            }
                            if (isgood)
                                _Brigades[i].Add(b);
                        }
                    }
                }
                ViewBag.Order = _Order;
                ViewBag.Services = db.Service.ToList();
                ViewBag.TasksServ = _Services;
                ViewBag.TasksBrig = _Brigades;
                ViewBag.Brigades = _brigades;
                ViewBag.isBr = isBr;
                ViewBag.isReady = isReady;
                return true;
            }
            return false;
        }

        private bool FillViewBag(DateTime? datestart, DateTime? dateend)
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
                if (ViewBag.UserProfession == "Директор")
                {
                    ViewBag.datestart = datestart;
                    ViewBag.dateend = dateend;
                    var BrigadeTasks = db.GetBrigadesAndTasks().ToList();
                    var Orders = db.GetOrdersAndTasksCount().ToList();
                    var Tasks = db.GetTasksAndOName().ToList();
                    if ((datestart != null) & (dateend != null))
                    {
                        BrigadeTasks = db.GetBrigadesAndTasks().Where(
                            b => (b.tde <= dateend) & (b.tde <= dateend) & (b.tds <= dateend) & (b.tds <= dateend)).ToList().Where(
                            b => (b.tde >= datestart) & (b.tde >= datestart) & (b.tds >= datestart) & (b.tds >= datestart)).ToList();
                        Orders = db.GetOrdersAndTasksCount().Where(
                            b => (b.ode <= dateend) & (b.ode <= dateend) & (b.ods <= dateend) & (b.ods <= dateend)).ToList().Where(
                            b => (b.ode >= datestart) & (b.ode >= datestart) & (b.ods >= datestart) & (b.ods >= datestart)).ToList();
                        Tasks = db.GetTasksAndOName().Where(
                            b => (b.tde <= dateend) & (b.tde <= dateend) & (b.tds <= dateend) & (b.tds <= dateend)).ToList().Where(
                            b => (b.tde >= datestart) & (b.tde >= datestart) & (b.tds >= datestart) & (b.tds >= datestart)).ToList();
                    }
                    ViewBag.BrigadeTasks = BrigadeTasks;
                    var BrigadeInfo = from b in BrigadeTasks
                                      group b by b.bname
                                      into _b
                                      select new
                                      {
                                          _b.FirstOrDefault().bname,
                                          _b.FirstOrDefault().bfio,
                                          _b.FirstOrDefault().cworkers,
                                          isnull = _b.Where(bb => bb.tde == null).ToList().Count(),
                                          isnotnull = _b.Where(bb => bb.tde != null).ToList().Count(),
                                          sumhours = _b.Where(bb => bb.tde != null).ToList().Sum(bbb => bbb.tddif)
                                      };
                    List<BrigadeInfo> _BrigadeInfo = new List<BrigadeInfo>();
                    foreach (var b in BrigadeInfo)
                    {
                        _BrigadeInfo.Add(new Controllers.BrigadeInfo
                        {
                            bname = b.bname,
                            bfio = b.bfio,
                            cworkers = b.cworkers,
                            isnotnull = b.isnotnull,
                            isnull = b.isnull,
                            sumhours = b.sumhours
                        });
                    }
                    ViewBag.BrigadeInfo = _BrigadeInfo;
                    ViewBag.Orders = Orders;
                    ViewBag.Tasks = Tasks;
                    var TasksCount = from t in Tasks
                                     group t by t.oname
                               into _t
                                     select new
                                     {
                                         name = _t.FirstOrDefault().oname,
                                         _count = _t.Where(ta => ta.tde != null).ToList().Count(),
                                         count = _t.Count()
                                     };
                    List<TasksCount> _TasksCount = new List<TasksCount>();
                    foreach (var tc in TasksCount)
                    {
                        _TasksCount.Add(new Controllers.TasksCount
                        {
                            name = tc.name,
                            count = tc.count,
                            _count = tc._count,
                            procent = Convert.ToInt32(Convert.ToDouble(tc._count)/ Convert.ToDouble(tc.count) * 100d).ToString() + "%"
                        });
                    }
                    ViewBag.TasksCount = _TasksCount;
                }
                return true;
            }
            return false;
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using vmz.Models;

namespace vmz.Controllers
{
    [RoutePrefix("Account")]
    public class AccountController : Controller
    {
        Entities db = new Entities();
        public ActionResult Registration()
        {
            ViewBag.Prof = db.Profession.Where(p => p.name != "Не подтвержден").ToList();
            if (Request.Cookies["user"] != null)
                return RedirectToAction("Index", "Home");
            else
                return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration(AccountRegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = null;

                //Проверяем существует ли такой пользователь в БД
                user = db.User.FirstOrDefault(u => u.login == model.Login);

                //Если такого пользователя нет, то заносим его в БД
                if (user == null)
                {
                    if (model.Password == model.Password2)
                    {
                        using (MD5 md5Hash = MD5.Create())
                        {
                            string source = "login:" + model.Login + "###password:" + model.Password;
                            string hash = GetMd5Hash(md5Hash, source);
                            db.User.Add(new Models.User()
                            {
                                login = model.Login,
                                name = model.Surname + " " + model.Name + " " + model.Secondname,
                                md5 = hash,
                                professionid = 10,
                                confprofid = db.Profession.Where(p => p.name == model.ConfProf).FirstOrDefault().id
                            });
                            db.SaveChanges();
                            user = db.User.Where(u => u.login == model.Login).FirstOrDefault();
                        }
                        //Если пользователь успешно добавлен - возвращаемся
                        if (user != null)
                        {
                            ModelState.AddModelError("Reg", "Спасибо за регистрацию " + user.name);
                            HttpCookie cookie = new HttpCookie("user")
                            {
                                Name = "user",
                                Value = user.id.ToString(),
                                Expires = DateTime.Now.AddDays(7),
                            };
                            Response.SetCookie(cookie);
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Err", "Пароли не совпадают");
                    }
                }
                else
                {
                    ModelState.AddModelError("Err", "Пользователь с таким логином уже существует");
                }
            }
            return View(model);
        }

        //Вход пользователя
        public ActionResult Login()
        {
            if (Request.Cookies["user"] != null)
                return RedirectToAction("Index", "Home");
            else
                return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AccountLoginModel model)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            if (ModelState.IsValid)
            {
                User user = null;
                using (MD5 md5Hash = MD5.Create())
                {
                    string source = "login:" + model.Login + "###password:" + model.Password;
                    string hash = GetMd5Hash(md5Hash, source);
                    user = db.User.FirstOrDefault(u => u.md5 == hash);
                }
                //Если данные введены верно
                //Заносим в куки Логин пользователя
                if (user != null)
                {
                    //Файл куки может сохраняться между сеансами браузера
                    HttpCookie cookie = new HttpCookie("user")
                    {
                        Name = "user",
                        Value = user.id.ToString(),
                        Expires = DateTime.Now.AddDays(7),
                    };
                    Response.SetCookie(cookie);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("Err", "Неверный логин или пароль");
                }
            }
            return View(model);
        }

        //Выход пользователя из аккаунта
        public ActionResult Logout()
        {
            HttpCookie currentUserCookie = HttpContext.Request.Cookies["user"];
            HttpContext.Response.Cookies.Remove("user");
            currentUserCookie.Expires = DateTime.Now.AddDays(-7);
            currentUserCookie.Value = null;
            HttpContext.Response.SetCookie(currentUserCookie);
            return RedirectToAction("Index", "Home");
        }
        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }

    /*
                  2	Администратор
                  5	Директор
                  6	Секретарь
                  7	Бригадир
                  8	Рабочий
                  9	Клиент
                  */
}
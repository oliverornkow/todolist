using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using todolist.Models;
using BC = BCrypt.Net.BCrypt;

namespace todolist.Controllers
{
    public class LoginController : Controller
    {
        private readonly TodoContext _context;
        private IDataProtector _provider;
        private readonly IConfiguration _config;

        public LoginController(TodoContext context, IDataProtectionProvider provider, IConfiguration configuration)
        {
            _context = context;
            _config = configuration;
            _provider = provider.CreateProtector(_config["SecretKey"]);
        }

        // GET: Login
        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            ViewBag.userId = userId;
            return View();
        }
        [HttpPost]
        public IActionResult Index(string username, string password)
        {

            Login login = _context.Login.SingleOrDefault(l => l.Username == username);
            if (login != null)
            {
                //if (login.Password == password)
                if (BC.Verify(password, login.Password))
                {
                    HttpContext.Session.SetInt32("userId", login.Id);
                    ViewBag.Message = "Vi er logget ind...!";
                    return Redirect("/Todo");
                }
                else ViewBag.Message = "Forkert Password";
            }
            else ViewBag.Message = "Brugeren kunne ikke findes.";

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("userId");

            return Redirect("/login");
        }
        // GET: Login/Profil/5
        public IActionResult Profil(int id)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            ViewBag.userId = userId;

            if (userId == null)
            {
                return Redirect("/login");
            }

            var user = _context.Login
                .FirstOrDefault(m => m.Id == id);
            if (user == null)
            {
                return Redirect("/login");
            }
            else {
                user.Email = _provider.Unprotect(user.Email);
                return View(user);
            }
        }

        // GET: Login/Create
        public IActionResult Create()
        {
            return View();
        }


        // GET: Login/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            ViewBag.userId = userId;
            if (id == null)
            {
                return Redirect("/todo");
            }

            var login = _context.Login.Find(id);
            if (login == null)
            {
                return Redirect("/login");
            }
            return View(login);
        }

        // POST: Login/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public IActionResult Edit(int id, string Email, string Username, string Password)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            ViewBag.userId = userId;
            if (userId == null)
            {
                return Redirect("/login");
            }
            var user = _context.Login.SingleOrDefault(t => t.Id == id && t.Id == userId);
            user.Username = Username;
            user.Password = BC.HashPassword(Password);
            user.Email = _provider.Protect(Email);
          
            _context.SaveChanges();


            return Redirect("/login/profil/" + id);

        }

        // GET: Login/Delete/5
        public IActionResult Delete(int id)
        {
            if (id == null)
            {
                return Redirect("/todo");
            }

            var login = _context.Login
                .FirstOrDefault(m => m.Id == id);
            if (login == null)
            {
                return Redirect("/login");
            }

            return View(login);
        }

        private bool LoginExists(int id)
        {
            return _context.Login.Any(e => e.Id == id);
        }
    }
}

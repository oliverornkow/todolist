using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using todolist.Models;
using BC = BCrypt.Net.BCrypt;

namespace todolist.Controllers
{
    public class HomeController : Controller
    {
        private readonly TodoContext _todoContext;
        private readonly ILogger<HomeController> _logger;
        private IDataProtector _provider;
        private readonly IConfiguration _config;


        public HomeController(ILogger<HomeController> logger, TodoContext context, IDataProtectionProvider provider, IConfiguration configuration)
        {
            _logger = logger;
            _todoContext = context;
            _config = configuration;
           // _todoContext = context;
            _provider = provider.CreateProtector(_config["SecretKey"]);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            ViewBag.userId = userId;
            
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password)
        {
            Login knownLogin = _todoContext.Login.SingleOrDefault(l => l.Username == username);
            if (knownLogin == null)
            {
                Login login = new Login
                {
                    Username = username,
                    Email = _provider.Protect(email),
                    Password = BC.HashPassword(password)
                };

                _todoContext.Login.Add(login);
                _todoContext.SaveChanges();
                ViewBag.Message = "Bruger er nu oprettet";
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

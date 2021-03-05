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

namespace todolist.Controllers
{
    public class TodoController : Controller
    {
        private readonly TodoContext _context;
        private IDataProtector _provider;
        private readonly IConfiguration _config;

        public TodoController(TodoContext context, IDataProtectionProvider provider, IConfiguration configuration)
        {
            _context = context;
            _config = configuration;
            _provider = provider.CreateProtector(_config["SecretKey"]);
        }

        // GET: Todo
        public IActionResult Index()
        {

            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return Redirect("/");
            }

            ViewBag.userId = userId;

            List<TodoItem> todos = _context.TodoItem.Where(t => t.loginId == userId).ToList();


            foreach (TodoItem todo in todos)
            {
                todo.Title = _provider.Unprotect(todo.Title);
                todo.Description = _provider.Unprotect(todo.Description);
   
            }

            ViewBag.Todos = todos;

            return View();
        }

        // GET: Todo/Details/5
        public IActionResult Show(int id)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            ViewBag.userId = userId;
            if (userId == null)
            {
                return Redirect("/");
            }

            var todoItem = _context.TodoItem
                .FirstOrDefault(m => m.Id == id);
            if (todoItem == null)
            {
                return Redirect("/");
            }

            todoItem.Title = _provider.Unprotect(todoItem.Title);
            todoItem.Description = _provider.Unprotect(todoItem.Description);

            return View(todoItem);
        }

        // GET: Todo/Create
        [HttpPost]
        public IActionResult Create(string itemTitle, string itemDescription)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return Redirect("/login");
            }
            ViewBag.userId = userId;

            _context.TodoItem.Add(new TodoItem
            {
                Title = _provider.Protect(itemTitle),
                Description = _provider.Protect(itemDescription),
                Added = DateTime.Now,
                loginId = (int)userId
            });
            _context.SaveChanges();

            ViewBag.Message = "Test";
            return Redirect("/todo/");
        }

        // POST: Todo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Title,Description,Added,IsDone,loginId")] TodoItem todoItem)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(todoItem);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(todoItem);
        //}

        // GET: Todo/Edit/5
        [HttpGet]
        public IActionResult Edit(int id, string itemTitle, string itemDescription)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            ViewBag.userId = userId;
            if (userId == null)
            {
                return Redirect("/");
            }

            var todoItem = _context.TodoItem.SingleOrDefault(t => t.Id == id && t.loginId == userId);
            if (todoItem == null)
            {
                return Redirect("/todo");
            }
            

            return View();
        }

        [HttpPost]
        public IActionResult Update(int id, string itemTitle, string itemDescription, bool itemIsDone)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return Redirect("/");
            }
            var todoItem = _context.TodoItem.SingleOrDefault(t => t.Id == id && t.loginId == userId);
            todoItem.Title = _provider.Protect(itemTitle);
            todoItem.Description = _provider.Protect(itemDescription);
            todoItem.IsDone = itemIsDone;
            _context.SaveChanges();


            return Redirect("show/" + id);
        }

        // GET: Todo/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return Redirect("/");
            }


            var todoItem = _context.TodoItem.SingleOrDefault(t => t.Id == id && t.loginId == userId);
            if (todoItem == null) return Redirect("/todo");


            _context.TodoItem.Remove(todoItem);
            _context.SaveChanges();

            return Redirect("/todo");

        }

    }
}

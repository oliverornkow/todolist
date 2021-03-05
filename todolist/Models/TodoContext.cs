using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace todolist.Models
{
    public class TodoContext : DbContext
    {

        public TodoContext(DbContextOptions<TodoContext> options) : base(options)
        {

        }

        public DbSet<Login> Login { get; set; }
        public DbSet<TodoItem> TodoItem { get; set; }
    }
}

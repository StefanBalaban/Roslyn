using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Roslyn.Classes
{
    public class AppDbContext : DbContext
    {
        public DbSet<Example> Examples { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=ws-dell5540-460;Initial Catalog=motopro-db;Integrated Security=True");
        }
    }
}

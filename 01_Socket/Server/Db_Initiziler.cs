using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Db_Initiziler : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(@"Data Source = DESKTOP-3P42OP0\SQLEXPRESS;
                                          Initial Catalog = CitiesDB;
                                          Integrated Security = True;
                                          Encrypt = False;
                                          Trust Server Certificate = True;
                                          Application Intent = ReadWrite;
                                          Multi Subnet Failover = False");
        }

        public DbSet<City> Cities { get; set; }
    }
}

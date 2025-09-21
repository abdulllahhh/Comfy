using infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string not found");

            optionsBuilder.UseMySql(conn, ServerVersion.AutoDetect(conn));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}

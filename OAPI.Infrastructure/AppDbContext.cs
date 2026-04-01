using Microsoft.EntityFrameworkCore;
using OAPI.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Infrastructure
{
	public class AppDbContext: DbContext
	{
		public DbSet<Order> Orders => Set<Order>();

		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{

		}
	}
}

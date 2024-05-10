using GenericCacheServiceWithRedis.Models;
using Microsoft.EntityFrameworkCore;

namespace GenericCacheServiceWithRedis.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
	:DbContext(options)
{
	public DbSet<Employee> Employees { get; set; }
}

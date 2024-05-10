using Bogus;
using GenericCacheServiceWithRedis.Data;
using GenericCacheServiceWithRedis.Models;
using GenericCacheServiceWithRedis.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenericCacheServiceWithRedis.Controllers;
[Route("api/[controller]")]
[ApiController]
public class EmployeesController(
	ApplicationDbContext context,
	ICacheService cacheService
	) : ControllerBase
{
	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var hashKey = "employeesHashKey";

		IEnumerable<Employee> employees = await cacheService.GetHash<Employee>(hashKey);
		if(employees is null)
		{
			employees = await context.Employees
				.Where(x => x.Id % 2 != 0 && x.FirstName.Contains("a"))
				.ToListAsync();

			await cacheService.SetHash(employees.Select(e=> (e.Id.ToString(), e)), hashKey, TimeSpan.FromMinutes(10));
		}

		return Ok(employees);
	}

	[HttpGet(nameof(GetAllCount))]
	public async Task<IActionResult> GetAllCount()
	{
		var employees = await context.Employees.CountAsync();
		return Ok(employees);
	}


	[HttpGet(nameof(GetAllWithOutCaching))]
	public async Task<IActionResult> GetAllWithOutCaching()
	{
		var	employees = await context.Employees
			.Where(x=> x.Id % 2 != 0 && x.FirstName.Contains("a"))
			.ToListAsync();
		return Ok(employees);
	}


	[HttpPost(nameof(Create))]
	public async Task<IActionResult> Create(Employee employee)
	{
		var hashKey = "employeesHashKey";

		await context.Employees.AddAsync(employee);
		await context.SaveChangesAsync();

		await cacheService.SetToHash(employee, hashKey, employee.Id.ToString());

		return Ok();
	}


	[HttpPost(nameof(Edit))]
	public async Task<IActionResult> Edit(Employee employee)
	{
		var hashKey = "employeesHashKey";
		var empToEdit = await context.Employees.FirstOrDefaultAsync(x=> x.Id == employee.Id);
		if (empToEdit is null)
		{
			return NotFound();
		}
		empToEdit.FirstName = employee.FirstName;
		empToEdit.LastName = employee.LastName;
		empToEdit.Age = employee.Age;
		await context.SaveChangesAsync();

		await cacheService.SetToHash(empToEdit, hashKey, empToEdit.Id.ToString());

		return Ok();
	}


	[HttpDelete(nameof(Delete))]
	public async Task<IActionResult> Delete(int id)
	{
		var hashKey = "employeesHashKey";

		var empTodelete = await context.Employees.FirstOrDefaultAsync(x=> x.Id == id);
		if(empTodelete is  null)
		{
			return NotFound();
		}

		context.Employees.Remove(empTodelete);
		await context.SaveChangesAsync();

		await cacheService.RemoveFromHash( hashKey, empTodelete.Id.ToString());

		return Ok();
	}


	[HttpPost]
	public async Task<IActionResult> AddFackDataToDatabase()
	{
		var personList = GeneratePersonList(1000);
		await context.Employees.AddRangeAsync(personList);
		await context.SaveChangesAsync();
		return Ok();
	}


	private List<Employee> GeneratePersonList(int count)
	{
		return new Faker<Employee>()
		   .RuleFor(p => p.FirstName, f => f.Name.FirstName())
		   .RuleFor(p => p.LastName, f => f.Name.LastName())
		   .RuleFor(p => p.Age, f => f.Random.Int(20, 60)).Generate(count);
	}
}

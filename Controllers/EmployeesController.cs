using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWWebAPI.Data;
using SWWebAPI.Models;
using SWWebAPI.Models.Entities;

namespace SWWebAPI.Controllers
{
    //localhost:xxxx/api/employees
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        public EmployeesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAllEmployees()
        { 
            return Ok(dbContext.employees.ToList());

        }
        [HttpGet("{id}")]
        public IActionResult GetAllEmployeeById(int id)
        {
            var employee = dbContext.employees.Find(id);
            if (employee is null)
            {
                return NotFound();
            }
           
           return Ok(employee);
        
        }
        [HttpPut]
        public IActionResult UpdateEmployee(int id, UpdateEmployeeDto updateEmployeeDto)
        {
            var employee = dbContext.employees.Find(id);
            if (employee is null)
            {
                return NotFound();
            }
            employee.first_name = updateEmployeeDto.first_name;
            employee.last_name = updateEmployeeDto.last_name;
            employee.email = updateEmployeeDto.email;
            employee.username=updateEmployeeDto.username;
            employee.pswd = updateEmployeeDto.pswd;
            employee.mobilenumber = updateEmployeeDto.mobilenumber;
            employee.modified_by= updateEmployeeDto.modified_by;
            dbContext.SaveChanges();
            return Ok(employee);    
        }
        [HttpDelete]
        public IActionResult DeleteEmployee(int id)
        {
            var employee = dbContext.employees.Find(id);
            if (employee is null)
            {
                return NotFound();
            }
            dbContext.Remove(employee);
            dbContext.SaveChanges();
            return Ok();

        }
        
       [HttpPost]
        public IActionResult AddEmployee(AddEmployeeDto addEmployeeDto)
        {
           var employeeEntity = new employee()
            {
                first_name = addEmployeeDto.first_name,
                last_name = addEmployeeDto.last_name,
                username = addEmployeeDto.username,
                pswd = addEmployeeDto.pswd,
                mobilenumber = addEmployeeDto.mobilenumber,
                email = addEmployeeDto?.email,
                created_by = addEmployeeDto.created_by
           };
            dbContext.employees.Add(employeeEntity);
            dbContext.SaveChanges();
            return Ok(employeeEntity);
        }
    
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net;
using WorkingShiftActivity.Context.Models;
using WorkingShiftActivity.DataManager;
using WorkingShiftActivity.Enums;
using WorkingShiftActivity.Models.RequestModels;
using WorkingShiftActivity.Models.ResponseModels;
using WorkingShiftActivity.Repository;

namespace WorkingShiftActivity.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthDataRepository<Employee, EmployeeDto> _dataRepository;
        public AuthController(IAuthDataRepository<Employee, EmployeeDto> dataRepository)
        {
            _dataRepository = dataRepository;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] AuthRequest request)
        {
            if (request is null)
            {
                return BadRequest("request contains null value!");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                var employee = _dataRepository.Get(request.EmployeeID);
                if (employee != null)
                {
                    var token = _dataRepository.Authenticate(employee);

                    if (token == null)
                    {
                        return StatusCode((int)HttpStatusCode.Unauthorized, new Response<AuthResponse> { Message = "Unauthorized!" });
                    }
                    return StatusCode((int)HttpStatusCode.OK, new Response<AuthResponse> { Message = "Login Successfully!", Data = new AuthResponse { EmployeeId = employee.Id, Role = employee.Role, Tokens = token } });
                }
                return StatusCode((int)HttpStatusCode.NotFound, new Response<EmployeeResponse> { Message = "Employee does not exist!" });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.InnerException?.Message);
            }
        }

        [HttpPost]
        [Route("Register")]
        public IActionResult RegisterEmployee([FromBody] EmployeeRequest request)
        {
            try
            {
                if (request is null)
                {
                    return BadRequest("request contains null value!");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                Employee emp = new Employee();
                emp.Id = this.GenerateID();
                emp.Name = request.Name;
                emp.Email = request.Email;
                emp.Role = (int)RoleEnum.NonAdmin;
                emp.CreatedOn = DateTime.Now;

                int value = _dataRepository.Add(emp);
                if (value > 0)
                {
                    return StatusCode((int)HttpStatusCode.Created, new Response<AuthResponse> { Message = "Employee registered successfully with id: " + emp.Id });
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Conflict, new Response<AuthResponse> { Message = "Employee already registered!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new Response<AuthResponse> { Message = ex.InnerException?.Message });
            }
        }

        protected string GenerateID()
        {
            string alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string small_alphabets = "abcdefghijklmnopqrstuvwxyz";
            string numbers = "1234567890";

            string characters = numbers;
            characters += alphabets + small_alphabets + numbers;
            int length = 5;
            string id = string.Empty;
            for (int i = 0; i < length; i++)
            {
                string character;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (id.IndexOf(character) != -1);
                id += character;
            }
            return id;
        }
    }
}

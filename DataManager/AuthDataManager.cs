using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WorkingShiftActivity.Context;
using WorkingShiftActivity.Context.Models;
using WorkingShiftActivity.Models.RequestModels;
using WorkingShiftActivity.Repository;
using Microsoft.Extensions.Configuration;
using WorkingShiftActivity.Models.ResponseModels;

namespace WorkingShiftActivity.DataManager
{
    public class AuthDataManager : IAuthDataRepository<Employee, EmployeeDto>
    {
        private readonly IConfiguration _configuration;
        private WorkingShiftContext _workingShiftDbContext { get; set; }
        public AuthDataManager(WorkingShiftContext context, IConfiguration configuration)
        {
            _workingShiftDbContext = context;
            this._configuration = configuration;
        }
        public Employee Get(string id)
        {
            return _workingShiftDbContext.Employees.FirstOrDefault(b => b.Id == id);
        }

        public Tokens Authenticate(Employee employee)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
              {
             new Claim(ClaimTypes.Name, employee.Name)
              }),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new Tokens { Token = tokenHandler.WriteToken(token) };

        }

        public int Add(Employee entity)
        {
            var record = _workingShiftDbContext.Employees.FirstOrDefault(emp => emp.Email.Equals(entity.Email));
            if (record is null)
            {
                _workingShiftDbContext.Employees.Add(entity);
            }
            return _workingShiftDbContext.SaveChanges();
        }
    }

    public class EmployeeDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int Role { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}

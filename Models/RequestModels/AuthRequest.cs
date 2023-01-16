using System.ComponentModel.DataAnnotations;

namespace WorkingShiftActivity.Models.RequestModels
{
    public class AuthRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "EmployeeId is Required!")]
        public string EmployeeID { get; set; } = null!;
    }
}

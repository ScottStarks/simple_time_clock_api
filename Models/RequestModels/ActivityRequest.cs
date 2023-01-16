using System.ComponentModel.DataAnnotations;

namespace WorkingShiftActivity.Models.RequestModels
{
    public class ActivityRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "ShiftId is Required!")]
        public int ShiftId { get; set; }
    }
}

namespace WorkingShiftActivity.Models.ResponseModels
{
    public class AuthResponse
    {
        public string EmployeeId { get; set; }
        public int Role { get; set; }
        public Tokens Tokens { get; set; }
    }
}

namespace arna.HRMS.Models.ViewModels.Leave
{
    public class LeaveBalanceViewModel
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = "";
        public int TotalDays { get; set; }
        public int UsedDays { get; set; }
        public int RemainingDays => TotalDays - UsedDays;
    }

}

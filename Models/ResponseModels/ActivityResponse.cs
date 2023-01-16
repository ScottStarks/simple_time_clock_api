using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkingShiftActivity.Models.ResponseModels
{
    public class ActivityResponse
    {
        public int Id { get; set; }

        public DateTime WorkShiftStartTime { get; set; }

        public DateTime? WorkShiftEndTime { get; set; }

        public DateTime? BreakStartTime { get; set; }

        public DateTime? BreakEndTime { get; set; }

        public DateTime? LunchStartTime { get; set; }

        public DateTime? LunchEndTime { get; set; }

        public bool IsShiftActive { get; set; }

        public bool IsBreakActive { get; set; }

        public bool IsLunchActive { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WorkingShiftActivity.Context.Models;

[Table("Activity")]
public partial class Activity
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("EmployeeID")]
    public string EmployeeId { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime WorkShiftStartTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? WorkShiftEndTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? BreakStartTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? BreakEndTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LunchStartTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LunchEndTime { get; set; }

    public bool IsShiftActive { get; set; }

    public bool IsBreakActive { get; set; }

    public bool IsLunchActive { get; set; }

    public virtual Employee Employee { get; set; } 
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WorkingShiftActivity.Context.Models;

[Table("Employee")]
public partial class Employee
{
    [Key]
    [Column("ID")]
    public string Id { get; set; } = null!;

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [StringLength(100)]
    public string Email { get; set; } = null!;

    public int Role { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedOn { get; set; }

    [ForeignKey("Role")]
    [InverseProperty("Employees")]
    public virtual Role RoleNavigation { get; set; } = null!;
}

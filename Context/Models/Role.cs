using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WorkingShiftActivity.Context.Models;

[Table("Role")]
public partial class Role
{
    [Key]
    [Column("Role")]
    public int Role1 { get; set; }

    [InverseProperty("RoleNavigation")]
    public virtual ICollection<Employee> Employees { get; } = new List<Employee>();
}

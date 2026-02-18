using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("instructor_types")]
[Index("Name", Name = "UQ__instruct__72E12F1B722BA49E", IsUnique = true)]
public partial class InstructorType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(20)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

}

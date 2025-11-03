#nullable disable
using System.ComponentModel.DataAnnotations;


namespace GiftOfTheGivers2.Models
{
    public class VolunteerAssignment
    {
       
            public Guid AssignmentId { get; set; } = Guid.NewGuid();

            [Required]
            public Guid TaskId { get; set; }

            [Required]
            public Guid UserId { get; set; }

            public DateTime AssignmentDate { get; set; } = DateTime.Now;
            public string Status { get; set; } = "Assigned";

            [Display(Name = "Hours Worked")]
            [Range(0, 999.99)]
            public decimal? HoursWorked { get; set; }

            public string Notes { get; set; }

            // Navigation properties
            public VolunteerTask Task { get; set; }
            public User User { get; set; }
        }
    }


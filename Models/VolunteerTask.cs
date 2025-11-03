
#nullable disable
using System.ComponentModel.DataAnnotations;
using System;

namespace GiftOfTheGivers2.Models
{
    public class VolunteerTask
    {
        public Guid TaskId { get; set; } = Guid.NewGuid();
        public Guid? IncidentId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Task Type")]
        public string TaskType { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        [Display(Name = "Volunteers Needed")]
        [Range(1, int.MaxValue)]
        public int RequiredVolunteers { get; set; }

        [Display(Name = "Current Volunteers")]
        public int CurrentVolunteers { get; set; } = 0;

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        public string Status { get; set; } = "Open";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public DisasterIncident Incident { get; set; }
    }
}

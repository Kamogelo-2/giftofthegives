#nullable disable
using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers2.Models
{
    public class DisasterIncident
    {
        public Guid IncidentId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Incident Type")]
        public string IncidentType { get; set; }

        [Required]
        public string Location { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [Display(Name = "Severity Level")]
        public string SeverityLevel { get; set; } = "Medium";

        public string Status { get; set; } = "Reported";

        [Display(Name = "People Affected")]
        [Range(0, int.MaxValue)]
        public int? PeopleAffected { get; set; }

        [Display(Name = "Immediate Needs")]
        public string ImmediateNeeds { get; set; }

        public DateTime ReportedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public User User { get; set; }
    }
}

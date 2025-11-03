#nullable disable
using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers2.Models
{
    public class Donation
    {
        public Guid DonationId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        [Display(Name = "Item Name")]
        [StringLength(255)]
        public string ItemName { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public string Description { get; set; }

        [Required]
        [Display(Name = "Donation Type")]
        public string DonationType { get; set; }

        public string Status { get; set; } = "Pending";
        public DateTime DonatedAt { get; set; } = DateTime.Now;
        public DateTime? ReceivedAt { get; set; }

        // Navigation properties
        public User User { get; set; }
        public ResourceCategory Category { get; set; }
    }
}

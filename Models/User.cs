#nullable disable
using System;
using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers2.Models
{
    public class User
    {
        public Guid UserId { get; set; } = Guid.NewGuid();

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string UserType { get; set; } = "Volunteer";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLogin { get; set; }
    }
}


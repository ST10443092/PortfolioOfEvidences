using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLDV1.Models
{
    public class Venue
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int VenueId { get; set; }
        public ICollection<Booking> Booking { get; set; }

        [Required]
        [StringLength(255)]
        public string Location { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }


        [NotMapped]  // This won't be stored in DB
        public IFormFile? ImageFile { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public ICollection<Event> Events { get; set; }

        public bool IsAvailable { get; set; } = true;

    }
}

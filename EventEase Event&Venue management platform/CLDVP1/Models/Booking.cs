using CLDVP1.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLDV1.Models
{
    public class Booking
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int BookingId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        // Navigation properties
        [ForeignKey("EventId")]
        public int EventId { get; set; }

        public Event Event { get; set; }

        [ForeignKey("Venue")]
        public int VenueId { get; set; }  // Reference to venue
        public Venue Venue { get; set; }  // Navigation property

        public EventType EventType { get; set; }
        public int EventTypeId { get; set; }


        [NotMapped]
        public string VenueImageUrl { get; set; }

       
      









    }
}

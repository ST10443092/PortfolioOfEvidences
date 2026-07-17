using CLDVP1.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLDV1.Models
{
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Disable auto-generation
        [Required(ErrorMessage = "Event ID is required")]
        public int EventId { get; set; }
        public ICollection<Booking> Booking { get; set; }



        [StringLength(255)]
        public string EventName { get; set; }



        public DateTime EventDate { get; set; }


        public string Description { get; set; }

        public Venue Venue { get; set; }



        public int VenueId { get; set; }

        //public int EventTypeId { get; set; }
        [ForeignKey("EventTypeId")]
        public EventType EventType { get; set; }
        public int EventTypeId { get; set; }






    }
}

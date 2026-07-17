using CLDV1.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLDVP1.Models
{
    public class EventType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Disable auto-generation
        [Required(ErrorMessage = "Event ID is required")]
        public int EventTypeId { get; set; }
        [Required]
        public string TypeName { get; set; }

        public ICollection<Event> Events { get; set; }
    }
}

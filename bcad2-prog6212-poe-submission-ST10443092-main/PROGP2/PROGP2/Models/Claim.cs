using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROGP2.Models
{
    public class Claim
    {
        [Key]
        public int ID { get; set; }

        [Range(0, 210, ErrorMessage = "Hours must be a number between 0 and 24.")]
        public double Hours { get; set; }

        

        [Range(0, double.MaxValue, ErrorMessage = "Rate must be a non-negative number.")]
        public double Rate { get; set; }

        // Calculated total amount (Rate � Hours)
        public double Total => Rate * (double)Hours;

        // ?? Stores the binary data for the uploaded PDF (optional)
        public byte[]? RatePDF { get; set; }

        // Claim status: Pending (default), Verified, Approved, Declined
        // Workflow: Pending -> Verified (by Coordinator) -> Approved (by Manager)
        public string Status { get; set; } = "Pending";

        // Date when the claim was created
        [DataType(DataType.DateTime)]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Foreign key reference
        public int? UserID { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }

    }
}

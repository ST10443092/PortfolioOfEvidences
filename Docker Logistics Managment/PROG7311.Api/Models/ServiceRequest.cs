using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace PROG7311.Api.Models
{
    public class ServiceRequest
    {
        public int ServiceRequestId { get; set; }

        [Required]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [Required]
        public int ContractId { get; set; }
        public Contract? Contract { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Cost { get; set; }

        /// <summary>ISO 4217 code for <see cref="Cost"/> (e.g. USD, GBP, ZAR).</summary>
        [Required]
        [StringLength(3)]
        public string CostCurrencyCode { get; set; } = "ZAR";

        /// <summary>Same charge expressed in South African Rand (reporting).</summary>
        [Required]
        public decimal CostInZar { get; set; }

        /// <summary>ZAR per 1 unit of <see cref="CostCurrencyCode"/> at conversion time.</summary>
        [Required]
        public decimal ExchangeRateToZar { get; set; }

        /// <summary>ECB rate date returned by the provider (UTC calendar date).</summary>
        public DateOnly? CurrencyRateDate { get; set; }

        public DateTime? CurrencyConvertedAtUtc { get; set; }

        [Required]
        public string Status { get; set; }
    }
}
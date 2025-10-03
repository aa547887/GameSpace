using System;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    // Form models for EVoucher create/edit pages
    public class EVoucherCreateModel
    {
        [Required]
        [Display(Name = "EVoucher Code")]
        public string EvoucherCode { get; set; } = string.Empty;
        public string EVoucherCode { get => EvoucherCode; set => EvoucherCode = value; }

        [Required]
        [Display(Name = "User Id")]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "EVoucher Type")]
        public int EvoucherTypeId { get; set; }
        public int EVoucherTypeID { get => EvoucherTypeId; set => EvoucherTypeId = value; }

        [Display(Name = "Acquired Time")]
        public DateTime? AcquiredTime { get; set; }

        // Optional fields referenced by views
        public decimal? Value { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}


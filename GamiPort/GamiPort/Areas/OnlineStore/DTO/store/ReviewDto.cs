using System;

namespace GamiPort.Areas.OnlineStore.DTO.Store
{
    public class ReviewDto
    {
        public long RatingId { get; set; }
        public int UserId { get; set; }
        public byte Rating { get; set; }
        public string? ReviewText { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

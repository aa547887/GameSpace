namespace GamiPort.Areas.OnlineStore.DTO.Store
{
    public class RateRequest
    {
        public int UserId { get; set; }
        public byte Rating { get; set; }
        public string? Review { get; set; }
    }
}


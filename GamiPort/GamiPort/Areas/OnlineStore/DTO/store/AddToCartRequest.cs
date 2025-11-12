namespace GamiPort.Areas.OnlineStore.DTO.Store
{
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Qty { get; set; } = 1;
    }
}


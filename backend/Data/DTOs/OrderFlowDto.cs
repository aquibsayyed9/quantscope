namespace FixMessageAnalyzer.Data.DTOs
{
    public class OrderFlowDto
    {
        public string OrderId { get; set; }
        public string Symbol { get; set; }
        public string Side { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public List<OrderStateDto> States { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

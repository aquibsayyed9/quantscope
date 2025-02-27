namespace FixMessageAnalyzer.Data.DTOs
{
    public enum OrderTrackingMode
    {
        OrderId,
        ClOrdId
    }
    public class OrderFlowFilterDto
    {
        public string OrderId { get; set; }
        public string ClOrdId { get; set; }
        public string Symbol { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
        public OrderTrackingMode? TrackingMode { get; set; }
    }
}

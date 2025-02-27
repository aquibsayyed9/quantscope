namespace FixMessageAnalyzer.Data.DTOs
{
    public class OrderFlowResponse
    {
        public List<OrderFlowDto> Orders { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}

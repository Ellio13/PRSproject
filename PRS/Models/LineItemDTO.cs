using System.Text.Json.Serialization;

namespace PRS.Models
{


    public class LineItemDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int RequestId { get; set; }
    }
}
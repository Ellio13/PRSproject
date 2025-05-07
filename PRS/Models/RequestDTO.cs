using System.Text.Json.Serialization;

namespace PRS.Models
{
    public class RequestDTO
    {
        public int UserId { get; set; }
        public string Description { get; set; }
        public string Justification { get; set; }
        public DateTime DateNeeded { get; set; }
        public string DeliveryMode { get; set; }

        [JsonIgnore] // Exclude from Swagger and serialization
        public List<LineItemDTO> LineItems { get; set; } = new List<LineItemDTO>();
    }

    public class RejectRequestDTO
    {
        public string ReasonForRejection { get; set; }
    }


}

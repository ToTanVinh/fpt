using System.ComponentModel.DataAnnotations;

namespace fpt.DTOs
{
    public class HotspotDTO
    {    
        public int HotspotId { get; set; }
        public double? Pitch { get; set; }
        public double? Yaw { get; set; }
        public string? Type { get; set; }   // Ví dụ: "scene", "info", "image"
        public string? Text { get; set; }
        public string? SceneIdTarget { get; set; } // Dùng cho loại hotspot "scene"
        public string? URL { get; set; }           // Dùng cho loại hotspot "info"
        public string? Target { get; set; }        // Dùng cho loại hotspot "image"
        public string? Image { get; set; }         // Dùng cho loại hotspot "image"
        public string? Width { get; set; }         // Dùng cho loại hotspot "image"
        public string? Height { get; set; }        // Dùng cho loại hotspot "image"
        public string? HotspotIdentifier { get; set; } // Mã định danh duy nhất
    }
}

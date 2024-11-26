namespace fpt.DTOs
{
    public class SceneDTO
    {
        public string SceneId { get; set; }    // Mã ID của cảnh
        public string? Title { get; set; }      // Tiêu đề của cảnh
        public int? Hfov { get; set; }         // Góc nhìn theo chiều ngang
        public double? Pitch { get; set; }     // Góc pitch
        public double? Yaw { get; set; }       // Góc yaw
        public string? Type { get; set; }       // Loại cảnh (ví dụ: "equirectangular")
        public string? Panorama { get; set; }   // Đường dẫn tới hình ảnh panorama
        public List<AudioDTO> Audio { get; set; }    // Danh sách âm thanh (tiếng Việt, tiếng Anh)
        public List<HotspotDTO> HotSpots { get; set; } // Danh sách các hotspot
    }
}

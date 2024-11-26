using fpt.DTOs;
using fpt.Entites;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace fpt.Controllers
{
    [Route("api/panorama")]
    [ApiController]
    public class GetAllPanoramaController : ControllerBase
    {   
        private readonly PanoramaContext _context;

        public GetAllPanoramaController(PanoramaContext context)
        {
            _context = context;
        }

        [HttpGet("panoramas")]
        public async Task<IActionResult> GetPanoramaData()
        {
            // Lấy tất cả các cảnh từ cơ sở dữ liệu
            var scenes = await _context.Scenes
                .Include(s => s.Audios)            // Bao gồm các âm thanh của cảnh
                .Include(s => s.Hotspots)     // Bao gồm các điểm hotspot
                .ToListAsync();

            // Kiểm tra nếu không có cảnh nào trong cơ sở dữ liệu
            if (!scenes.Any())
            {
                return NotFound(new { message = "No panorama scenes found." });
            }

            // Tạo dữ liệu trả về theo cấu trúc Pannellum
            var panoramaData = new
            {
                defaultData = new  // Thay tên default thành defaultData để tránh xung đột
                {
                    firstScene = scenes.First().SceneId,  // Cảnh đầu tiên
                    author = "Your Author Name",          // Tên tác giả
                    sceneFadeDuration = 100,              // Thời gian chuyển cảnh
                    autoLoad = true,                      // Tự động tải pano
                    autoRotate = -2                       // Quay tự động pano
                },
                scenes = scenes.ToDictionary(s => s.SceneId, s => new
                {
                    s.SceneId,
                    s.Title,
                    s.Hfov,
                    s.Pitch,
                    s.Yaw,
                    s.Type,
                    s.Panorama,
                    audio = s.Audios
                        .GroupBy(a => a.Language)
                        .ToDictionary(g => g.Key, g => g.First().AudioFile),  // Tránh trùng Language
                    hotSpots = s.Hotspots.Select(h => new HotspotDTO
                    {   
                        HotspotId = h.HotspotId,
                        Pitch = h.Pitch,
                        Yaw = h.Yaw,
                        Type = h.Type,
                        Text = h.Text,
                        SceneIdTarget = h.SceneIdTarget,   // Chỉnh lại trường này cho phù hợp
                        URL = h.Url,
                        Target = h.Target,
                        Image = h.Image,
                        Width = h.Width,
                        Height = h.Height,
                        HotspotIdentifier = h.HotspotIdentifier // Đổi id thành HotspotIdentifier
                    }).ToList()
                })
            };

           
            return Ok(panoramaData);
        }

        [HttpGet("{sceneId}/hotspot")]
        public async Task<IActionResult> getHotspotBySceneId(string sceneId)
        {
            var scene = await _context.Scenes.
                Include(s => s.Hotspots).
                FirstOrDefaultAsync(s => s.SceneId == sceneId);

            if (scene == null)
            {
                return NotFound(new { message = "Scene not found" });
            }

            if (scene.Hotspots == null || !scene.Hotspots.Any())
            {
                return NotFound(new { massgae = "No hotspots found for this scene" });

            }
            var hotspot = scene.Hotspots.Select(h => new
            {  
                h.HotspotId,
                h.Type,
                h.Pitch,
                h.Yaw,
                h.Text,
                h.SceneIdTarget,
                h.Url,
                h.Target,
                h.Image,
                h.Width,
                h.Height
            }).ToList();
            return Ok(hotspot);
        }

        [HttpPost("scenes")]
        public async Task<IActionResult> AddScene([FromBody] SceneDTO sceneDto)
        {
            if (sceneDto == null)
            {
                return BadRequest(new { message = "Invalid data." });
            }
            // Tạo đối tượng Scene từ SceneDTO
            var newScene = new Scene
            {
                SceneId = sceneDto.SceneId,
                Title = sceneDto.Title,
                Hfov = sceneDto.Hfov,
                Pitch = sceneDto.Pitch,
                Yaw = sceneDto.Yaw,
                Type = sceneDto.Type,
                Panorama = sceneDto.Panorama
            };

            // Thêm đối tượng Scene vào DbContext
            _context.Scenes.Add(newScene);

            // Lưu các audio vào cơ sở dữ liệu
            if (sceneDto.Audio != null)
            {
                foreach (var audioDto in sceneDto.Audio)
                {
                    var audio = new Audio
                    {
                        Language = audioDto.Language,
                        AudioFile = audioDto.AudioFile,
                        SceneId = sceneDto.SceneId // Gắn Audio vào Scene
                    };

                    _context.Audios.Add(audio);
                }
            }

            // Lưu các hotspot vào cơ sở dữ liệu
            if (sceneDto.HotSpots != null)
            {
                foreach (var hotspotDto in sceneDto.HotSpots)
                {
                    var newHotspot = new Hotspot
                    {
                        SceneId = sceneDto.SceneId,
                        Type = hotspotDto.Type,
                        Pitch = hotspotDto.Pitch,
                        Yaw = hotspotDto.Yaw,
                        Text = hotspotDto.Text,
                        SceneIdTarget = hotspotDto.SceneIdTarget,
                        Url = hotspotDto.URL,
                        Target = hotspotDto.Target,
                        Image = hotspotDto.Image,
                        Width = hotspotDto.Width,
                        Height = hotspotDto.Height,
                        HotspotIdentifier = hotspotDto.HotspotIdentifier
                    };

                    _context.Hotspots.Add(newHotspot);
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Scene added successfully!", sceneId = newScene.SceneId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while saving the scene.", error = ex.InnerException?.Message ?? ex.Message });
            }
        }
        
        [HttpPut("{sceneId}")]
        public async Task<IActionResult> UpdateScene(string sceneId, [FromBody] SceneDTO sceneDto)
        {
            if (sceneDto == null)
            {
                return BadRequest(new { message = "Invalid data." });
            }

            // Tìm cảnh trong cơ sở dữ liệu theo SceneId
            var existingScene = await _context.Scenes
                .FirstOrDefaultAsync(s => s.SceneId == sceneId);

            if (existingScene == null)
            {
                return NotFound(new { message = "Scene not found." });
            }

            // Cập nhật các thuộc tính của Scene
            existingScene.Title = sceneDto.Title;
            existingScene.Hfov = sceneDto.Hfov;
            existingScene.Pitch = sceneDto.Pitch;
            existingScene.Yaw = sceneDto.Yaw;
            existingScene.Type = sceneDto.Type;
            existingScene.Panorama = sceneDto.Panorama;

            // Cập nhật các đối tượng Audio
            foreach (var audioDto in sceneDto.Audio)
            {
                var audio = existingScene.Audios.FirstOrDefault(a => a.Language == audioDto.Language);
                if (audio != null)
                {
                    audio.AudioFile = audioDto.AudioFile;
                }
                else
                {
                    // Thêm mới nếu không có audio với ngôn ngữ này
                    existingScene.Audios.Add(new Audio
                    {
                        Language = audioDto.Language,
                        AudioFile = audioDto.AudioFile,
                        SceneId = sceneId
                    });
                }
            }

            // Cập nhật các Hotspot
            foreach (var hotspotDto in sceneDto.HotSpots)
            {
                var existingHotspot = existingScene.Hotspots
                    .FirstOrDefault(h => h.HotspotIdentifier == hotspotDto.HotspotIdentifier);

                if (existingHotspot != null)
                {
                    existingHotspot.Pitch = hotspotDto.Pitch;
                    existingHotspot.Yaw = hotspotDto.Yaw;
                    existingHotspot.Text = hotspotDto.Text;
                    existingHotspot.SceneIdTarget = hotspotDto.SceneIdTarget;
                    existingHotspot.Url = hotspotDto.URL;
                    existingHotspot.Target = hotspotDto.Target;
                    existingHotspot.Image = hotspotDto.Image;
                    existingHotspot.Width = hotspotDto.Width;
                    existingHotspot.Height = hotspotDto.Height;
                }
                else
                {
                    // Thêm mới hotspot nếu không có
                    existingScene.Hotspots.Add(new Hotspot
                    {
                        Pitch = hotspotDto.Pitch,
                        Yaw = hotspotDto.Yaw,
                        SceneId = sceneId,
                        Type = hotspotDto.Type,
                        Text = hotspotDto.Text,
                        SceneIdTarget = hotspotDto.SceneIdTarget,
                        Url = hotspotDto.URL,
                        Target = hotspotDto.Target,
                        Image = hotspotDto.Image,
                        Width = hotspotDto.Width,
                        Height = hotspotDto.Height,
                        HotspotIdentifier = hotspotDto.HotspotIdentifier
                    });
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Scene updated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the scene.", error = ex.Message });
            }
        }

        [HttpPost("{sceneId}/hotspot")]
        public async Task<IActionResult> AddHotspot(string sceneId, [FromBody] HotspotDTO hotspotDto)
        {
            if (hotspotDto == null || string.IsNullOrWhiteSpace(sceneId))
            {
                return BadRequest(new { message = "Invalid data or SceneId." });
            }

            var scene = await _context.Scenes.FirstOrDefaultAsync(s => s.SceneId == sceneId);
            if (scene == null)
            {
                return NotFound(new { message = "Scene not found." });
            }

            var existingHotspot = await _context.Hotspots
             .FirstOrDefaultAsync(h => h.HotspotId == hotspotDto.HotspotId && h.SceneId == sceneId);
            if (existingHotspot != null)
            {
                return BadRequest(new { message = "Hotspot already exists in this scene." });
            }


            if (hotspotDto.Type == "scene" && !string.IsNullOrEmpty(hotspotDto.SceneIdTarget))
            {
                var targetSceneExists = await _context.Scenes.AnyAsync(s => s.SceneId == hotspotDto.SceneIdTarget);
                if (!targetSceneExists)
                {
                    return BadRequest(new { message = $"SceneIdTarget '{hotspotDto.SceneIdTarget}' không tồn tại." });
                }
            }
            else if (hotspotDto.Type != "scene" && !string.IsNullOrEmpty(hotspotDto.SceneIdTarget))
            {
                // Nếu Type khác "scene", không nên có SceneIdTarget
                return BadRequest(new { message = "SceneIdTarget không được cung cấp khi Type khác 'scene'." });
            }


            var newHotspot = new Hotspot
            {
                Pitch = hotspotDto.Pitch,
                Yaw = hotspotDto.Yaw,
                SceneId = sceneId,
                Type = hotspotDto.Type,
                Text = hotspotDto.Text,
                SceneIdTarget = hotspotDto.SceneIdTarget,
                Url = hotspotDto.URL,
                Target = hotspotDto.Target,
                Image = hotspotDto.Image,
                Width = hotspotDto.Width,
                Height = hotspotDto.Height,
                HotspotIdentifier = hotspotDto.HotspotIdentifier
            };

            scene.Hotspots.Add(newHotspot);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Hotspot added successfully!" });
            }
            catch (DbUpdateException ex)
            {
                var sqlException = ex.InnerException as SqlException;
                if (sqlException != null && sqlException.Number == 547)
                {
                    return BadRequest(new
                    {
                        message = "Foreign Key constraint failed.",
                        detail = "Ensure SceneId and SceneIdTarget exist in the Scene table."
                    });
                }

                return StatusCode(500, new
                {
                    message = "An error occurred while adding the hotspot.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }


        [HttpPut("{sceneId}/hotspot/{hotspotId}")]
        public async Task<IActionResult> UpdateHotspot(string sceneId, int hotspotId, [FromBody] HotspotDTO hotspotDto)
        {
            if (hotspotDto == null)
            {
                return BadRequest(new { message = "Invalid hotspot data" });
            }
            var hotspot = await _context.Hotspots.FirstOrDefaultAsync(h => h.SceneId == sceneId && h.HotspotId == hotspotId);

            if (hotspot == null)
            {
                return NotFound(new { message = "Hotspot not found in the specified scene" });
            }
            hotspot.Pitch = hotspotDto.Pitch;
            hotspot.Yaw = hotspotDto.Yaw;
            hotspot.Type = hotspotDto.Type;
            hotspot.Text = hotspotDto.Text;
            hotspot.SceneIdTarget = hotspotDto.SceneIdTarget;
            hotspot.Url = hotspotDto.URL;
            hotspot.Target = hotspotDto.Target;
            hotspot.Image = hotspotDto.Image;
            hotspot.Width = hotspotDto.Width;
            hotspot.Height = hotspotDto.Height;
            hotspot.HotspotIdentifier = hotspotDto.HotspotIdentifier;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Hotspot updated successfully"});

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the hotspot", error = ex.InnerException?.Message ?? ex.Message });

            }

        }


        [HttpDelete("{sceneId}")]
        public async Task<IActionResult> DeleteScene(string sceneId)
        {
            // Tìm Scene cần xóa
            var scene = await _context.Scenes
                .Include(s => s.Hotspots)  // Bao gồm các Hotspot liên quan
                .Include(s => s.Audios)         // Bao gồm các Audio liên quan
                .FirstOrDefaultAsync(s => s.SceneId == sceneId);

            if (scene == null)
            {
                return NotFound(new { message = "Scene không tồn tại." });
            }

            _context.Audios.RemoveRange(scene.Audios);

            var hotspotsTargetingScene = await _context.Hotspots
                .Where(h => h.SceneIdTarget == sceneId)
                .ToListAsync();
            _context.Hotspots.RemoveRange(hotspotsTargetingScene);

            _context.Hotspots.RemoveRange(scene.Hotspots);

            _context.Scenes.Remove(scene);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Scene và các Hotspot và Audio liên quan đã được xóa thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the scene.", error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpDelete("{sceneId}/{hotspotId}")]
        public async Task<IActionResult> DeleteHotspot(string sceneId, int hotspotId)
        {
            var hotspot = await _context.Hotspots.Where(h => h.SceneId == sceneId && h.HotspotId == hotspotId).FirstOrDefaultAsync();
            if (hotspot == null)
            {
                return NotFound(new { message = "Hotspot không tồn tại trong Scene này" });
            }
            _context.Hotspots.Remove(hotspot);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { massage = "Hotspot đã được xóa thành công khỏi Scene" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { massage = "An error occurred while deleting the hotspot", error = ex.InnerException?.Message ?? ex.Message });

            }
        }
    }
}

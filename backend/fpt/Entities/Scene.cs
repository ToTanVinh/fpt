using System;
using System.Collections.Generic;

namespace fpt.Entites;

public partial class Scene
{
    public string SceneId { get; set; } = null!;

    public string? Title { get; set; }

    public int? Hfov { get; set; }

    public double? Pitch { get; set; }

    public double? Yaw { get; set; }

    public string? Type { get; set; }

    public string? Panorama { get; set; }

    public string? AudioVn { get; set; }

    public string? AudioEn { get; set; }

    public virtual ICollection<Audio> Audios { get; set; } = new List<Audio>();

    public virtual ICollection<Hotspot> Hotspots { get; set; } = new List<Hotspot>();
}

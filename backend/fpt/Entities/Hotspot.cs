using System;
using System.Collections.Generic;

namespace fpt.Entites;

public partial class Hotspot
{
    public int HotspotId { get; set; }

    public string? SceneId { get; set; }

    public string? Type { get; set; }

    public double? Pitch { get; set; }

    public double? Yaw { get; set; }

    public string? Text { get; set; }

    public string? SceneIdTarget { get; set; }

    public string? Url { get; set; }

    public string? Target { get; set; }

    public string? Image { get; set; }

    public string? Width { get; set; }

    public string? Height { get; set; }

    public string? HotspotIdentifier { get; set; }

    public virtual Scene? Scene { get; set; }
}

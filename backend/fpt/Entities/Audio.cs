using System;
using System.Collections.Generic;

namespace fpt.Entites;

public partial class Audio
{
    public int AudioId { get; set; }

    public string? SceneId { get; set; }

    public string? Language { get; set; }

    public string? AudioFile { get; set; }

    public virtual Scene? Scene { get; set; }
}

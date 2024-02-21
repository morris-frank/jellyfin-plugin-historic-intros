using System;
using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.HistoricalIntros.Configuration;

public class IntroPluginConfiguration : BasePluginConfiguration
{
    public string TrailersPath { get; set; } = string.Empty;

    public string PrerollsPath { get; set; } = string.Empty;

    public string PreTrailersPath { get; set; } = string.Empty;

    public int NumberOfTrailers { get; set; } = 1;

}

public class IntroVideo
{
    public string Name { get; set; }

    public Guid ItemId { get; set; }
}

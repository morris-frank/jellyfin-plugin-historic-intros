
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Jellyfin.Plugin.HistoricalIntros.Configuration;
using Jellyfin.Data.Entities;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Entities.Movies;
using Microsoft.Extensions.Logging;


namespace Jellyfin.Plugin.HistoricalIntros;
public class IntroProvider : IIntroProvider
{
    private readonly ILogger<IntroProvider> logger;

    public IntroProvider(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger<IntroProvider>();
    }

    public string Name { get; } = "Intros";

    public Task<IEnumerable<IntroInfo>> GetIntros(BaseItem item, User user)
    {
        try
        {
            return Task.FromResult(LoadLocalFileIntros(item));
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Error retrieving intros");
            return Task.FromResult(Enumerable.Empty<IntroInfo>());
        }
    }

    private readonly CookieContainer _cookieContainer = new CookieContainer();

    private readonly Random _random = new Random();

    private IEnumerable<IntroInfo> LoadLocalFileIntros(BaseItem item)
    {
        if (item.GetBaseItemKind() != Data.Enums.BaseItemKind.Movie)
        {
            throw new Exception("Pre-rolls only for movies");
        }

        var year = item.ProductionYear ?? DateTime.Now.Year;

        var allTrailers = HistoricalIntrosPlugin.LibraryManager.GetItemsResult(new InternalItemsQuery
        {
            Years = new[] { year },
            HasAnyProviderId = new Dictionary<string, string>
            {
                {"trailers.prerolls.video", ""}
            }
        }).Items.ToList();
        var NTrailers = Math.Min(HistoricalIntrosPlugin.Instance.Configuration.NumberOfTrailers, allTrailers.Count);
        var trailers = allTrailers.OrderBy(x => _random.Next()).Take(NTrailers).ToList();
        logger.LogDebug("Found {0} trailers for {2} using {1}", allTrailers.Count, trailers.Count, item.Name);

        var prerolls = HistoricalIntrosPlugin.LibraryManager.GetItemsResult(new InternalItemsQuery
        {
            HasAnyProviderId = new Dictionary<string, string>
            {
                {"prerolls.prerolls.video", ""}
            }
        }).Items.ToList();
        logger.LogDebug("Found {0} prerolls", prerolls.Count);

        return trailers.Concat(prerolls).Select(x => new IntroInfo
        {
            Path = x.Path,
            ItemId = item.Id,
        });
    }

}

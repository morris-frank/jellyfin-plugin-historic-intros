
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Jellyfin.Plugin.HistoricIntros.Configuration;
using Jellyfin.Data.Entities;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Entities.Movies;
using Microsoft.Extensions.Logging;


namespace Jellyfin.Plugin.HistoricIntros;
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

    private static string prerollsPath => HistoricIntrosPlugin.Instance.Configuration.PrerollsPath;

    private static string trailersPath => HistoricIntrosPlugin.Instance.Configuration.TrailersPath;

    private static int numberOfTrailers => HistoricIntrosPlugin.Instance.Configuration.NumberOfTrailers;

    private IEnumerable<IntroInfo> LoadLocalFileIntros(BaseItem item)
    {
        if (item.GetBaseItemKind() != Data.Enums.BaseItemKind.Movie)
        {
            throw new Exception("Pre-rolls only for movies");
        }

        var year = item.ProductionYear ?? DateTime.Now.Year;

        var trailers = HistoricIntrosPlugin.LibraryManager.GetItemsResult(new InternalItemsQuery
        {
            Years = new[] { year },
            HasAnyProviderId = new Dictionary<string, string>
            {
                {"intros.trailers.video", ""}
            }
        }).Items.ToList();
        logger.LogDebug("Found {0} trailers for {1}", trailers.Count, item.Name);

        var prerolls = HistoricIntrosPlugin.LibraryManager.GetItemsResult(new InternalItemsQuery
        {
            HasAnyProviderId = new Dictionary<string, string>
            {
                {"intros.prerolls.video", ""}
            }
        }).Items.ToList();
        logger.LogDebug("Found {0} prerolls", prerolls.Count);

        return trailers.Concat(prerolls).Select(x => new IntroInfo
        {
            Path = x.Path,
            ItemId = item.Id,
        });

        //         var NTrailers = Math.Min(numberOfTrailers, trailerFiles.Length);
        //         var trailers = trailerFiles.OrderBy(x => _random.Next()).Take(NTrailers);
    }

}

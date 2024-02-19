
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
            logger.LogError(e, "Error retrieving intros");
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
        if (
            HistoricIntrosPlugin.Instance.Configuration.PrerollsPath == string.Empty ||
            HistoricIntrosPlugin.Instance.Configuration.TrailersPath == string.Empty ||
            !Directory.Exists(trailersPath) ||
            !Directory.Exists(prerollsPath) ||
            HistoricIntrosPlugin.Instance.Configuration.NumberOfTrailers == 0
        )
        {
            throw new Exception("Invalid configuration");
        }

        if (item.GetBaseItemKind() != Data.Enums.BaseItemKind.Movie)
        {
            throw new Exception("Pre-rolls only for movies");
        }


        var intros = new List<string>();

        // Load trailers
        var premiereYear = item.PremiereDate?.Year ?? DateTime.Now.Year;
        var trailerYearPath = Path.Combine(trailersPath, premiereYear.ToString());
        if (Directory.Exists(trailerYearPath))
        {
            var trailerFiles = Directory.GetFiles(trailerYearPath);
            trailerFiles = trailerFiles.Where(x => !Path.GetFileName(x).StartsWith("._")).ToArray();
            if (trailerFiles.Length > 0)
            {
                var NTrailers = Math.Min(numberOfTrailers, trailerFiles.Length);
                var trailers = trailerFiles.OrderBy(x => _random.Next()).Take(NTrailers);
                foreach (var trailer in trailers)
                {
                    intros.Add(trailer);
                }

            }
        }

        // Load intros
        var introFiles = Directory.GetFiles(prerollsPath);
        introFiles = introFiles.Where(x => !Path.GetFileName(x).StartsWith("._")).ToArray();
        if (introFiles.Length > 0)
        {
            var prerolls = introFiles.OrderBy(x => x);
            foreach (var preroll in prerolls)
            {
                intros.Add(preroll);
            }
        }


        logger.LogInformation("Found {0} intros for {1}", intros.Count, item.Name);
        logger.LogInformation("Intros: {0}", string.Join(", ", intros));

        return intros.Select(x => new IntroInfo
        {
            Path = x,
        });
    }

}

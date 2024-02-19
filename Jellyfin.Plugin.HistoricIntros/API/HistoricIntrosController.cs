using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using MediaBrowser.Common;
using MediaBrowser.Controller.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Jellyfin.Plugin.HistoricIntros.Configuration;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HistoricIntros;


[ApiController]
[Authorize(Policy = "RequiresElevation")]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class HistoricIntrosController : ControllerBase
{
    private readonly ILogger<HistoricIntrosController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LdapController"/> class.
    /// </summary>
    /// <param name="appHost">The application host to get the LDAP Authentication Provider from.</param>
    public HistoricIntrosController(IApplicationHost appHost, ILoggerFactory loggerFactory)
    {
        this.logger = loggerFactory.CreateLogger<HistoricIntrosController>();
    }

    [HttpPost("LoadIntros")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult LoadIntros()
    {
        logger.LogDebug("Loading Intros");
        PopulateIntroLibrary();

        var inLibrary = HistoricIntrosPlugin.LibraryManager.GetItemsResult(new InternalItemsQuery
        {
            Tags = new[] { "intros.video" },
            HasAnyProviderId = new Dictionary<string, string>
            {
                {"intros.trailers.video", ""},
                {"intros.prerolls.video", ""}
            }
        }).Items.ToDictionary(x => x.Path, x => x);
        foreach (var item in inLibrary)
        {
            logger.LogDebug("Found {0}", item.Value.Name);
        }
        return Ok();
    }

    private static string trailersPath => HistoricIntrosPlugin.Instance.Configuration.TrailersPath;
    private static string prerollsPath => HistoricIntrosPlugin.Instance.Configuration.PrerollsPath;
    private static int numberOfTrailers => HistoricIntrosPlugin.Instance.Configuration.NumberOfTrailers;

    private void PopulateIntroLibrary()
    {
        var inLibrary = HistoricIntrosPlugin.LibraryManager.GetItemsResult(new InternalItemsQuery
        {
            Tags = new[] { "intros.video" },
            HasAnyProviderId = new Dictionary<string, string>
            {
                {"intros.trailers.video", ""},
                {"intros.prerolls.video", ""}
            }
        }).Items.ToDictionary(x => x.Path, x => x);

        var trailerFolders = Directory.GetDirectories(trailersPath);
        foreach (var folder in trailerFolders)
        {
            var year = Path.GetFileName(folder);
            var trailers = Directory.GetFiles(folder);
            foreach (var path in trailers)
            {
                if (inLibrary.ContainsKey(path) || path.StartsWith("."))
                {
                    continue;
                }

                var item = new Video
                {
                    Id = Guid.NewGuid(),
                    Name = Path.GetFileNameWithoutExtension(path),
                    Path = path,
                    ProductionYear = int.Parse(year),
                    ProviderIds = new Dictionary<string, string>
                    {
                        {"intros.trailers.video", ""}
                    },
                };
                logger.LogDebug("Creating trailer {0}", item.Name);
                HistoricIntrosPlugin.LibraryManager.CreateItem(item, null);
            }
        }

        var prerolls = Directory.GetFiles(prerollsPath, "*.*", SearchOption.AllDirectories);
        foreach (var path in prerolls)
        {
            if (inLibrary.ContainsKey(path) || path.StartsWith("."))
            {
                continue;
            }

            var item = new Video
            {
                Id = Guid.NewGuid(),
                Name = Path.GetFileNameWithoutExtension(path),
                Path = path,
                ProviderIds = new Dictionary<string, string>
                {
                    {"intros.prerolls.video", ""}
                },
            };
            logger.LogDebug("Creating preroll {0}", item.Name);
            HistoricIntrosPlugin.LibraryManager.CreateItem(item, null);
        }


    }

}
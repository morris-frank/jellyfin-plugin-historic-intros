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
using Jellyfin.Plugin.HistoricalIntros.Configuration;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HistoricalIntros;


[ApiController]
[Authorize(Policy = "RequiresElevation")]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class HistoricalIntrosController : ControllerBase
{
    private readonly ILogger<HistoricalIntrosController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LdapController"/> class.
    /// </summary>
    /// <param name="appHost">The application host to get the LDAP Authentication Provider from.</param>
    public HistoricalIntrosController(IApplicationHost appHost, ILoggerFactory loggerFactory)
    {
        this.logger = loggerFactory.CreateLogger<HistoricalIntrosController>();
    }

    [HttpPost("LoadIntros")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult LoadIntros()
    {
        logger.LogDebug("Loading Intros");
        PopulateIntroLibrary();
        return Ok();
    }

    private static string trailersPath => HistoricalIntrosPlugin.Instance.Configuration.TrailersPath;
    private static string prerollsPath => HistoricalIntrosPlugin.Instance.Configuration.PrerollsPath;
    private static string preTrailersPath => HistoricalIntrosPlugin.Instance.Configuration.PreTrailersPath;
    private static int numberOfTrailers => HistoricalIntrosPlugin.Instance.Configuration.NumberOfTrailers;

    private void DeleteByProviderId(string providerId)
    {
        logger.LogDebug("Deleting intros with providerId {0}", providerId);
        HistoricalIntrosPlugin.LibraryManager.GetItemsResult(new InternalItemsQuery
        {
            HasAnyProviderId = new Dictionary<string, string>
            {
                {providerId, ""}
            }
        }).Items.ToList().ForEach(x =>
        {
            logger.LogDebug("Deleting {0}", x);
            HistoricalIntrosPlugin.LibraryManager.DeleteItem(x, new DeleteOptions());
        });
    }

    private void PopulateIntroLibrary()
    {
        DeleteByProviderId("pretrailers.prerolls.video");
        DeleteByProviderId("trailers.prerolls.video");
        DeleteByProviderId("prerolls.prerolls.video");

        var trailerFolders = Directory.GetDirectories(trailersPath);
        foreach (var folder in trailerFolders)
        {
            var year = Path.GetFileName(folder);
            var trailers = Directory.GetFiles(folder);
            foreach (var path in trailers)
            {
                var name = Path.GetFileNameWithoutExtension(path);
                if (name.StartsWith("."))
                {
                    continue;
                }

                var item = new Video
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Path = path,
                    ProductionYear = int.Parse(year),
                    ProviderIds = new Dictionary<string, string>
                    {
                        {"trailers.prerolls.video", path}
                    },
                };
                logger.LogDebug("Creating trailer {0} ({1})", item.Name, item.ProductionYear);
                HistoricalIntrosPlugin.LibraryManager.CreateItem(item, null);
            }
        }

        var prerolls = Directory.GetFiles(prerollsPath, "*.*", SearchOption.AllDirectories);
        foreach (var path in prerolls)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            if (name.StartsWith("."))
            {
                continue;
            }

            var item = new Video
            {
                Id = Guid.NewGuid(),
                Name = name,
                Path = path,
                ProviderIds = new Dictionary<string, string>
                {
                    {"prerolls.prerolls.video", path}
                },
            };
            logger.LogDebug("Creating preroll {0}", item.Name);
            HistoricalIntrosPlugin.LibraryManager.CreateItem(item, null);
        }
        HistoricalIntrosPlugin.Instance.SaveConfiguration();

        var preTrailers = Directory.GetFiles(preTrailersPath, "*.*", SearchOption.AllDirectories);
        foreach (var path in preTrailers)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            if (name.StartsWith("."))
            {
                continue;
            }

            var item = new Video
            {
                Id = Guid.NewGuid(),
                Name = name,
                Path = path,
                ProviderIds = new Dictionary<string, string>
                {
                    {"pretrailers.prerolls.video", path}
                },
            };
            logger.LogDebug("Creating pretrailer {0}", item.Name);
            HistoricalIntrosPlugin.LibraryManager.CreateItem(item, null);
        }
    }

}
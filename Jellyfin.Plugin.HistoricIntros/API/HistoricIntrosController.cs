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
        return Ok();
    }

    private static string trailersPath => HistoricIntrosPlugin.Instance.Configuration.TrailersPath;
    private static string prerollsPath => HistoricIntrosPlugin.Instance.Configuration.PrerollsPath;
    private static int numberOfTrailers => HistoricIntrosPlugin.Instance.Configuration.NumberOfTrailers;

    private void DeleteByProviderId(string providerId)
    {
        logger.LogDebug("Deleting intros with providerId {0}", providerId);
        HistoricIntrosPlugin.LibraryManager.GetItemsResult(new InternalItemsQuery
        {
            HasAnyProviderId = new Dictionary<string, string>
            {
                {providerId, ""}
            }
        }).Items.ToList().ForEach(x =>
        {
            logger.LogDebug("Deleting {0}", x);
            HistoricIntrosPlugin.LibraryManager.DeleteItem(x, new DeleteOptions());
        })
    }

    private void PopulateIntroLibrary()
    {
        DeleteByProviderId("intros.trailers.video");
        DeleteByProviderId("intros.prerolls.video");

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
                        {"intros.trailers.video", path}
                    },
                };
                logger.LogDebug("Creating trailer {0} ({1})", item.Name, item.ProductionYear);
                HistoricIntrosPlugin.LibraryManager.CreateItem(item, null);
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
                    {"intros.prerolls.video", path}
                },
            };
            logger.LogDebug("Creating preroll {0}", item.Name);
            HistoricIntrosPlugin.LibraryManager.CreateItem(item, null);
        }


    }

}
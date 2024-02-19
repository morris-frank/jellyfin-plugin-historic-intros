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

    private static string trailersPath => HistoricIntrosPlugin.Instance.Configuration.TrailersPath;
    private static string prerollsPath => HistoricIntrosPlugin.Instance.Configuration.PrerollsPath;
    private static int numberOfTrailers => HistoricIntrosPlugin.Instance.Configuration.NumberOfTrailers;
}
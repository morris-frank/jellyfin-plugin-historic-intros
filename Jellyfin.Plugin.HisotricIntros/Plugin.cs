using System;
using System.Collections.Generic;
using Jellyfin.Plugin.HistoricIntros.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.HistoricIntros
{
    public class HistoricIntrosPlugin : BasePlugin<IntroPluginConfiguration>, IHasWebPages
    {
        public override string Name => "Historic Intros";

        public override Guid Id => Guid.Parse("A03D1027-7034-4474-8A12-17C4DC62B17A");

        public const int DefaultResolution = 1080;

        public static HistoricIntrosPlugin Instance { get; private set; }

        public static ILibraryManager LibraryManager { get; private set; }

        public HistoricIntrosPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILibraryManager libraryManager)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            LibraryManager = libraryManager;
        }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            yield return new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.config.html"
            };
        }
    }
}

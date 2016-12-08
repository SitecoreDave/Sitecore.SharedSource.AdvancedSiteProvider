// Copyright (c) [2016] [David Walker] - MIT License - see License.txt
using System;
using System.Collections.Generic;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.HttpRequest;

namespace Sitecore.SharedSource.AdvancedSiteProvider
{
    /// <summary>
    /// AdvancedSiteProvider
    /// </summary>
    [UsedImplicitly]
    public class AdvancedSiteProviderSiteResolver : SiteResolver
    {
        private static string Namespace => typeof(AdvancedSiteProvider).Module.Name.Replace(".dll", "");
        //private static List<SiteInfo> _sites;
        public override void Process(HttpRequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            try
            {
                //intermittently not resolving...
                //insures Advanced Sites are loaded, otherwise they don't resolve and even if below stops resolution
                //if (_sites == null)
                //{
                //_sites = SiteContextFactory.Sites;
                //}
                //even this fails intermittenly:
                //new AdvancedSiteProvider().InitializeSites();

                //this is needed to get the sites added for some reason. Would think the SiteProvider would be enough
                var siteProvider = new AdvancedSiteProvider();
                var sites = siteProvider.GetSites();
                
                var site = ResolveSiteContext(args);

                //skip systemSites we only care about actual sites, also shell breaks?
                var systemSites = new List<string>()
                {
                    {"scheduler"},
                    {"shell" },
                    {"system"},
                    {"publisher"}
                };
                if (systemSites.Contains(site.Name)) return;

                //older stuff:
                //we only lookup websites
                //if (site.Name != "website") return;

                //var newSite = GetAdvancedSite(site);
                //if (newSite.Name == "website") return;

                UpdatePaths(args, site);
                Context.Site = site;

                //Log.Info($"{Namespace} set Context.Site to: {newSite.Name}", typeof(AdvancedSiteProviderSiteResolver));
            }
            catch (Exception exception)
            {
                Log.Error($"Error in {Namespace}.Process: {exception}", exception, typeof(AdvancedSiteProviderSiteResolver));
            }
        }
    }
}
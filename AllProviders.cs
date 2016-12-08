// Copyright (c) [2016] [David Walker] - MIT License - see License.txt
using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Diagnostics;
using Sitecore.Sites;

namespace Sitecore.SharedSource.AdvancedSiteProvider
{
    public class AllProviders : SiteProvider
    {
        private static string Namespace => typeof(AdvancedSiteProvider).Module.Name.Replace(".dll", "");

        /// <summary>The initialize lock.</summary>
        private readonly object _initializeLock = new object();

        /// <summary>The list of providers.</summary>
        private IEnumerable<SiteProvider> _providers;

        /// <summary>Gets the providers.</summary>
        /// <value>The provider.</value>
        public IEnumerable<SiteProvider> Providers
        {
            get
            {
                if (_providers != null)
                {
                    return _providers;
                }
                lock (_initializeLock)
                {
                    _providers = ReadProviders();
                }
                return _providers;
            }
        }

        /// <summary>Gets a site.</summary>
        /// <param name="siteName">Name of the site.</param>
        /// <returns>A site.</returns>
        public override Site GetSite(string siteName)
        {
            Assert.ArgumentNotNull(siteName, "siteName");
            try
            {
                return Providers.Select(p => p.GetSite(siteName)).FirstOrDefault(s => s != null);
            }
            catch (Exception exception)
            {
                Log.Error($"Error in {Namespace}.GetSiteDefinitionItems: {exception}", exception);
            }
            return null;
        }

        /// <summary>Gets the list of all known sites.</summary>
        /// <returns>The list of all known sites.</returns>
        public override SiteCollection GetSites()
        {
            var siteCollection = new SiteCollection();
            try
            {
                siteCollection.AddRange(Providers.SelectMany(p => p.GetSites()));
            }
            catch (Exception exception)
            {
                Log.Error($"Error in {Namespace}.GetSiteDefinitionItems: {exception}", exception);
            }
            return siteCollection;
        }

        /// <summary>Reads the referenced providers.</summary>
        /// <returns>The referenced providers.</returns>
        protected IEnumerable<SiteProvider> ReadProviders()
        {
            var siteProviderList = new List<SiteProvider>();
            try
            {
                foreach (SiteProvider provider in SiteManager.Providers)
                {
                    if (provider.Name.ToLower() == "allproviders") continue;

                    //var provider = SiteManager.Providers[providerName];
                    Assert.IsNotNull(provider, $"Site provider '{provider.Name}' cannot be found.");
                    siteProviderList.Add(provider);
                }
            }
            catch (Exception exception)
            {
                Log.Error($"Error in {Namespace}.GetSiteDefinitionItems: {exception}", exception);
            }
            return siteProviderList;
        }
    }
}
// Copyright (c) [2016] [David Walker] - MIT License - see License.txt
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Sites;
using Sitecore.Web;
using StringDictionary = Sitecore.Collections.StringDictionary;

namespace Sitecore.SharedSource.AdvancedSiteProvider
{
    /// <summary>
    /// AdvancedSiteProvider
    /// </summary>
    public class AdvancedSiteProvider : SiteProvider
    {
        private static string Namespace => typeof(AdvancedSiteProvider).Module.Name.Replace(".dll", "");
        private readonly object _lock = new object();
        private SafeDictionary<string, Site> _siteDictionary;
        //private SiteCollection _sites;

        /// <summary>
        /// GetSites
        /// </summary>
        /// <returns></returns>
        public override SiteCollection GetSites()
        {
            InitializeSites();
            var sites = new SiteCollection();
            sites.AddRange(_siteDictionary.Values);
            return sites;
        }

        /// <summary>
        /// GetSite
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public override Site GetSite(string siteName)
        {
            Assert.ArgumentNotNullOrEmpty(siteName, "siteName");
            InitializeSites();
            Site site = null;
            _siteDictionary?.TryGetValue(siteName, out site);
            return site;
        }

        /// <summary>
        /// InitializeSites
        /// </summary>
        public void InitializeSites()
        {
            if (_siteDictionary != null) return;
            try
            {
                lock (_lock)
                {
                    if (_siteDictionary != null) return;

                    //var sites = new SiteCollection();
                    var siteDictionary = new SafeDictionary<string, Site>(StringComparer.OrdinalIgnoreCase);

                    var database = Context.Database ??
                                   Factory.GetDatabase("web", false) ?? Factory.GetDatabase("master", false);

                    if (database == null)
                    {
                        return;
                    }

                    foreach (var siteItem in GetSiteDefinitionItems(database))
                    {
                        var site = ResolveSite(siteItem);
                        if (site == null) continue;
                        siteDictionary[site.Name] = site;
                        Log.Info($"{Namespace} added site: {site.Name}", typeof(AdvancedSiteProvider));
                        //sites.Add(site);
                    }

                    //_sites = sites;
                    _siteDictionary = siteDictionary;
                }
            }
            catch(Exception exception)
            {
                Log.Error($"Error in {Namespace}.GetSiteDefinitionItems: {exception}", exception);
            }
        }
        
        /// <summary>
        /// GetSiteDefinitionItems - get all site items under the root folder 
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public virtual List<Item> GetSiteDefinitionItems(Database db)
        {
            if (db == null)
            {
                Log.Warn("GetSiteDefinitionItem failed. Database argument is null", this);
                return null;
            }

            var sites = new List<Item>();

            try
            {
                using (var context = ContentSearchManager.GetIndex("sitecore_web_index").CreateSearchContext())
                {
                    var siteItems = context.GetQueryable<SearchResultItem>().Where(
                        i => i.TemplateName.Contains("Site") && !i.Name.Equals("__Standard Values")
                             && !i.TemplateName.Contains("Sites"));

                    foreach (var siteSearchResultItem in siteItems)
                    {
                        var siteItem = siteSearchResultItem.GetItem();

                        if (siteItem.Parent.TemplateName.Contains("Sites"))
                        {
                            sites.Add(siteItem);
                        }
                    }
                }

                //Item root = db.GetItem(ItemIDs.ContentRoot);
                //if (root != null)
                //{
                //    sites.AddRange(root.GetChildren(ChildListOptions.SkipSorting).Where(siteItem => siteItem.TemplateID == TemplateIDs.ExternalSite));
                //}
            }
            catch (Exception exception)
            {
                Log.Error($"Error in {Namespace}.GetSiteDefinitionItems: {exception}", exception);
            }
            return sites;
        }

        /// <summary>
        /// ResolveSite
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual Site ResolveSite(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            try
            {
                if (IsValidSiteNameBySitecore(item.Name))
                {
                    var properties = GetProperties(item);
                    return new Site(item.Name, properties);
                }
            }
            catch (Exception exception)
            {
                Log.Error($"Error in {Namespace}.ResolveSite: {exception}", exception);
            }
            return null;
        }

        /// <summary>
        /// GetProperties
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual StringDictionary GetProperties(Item item)
        {
            try
            {
                var host = WebUtil.GetRequestUri().Host;
                if (host.Count(s => s == '.') > 1)
                {
                    var index = host.LastIndexOf(".", StringComparison.Ordinal);
                    index = host.LastIndexOf(".", index - 1, StringComparison.Ordinal);
                    host = host.Remove(0, index);
                }
                //NameValueListField field2 = item.Fields[FieldIDs.SiteResolver.SiteParameters];

                //var collection = field2.NameValues;

                //foreach (string key in collection)
                //{
                //    parameters.Add(key, HttpUtility.UrlDecode(collection[key]));
                //}
                //var configSiteProvider = new ConfigSiteProvider();
                //var defaultSiteContext = configSiteProvider.GetSite("website");
                //var parameters = new StringDictionary
                //{
                //    {"virtualFolder", defaultSiteContext.Properties["VirtualFolder"]},
                //    {"physicalFolder", defaultSiteContext.Properties["PhysicalFolder"]},
                //    {"rootPath", defaultSiteContext.Properties["RootPath"]},
                //    {"startItem", defaultSiteContext.Properties["StartItem"]},
                //    {"database", defaultSiteContext.Properties["Database.Name"]},
                //    {"domain", defaultSiteContext.Properties["Domain.Name"]},
                //    {"allowDebug", defaultSiteContext.Properties["AllowDebug"]},
                //    {"cacheHtml", defaultSiteContext.Properties["CacheHtml"]},
                //    {"htmlCacheSize", "10MB"},
                //    {"registryCacheSize", "0"},
                //    {"viewStateCacheSize", "0"},
                //    {"xslCacheSize", "5MB"},
                //    {"filteredItemsCacheSize", "2MB"},
                //    {"enablePreview", defaultSiteContext.Properties["EnablePreview"]},
                //    {"enableWebEdit", defaultSiteContext.Properties["EnableWebEdit"]},
                //    {"enableDebugger", defaultSiteContext.Properties["EnableDebugger"]},
                //    {"disableClientData", defaultSiteContext.Properties["DisableClientData"]},
                //    {"name", item.Name },
                //    {"hostName", host},
                //    {"startItem","/" + item.Name },
                //    {"rootPath", item.Parent.Paths.FullPath }
                //};

                var parameters = new StringDictionary
                {
                    {"virtualFolder", "/"},
                    {"physicalFolder", "/"},
                    {"database", "web"},
                    {"domain", "extranet"},
                    {"allowDebug", "true"},
                    {"cacheHtml", "true"},
                    {"htmlCacheSize", "10MB"},
                    {"registryCacheSize", "0"},
                    {"viewStateCacheSize", "0"},
                    {"xslCacheSize", "5MB"},
                    {"filteredItemsCacheSize", "2MB"},
                    {"enablePreview", "true"},
                    {"enableWebEdit", "true"},
                    {"enableDebugger", "true"},
                    {"disableClientData", "false"},
                    {"name", item.Name},
                    {"hostName", item.Name + host},
                    {"startItem", "/" + item.Name},
                    {"rootPath", item.Parent.Paths.FullPath}
                };

                return parameters;
            }
            catch (Exception exception)
            {
                Log.Error($"Error in {Namespace}.GetProperties: {exception}", exception);
            }
            return new StringDictionary();
        }

        /// <summary>
        /// IsValidSiteNameBySitecore - found somewhere... is this needed?
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static bool IsValidSiteNameBySitecore(string siteName)
        {
            try
            {
                //workaround for invalid site names
                var tmp = Factory.CreateObject("cacheSizes/sites/" + siteName, false) as string;
                return true;
            }
            catch (Exception exception)
            {
                Log.Error($"Error in {Namespace}: - Invalid SiteName - {exception}", exception);
                return false;
            }
        }
    }
}
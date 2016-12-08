// Copyright (c) [2016] [David Walker] - MIT License - see License.txt
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Links;

namespace Sitecore.SharedSource.AdvancedSiteProvider
{
    /// <summary>
    /// AdvancedSiteProvider
    /// </summary>
    public class AdvancedSiteProviderLinkProvider : LinkProvider
    {
        /// <summary>
        /// GetDefaultUrlOptions
        /// </summary>
        /// <returns></returns>
        public override UrlOptions GetDefaultUrlOptions()
        {
            var urlOptions = base.GetDefaultUrlOptions();
            urlOptions.SiteResolving = Settings.Rendering.SiteResolving;
            return urlOptions;
        }

        /// <summary>
        /// GetItemUrl
        /// </summary>
        /// <param name="item"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override string GetItemUrl(Item item, UrlOptions options)
        {
            options.SiteResolving = Settings.Rendering.SiteResolving;
            var itemUrl = base.GetItemUrl(item, options);
            return itemUrl;
        }
    }
}
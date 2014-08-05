﻿using System.Globalization;
using System.Linq;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
using Orchard.ContentManagement;

namespace Orchard.AuditTrail.Providers.Content {
    public class ContentAuditTrailEventProvider : AuditTrailEventProviderBase {
        private readonly IContentManager _contentManager;
        public ContentAuditTrailEventProvider(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public const string Created = "Created";
        public const string Saved = "Saved";
        public const string Published = "Published";
        public const string Unpublished = "Unpublished";
        public const string Removed = "Removed";

        public static Filters CreateFilters(int contentId, IUpdateModel updateModel) {
            return new Filters(updateModel) {
                {"content", contentId.ToString(CultureInfo.InvariantCulture)}
            };
        }

        public override void Describe(DescribeContext context) {
            context.For("Content", T("Content Items"))
                .Event(this, Created, T("Created"), T("A content item was created."), enableByDefault: true)
                .Event(this, Saved, T("Saved"), T("A content item was saved."), enableByDefault: true)
                .Event(this, Published, T("Published"), T("A content item was published."), enableByDefault: true)
                .Event(this, Unpublished, T("Unpublished"), T("A content item was unpublished."), enableByDefault: true)
                .Event(this, Removed, T("Removed"), T("A content item was deleted."), enableByDefault: true);

            context.QueryFilter(QueryFilter);
            context.DisplayFilter(DisplayFilter);
        }

        private void QueryFilter(QueryFilterContext context) {
            if (!context.Filters.ContainsKey("content"))
                return;

            var contentId = context.Filters["content"].ToInt32();
            context.Query = context.Query.Where(x => x.EventFilterKey == "content" && x.EventFilterData == contentId.ToString());
        }

        private void DisplayFilter(DisplayFilterContext context) {
            var contentItemId = context.Filters.Get("content").ToInt32();
            if (contentItemId != null) {
                var contentItem = contentItemId != null ? _contentManager.Get(contentItemId.Value, VersionOptions.Latest) : default(ContentItem);
                var filterDisplay = context.ShapeFactory.AuditTrailFilter__ContentItem(ContentItem: contentItem);

                context.FilterDisplay.Add(filterDisplay);
            }
        }
    }
}
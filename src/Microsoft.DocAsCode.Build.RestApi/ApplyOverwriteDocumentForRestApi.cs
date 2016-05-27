﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.Build.RestApi
{
    using System;
    using System.Collections.Generic;
    using System.Composition;
    using System.Linq;

    using Microsoft.DocAsCode.Build.Common;
    using Microsoft.DocAsCode.Build.RestApi.ViewModels;
    using Microsoft.DocAsCode.DataContracts.Common;
    using Microsoft.DocAsCode.Plugins;

    [Export(nameof(RestApiDocumentProcessor), typeof(IDocumentBuildStep))]
    public class ApplyOverwriteDocumentForRestApi : ApplyOverwriteDocument
    {
        public override string Name => nameof(ApplyOverwriteDocumentForRestApi);

        public override int BuildOrder => 0x10;

        public Func<FileModel, string, IHostService, IEnumerable<RestApiRootItemViewModel>> GetRootItemsFromOverwriteDocument =
            (((overwriteModel, uid, host) =>
            {
                return OverwriteDocumentReader.Transform<RestApiRootItemViewModel>(
                    overwriteModel,
                    uid,
                    s => (RestApiRootItemViewModel)BuildRestApiDocument.BuildItem(host, s, overwriteModel, content => content != null && content.Trim() == Constants.ContentPlaceholder));
            }));

        public Func<FileModel, string, IHostService, IEnumerable<RestApiRootItemViewModel>> GetRootItemsToOverwrite =
            (((articleModel, uid, host) => new[] { (RestApiRootItemViewModel)articleModel.Content }));

        public Func<FileModel, string, IHostService, IEnumerable<RestApiChildItemViewModel>> GetChildItemsFromOverwriteDocument =
            (((overwriteModel, uid, host) =>
            {
                return OverwriteDocumentReader.Transform<RestApiChildItemViewModel>(
                    overwriteModel,
                    uid,
                    s => (RestApiChildItemViewModel)BuildRestApiDocument.BuildItem(host, s, overwriteModel, content => content != null && content.Trim() == Constants.ContentPlaceholder));
            }));

        public Func<FileModel, string, IHostService, IEnumerable<RestApiChildItemViewModel>> GetChildItemsToOverwrite =
            (((articleModel, uid, host) =>
            {
                return ((RestApiRootItemViewModel)articleModel.Content).Children.Where(c => c.Uid == uid);
            }));

        protected override void ApplyOverwrite(IHostService host, List<FileModel> od, string uid, List<FileModel> articles)
        {
            if (articles.Any(a => uid == ((RestApiRootItemViewModel)a.Content).Uid))
            {
                ApplyOverwrite(host, od, uid, articles, GetRootItemsFromOverwriteDocument, GetRootItemsToOverwrite);
            }
            else if (articles.Any(a => ((RestApiRootItemViewModel)a.Content).Children.Any(c => uid == c.Uid)))
            {
                ApplyOverwrite(host, od, uid, articles, GetChildItemsFromOverwriteDocument, GetChildItemsToOverwrite);
            }
        }
    }
}

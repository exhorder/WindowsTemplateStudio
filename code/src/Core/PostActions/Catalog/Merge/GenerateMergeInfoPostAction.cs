﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Templates.Core.Gen;

namespace Microsoft.Templates.Core.PostActions.Catalog.Merge
{
    public class GenerateMergeInfoPostAction : PostAction<string>
    {
        private const string Suffix = "postaction";
        private const string NewSuffix = "failedpostaction";

        public const string Extension = "_" + Suffix + ".";
        public const string PostactionRegex = @"(\$\S*)?(_" + Suffix + "|_g" + Suffix + @")\.";

        public GenerateMergeInfoPostAction(string relatedTemplate, string config)
            : base(relatedTemplate, config)
        {
        }

        internal override void ExecuteInternal()
        {
            var parentGenerationOutputPath = Directory.GetParent(GenContext.Current.GenerationOutputPath).FullName;
            var postAction = File.ReadAllText(Config).AsUserFriendlyPostAction();
            var sourceFile = Regex.Replace(Config, PostactionRegex, ".");
            var mergeType = GetMergeType();
            var relFilePath = sourceFile.Replace(parentGenerationOutputPath + Path.DirectorySeparatorChar, string.Empty);

            if (GenContext.Current.MergeFilesFromProject.ContainsKey(relFilePath))
            {
                var mergeFile = GenContext.Current.MergeFilesFromProject[relFilePath];

                mergeFile.Add(new MergeInfo { Format = mergeType, PostActionCode = postAction });
            }
        }

        private string GetMergeType()
        {
            switch (Path.GetExtension(Config).ToUpperInvariant())
            {
                case ".CS":
                    return "CSHARP";
                case ".VB":
                    return "VB.NET";
                case ".CSPROJ":
                case ".VBPROJ":
                case ".XAML":
                    return "XML";
                default:
                    return string.Empty;
            }
        }
    }
}

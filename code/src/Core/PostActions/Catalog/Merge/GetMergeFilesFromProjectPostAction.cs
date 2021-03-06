﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Templates.Core.Gen;

namespace Microsoft.Templates.Core.PostActions.Catalog.Merge
{
    public class GetMergeFilesFromProjectPostAction : PostAction<string>
    {
        public GetMergeFilesFromProjectPostAction(string relatedTemplate, string config)
            : base(relatedTemplate, config)
        {
        }

        internal override void ExecuteInternal()
        {
            if (Regex.IsMatch(Config, MergeConfiguration.GlobalExtension))
            {
                GetFileFromProject();
            }
            else
            {
                if (!CheckLocalMergeFileAvailable())
                {
                    GetFileFromProject();
                }
            }
        }

        private void GetFileFromProject()
        {
            var parentGenerationOutputPath = Directory.GetParent(GenContext.Current.GenerationOutputPath).FullName;
            var destinationParentPath = Directory.GetParent(GenContext.Current.DestinationPath).FullName;

            var filePath = GetMergeFileFromDirectory(Path.GetDirectoryName(Config.Replace(parentGenerationOutputPath, destinationParentPath)));
            var relFilePath = GetRelativePath(filePath, destinationParentPath + Path.DirectorySeparatorChar);

            if (!GenContext.Current.MergeFilesFromProject.ContainsKey(relFilePath))
            {
                GenContext.Current.MergeFilesFromProject.Add(relFilePath, new List<MergeInfo>());
                if (File.Exists(filePath))
                {
                    var destFile = filePath.Replace(destinationParentPath, parentGenerationOutputPath);
                    File.Copy(filePath, destFile, true);
                }
            }
        }

        private string GetRelativePath(string filePath, string rootPath)
        {
            var index = filePath.IndexOf(rootPath, StringComparison.OrdinalIgnoreCase);
            if (index == 0)
            {
                return filePath.Remove(0, rootPath.Length);
            }
            else
            {
                return filePath;
            }
        }

        private bool CheckLocalMergeFileAvailable()
        {
            var filePath = GetMergeFileFromDirectory(Path.GetDirectoryName(Config));
            return File.Exists(filePath) ? true : false;
        }

        private string GetMergeFileFromDirectory(string directory)
        {
            var filePath = Path.Combine(directory, Path.GetFileName(Config));
            var path = Regex.Replace(filePath, MergeConfiguration.PostactionAndSearchReplaceRegex, ".");

            return path;
        }
    }
}

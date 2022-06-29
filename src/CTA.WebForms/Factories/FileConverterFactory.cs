﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CTA.Rules.Config;
using CTA.WebForms.FileConverters;
using CTA.WebForms.Helpers;
using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.Metrics;
using CTA.WebForms.ProjectManagement;
using CTA.WebForms.Services;

namespace CTA.WebForms.Factories
{
    public class FileConverterFactory
    {
        private readonly string _sourceProjectPath;
        private readonly WorkspaceManagerService _blazorWorkspaceManager;
        private readonly ProjectAnalyzer _webFormsProjectAnalyzer;
        private readonly ViewImportService _viewImportService;
        private readonly CodeBehindReferenceLinkerService _codeBehindLinkerService;
        private readonly ClassConverterFactory _classConverterFactory;
        private readonly HostPageService _hostPageService;
        private readonly TaskManagerService _taskManagerService;
        private readonly TagConfigParser _tagConfigParser;
        private readonly WebFormMetricContext _metricsContext;

        // TODO: Organize these into "types" and force
        // content separation in file system if it doesn't
        // already exist
        public readonly HashSet<string> StaticResourceExtensions = new HashSet<string>
        {
            ".jpeg", ".jpg", ".jif", ".jfif", ".gif", ".tif", ".tiff", ".jp2", ".jpx", ".j2k", ".j2c", ".fpx", ".pcd",
            ".png", ".pdf", ".ico", ".css", ".map", ".eot", ".otf", ".svg", ".tff", ".woff", ".woff2", ".fnt",".fon",
            ".ttc", ".pfa", ".fot", ".sfd", ".vlw", ".pfb", ".etx", ".odttf", ".ttf"
        };

        public FileConverterFactory(
            string sourceProjectPath,
            WorkspaceManagerService blazorWorkspaceManager,
            ProjectAnalyzer webFormsProjectAnalyzer,
            ViewImportService viewImportService,
            CodeBehindReferenceLinkerService codeBehindLinkerService,
            ClassConverterFactory classConverterFactory,
            HostPageService hostPageService,
            TaskManagerService taskManagerService,
            TagConfigParser tagConfigParser,
            WebFormMetricContext metricsContext)
        {
            _sourceProjectPath = sourceProjectPath;
            _blazorWorkspaceManager = blazorWorkspaceManager;
            _webFormsProjectAnalyzer = webFormsProjectAnalyzer;
            _viewImportService = viewImportService;
            _codeBehindLinkerService = codeBehindLinkerService;
            _classConverterFactory = classConverterFactory;
            _hostPageService = hostPageService;
            _taskManagerService = taskManagerService;
            _tagConfigParser = tagConfigParser;
            _metricsContext = metricsContext;
        }

        public FileConverter Build(FileInfo document)
        {
            // NOTE
            // Existing Type:   FileInfo = System.IO.FileInfo
            // Our New Type:    FileInformation = CTA.WebForms.FileInformationModel.FileInformation

            // Add logic to determine the type of FileInformation
            // object to create, likely using the file type specified
            // in the FileInfo object

            string extension = document.Extension;
            FileConverter fc;
            try
            {
                if (extension.Equals(Constants.CSharpCodeFileExtension))
                {
                    fc = new CodeFileConverter(_sourceProjectPath, document.FullName, _blazorWorkspaceManager,
                        _webFormsProjectAnalyzer, _classConverterFactory, _taskManagerService, _metricsContext);
                }
                else if (extension.Equals(Constants.WebFormsPageMarkupFileExtension)
                         || extension.Equals(Constants.WebFormsControlMarkupFileExtenion)
                         || extension.Equals(Constants.WebFormsMasterPageMarkupFileExtension)
                         || extension.Equals(Constants.WebFormsGlobalMarkupFileExtension))
                {
                    fc = new ViewFileConverter(_sourceProjectPath, document.FullName, _viewImportService,
                        _codeBehindLinkerService, _taskManagerService, _tagConfigParser, _metricsContext);
                }
                else if (StaticResourceExtensions.Contains(extension))
                {
                    fc = new StaticResourceFileConverter(_sourceProjectPath, document.FullName, _hostPageService,
                        _taskManagerService, _metricsContext);
                }
                else
                {
                    fc = new StaticFileConverter(_sourceProjectPath, document.FullName, _taskManagerService, _metricsContext);
                }

                return fc;
            }
            catch (Exception e)
            {
                LogHelper.LogError(e,$"{Rules.Config.Constants.WebFormsErrorTag}Could not build appropriate file converter for {document.FullName}.");
                return null;
            }
        }

        public IEnumerable<FileConverter> BuildMany(IEnumerable<FileInfo> documents)
        {
            return documents.Select(document => Build(document))
                .Where(fileConverter => fileConverter != null)
                .ToList();
        }
    }
}

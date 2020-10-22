﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using FlubuCore.Context;
using FlubuCore.Infrastructure;
using FlubuCore.Infrastructure.Terminal;
using FlubuCore.IO;
using FlubuCore.Scripting;
using FlubuCore.Tasks.Packaging;
using Newtonsoft.Json;

namespace FlubuCore.Commanding.Internal
{
    public partial class InternalCommandExecutor
    {
        private const string TmpZipPath = "./template.zip";

        private const string TemplateJsonFileName = "template.json";

        private bool DefaultTemplateCommand => Args.MainCommands.Count == 1;

        internal async Task CreateNewProject()
        {
            if (DefaultTemplateCommand)
            {
                await DownloadAndPrepareProject("https://github.com/flubu-core/EmptyTemplate/archive/master.zip");
            }
        }

        internal async Task DownloadAndPrepareProject(string url)
        {
            using (var client = new HttpClient())
            {
                await client.DownloadFileAsync(url, TmpZipPath);
                var rootDir = Path.GetFullPath(".");
                var files = FlubuSession.Tasks().UnzipTask(TmpZipPath, rootDir).NoLog().Execute(FlubuSession);

                string templateJsonPath = files.FirstOrDefault(x => x.EndsWith(TemplateJsonFileName, StringComparison.OrdinalIgnoreCase));
                TemplateModel templateData = null;
                if (templateJsonPath != null)
                {
                    var json = File.ReadAllText(templateJsonPath);
                    templateData = JsonConvert.DeserializeObject<TemplateModel>(json);
                }

                var replacementTokens = GetReplacementTokens(templateData);

                foreach (var sourceFile in files)
                {
                    string relativePath = sourceFile.Replace(rootDir, string.Empty).TrimStart(Path.DirectorySeparatorChar);
                    var destinationFile = Path.Combine(rootDir, relativePath
                        .Substring(relativePath.IndexOf(Path.DirectorySeparatorChar))
                        .TrimStart(Path.DirectorySeparatorChar));
                    var destinationDir = Path.GetDirectoryName(destinationFile);

                    if (replacementTokens.Any())
                    {
                        FlubuSession.Tasks().ReplaceTokensTask(sourceFile)
                            .Replace(replacementTokens.ToArray()).Execute(FlubuSession);
                    }

                    if (!string.IsNullOrEmpty(destinationDir))
                    {
                        Directory.CreateDirectory(destinationDir);
                    }

                    File.Copy(sourceFile, destinationFile, true);
                }

                var tmp = files[0].Substring(rootDir.Length).TrimStart(Path.DirectorySeparatorChar);
                var gitDirName = tmp.Substring(0, tmp.IndexOf(Path.DirectorySeparatorChar));
                Directory.Delete(gitDirName, true);
                File.Delete(TmpZipPath);
            }
        }

        private static List<Tuple<string, string>> GetReplacementTokens(TemplateModel templateData)
        {
            List<Tuple<string, string>> replacmentTokens = new List<Tuple<string, string>>();

            if (templateData?.Tokens != null && templateData.Tokens.Count > 0)
            {
                foreach (var token in templateData.Tokens)
                {
                    var initialText = !string.IsNullOrEmpty(token.Description)
                        ? token.Description
                        : $"Enter replacement value for {token.Token}";
                    var console = new FlubuConsole(new List<Hint>(), options: o =>
                    {
                        o.WritePrompt = false;
                        o.InitialText = initialText;
                    });

                    var newValue = console.ReadLine();
                    replacmentTokens.Add(new Tuple<string, string>(token.Token, newValue));
                }
            }

            return replacmentTokens;
        }
    }
}

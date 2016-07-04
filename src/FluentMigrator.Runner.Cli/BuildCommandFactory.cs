using Microsoft.DotNet.Cli.Utils;
using NuGet.Frameworks;
using System.Collections.Generic;
namespace FluentMigrator.Runner.Cli
{
    public class BuildCommandFactory
    {
        public static ICommand Create(string project, string configuration,
               NuGetFramework framework, string buildBasePath, string output)
        {
            // TODO: Specify --runtime?
            var args = new List<string>
            {
                project,
                "--configuration", configuration,
                "--framework", framework.GetShortFolderName()
            };
            if (buildBasePath != null)
            {
                args.Add("--build-base-path");
                args.Add(buildBasePath);
            }
            if (output != null)
            {
                args.Add("--output");
                args.Add(output);
            }
            return Command.CreateDotNet("build", args, framework, configuration);
        }
    }
}
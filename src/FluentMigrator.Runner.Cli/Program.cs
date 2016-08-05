using DocoptNet;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Extensions.PlatformAbstractions;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static System.Console;

namespace FluentMigrator.Runner.Cli
{
    public class Program
    {
        private static bool verbose;
        private static bool noConnection;
        private static string provider;
        private static string connection;
        private static string task;
        private static long startVersion;
        private static long migrateToVersion;
        private static string profile;
        private static string outputFile;
        private static IEnumerable<string> tags;
        private static string assembly;

        public static void Main(string[] args)
        {
            const string usage = @"Fluent Migrator .NET CLI Runner
  Usage:
    dotnet migrate --provider PROVIDER --connectionString CONNECTION [--outputFile FILE] [--task TASK] [--migrateToVersion END] [--profile PROFILE] [--tag TAG] [--configuration CONFIGURATION] [--framework FRAMEWORK] [--build-base-path BUILD_BASE_PATH] [--output OUTPUT_DIR] [--verbose]
    dotnet migrate --provider PROVIDER --noConnection --outputFile FILE [--task TASK] [--startVersion START] [--migrateToVersion END] [--profile PROFILE] [--tag TAG] [--configuration CONFIGURATION] [--framework FRAMEWORK] [--build-base-path BUILD_BASE_PATH] [--output OUTPUT_DIR] [--verbose]
    dotnet migrate --version
    dotnet migrate --help

  Options:
    --provider PROVIDER -p PROVIDER                      Database type. Possible values:
                                                           * sqlserver2000
                                                           * sqlserver2005
                                                           * sqlserver2008
                                                           * sqlserver2012
                                                           * sqlserverce
                                                           * sqlserver
                                                           * mysql
                                                           * postgres
                                                           * oracle
                                                           * sqlite
                                                           * jet
    --connectionString CONNECTION -c CONNECTION          The connection string. Required.
    --outputFile FILE -f FILE                                File to output the script. If specified will write to a file instead of running the migration. [default: migration.sql]
    --task TASK -t TASK                                  The task to run. [default: migrate]
    --noConnection                                       Indicates that migrations will be generated without consulting a target database. Should only be used when generating an output file.
    --startVersion START                                 The specific version to start migrating from. Only used when NoConnection is true. [default: 0]
    --migrateToVersion END                               The specific version to migrate. Default is 0, which will run all migrations. [default: 0]
    --profile PROFILE                                    The profile to run after executing migrations.
    --tag TAG                                            Filters the migrations to be run by tag.
    --verbose                                            Verbose. Optional.
    --help -h                                            Show this screen.
    --version -v                                         Show version.
    --configuration CONFIGURATION                        Configuration under which to load. [default: Debug]
    --framework FRAMEWORK                                Target framework to load
    --build-base-path BUILD_BASE_PATH                    Directory in which to find temporary outputs
    --output OUTPUT_DIR -o OUTPUT_DIR                    Directory in which to find outputs
";
            var argsWithRun = args;
            if (args.Any() && args[0] != "migrate")
                argsWithRun = new[] { "migrate" }.Concat(args).ToArray();
            var arguments = new Docopt().Apply(usage, argsWithRun, version: Assembly.GetEntryAssembly().GetName().Version, exit: true);
            verbose = arguments["--verbose"].IsTrue;
            provider = arguments["--provider"].ToString();
            noConnection = arguments["--noConnection"].IsTrue;
            var configuration = arguments["--configuration"].ToString();
            if (noConnection)
                startVersion = arguments["--startVersion"].AsLong();
            else
                connection = arguments["--connectionString"].ToString();
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
            string frameworkOption = null, buildBasePath = null, output = null;
            if (arguments["--framework"] != null) frameworkOption = arguments["--framework"].ToString();
            if (arguments["--build-base-path"] != null) buildBasePath = arguments["--build-base-path"].ToString();
            if (arguments["--output"] != null) output = arguments["--output"].ToString();
            assembly = Build(frameworkOption, configuration, buildBasePath, output);
            if (assembly == null || !File.Exists(assembly))
            {
                WriteLine("Could not compile.");
                return;
            }
            if (arguments["--task"] != null)
                task = arguments["--task"].ToString();
            if (arguments["--migrateToVersion"] != null)
                migrateToVersion = arguments["--migrateToVersion"].AsLong();
            if (arguments["--profile"] != null)
                profile = arguments["--profile"].ToString();
            if (arguments["--tag"] != null)
                tags = arguments["--tag"].AsList.Cast<string>();
            if (arguments["--outputFile"].ToString() != "migration.sql")
                outputFile = arguments["--outputFile"].ToString();
            var argsToInvoke = new Dictionary<string, ValueObject>(arguments);
            argsToInvoke.Remove("--configuration");
            argsToInvoke.Remove("--framework");
            argsToInvoke.Remove("--build-base-path");
            argsToInvoke.Remove("--output");
            if (argsToInvoke["--startVersion"].AsLong() == 0)
                argsToInvoke.Remove("--startVersion");
            if (string.IsNullOrWhiteSpace(outputFile))
                argsToInvoke.Remove("--outputFile");
            var argsToInvokeString = argsToInvoke
                .Where(kv => kv.Value != null)
                .Where(kv => !kv.Value.IsFalse)
                .Select(kv => new string[] { kv.Key, kv.Value.IsTrue ? null : kv.Value.ToString() })
                .SelectMany(v => v)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();
            argsToInvokeString.Add("--assembly");
            argsToInvokeString.Add(assembly);
            Invoke(frameworkOption, configuration, buildBasePath, output, argsToInvokeString);
        }

        private static void Invoke(string frameworkOption, string configuration, string buildBasePath, string output, IEnumerable<string> arguments)
        {
            var projectPath = Directory.GetCurrentDirectory();
            Reporter.Verbose.WriteLine($"Using project '{projectPath}'.");
            var projectFile = ProjectReader.GetProject(projectPath);
            var framework = string.IsNullOrWhiteSpace(frameworkOption)
                    ? null
                    : NuGetFramework.Parse(frameworkOption);
            if (framework == null)
            {
                var frameworks = projectFile.GetTargetFrameworks().Select(i => i.FrameworkName);
                framework = NuGetFrameworkUtility.GetNearest(frameworks, FrameworkConstants.CommonFrameworks.Net463, f => f)
                            ?? frameworks.FirstOrDefault();
                Reporter.Verbose.WriteLine($"Using framework '{framework.GetShortFolderName()}'.");
            }
            var projectContext = CreateProjectContext(projectPath, framework);
            var executorFileName = "FluentMigrator.Runner.Cli.Executor";
            Reporter.Verbose.WriteLine($"Invoking '{executorFileName}' for '{projectContext.TargetFramework}'.");
            try
            {
                var commandSpec = ProjectDependenciesCommandFactory.FindProjectDependencyCommand(executorFileName,
                            arguments,
                            configuration,
                            projectContext.TargetFramework,
                            output,
                            buildBasePath,
                            projectContext.ProjectDirectory);
                var command = Command.Create(commandSpec);
                var exitCode = command
                    .ForwardStdErr()
                    .ForwardStdOut()
                    .Execute()
                    .ExitCode;
            }
            catch (CommandUnknownException)
            {
                Reporter.Verbose.WriteLine($"Command not found.");
                return;
            }
        }

        public static string Build(string frameworkOption, string configuration, string buildBasePath, string output)
        {
            var project = Directory.GetCurrentDirectory();
            Reporter.Verbose.WriteLine($"Using project '{project}'.");
            var projectFile = ProjectReader.GetProject(project);
            var framework = string.IsNullOrWhiteSpace(frameworkOption)
                    ? null
                    : NuGetFramework.Parse(frameworkOption);
            if (framework == null)
            {
                var frameworks = projectFile.GetTargetFrameworks().Select(i => i.FrameworkName);
                framework = NuGetFrameworkUtility.GetNearest(frameworks, FrameworkConstants.CommonFrameworks.Net463, f => f)
                            ?? frameworks.FirstOrDefault();
                Reporter.Verbose.WriteLine($"Using framework '{framework.GetShortFolderName()}'.");
            }
            if (configuration == null)
            {
                configuration = Constants.DefaultConfiguration;
                Reporter.Verbose.WriteLine($"Using configuration '{configuration}'.");
            }
            var buildExitCode = BuildCommandFactory.Create(projectFile.ProjectFilePath, configuration,
                framework, buildBasePath, output)
                .ForwardStdErr()
                .ForwardStdOut()
                .Execute()
                .ExitCode;
            if (buildExitCode != 0)
                throw new OperationException($"Build failed on '{projectFile.Name}'.");
            var projectContext = ProjectContext.Create(projectFile.ProjectFilePath, framework);
            var runtimeFiles = projectContext.GetOutputPaths(configuration, buildBasePath, output)?.RuntimeFiles;
            return runtimeFiles?.Assembly;
        }

        private static ProjectContext CreateProjectContext(string projectPath, NuGetFramework framework)
        {
            if (!projectPath.EndsWith(Project.FileName))
                projectPath = Path.Combine(projectPath, Project.FileName);
            if (!File.Exists(projectPath))
                throw new InvalidOperationException($"{projectPath} does not exist.");
            return ProjectContext.Create(projectPath, framework);
        }


        ///////////////////////////

    }

    public class OutputPathCommandResolver2 : ICommandResolver
    {
        protected IEnvironmentProvider environment;
        protected IPlatformCommandSpecFactory commandSpecFactory;
        public OutputPathCommandResolver2(IEnvironmentProvider environment,
            IPlatformCommandSpecFactory commandSpecFactory)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));
            if (commandSpecFactory == null)
                throw new ArgumentNullException(nameof(commandSpecFactory));
            this.environment = environment;
            this.commandSpecFactory = commandSpecFactory;
        }
        public CommandSpec Resolve(CommandResolverArguments commandResolverArguments)
        {
            if (commandResolverArguments.CommandName == null)
                return null;
            var commandPath = ResolveCommandPath(commandResolverArguments);
            if (commandPath == null)
                return null;
            return commandSpecFactory.CreateCommandSpec(
                    commandResolverArguments.CommandName,
                    commandResolverArguments.CommandArguments.OrEmptyIfNull(),
                    commandPath,
                    CommandResolutionStrategy.OutputPath,
                    environment);
        }

        private string ResolveCommandPath(CommandResolverArguments commandResolverArguments)
        {
            if (commandResolverArguments.Framework == null || commandResolverArguments.ProjectDirectory == null
                || commandResolverArguments.Configuration == null || commandResolverArguments.CommandName == null)
                return null;

            return ResolveFromProjectOutput(
                commandResolverArguments.ProjectDirectory,
                commandResolverArguments.Framework,
                commandResolverArguments.Configuration,
                commandResolverArguments.CommandName,
                commandResolverArguments.CommandArguments.OrEmptyIfNull(),
                commandResolverArguments.OutputPath,
                commandResolverArguments.BuildBasePath);
        }

        private string ResolveFromProjectOutput(string projectDirectory, NuGetFramework framework,
            string configuration, string commandName, IEnumerable<string> commandArguments,
            string outputPath, string buildBasePath)
        {
            var projectContext = GetProjectContextFromDirectory(
                projectDirectory,
                framework);
            if (projectContext == null) return null;
            var buildOutputPath = projectContext.GetOutputPaths(configuration, buildBasePath, outputPath).RuntimeFiles.BasePath;
            if (!Directory.Exists(buildOutputPath))
            {
                Reporter.Verbose.WriteLine($"outputpathresolver: {buildOutputPath} does not exist");
                return null;
            }
            return environment.GetCommandPathFromRootPath(buildOutputPath, commandName);
        }

        private static ProjectContext GetProjectContextFromDirectory(string projectDirectory, NuGetFramework framework)
        {
            if (projectDirectory == null || framework == null)
                return null;
            var projectRootPath = projectDirectory;
            if (!File.Exists(Path.Combine(projectRootPath, Project.FileName)))
                throw new InvalidOperationException($"{projectRootPath} does not exist.");
            var projectContext = ProjectContext.Create(projectRootPath, framework);
            return projectContext;
        }
    }
}
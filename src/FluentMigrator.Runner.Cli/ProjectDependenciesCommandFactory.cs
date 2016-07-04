using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.InternalAbstractions;
using NuGet.Frameworks;
using System.Collections.Generic;

namespace FluentMigrator.Runner.Cli
{
    public class ProjectDependenciesCommandFactory
    {
        public static CommandSpec FindProjectDependencyCommand(
            string commandName,
            IEnumerable<string> commandArgs,
            string configuration,
            NuGetFramework framework,
            string outputPath,
            string buildBasePath,
            string projectDirectory)
        {
            var commandResolverArguments = new CommandResolverArguments
            {
                CommandName = commandName,
                CommandArguments = commandArgs,
                Framework = framework,
                Configuration = configuration,
                OutputPath = outputPath,
                BuildBasePath = buildBasePath,
                ProjectDirectory = projectDirectory
            };

            var commandResolver = GetProjectDependenciesCommandResolver(framework);

            var commandSpec = commandResolver.Resolve(commandResolverArguments);
            if (commandSpec == null)
            {
                throw new CommandUnknownException(commandName);
            }

            return commandSpec;
        }

        private static ICommandResolver GetProjectDependenciesCommandResolver(NuGetFramework framework)
        {
            var environment = new EnvironmentProvider();

            if (framework.IsDesktop())
            {
                IPlatformCommandSpecFactory platformCommandSpecFactory = null;
                if (RuntimeEnvironment.OperatingSystemPlatform == Platform.Windows)
                {
                    platformCommandSpecFactory = new WindowsExePreferredCommandSpecFactory();
                }
                else
                {
                    platformCommandSpecFactory = new GenericPlatformCommandSpecFactory();
                }

                return new OutputPathCommandResolver2(environment, platformCommandSpecFactory);
            }
            else
            {
                var packagedCommandSpecFactory = new PackagedCommandSpecFactory();
                return new ProjectDependenciesCommandResolver(environment, packagedCommandSpecFactory);
            }
        }
    }
}
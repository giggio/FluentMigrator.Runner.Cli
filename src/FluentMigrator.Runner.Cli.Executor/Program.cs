using DocoptNet;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static System.Console;

namespace FluentMigrator.Runner.Cli.Executor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var usage = @"Fluent Migrator .NET CLI Runner
  Usage:
    dotnet migrate --provider PROVIDER --connectionString CONNECTION --assembly ASSEMBLY [--outputFile FILE] [--task TASK] [--migrateToVersion END] [--profile PROFILE] [--tag TAG] [--verbose]
    dotnet migrate --provider PROVIDER --noConnection --outputFile FILE --assembly ASSEMBLY [--task TASK] [--startVersion START] [--migrateToVersion END] [--profile PROFILE] [--tag TAG] [--verbose]
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
    --assembly ASSEMBLY -a ASSEMBLY                      The project or assembly which contains the migrations. Required.
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
";
            var argsWithRun = args;
            var arguments = new Docopt().Apply(usage, argsWithRun, version: Assembly.GetEntryAssembly().GetName().Version, exit: true);
            verbose = arguments["--verbose"].IsTrue;
            provider = arguments["--provider"].ToString();
            noConnection = arguments["--noConnection"].IsTrue;
            if (noConnection)
                startVersion = arguments["--startVersion"].AsLong();
            else
                connection = arguments["--connectionString"].ToString();
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
            assembly = arguments["--assembly"].ToString();
            if (!Path.IsPathRooted(assembly))
                assembly = Path.GetFullPath(Path.Combine(applicationBasePath, assembly));
            if (string.Compare(Path.GetExtension(assembly), ".dll", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (!File.Exists(assembly))
                {
                    WriteLine($"File {assembly} does not exist.");
                    return;
                }
            }
            else
            {
                WriteLine($"Incorrect assembly name.");
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
            if (arguments["--outputFile"].ToString() == "migration.sql")
                ExecuteMigrations(); //TODO
            else
                ExecuteMigrations(arguments["--outputFile"].ToString()); //TODO
        }


        private static readonly ConsoleAnnouncer consoleAnnouncer = new ConsoleAnnouncer();
        private static bool verbose;
        private static bool noConnection;
        private static string provider;
        private static string connection;
        private static string task;
        private static long startVersion;
        private static long migrateToVersion;
        private static string profile;
        private static IEnumerable<string> tags;
        private static string assembly;

        private static void ExecuteMigrations()
        {
            consoleAnnouncer.ShowElapsedTime = verbose;
            consoleAnnouncer.ShowSql = verbose;
            ExecuteMigrations(consoleAnnouncer);
        }

        private static void ExecuteMigrations(string outputTo)
        {
            using (var sw = new StreamWriter(outputTo))
            {
                var fileAnnouncer = ExecutingAgainstMsSql ?
                    new TextWriterWithGoAnnouncer(sw) :
                    new TextWriterAnnouncer(sw);
                fileAnnouncer.ShowElapsedTime = false;
                fileAnnouncer.ShowSql = true;
                consoleAnnouncer.ShowElapsedTime = verbose;
                consoleAnnouncer.ShowSql = verbose;
                var announcer = new CompositeAnnouncer(consoleAnnouncer, fileAnnouncer);
                ExecuteMigrations(announcer);
            }
        }

        private static bool ExecutingAgainstMsSql =>
            provider.StartsWith("SqlServer", StringComparison.InvariantCultureIgnoreCase);

        private static void ExecuteMigrations(IAnnouncer announcer) =>
            new TaskExecutor(new RunnerContext(announcer)
            {
                Database = provider,
                Connection = connection,
                Targets = new[] { assembly },
                PreviewOnly = false,
                Task = task,
                NoConnection = noConnection,
                StartVersion = startVersion,
                Version = migrateToVersion,
                Profile = profile,
                Tags = tags
            }).Execute();
    }
}
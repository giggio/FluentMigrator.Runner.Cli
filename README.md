# FluentMigrator .NET CLI Runner

A .NET CLI runner for [Fluent Migrator](https://github.com/schambers/fluentmigrator/).

## Installing

Add the runner to your project manually, here is an example project.json:

````json
{
    "version": "1.0.0-*",
    "dependencies": {
        "FluentMigrator.Runner.Cli.Executor": "1.0.0-*"
    },
    "frameworks": {
        "net461": { }
    },
    "tools": {
        "FluentMigrator.Runner.Cli": "1.0.0-*"
    }
}
````

Or use the Package Manager in Visual Studio. (only for dependencies, not tools).

## Running

If you simply run `dotnet migrate` the command line options will show up for you.
Here is a sample for SQL Server 2012:

````powershell
dotnet migrate --provider sqlserver2012 --connectionString <yourconnectionstring>
````

Here are all the options when you run `dotnet migrate --help`:

````
Usage:
    dotnet migrate --provider PROVIDER --connectionString CONNECTION [--outputFile FILE] [--task TASK] [--migrateToVersion END] [--profile PROFILE] [--tag TAG] [--configuration CONFIGURATION] [--framework FRAMEWORK] [--build-base-path BUILD_BASE_PATH] [--output OUTPUT_DIR] [--verbose]
    dotnet migrate --provider PROVIDER --noConnection --outputFile FILE [--task TASK] [--startVersion START] [--migrateToVersion END] [--profile PROFILE] [--tag TAG] [--configuration CONFIGURATION] [--framework FRAMEWORK] [--build-base-path BUILD_BASE_PATH] [--output OUTPUT_DIR] [--verbose]
    dotnet migrate --version
    dotnet migrate --help
````

## Maintainer

* [Giovanni Bassi](http://blog.lambda3.com.br/L3/giovannibassi/), aka Giggio, [Lambda3](http://www.lambda3.com.br), [@giovannibassi](http://twitter.com/giovannibassi)

Contributors can be found at the [contributors](https://github.com/giggio/FluentMigrator.Runner.Cli/graphs/contributors) page on Github.

## Contact

Use the github issues, or contact me on Twitter @giovannibassi.

## License

This software is open source, licensed under the Apache License, Version 2.0.
See [LICENSE.txt](https://github.com/code-cracker/code-cracker/blob/master/LICENSE.txt) for details.
Check out the terms of the license before you contribute, fork, copy or do anything
with the code. If you decide to contribute you agree to grant copyright of all your contribution to this project, and agree to
mention clearly if do not agree to these terms. Your work will be licensed with the project at Apache V2, along the rest of the code.
# FluentMigrator Aspnet Runner

A runner for ASP.NET 5+ for [Fluent Migrator](https://github.com/schambers/fluentmigrator/).

## Installing

The nupkg is [on Nuget.org](https://www.nuget.org/packages/FluentMigrator.Runner.Aspnet), so
simply run:

```powershell
dnu install FluentMigrator.Runner.Aspnet
```

And add the command with name `FluentMigrator.Runner.Aspnet` to your project.

Or add the runner to your project manually, here is an example project.json:

````json
{
    "version": "1.0.0-*",
    "dependencies": {
        "FluentMigrator": "1.5.1",
        "FluentMigrator.Runner.Aspnet": "1.0.0-*"
    },
    "frameworks": {
        "dnx451": { }
    },
    "commands": {
        "migrate": "FluentMigrator.Runner.Aspnet"
    }
}
````

Or use the Package Manager in Visual Studio.

## Running

If you simply run `dnx migrate` the command line options will show up for you

````powershell
dnx . migrate --provider sqlserver2012 --connectionString <yourconnectionstring>
````

Here are all the options:

````
Usage:
    dnx . run --provider PROVIDER --connectionString CONNECTION [--assembly ASSEMBLY] [--output FILE] [--task TASK] [--migrateToVersion VERSION] [--profile PROFILE] [--tag TAG] [--verbose]
    dnx . run --version
    dnx . run --help
````

## Maintainer

* [Giovanni Bassi](http://blog.lambda3.com.br/L3/giovannibassi/), aka Giggio, [Lambda3](http://www.lambda3.com.br), [@giovannibassi](http://twitter.com/giovannibassi)

Contributors can be found at the [contributors](https://github.com/giggio/FluentMigrator.Runner.Aspnet/graphs/contributors) page on Github.

## Contact

I am only on Jabbr most of the day, usualy on the [ASP.NET vNext room](https://jabbr.net/#/rooms/AspNetvNext), with user name `Giggio`.

## License

This software is open source, licensed under the Apache License, Version 2.0.
See [LICENSE.txt](https://github.com/code-cracker/code-cracker/blob/master/LICENSE.txt) for details.
Check out the terms of the license before you contribute, fork, copy or do anything
with the code. If you decide to contribute you agree to grant copyright of all your contribution to this project, and agree to
mention clearly if do not agree to these terms. Your work will be licensed with the project at Apache V2, along the rest of the code.

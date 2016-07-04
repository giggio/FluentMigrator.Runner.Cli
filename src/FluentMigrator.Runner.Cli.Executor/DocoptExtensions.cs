using DocoptNet;
using System;

namespace FluentMigrator.Runner.Cli.Executor
{
    public static class DocoptExtensions
    {
        public static long AsLong(this ValueObject obj) =>
            obj.IsList ? 0 : Convert.ToInt64(obj.Value);
    }
}
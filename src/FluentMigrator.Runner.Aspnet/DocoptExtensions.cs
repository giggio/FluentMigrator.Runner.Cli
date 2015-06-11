using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocoptNet;

namespace FluentMigrator.Runner.Aspnet
{
    public static class DocoptExtensions
    {
		public static long AsLong ( this ValueObject obj ) {
			if (!obj.IsList)
				return Convert.ToInt64(obj.Value);
			return 0;
		}
	}
}

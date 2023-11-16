using Elite.DotNetVersion.Application.Verbs;
using System;
using System.Collections.Generic;

namespace Elite.DotNetVersion.Application
{
    static class VerbManager
    {
        public static IEnumerable<Type> GetVerbs()
        {
            yield return typeof(EpochVerb.Options);
            yield return typeof(VersionVerb.Options);
            yield return typeof(IncrementVerb.Options);
        }
    }
}
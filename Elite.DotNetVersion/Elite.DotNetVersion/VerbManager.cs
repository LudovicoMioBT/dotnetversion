using Elite.DotNetVersion.Verbs;
using System;
using System.Collections.Generic;

namespace Elite.DotNetVersion
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
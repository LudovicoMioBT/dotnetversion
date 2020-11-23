using Elite.DotNetVersion.Verbs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.DotNetVersion
{
    static class VerbManager
    {
        public static IEnumerable<Type> GetVerbs()
        {
            yield return typeof(EpochVerb.Options);
            yield return typeof(IncrementVerb.Options);
        }
    }
}
using Elite.DotNetVersion;
using System;

namespace Elite.DotNetVersion.Formatters
{
    static class FormatterFactory
    {
        public static IFormatter Create(OutputType type, string query)
        {
            if (type == OutputType.Json)
                return new JsonFormatter(query);

            throw new NotSupportedException($"Format type {type} is not supported");
        }
    }
}

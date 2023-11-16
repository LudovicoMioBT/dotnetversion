using CommandLine;
using System;

namespace Elite.DotNetVersion
{
    interface IProgramOptions
    {
        OutputType? Output { get; }
        IVerb Create();
    }

    abstract class ProgramOptions<T> : IProgramOptions
        where T : IVerb
    {
        [Option('o', "output", Required = false, HelpText = "Type of output", Default = OutputType.Json)]
        public OutputType? Output { get; set; }

        [Option('q', "query", Required = false, HelpText = "Jmespath query for JSON output http://jmespath.org/", Default = "")]
        public string Query { get; set; }

        public IVerb Create()
        {
            return (T)Activator.CreateInstance(typeof(T), this);
        }
    }
}

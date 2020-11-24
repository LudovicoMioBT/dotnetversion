using CommandLine;
using Elite.DotNetVersion.Formatters;
using Elite.DotNetVersion.Projects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elite.DotNetVersion.Verbs
{
    class VersionVerb : IVerb
    {
        public Options VerbOptions { get; }

        public VersionVerb(Options options)
        {
            this.VerbOptions = options;
        }

        public Task RunAsync()
        {
            var date = this.VerbOptions.Date.GetValueOrDefault(DateTime.Today);
            var epoch = (Epoch)date;
            var version = this.VerbOptions.Number ?? new Version("1.0.0.0");
            var level = this.VerbOptions.VersionLevel;

            Version result = null;

            if (level == VersionLevel.Revision)
                result = new Version(version.Major, version.Minor, version.Build, epoch);
            else if (level == VersionLevel.Build)
                result = new Version(version.Major, version.Minor, version.Build + 1, epoch);
            else if (level == VersionLevel.Minor)
                result = new Version(version.Major, version.Minor + 1, 0, epoch);
            else if (level == VersionLevel.Major)
                result = new Version(version.Major + 1, 0, 0, epoch);

            var formatter = FormatterFactory.Create(
                this.VerbOptions.Output.GetValueOrDefault(OutputType.Json),
                this.VerbOptions.Query);

            return formatter.WriteAsync(Console.Out, new
            {
                Date = date.ToString("yyyy-MM-dd"),
                Year = date.ToString("yyyy"),
                Month = date.ToString("MM"),
                Day = date.ToString("dd"),
                Number = result.ToString(),
                Major = result.Major,
                Minor = result.Minor,
                Build = result.Build,
                Revision = result.Revision
            });
        }

        [Verb("number", HelpText = "shows and calculate version numbers")]
        public class Options : ProgramOptions<VersionVerb>
        {
            [Option('d', "date", Required = false, HelpText = "date to set", Default = null)]
            public DateTime? Date { get; set; }

            [Option('v', "version", Required = false, HelpText = "number of version", Default = null)]
            public Version Number { get; set; }

            [Option('l', "level", Required = false, HelpText = "choose which level increase by 1", Default = VersionLevel.Revision)]
            public VersionLevel VersionLevel { get; set; }
        }
    }
}

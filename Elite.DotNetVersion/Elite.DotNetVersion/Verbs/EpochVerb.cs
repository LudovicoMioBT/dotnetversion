using CommandLine;
using Elite.DotNetVersion.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elite.DotNetVersion.Verbs
{
    class EpochVerb : IVerb
    {
        public Options VerbOptions { get; }

        public EpochVerb(Options options)
        {
            if (options.RevisionDate.HasValue && options.RevisionNumber != 0)
                throw new ApplicationException($"RevisionDate and RevisionNumber are mutually exclusive");

            this.VerbOptions = options;
        }

        public Task RunAsync()
        {
            Epoch epoch;

            if (this.VerbOptions.RevisionDate.HasValue)
                epoch = (Epoch)this.VerbOptions.RevisionDate;
            else if (this.VerbOptions.RevisionNumber != 0)
                epoch = (Epoch)this.VerbOptions.RevisionNumber;
            else
                epoch = (Epoch)DateTime.Today;

            var formatter = FormatterFactory.Create(
                this.VerbOptions.Output.GetValueOrDefault(OutputType.Json), 
                this.VerbOptions.Query);

            return formatter.WriteAsync(Console.Out, epoch);
        }

        [Verb("epoch", HelpText = "shows and calculate release dates")]
        public class Options : ProgramOptions<EpochVerb>
        {
            [Option('n', "epoch-num", Required = false, HelpText = "number of the epoch (0 = today)", Default = 0)]
            public int RevisionNumber { get; set; }

            [Option('d', "epoch-date", Required = false, HelpText = "date of the epoch (YYYY-MM-DD)")]
            public DateTime? RevisionDate { get; set; }
        }

    }
}

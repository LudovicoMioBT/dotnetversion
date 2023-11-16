using CommandLine;
using Elite.DotNetVersion.Domain.Entities;
using Elite.DotNetVersion.Domain.Enums;
using Elite.DotNetVersion.Domain.Interfaces;
using Elite.DotNetVersion.Infrastructure.Formatters;
using System;
using System.Threading.Tasks;

namespace Elite.DotNetVersion.Application.Verbs
{
    class EpochVerb : IVerb
    {
        public EpochVerb(Options options)
        {
            if (options.RevisionDate.HasValue && options.RevisionNumber != 0)
                throw new ApplicationException($"RevisionDate and RevisionNumber are mutually exclusive");

            VerbOptions = options;
        }

        public Options VerbOptions { get; }

        public Task RunAsync()
        {
            Epoch epoch;

            if (VerbOptions.RevisionDate.HasValue)
                epoch = (Epoch)VerbOptions.RevisionDate;
            else if (VerbOptions.RevisionNumber != 0)
                epoch = (Epoch)VerbOptions.RevisionNumber;
            else
                epoch = (Epoch)DateTime.Today;

            var formatter = FormatterFactory.Create(
                VerbOptions.Output.GetValueOrDefault(OutputType.Json),
                VerbOptions.Query);

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

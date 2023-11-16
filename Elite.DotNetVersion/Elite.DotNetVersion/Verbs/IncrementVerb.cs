using CommandLine;
using Elite.DotNetVersion.Formatters;
using Elite.DotNetVersion.Projects;
using Microsoft.Build.Construction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.DotNetVersion.Verbs
{
    class IncrementVerb : IVerb
    {
        public IncrementVerb(Options options)
        {
            if (!options.SolutionFile.Exists)
                throw new ApplicationException($"Solution file '{options.SolutionFile.FullName}' does not exists.");

            if (options.EpochDate.HasValue && options.EpochNumber != 0)
                throw new ApplicationException($"RevisionDate and RevisionNumber are mutually exclusive");

            if (options.Projects == null)
                throw new ApplicationException($"Please specify a list of projects separated by space using -p (--projects) option");

            this.VerbOptions = options;
        }

        public Options VerbOptions { get; }

        public Task RunAsync()
        {
            var solutionFile = SolutionFile.Parse(this.VerbOptions.SolutionFile.FullName);
            var solutionName = Path.GetFileNameWithoutExtension(this.VerbOptions.SolutionFile.FullName);
            Solution solution = Solution.FromProjects(solutionName, solutionFile.ProjectsInOrder);
            var projects = this.VerbOptions.Projects;

            var pendingChanges = PendingChanges.FromSolution(VerbOptions.SolutionFile.FullName);

            if (!projects.Any())
            {
                throw new ApplicationException("There isn't any project changed");
            }

            var projectsToChange = solution.FindDependentsByName(projects);

            Epoch epoch = this.GetReleaseDate();

            foreach (var project in projectsToChange)
            {
                project.IncreaseVersion(this.VerbOptions.VersionLevel, epoch);
                pendingChanges.UpdateProject(project);
            }

            if (this.VerbOptions.Commit)
            {
                this.Save(projectsToChange);
                UpdatePendingChanges(pendingChanges);
            }

            return this.OutputAsync(solutionName, projects, projectsToChange, epoch);
        }

        private Task OutputAsync(string solutionName, IEnumerable<string> projects, IEnumerable<Project> projectsToChange, Epoch epoch)
        {
            var formatter = FormatterFactory.Create(
                this.VerbOptions.Output.GetValueOrDefault(OutputType.Json),
                this.VerbOptions.Query);

            dynamic result = new
            {
                changes = (from project in projectsToChange
                           select new
                           {
                               name = project.Name,
                               oldVersion = project.Version,
                               newVersion = project.NewVersion
                           }).ToArray(),
                request = new
                {
                    commited = this.VerbOptions.Commit,
                    revision = epoch,
                    level = new
                    {
                        number = this.VerbOptions.VersionLevel,
                        name = this.VerbOptions.VersionLevel.ToString()
                    },
                    solution = new
                    {
                        name = solutionName,
                        file = this.VerbOptions.SolutionFile
                    },
                    projects = projects
                }
            };

            return formatter.WriteAsync(Console.Out, result);
        }

        private Epoch GetReleaseDate()
        {
            Epoch reldate;

            if (this.VerbOptions.EpochDate.HasValue)
                reldate = this.VerbOptions.EpochDate.Value;
            else
                reldate = this.VerbOptions.EpochNumber == 0 ? (Epoch)DateTime.Now : (Epoch)this.VerbOptions.EpochNumber;

            return reldate;
        }

        private void Save(IEnumerable<Project> projectsToChange)
        {
            foreach (var prj in projectsToChange)
            {
                prj.CommitUpdatedVersionToFile();
            }
        }

        private void UpdatePendingChanges(PendingChanges changes)
        {
            if (changes.NotPresent)
            {
                return;
            }

            string tempFile = Path.GetTempFileName();

            using (var reader = File.OpenText(changes.FullPath))
            {
                using var writer = new StreamWriter(tempFile);
                int currentRow = 0;

                while (!reader.EndOfStream)
                {
                    var content = reader.ReadLine();
                    currentRow++;

                    if (currentRow < changes.BeginRow || currentRow > changes.EndRow)
                    {
                        writer.WriteLine(content);
                    }

                    if (currentRow == changes.BeginRow)
                    {
                        writer.Write(changes.ToString());
                    }
                }
            }

            File.Delete(changes.FullPath);
            File.Move(tempFile, changes.FullPath);
        }

        [Verb("increment", HelpText = "increase version number of projects in solution")]
        public class Options : ProgramOptions<IncrementVerb>
        {
            [Option('s', "solution", Required = true, HelpText = "full path to *.sln file")]
            public FileInfo SolutionFile { get; set; }

            [Option('p', "projects", Required = false, HelpText = "list of projects changed in release (only first level, dependencies will be calculated)")]
            public IEnumerable<string> Projects { get; set; }

            [Option('c', "commit", Required = false, HelpText = "when specified saves changes to original csproj files", Default = false)]
            public bool Commit { get; set; }

            [Option('l', "level", Required = false, HelpText = "choose which level increase by 1", Default = VersionLevel.Build)]
            public VersionLevel VersionLevel { get; set; }

            [Option('n', "epoch-num", Required = false, HelpText = "number of the current revision (0 = today)", Default = 0)]
            public int EpochNumber { get; set; }

            [Option('d', "epoch-date", Required = false, HelpText = "date of the current revision (YYYY-MM-DD)")]
            public DateTime? EpochDate { get; set; }
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Elite.DotNetVersion.Domain.Entities
{
    internal class PendingChanges
    {
        private readonly List<ProjectChange> changedProjects;

        private PendingChanges()
        {
            changedProjects = new List<ProjectChange>();
        }

        internal bool NotPresent { get; private set; }
        internal string FullPath { get; private set; }
        internal int BeginRow { get; private set; }
        internal int EndRow { get; private set; }

        public static PendingChanges FromSolution(string solutionPath)
        {
            var changes = new PendingChanges();
            var pendingChangesFile = Directory.GetFiles(Path.GetDirectoryName(solutionPath), "PendingChanges.md", SearchOption.AllDirectories);

            if (pendingChangesFile != null && pendingChangesFile.Length == 1)
            {
                changes.FullPath = pendingChangesFile[0];
                using var stream = File.OpenText(changes.FullPath);
                int currentRow = 0;

                while (!stream.EndOfStream)
                {
                    currentRow++;
                    string rowContent = stream.ReadLine();

                    if (changes.BeginRow > 0 && rowContent.StartsWith("|") && !rowContent.StartsWith("|-"))
                    {
                        var projectInfos = rowContent.Split('|').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                        changes.changedProjects.Add(new ProjectChange()
                        {
                            Name = projectInfos[0],
                            Version = projectInfos[1],
                            Type = projectInfos[2]
                        });
                    }

                    if (rowContent.StartsWith("|Artifact"))
                    {
                        changes.BeginRow = currentRow;
                    }

                    if (changes.BeginRow > 0 && string.IsNullOrWhiteSpace(rowContent))
                    {
                        changes.EndRow = currentRow;
                        break;
                    }
                }
            }
            else
            {
                changes.NotPresent = true;
            }

            return changes;
        }

        internal void UpdateProject(Project project)
        {
            var change = changedProjects.Find(x => x.Name == project.Name);
            var version = project.NewVersion != null ? project.NewVersion : project.Version;

            if (change != null)
            {
                change.Version = $"{version.Major}.{version.Minor}.{version.Build}.<EPOCH>";
            }
            else
            {
                changedProjects.Add(new ProjectChange()
                {
                    Name = project.Name,
                    Version = $"{version.Major}.{version.Minor}.{version.Build}.<EPOCH>",
                    Type = "non-breaking"
                });
            }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("|Artifact|Released version|Impact|");
            stringBuilder.AppendLine("|--|--|--|");

            foreach (var project in changedProjects)
            {
                stringBuilder.AppendLine($"|{project.Name}|{project.Version}|{project.Type}|");
            }

            stringBuilder.AppendLine();

            return stringBuilder.ToString();
        }

    }
}

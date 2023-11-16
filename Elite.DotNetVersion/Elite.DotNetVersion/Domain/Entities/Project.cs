using Elite.DotNetVersion.Domain.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Elite.DotNetVersion.Domain.Entities
{
    class Project
    {
        public Project(ProjectMap item)
        {
            Id = item.Id;
            Name = item.Name;
            File = new FileInfo(item.File);
            UsesVersionPrefix = item.UsesVersionPrefix;
            Version = item.Version;
            NewVersion = item.Version;
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public FileInfo File { get; set; }

        public bool UsesVersionPrefix { get; set; }

        public Version Version { get; set; }

        [JsonIgnore]
        public Version NewVersion { get; set; }

        public IEnumerable<Project> Dependencies { get; set; }

        [JsonIgnore]
        public IEnumerable<Project> Dependents { get; set; } = new List<Project>();

        public override string ToString()
        {
            return ToString(-1);
        }

        public string ToString(int maxDepth)
        {
            return ToString(maxDepth, "");
        }

        public string ToString(int maxDepth, string indent = "")
        {
            StringBuilder builder = new StringBuilder();

            if (maxDepth != 0)
            {
                if (Version == NewVersion)
                    builder.AppendLine($"{indent}--{Name} ({Version})");
                else
                    builder.AppendLine($"{indent}--{Name} ({Version} => {NewVersion})");

                foreach (var item in Dependencies)
                    builder.Append(item.ToString(maxDepth - 1, indent + "  |"));
            }

            return builder.ToString();
        }

        internal void CommitUpdatedVersionToFile()
        {
            XDocument document = XDocument.Load(File.FullName);

            var namespaceManager = new XmlNamespaceManager(new NameTable());

            var version = UsesVersionPrefix ?
                document.XPathSelectElement("/Project/PropertyGroup/VersionPrefix", namespaceManager) :
                document.XPathSelectElement("/Project/PropertyGroup/Version", namespaceManager);

            version.Value = NewVersion.ToString();
            document.Save(File.FullName);
            Commit();
        }

        internal void IncreaseVersion(VersionLevel level, int revision, bool recursive = false)
        {
            if (level == VersionLevel.Revision)
                NewVersion = new Version(Version.Major, Version.Minor, Version.Build, revision);
            else if (level == VersionLevel.Build)
                NewVersion = new Version(Version.Major, Version.Minor, Version.Build + 1, revision);
            else if (level == VersionLevel.Minor)
                NewVersion = new Version(Version.Major, Version.Minor + 1, 0, revision);
            else if (level == VersionLevel.Major)
                NewVersion = new Version(Version.Major + 1, 0, 0, revision);

            if (recursive)
            {
                foreach (var dep in Dependents)
                    dep.IncreaseVersion(level, revision, recursive);
            }
        }

        public void Rollback()
        {
            NewVersion = Version;
        }

        public void Commit()
        {
            Version = NewVersion;
        }

        [JsonIgnore]
        public bool IsChanged
        {
            get
            {
                return Version != NewVersion;
            }
        }

        internal void AddDependsFrom(Project prj)
        {
            var deps = Dependents as IList<Project>;
            deps.Add(prj);
        }
    }
}

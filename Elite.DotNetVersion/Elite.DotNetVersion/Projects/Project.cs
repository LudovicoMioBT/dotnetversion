using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;

namespace Elite.DotNetVersion.Projects
{
    class Project
    {
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

        public Project(ProjectMap item)
        {
            this.Id = item.Id;
            this.Name = item.Name;
            this.File = new FileInfo(item.File);
            this.UsesVersionPrefix = item.UsesVersionPrefix;
            this.Version = item.Version;
            this.NewVersion = item.Version;
        }

        public override string ToString()
        {
            return this.ToString(-1);
        }

        public string ToString(int maxDepth)
        {
            return this.ToString(maxDepth, "");
        }

        public string ToString(int maxDepth, string indent = "")
        {
            StringBuilder builder = new StringBuilder();

            if (maxDepth != 0)
            {
                if (this.Version == this.NewVersion)
                    builder.AppendLine($"{indent}--{this.Name} ({this.Version})");
                else
                    builder.AppendLine($"{indent}--{this.Name} ({this.Version} => {this.NewVersion})");

                foreach (var item in this.Dependencies)
                    builder.Append(item.ToString(maxDepth - 1, indent + "  |"));
            }

            return builder.ToString();
        }

        internal void CommitUpdatedVersionToFile()
        {
            XDocument document = XDocument.Load(this.File.FullName);

            var namespaceManager = new XmlNamespaceManager(new NameTable());

            var version = this.UsesVersionPrefix ?
                document.XPathSelectElement("/Project/PropertyGroup/VersionPrefix", namespaceManager) :
                document.XPathSelectElement("/Project/PropertyGroup/Version", namespaceManager);

            version.Value = this.NewVersion.ToString();
            document.Save(this.File.FullName);
            this.Commit();
        }

        internal void IncreaseVersion(VersionLevel level, int revision, bool recursive = false)
        {
            if (level == VersionLevel.Revision)
                this.NewVersion = new Version(this.Version.Major, this.Version.Minor, this.Version.Build, revision);
            else if (level == VersionLevel.Build)
                this.NewVersion = new Version(this.Version.Major, this.Version.Minor, this.Version.Build + 1, revision);
            else if (level == VersionLevel.Minor)
                this.NewVersion = new Version(this.Version.Major, this.Version.Minor + 1, 0, revision);
            else if (level == VersionLevel.Major)
                this.NewVersion = new Version(this.Version.Major + 1, 0, 0, revision);

            if (recursive)
            {
                foreach (var dep in this.Dependents)
                    dep.IncreaseVersion(level, revision, recursive);
            }
        }

        public void Rollback()
        {
            this.NewVersion = this.Version;
        }

        public void Commit()
        {
            this.Version = this.NewVersion;
        }

        [JsonIgnore]
        public bool IsChanged
        {
            get
            {
                return this.Version != this.NewVersion;
            }
        }

        internal void AddDependsFrom(Project prj)
        {
            var deps = this.Dependents as IList<Project>;
            deps.Add(prj);
        }
    }
}

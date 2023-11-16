using Microsoft.Build.Construction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Elite.DotNetVersion.Domain.Entities
{
    class ProjectMap
    {
        public static readonly Version NotVersionedVersion = new Version("0.0.0.0");

        private ProjectMap() { }

        public string File { get; private set; }

        public string Name { get; private set; }

        public Guid Id { get; private set; }

        public Version Version { get; private set; }

        public bool UsesVersionPrefix { get; private set; }

        public IEnumerable<string> ProjectReferences { get; private set; }

        public IEnumerable<(string Name, string Version)> PackageReferences { get; private set; }

        public static ProjectMap Create(ProjectInSolution prj)
        {
            XDocument document = XDocument.Load(prj.AbsolutePath);

            var namespaceManager = new XmlNamespaceManager(new NameTable());
            var pRefs = document.XPathSelectElements("/Project/ItemGroup/ProjectReference", namespaceManager);
            var nRefs = document.XPathSelectElements("/Project/ItemGroup/PackageReference", namespaceManager);

            var version = GetVersion(
                document.XPathSelectElement("/Project/PropertyGroup/VersionPrefix", namespaceManager)?.Value,
                document.XPathSelectElement("/Project/PropertyGroup/Version", namespaceManager)?.Value);

            return new ProjectMap
            {
                Id = new Guid(prj.ProjectGuid),
                File = prj.AbsolutePath,
                Name = prj.ProjectName,
                Version = version.version,
                UsesVersionPrefix = version.usesPrefix,
                ProjectReferences = ExtractProjects(pRefs).ToArray(),
                PackageReferences = ExtractPackages(nRefs).ToArray(),
            };
        }

        private static (bool usesPrefix, Version version) GetVersion(string versionPrefix, string version)
        {
            if (string.IsNullOrEmpty(version) && string.IsNullOrEmpty(versionPrefix))
                return (false, NotVersionedVersion);

            bool usesPrefix = !string.IsNullOrEmpty(versionPrefix);
            return (usesPrefix, new Version(usesPrefix ? versionPrefix : version));
        }

        private static IEnumerable<string> ExtractProjects(IEnumerable<XElement> enumerable)
        {
            return from item in enumerable
                   select Path.GetFileNameWithoutExtension(item.Attribute("Include").Value);
        }

        private static IEnumerable<(string Name, string Version)> ExtractPackages(IEnumerable<XElement> enumerable)
        {
            return from item in enumerable
                   select (Name: item.Attribute("Include").Value, Version: item.Attribute("Version")?.Value ?? NotVersionedVersion.ToString());
        }

        public bool IsVersioned => Version != NotVersionedVersion;

        public override string ToString()
        {
            return $"{Id} | {Name} | {Version} | {ProjectReferences.Count()}";
        }
    }
}

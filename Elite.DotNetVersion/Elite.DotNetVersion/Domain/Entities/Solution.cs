using Microsoft.Build.Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elite.DotNetVersion.Domain.Entities
{
    class Solution
    {
        private Solution(string name, IEnumerable<Project> projects)
        {
            Name = name;
            Projects = projects;
        }

        public string Name { get; }

        public IEnumerable<Project> Projects { get; }

        public IEnumerable<Project> FindByNames(IEnumerable<string> names)
        {
            return (from p in Projects
                    where names.Contains(p.Name)
                    select p).ToArray();
        }

        public IEnumerable<Project> FindDependentsByName(IEnumerable<string> projects)
        {
            var found = FindByNames(projects);

            if (found != null)
                return FindDependentsByName(found).Distinct();

            return Enumerable.Empty<Project>();
        }

        private IEnumerable<Project> FindDependentsByName(IEnumerable<Project> items)
        {
            foreach (var item in items)
            {
                yield return item;

                foreach (var d in FindDependentsByName(item.Dependents))
                    yield return d;
            }
        }

        public static Solution FromProjects(string name, IEnumerable<ProjectInSolution> projects)
        {
            var map = from prj in projects
                      where prj.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat
                      || prj.ProjectType == SolutionProjectType.Unknown
                      select ProjectMap.Create(prj);

            return FromMap(name, map);
        }

        public static Solution FromMap(string name, IEnumerable<ProjectMap> map)
        {
            IEnumerable<Project> projects = (from item in map
                                             where item.IsVersioned
                                             select new Project(item)).ToArray();

            foreach (var item in map)
            {
                Project prj = projects.SingleOrDefault(o => o.Id == item.Id);

                if (prj != null)
                {
                    prj.Dependencies = (from dep in projects
                                        where item.ProjectReferences.Contains(dep.Name)
                                        select dep)
                                        .ToArray();

                    foreach (var by in prj.Dependencies)
                        by.AddDependsFrom(prj);
                }
            }

            return new Solution(name, projects);
        }

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

            builder.AppendLine(Name);

            if (maxDepth != 0)
            {
                foreach (var item in Projects)
                    builder.Append(item.ToString(maxDepth - 1, indent + "  |"));
            }

            return builder.ToString();
        }
    }
}

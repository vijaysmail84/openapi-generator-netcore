using APIGenerator.Builder.NetCore._6._0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIGenerator.Builder
{
    public class Generator
    {
        public ICodeBuilder builder = null;
        /// <summary>
        /// E.g. netcore, java, ruby
        /// </summary>
        public string BuilderType { get; set; }
        /// <summary>
        /// Net core Framework Version 
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Output of the final solution
        /// </summary>
        public string OutputPath { get; set; }

        /// <summary>
        /// Input of yaml file
        /// </summary>
        public string OpeAPISpecPath { get; set; }

        /// <summary>
        /// Solution name XXXX.sln
        /// </summary>
        public string SolutionName { get; set; }

        /// <summary>
        /// In case of single project name this be used for csproj
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Create solution by grouping csproj based on tags
        /// </summary>
        public bool BreakProjectsByTags { get; set; }

        /// <summary>
        /// Use sample inputs of pets.yaml instead of user input
        /// </summary>
        public bool UseSampelInput { get; set; }

        public Generator()
        {

        }

        public Generator(string _builderType, string _version)
        {
            ResolveBuilder();
        }

        public void setBuilder(ICodeBuilder _builder)
        {
            builder = _builder;
        }

        public void Generate()
        {
            builder.Version = Version;
            builder.OutputPath = OutputPath;
            builder.OpeAPISpecPath = OpeAPISpecPath;
            builder.SolutionName = SolutionName;
            builder.ProjectName = ProjectName;
            builder.BreakProjectsByTags = BreakProjectsByTags;
            builder.UseSampelInput = UseSampelInput;
            builder.Generate();
        }

        public void ResolveBuilder()
        {
            switch (BuilderType)
            {
                case "netcore":
                    builder = new NetCoreCodeBuilder();
                    break;
                default:
                    builder = null;
                    break;
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIGenerator.Builder
{
    public interface ICodeBuilder
    {
        public string Version { get; set; }
        public string OutputPath { get; set; }
        public string OpeAPISpecPath { get; set; }
        public string SolutionName { get; set; }
        public string ProjectName { get; set; }
        public bool BreakProjectsByTags { get; set; }
        public bool UseSampelInput { get; set; }
        Task InitiliazeYaml();
        void GenerateFolders();

        void GenerateSolutionFile();
        void GenerateCsProj();
        void GenerateModels(string modeltype);
        void GenerateDTOs();
        void GenerateControllers();
        void GenerateStartUp();
        void GenerateAppSettings();
        void GenerateAttributes();
        void GenerateServices(dynamic input, string projname);
        void GenerateDockerFile();
        void Generate();
    }
}

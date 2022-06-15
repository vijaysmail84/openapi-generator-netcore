using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Mustache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using APIGenerator.Shared;
using System.Globalization;

namespace APIGenerator.Builder.NetCore._6._0
{
    internal class NetCoreCodeBuilder : ICodeBuilder
    {

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

        /// <summary>
        /// Open API deserialized object
        /// </summary>
        public OpenApiDocument openApiDocument = null;

        /// <summary>
        /// Template path not to be changed.DONOT change
        /// </summary>
        public string basePath = @"..\..\..\Template\NetCore\";

        /// <summary>
        /// Distinct list of tags in open api spec.
        /// </summary>
        public List<string> tagList = new List<string>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public NetCoreCodeBuilder()
        {
        }

        /// <summary>
        /// Director to run builders in an order.
        /// </summary>
        public void Generate()
        {
            Console.WriteLine("********* Porcess started **********");
            InitiliazeYaml().Wait();
            Console.WriteLine("********* YAMl file fetched and parsed **********");
            GenerateFolders();
            Console.WriteLine("********* Folders generated **********");
            GenerateCsProj();
            Console.WriteLine("********* Csproj files generated **********");
            GenerateSolutionFile();
            Console.WriteLine("********* Solution file  generated **********");
            GenerateModels("Model");
            Console.WriteLine("********* Models generated **********");
            //GenerateDTOs();
            Console.WriteLine("********* DTOs generated **********");
            GenerateControllers();
            Console.WriteLine("********* Controllers generated **********");
            GenerateStartUp();
            Console.WriteLine("********* Program.cs generated **********");
            GenerateAttributes();
            Console.WriteLine("********* Attributes generated **********");
            GenerateAppSettings();
            Console.WriteLine("********* Appsettings generated **********");
            GenerateDockerFile();
            Console.WriteLine("********* Docker files generated **********");
            Console.WriteLine("********* Process end **********");
        }

        /// <summary>
        /// Initialze and deserialize yaml file 
        /// </summary>
        /// <returns></returns>
        public async Task InitiliazeYaml()
        {

            string input = UseSampelInput == true ? $"{basePath}\\{Version}\\pets.yaml" : OpeAPISpecPath;
            using (FileStream fs = File.OpenRead(input))
            {
                openApiDocument = new OpenApiStreamReader().Read(fs, out var diagnostic1);
                tagList = Utility.GetDistinctTags(openApiDocument);
            }
            Console.WriteLine($"   ------> File used -> {input}");
        }

        /// <summary>
        /// Generate Folders structure.
        /// </summary>
        public void GenerateFolders()
        {
            //Base directory structure
            Directory.CreateDirectory(@$"{OutputPath}");
            Directory.CreateDirectory(@$"{OutputPath}");
            Directory.CreateDirectory(@$"{OutputPath}\components");
            Directory.CreateDirectory(@$"{OutputPath}\obj");

            if (BreakProjectsByTags)
            {
                foreach (var t in tagList)
                {
                    //Default project folder naming will be Tag.API
                    var projname = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.ToLower())}.API";
                    Directory.CreateDirectory(@$"{OutputPath}\{projname}");
                    Directory.CreateDirectory(@$"{OutputPath}\{projname}\bin");
                    Directory.CreateDirectory(@$"{OutputPath}\{projname}\components");
                    Directory.CreateDirectory(@$"{OutputPath}\{projname}\obj");
                    Directory.CreateDirectory(@$"{OutputPath}\{projname}\.config");
                    Directory.CreateDirectory(@$"{OutputPath}\{projname}\Controllers");
                    Directory.CreateDirectory(@$"{OutputPath}\{projname}\Models");
                    Directory.CreateDirectory(@$"{OutputPath}\{projname}\Properties");
                    Directory.CreateDirectory(@$"{OutputPath}\{projname}\Services");
                    Directory.CreateDirectory(@$"{OutputPath}\{projname}\DTO");
                }
            }
            else
            {
                //Use as is project name passed by user for single project
                var projname = $"{ProjectName}";
                Directory.CreateDirectory(@$"{OutputPath}\{projname}");
                Directory.CreateDirectory(@$"{OutputPath}\{projname}\bin");
                Directory.CreateDirectory(@$"{OutputPath}\{projname}\components");
                Directory.CreateDirectory(@$"{OutputPath}\{projname}\obj");
                Directory.CreateDirectory(@$"{OutputPath}\{projname}\.config");
                Directory.CreateDirectory(@$"{OutputPath}\{projname}\Controllers");
                Directory.CreateDirectory(@$"{OutputPath}\{projname}\Models");
                Directory.CreateDirectory(@$"{OutputPath}\{projname}\Properties");
                Directory.CreateDirectory(@$"{OutputPath}\{projname}\Services");
                Directory.CreateDirectory(@$"{OutputPath}\{projname}\DTO");
            }
        }

        /// <summary>
        /// Generate csproj files
        /// </summary>
        public void GenerateCsProj()
        {
            if (BreakProjectsByTags)
            {
                foreach (var t in tagList)
                {
                    var projname = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.ToLower())}.API";
                    var input = new
                    {
                        projectdescriptionn = projname,
                        projectcopyright = projname
                    };

                    string outputpath = @$"{OutputPath}\{projname}\{projname}.csproj";
                    Utility.MergeTemplate(Component.Csproj, input, basePath, outputpath, Version);
                    Console.WriteLine($"   ------> Csproj -> {projname}");
                }
            }
            else
            {
                var projname = ProjectName;
                var input = new
                {
                    projectdescriptionn = projname,
                    projectcopyright = projname
                };

                string outputpath = @$"{OutputPath}\{projname}\{projname}.csproj";
                Utility.MergeTemplate(Component.Csproj, input, basePath, outputpath, Version);
                Console.WriteLine($"   ------> Csproj -> {projname}");
            }
        }

        /// <summary>
        /// Generate sln file
        /// </summary>
        public void GenerateSolutionFile()
        {
            string _solutionguid1 = "{" + Guid.NewGuid().ToString().ToUpper() + "}";
            string _solutionguid2 = "{" + Guid.NewGuid().ToString().ToUpper() + "}";

            List< ProjectModel> projectList = new List<ProjectModel>();
            if (BreakProjectsByTags)
            {
                foreach (var t in tagList)
                {
                    string projname = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.ToLower())}.API";
                    ProjectModel model = new ProjectModel()
                    {
                        projectfoldername = projname,
                        projectguid = "{" + Guid.NewGuid().ToString().ToUpper() + "}",
                        projectname = projname,
                        solutionguid1 = _solutionguid1
                    };

                    projectList.Add(model);
                }
            }
            else
            {
                string projname = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ProjectName.ToLower())}";
                ProjectModel model = new ProjectModel()
                {
                    projectfoldername = projname,
                    projectguid = "{" + Guid.NewGuid().ToString().ToUpper() + "}",
                    projectname = projname,
                    solutionguid1 = _solutionguid1
                };

                projectList.Add(model);
            }
             
            

            var input = new
            {
                solutionguid1 = _solutionguid1,
                solutionguid2 = _solutionguid2,
                projects  = projectList
            };

            string outputpath = @$"{OutputPath}\{SolutionName}.sln";
            Utility.MergeTemplate(Component.Sln, input, basePath, outputpath, Version);
            Console.WriteLine($"   ------> Solution  -> {SolutionName}");
        }

        /// <summary>
        /// Generate controllers.
        /// </summary>
        public void GenerateControllers()
        {

            //Create template input
            List<ControllerModel> operations = null;
            foreach (var t in tagList)
            {
                operations = new List<ControllerModel>();
                var opeationsToAddInController = Utility.GetOperationsByTag(openApiDocument, t);
                foreach (var o in opeationsToAddInController)
                {
                    //Generate parameter input
                    var _operation = o;
                    var _path = o.path;
                    var _verb = o.verb;

                    var inputParam = "";

                    //Handles query string,url and cookie oarameter
                    foreach (var param in _operation.Value.Parameters)
                    {
                        //E.g. ([Required][FromQuery] int customerid)
                        inputParam = inputParam +
                            Utility.IsRequired(param.Required) +
                            Utility.ParameterAttribute(param.In != null ? param.In.ToString() : "") +
                            Utility.MapDataType(param.Schema.Type.ToLower()) +
                            Utility.IsNullable(param.Schema.Nullable)+
                            param.Name
                            + ", ";

                    }

                    //Remove the last , from list.
                    if (inputParam.Length > 0)
                    {
                        inputParam = inputParam.Substring(0, inputParam.Length - 2);
                    }

                    //If requestBody present append it with params as model
                    var bodyModel = _operation.Value.RequestBody == null ? "" : _operation.Value.RequestBody.Content.FirstOrDefault().Value.Schema.Reference.Id.FirstCharToUpper();
                    if (bodyModel.Length > 0)
                    {
                        if (inputParam.Length > 0)
                        {
                            inputParam = $"{inputParam}, [FromBody]{bodyModel}Model {System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(bodyModel.ToLower())}Model";
                        }
                        else
                        {
                            inputParam = $"[FromBody]{bodyModel}Model {System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(bodyModel.ToLower())}Model"; 
                        }
                    }

                    //Generate response status code returned by controller action.
                    List<ResponseModel> responsemodel = new List<ResponseModel>();
                    foreach (var item in _operation.Value.Responses)
                    {
                        ResponseModel response = new ResponseModel();
                        response.statuscode = item.Key.ToString() == "default" ? "500" : item.Key.ToString();
                        response.description = item.Value.Description;
                        response.modelname =
                            item.Value.Content.Values.Select(x => x.Schema).Any() == false ? "" : item.Value.Content.Values.Select(x => x.Schema).FirstOrDefault().Reference.Id.FirstCharToUpper() + "Model";
                        response.modeltype =
                            item.Value.Content.Values.Select(x => x.Schema).Any() == false ? "" : item.Value.Content.Values.Select(x => x.Schema).FirstOrDefault().Type.ToString();
                        response.returns = Utility.MapResponseReturnType(
                            item.Value.Content.Values.Select(x => x.Schema).Any() == false ? "" : item.Value.Content.Values.Select(x => x.Schema).FirstOrDefault().Type.ToString(),
                            item.Value.Content.Values.Select(x => x.Schema).Any() == false ? "" : item.Value.Content.Values.Select(x => x.Schema).FirstOrDefault().Reference.Id.FirstCharToUpper() + "Model");
                        responsemodel.Add(response);
                    }

                    //Generate operation/action input for mustache templte
                    ControllerModel op = new ControllerModel();
                    op.OperationName = _operation.Value.OperationId == null ? "OperationIdNotDefined" : _operation.Value.OperationId.FirstCharToUpper();
                    op.OperationSummary = _operation.Value.Summary;
                    op.OperationPath = _path;
                    op.OperationVerb = _verb;
                    op.OperationParameters = inputParam;
                    op.OperationResponses = responsemodel;
                    op.ServiceParameters = inputParam.Replace("[FromBody]", "")
                                                    .Replace("[FromQuery]", "")
                                                    .Replace("[FromRoute]", "")
                                                    .Replace("[Required]", "");
                    operations.Add(op);
                }

                //Check if any models exists
                List<string> listofModelsinController = Utility.GetListOfModelsForTag(t, tagList, openApiDocument);

                //Input for mustache template. 
                var input = new
                {
                    controllername = $"{t.FirstCharToUpper()}Controller",
                    basenamespace = BreakProjectsByTags == true ? $"{t.FirstCharToUpper()}" : ProjectName.Replace(".API", ""),
                    servicename = $"{t.FirstCharToUpper()}Service",
                    Operations = operations,
                    HasModel = listofModelsinController.Count == 0 ? "" : "HasModel"
                };

                //Create controller in the right project folder. Either single project or multiple projects. Switch using the flag BreakProjectsByTags.
                var projname = BreakProjectsByTags == true ? t.FirstCharToUpper() + ".API" : ProjectName;

                string outputpath = @$"{OutputPath}\{projname}\Controllers\{input.controllername}.cs";

                Utility.MergeTemplate(Component.Controller, input, basePath, outputpath, Version);
                Console.WriteLine($"   ------> Controller created -> {projname}\\Controllers\\{input.controllername}.cs");
                //Generate services per controller. Put them in the right folder based on the flag BreakProjectsByTags.
                var inputServices = new
                {
                    servicename = $"{t.FirstCharToUpper()}Service",
                    basenamespace = BreakProjectsByTags == true ?$"{t.FirstCharToUpper()}" : ProjectName.Replace(".API", ""),
                    Operations = operations,
                    HasModel = listofModelsinController.Count == 0 ? "" : "HasModel"
                };

                GenerateServices(inputServices, projname);
            }
        }

        /// <summary>
        /// Generate services
        /// </summary>
        public void GenerateServices(dynamic input, string projname)
        {
            //Generate Interface
            string outputpathInterface = @$"{OutputPath}\{projname}\Services\I{input.servicename}.cs";
            Utility.MergeTemplate(Component.IServices, input, basePath, outputpathInterface, Version);

            //Generate class
            string outputpathClass = @$"{OutputPath}\{projname}\Services\{input.servicename}.cs";
            Utility.MergeTemplate(Component.Services, input, basePath, outputpathClass, Version);
            Console.WriteLine($"   ------> Service Interface created -> {projname}\\Services\\I{input.servicename}.cs");
            Console.WriteLine($"   ------> Service created -> {projname}\\Services\\{input.servicename}.cs");
        }

        /// <summary>
        /// Generate Model classes.
        /// </summary>
        public void GenerateModels(string modeltype)
        {
            //Move models to right foldes of project using flag BreakProjectsByTags.
            if (BreakProjectsByTags)
            {
                
                List<string> filteredModel = null;
                //Create models based on the tags and move them to right project.
                foreach (var tagItem in tagList)
                {
                    //Model filtering per tag START
                    filteredModel = new List<string>(); 
                    var operations = new List<ControllerModel>();
                    filteredModel = Utility.GetModelForController(tagItem, openApiDocument);
                    //Model filtering per tag END

                    //Get all the the componet -> schema list
                    var schemaList = openApiDocument.Components.Schemas.Values.Select(z => z);

                    //This is multiple project so get it from tag name running in loop.
                    var modelName = "";
                    var modelNameSpace = $"{tagItem.FirstCharToUpper()}.API";

                    List<ModelModel> modelitems = null;

                    //Lopp through each schema in compont -> schema list and create only filtered models.
                    foreach (var x in schemaList)
                    {
                        string modelNameInherited;
                        //Check if the model belongs to current tag running in loop.
                        if (filteredModel.Contains(x.Reference.Id))
                        {
                            //If the model is object or array.. 
                            //array - create IEnumerable of the reference component.
                            //object - get the datatype pf property as the object name.
                            modelitems = new List<ModelModel>();
                            if (x.Type.ToString() == "array")
                            {
                                //Type = "array"
                                modelName = x.Reference.Id.FirstCharToUpper() + modeltype;
                                bool hasOneAllOf = x.AllOf.Count == 1 ? true : false;
                                string allOfSchemaToInherit = hasOneAllOf == true ? x.AllOf.FirstOrDefault().Reference.Id : "";
                                
                                if (hasOneAllOf)
                                {
                                    modelNameInherited = $"{modelName} : {allOfSchemaToInherit.FirstCharToUpper()}{modeltype}";
                                }
                                else
                                {
                                    modelNameInherited = modelName;
                                }

                                ModelModel modelModel = new ModelModel()
                                {
                                    name = x.Items.Reference.Id.FirstCharToUpper(),
                                    datatype = $"IEnumerable<{x.Items.Reference.Id.FirstCharToUpper()}{modeltype}>",
                                    isreadonly = "",
                                    description = $"List of {x.Items.Reference.Id}",
                                    datamembername = x.Items.Reference.Id
                                };
                                modelitems.Add(modelModel);
                            }
                            else
                            {
                                //Type = "object"
                                modelName = x.Reference.Id.FirstCharToUpper() + modeltype;
                                bool hasOneAllOf = x.AllOf.Count == 1 ? true : false;
                                string allOfSchemaToInherit = hasOneAllOf == true ? x.AllOf.FirstOrDefault().Reference.Id : "";
                               
                                if (hasOneAllOf)
                                {
                                    modelNameInherited = $"{modelName} : {allOfSchemaToInherit.FirstCharToUpper()}{modeltype}"; 
                                }
                                else
                                {
                                    modelNameInherited = modelName;
                                }

                                foreach (var item in x.Properties)
                                {
                                    //TODO : For oneof we need to think the return datatype
                                    var modelReturnType = item.Value.Reference == null ? "dynamic" : item.Value.Reference.Id.FirstCharToUpper() + modeltype;

                                    string dataAnnotations = Utility.GenerateDataAnnotations(item);
                                    ModelModel modelModel = new ModelModel()
                                    {
                                        name = item.Key.ToString().FirstCharToUpper(),
                                        datatype = Utility.MapComplexDataType(item.Value.Type == "object" ? modelReturnType : item.Value.Type.ToString(), item, modeltype),
                                        isreadonly = "",
                                        description = item.Value.Description,
                                        datamembername = item.Key.ToString(),
                                        dataannotations = dataAnnotations
                                    };

                                    modelitems.Add(modelModel);
                                }
                            }

                            //Final input for model to mustache template.
                            var input = new
                            {
                                modelnamespace = modelNameSpace,
                                modelname = modelName,
                                modelItems = modelitems,
                                modelnameinherited = modelNameInherited
                            };

                            var projname = $"{tagItem.FirstCharToUpper()}.API";
                            string outputpath = string.Empty;
                            
                            //Choose between model or dto creation. Common method used for model and dto.
                            if (modeltype == "Model")
                            {
                                outputpath = @$"{OutputPath}\{projname}\Models\{input.modelname}.cs";
                            }
                            else
                            {
                                outputpath = @$"{OutputPath}\{projname}\DTO\{input.modelname}.cs";
                            }
                            Utility.MergeTemplate(modeltype == "Model" ? Component.Model : Component.Dto, input, basePath, outputpath, Version);
                            Console.WriteLine($"   ------> Model created -> {projname}\\Models\\{input.modelname}.cs");
                        }
                    }
                }
            }
            else
            {
                //Get all the the componet -> schema
                var schemaList = openApiDocument.Components.Schemas.Values.Select(z => z);

                //This is single project so get it from user.
                var modelName = "";
                var modelNameSpace = ProjectName;

                List<ModelModel> modelitems = null;

                foreach (var x in schemaList)
                {
                    modelitems = new List<ModelModel>();
                    if (x.Type.ToString() == "array")
                    {
                        modelName = x.Reference.Id + modeltype;
                        ModelModel modelModel = new ModelModel()
                        {
                             name = x.Items.Reference.Id.FirstCharToUpper(),
                             datatype = $"IEnumerable<{x.Items.Reference.Id.FirstCharToUpper()}{modeltype}>",
                             isreadonly = "",
                             description = $"List of {x.Items.Reference.Id}"
                         };
                        modelitems.Add(modelModel);
                    }
                    else
                    {
                        modelName = x.Reference.Id + modeltype;
                        foreach (var item in x.Properties)
                        {
                            ModelModel modelModel = new ModelModel()
                            {
                                name = item.Key.ToString().FirstCharToUpper(),
                                datatype = Utility.MapComplexDataType(item.Value.Type == "object" ? item.Value.Reference.Id.ToString().FirstCharToUpper() + modeltype : item.Value.Type.ToString(), item, modeltype),
                                isreadonly = "",
                                description = item.Value.Description
                            };

                            modelitems.Add(modelModel);
                        }
                    }

                    var input = new
                    {
                        modelnamespace = modelNameSpace,
                        modelname = modelName,
                        modelItems = modelitems
                    };

                    var projname = ProjectName;

                    string outputpath = string.Empty;
                    if (modeltype == "Model")
                    {
                        outputpath = @$"{OutputPath}\{projname}\Models\{input.modelname}.cs";
                    }
                    else
                    {
                        outputpath = @$"{OutputPath}\{projname}\DTO\{input.modelname}.cs";
                    }
                    Utility.MergeTemplate(modeltype == "Model" ? Component.Model : Component.Dto, input, basePath, outputpath, Version);
                }
            }
        }

        /// <summary>
        /// Generate DTO classes.
        /// </summary>
        public void GenerateDTOs()
        {
            GenerateModels("Dto");
        }

        /// <summary>
        /// Generate Startup.cs or Program.cs class
        /// </summary>
        public void GenerateStartUp()
        {
            //TODO: Modify template and add custom inputs.

            if (BreakProjectsByTags)
            {
                foreach (var t in tagList)
                {
                    var input = "";
                    var projname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.ToLower()) + ".API";

                    string outputpath = @$"{OutputPath}\{projname}\Program.cs";
                    Utility.MergeTemplate(Component.Program, input, basePath, outputpath, Version);
                    Console.WriteLine($"   ------> Program.cs created -> {projname}\\Program.cs");
                }
            }
            else
            {
                var input = "";
                var projname = ProjectName;

                string outputpath = @$"{OutputPath}\{projname}\Program.cs";
                Utility.MergeTemplate(Component.Program, input, basePath, outputpath, Version);
                Console.WriteLine($"   ------> Program.cs created -> {projname}\\Program.cs");
            }
          
        }


        /// <summary>
        /// Generate appsetting files.
        /// </summary>
        public void GenerateAppSettings()
        {
            if (BreakProjectsByTags)
            {
                foreach (var t in tagList)
                {
                    var input = "";
                    var projname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.ToLower()) + ".API";
                    //TODO: Update template and add custom inputs
                    string outputpath = @$"{OutputPath}\{projname}\appsettings.Development.json";
                    Utility.MergeTemplate(Component.Appsettings, input, basePath, outputpath, Version);
                    Console.WriteLine($"   ------> Appsetting created -> {projname}\\appsettings.Development.json");

                    //TODO: Update template and add custom inputs
                    outputpath = @$"{OutputPath}\{projname}\appsettings.Production.json";
                    Utility.MergeTemplate(Component.Appsettings, input, basePath, outputpath, Version);
                    Console.WriteLine($"   ------> Appsetting created -> {projname}\\appsettings.Production.json");
                }
            }
            else
            {
                var input = "";
                var projname = ProjectName;
                //TODO: Update template and add custom inputs
                string outputpath = @$"{OutputPath}\{projname}\appsettings.Development.json";
                Utility.MergeTemplate(Component.Appsettings, input, basePath, outputpath, Version);

                //TODO: Update template and add custom inputs
                outputpath = @$"{OutputPath}\{projname}\appsettings.Production.json";
                Utility.MergeTemplate(Component.Appsettings, input, basePath, outputpath, Version);

            }
        }

        /// <summary>
        /// Generate attributes classes.
        /// </summary>
        public void GenerateAttributes()
        {
        }

        public void GenerateDockerFile()
        {
            if (BreakProjectsByTags)
            {
                foreach (var t in tagList)
                {
                    var projname = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.ToLower())}.API";
                    var input = new
                    {
                        projfoldername = projname,
                        projname = projname
                    };

                    string outputpath = @$"{OutputPath}\{projname}\Dockerfile";
                    Utility.MergeTemplate(Component.Dockerfile, input, basePath, outputpath, Version);
                    Console.WriteLine($"   ------> Docker file for project -> {projname}");
                }
            }
            else
            {
                var projname = ProjectName;
                var input = new
                {
                    projfoldername = projname,
                    projname = projname
                };

                string outputpath = @$"{OutputPath}\{projname}\Dockerfile";
                Utility.MergeTemplate(Component.Dockerfile, input, basePath, outputpath, Version);
                Console.WriteLine($"   ------> Docker file for project -> {projname}");
            }

        }
    }
}

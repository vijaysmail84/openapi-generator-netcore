using APIGenerator.Builder.NetCore._6._0;
using Microsoft.OpenApi.Models;
using Mustache;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIGenerator.Shared
{
    static class Utility
    {
        public static string GetTemplatePath(Component component, string version, string basePath)
        {
            return $"{basePath}\\{version}\\{component}.mustache";
        }

        public static dynamic GetSettings(string basePath, string version)
        {
            using (StreamReader r = new StreamReader($"{basePath}\\{version}\\appsettings.json"))
            {
                string json = r.ReadToEnd();
                List<Settings> items = JsonConvert.DeserializeObject<List<Settings>>(json);
                return items;
            }

        }

        public static string IsRequired(bool required)
        {
            return (required == true ? "[Required]" : "");
        }
        public static string IsNullable(bool nullable)
        {
            return (nullable == true ? "? " : " ");
        }


        public static string ParameterAttribute(string input)
        {
            switch (input)
            {
                case "query":
                    return "[FromQuery]";
                case "path":
                    return "[FromRoute]";
                case "cookie":
                    return "[FromCookie]";
                default:
                    return "";
            }
        }

        public static string MapComplexDataType(string input, KeyValuePair<string, OpenApiSchema> item, string modelsuffix)
        {
            if(item.Value.Reference == null)
            {
                if (item.Value.Type == "object")
                {
                    
                }
                else if(item.Value.Type == "array")
                { 
                    //IF we have one OneOf pick the schema name IEnumerable<name>
                    if(item.Value.Items.Reference == null)
                    {
                        if(item.Value.Items.OneOf.Count == 1)
                        {
                            foreach (var x in item.Value.Items.OneOf)
                            {
                                input = $"IEnumerable<{x.Reference.Id.FirstCharToUpper()}{modelsuffix}>" ;
                            }
                        }
                        
                    }
                    else
                    {
                        input = $"IEnumerable<{item.Value.Items.Reference.Id.FirstCharToUpper()}{modelsuffix}>";
                    }
                }

            }

            switch (input)
            {
                case "int32":
                case "int64":
                case "integer":
                    return "int";
                case "number":
                    return "decimal";
                case "float":
                    return "float";
                case "object":

                default:
                    return input;
            }
        }

        public static string MapDataType(string input)
        {
           
            switch (input)
            {
                case "int32":
                case "int64":
                case "integer":
                    return "int";
                case "number":
                    return "decimal";
                case "float":
                    return "float";
                case "object":

                default:
                    return input;
            }
        }

        public static void MergeTemplate(Component component, dynamic input, string basePath, string outputpath, string version)
        {
            var settings = Utility.GetSettings(basePath, version);

            //Get template
            string readText = File.ReadAllText(Utility.GetTemplatePath(component, version, basePath));

            //Init Mustache engine
            FormatCompiler compiler = new FormatCompiler();
            compiler.RemoveNewLines = false;

            //Merge template with data
            Mustache.Generator generator = compiler.Compile(readText);

            //Create output file
            string result = generator.Render(input);

            //Get from appsettings config
            File.WriteAllText(outputpath, result, Encoding.UTF8);
        }

        public static string MapResponseReturnType(string input, string modelname)
        {
            switch (input)
            {
                case "array":
                    return $"typeof(IEnumerable<{modelname}>)"; //What if no model passed like string?
                case "object":
                    return $"typeof({modelname})";
                default:
                    return "";
            }
        }

        public static List<string> GetModelForController(string tagItem, OpenApiDocument openApiDocument)
        {
            List<string> filteredModel  = new List<string>();
            var operations = new List<ControllerModel>();

            //Get operations belonging to current tag in loop.
            var opeationsToAddInController = Utility.GetOperationsByTag(openApiDocument, tagItem);

            //Get Response schemas for all the operations for current tag in loop.
            var oo = opeationsToAddInController
             .SelectMany(y => y.Value.Responses)
             .SelectMany(x => x.Value.Content.Values)
             .Select(z => z.Schema.Reference.Id)
             .ToList();

            //Add responses for the operations to the filtermodel.
            foreach (var tt in oo)
            {
                filteredModel.Add(tt);
                var childSchemas = Utility.GetChildItems(tt, openApiDocument.Components.Schemas.Values.Select(z => z), true);
                filteredModel.AddRange(childSchemas);
            }

            //Get Request schemas for all the operations for current tag in loop.
            var aa = opeationsToAddInController
             .Select(y => y.Value.RequestBody);
            foreach (var xx in aa)
            {
                if (xx != null)
                {
                    var ss = xx.Content.Values.Select(v => v.Schema.Reference.Id).FirstOrDefault().ToString();
                    filteredModel.Add(ss);
                    var childSchemas = Utility.GetChildItems(ss, openApiDocument.Components.Schemas.Values.Select(z => z), true);
                    filteredModel.AddRange(childSchemas);
                }
            }
            return filteredModel;
        }

        public static List<string> GetDistinctTags(OpenApiDocument openApiDocument)
        {

            return openApiDocument.Paths.SelectMany(x => x.Value.Operations)
                .SelectMany(y => y.Value.Tags)
                .DistinctBy(v => v.Name)
                .Select(z => z.Name)
                .ToList();

        }

        public static IEnumerable<OperationModel> GetOperationsByTag(OpenApiDocument openApiDocument, string tagName)
        {

            var tags = openApiDocument.Paths
                .SelectMany(a => a.Value.Operations
                .Select(b => new OperationModel
                {
                    path = a.Key.ToString(),
                    Value = b.Value,
                    verb = b.Key.ToString()
                })).Where(x => x.Value.Tags.Any(c => c.Name == tagName));
                        
            return tags;
        }

        public static void HandleOneof(KeyValuePair<string, OpenApiSchema> input, IEnumerable<OpenApiSchema> schemaList)
        {
            if(input.Value.OneOf != null)
            {
                foreach(var item in input.Value.OneOf)
                {
                    child.Add(item.Reference.Id.ToString());
                    GetChildItems(item.Reference.Id.ToString(), schemaList, false);
                }
            }

        }

        static List<string> child;
        public static List<string> GetChildItems(string inputschema, IEnumerable<OpenApiSchema> schemaList, bool isParent)
        {
            if (isParent)
            {
                child = new List<string>();
            }

            var schemaObject = schemaList.Select(z => z).Where(x => x.Reference.Id == inputschema).FirstOrDefault();
            if (schemaObject.Type.ToString() == "object")
            {
                //If allOf at schema level
                if(schemaObject.AllOf.Count == 1)
                {
                    child.Add(schemaObject.AllOf.FirstOrDefault().Reference.Id.ToString());
                    GetChildItems(schemaObject.AllOf.FirstOrDefault().Reference.Id.ToString(), schemaList, false);
                }

                foreach (var item in schemaObject.Properties)
                {
                    if (item.Value.Type == "object")
                    {
                        //Can happend in case of Oneof 
                        if (item.Value.Reference == null)
                        {
                            HandleOneof(item, schemaList);
;                       }
                        else
                        {
                            child.Add(item.Value.Reference.Id.ToString());
                            GetChildItems(item.Value.Reference.Id.ToString(), schemaList, false);
                          
                        }

                        //Handle all off at property level
                        if(item.Value.AllOf.Count > 0 )
                        {
                            GetChildItems(item.Value.Reference.Id.ToString(), schemaList, false);
                        }
                    }
                    else if (item.Value.Type == "array")
                    {
                        if(item.Value.Reference == null)
                        {
                            if (item.Value.Items.Reference != null)
                            {
                                child.Add(item.Value.Items.Reference.Id.ToString());
                                GetChildItems(item.Value.Items.Reference.Id.ToString(), schemaList, false);
                            }
                            else if (item.Value.Items.OneOf.Count > 0)
                            {
                                foreach(var x in item.Value.Items.OneOf)
                                {
                                    child.Add(x.Reference.Id);
                                    GetChildItems(x.Reference.Id, schemaList, false);
                                }
                               
                            }
                        }
                      
                    }
                }
            }

            if(schemaObject.Type.ToString() == "array")
            {
                child.Add(schemaObject.Items.Reference.Id.ToString());
                GetChildItems(schemaObject.Items.Reference.Id.ToString(), schemaList, false);
            }

            return child;
        }

        public static List<string> GetListOfModelsForTag(string currentTag, List<string> tagList, OpenApiDocument openApiDocument)
        {

            List<string> filteredModel = null;
            //Create models based on the tags and move them to right project.
            foreach (var tagItem in tagList)
            {
                if (currentTag == tagItem)
                {
                    //Model filtering per tag START
                    filteredModel = new List<string>();
                    var operations = new List<ControllerModel>();

                    //Get operations belonging to current tag in loop.
                    var opeationsToAddInController = Utility.GetOperationsByTag(openApiDocument, tagItem);

                    //Get Response schemas for all the operations for current tag in loop.
                    var oo = opeationsToAddInController
                     .SelectMany(y => y.Value.Responses)
                     .SelectMany(x => x.Value.Content.Values)
                     .Select(z => z.Schema.Reference.Id)
                     .ToList();

                    //Add responses for the operations to the filtermodel.
                    foreach (var tt in oo)
                    {
                        filteredModel.Add(tt);
                        var childSchemas = Utility.GetChildItems(tt, openApiDocument.Components.Schemas.Values.Select(z => z), true);
                        filteredModel.AddRange(childSchemas);
                    }

                    //Get Request schemas for all the operations for current tag in loop.
                    var aa = opeationsToAddInController
                     .Select(y => y.Value.RequestBody);
                    foreach (var xx in aa)
                    {
                        if (xx != null)
                        {
                            var ss = xx.Content.Values.Select(v => v.Schema.Reference.Id).FirstOrDefault().ToString();
                            filteredModel.Add(ss);
                            var childSchemas = Utility.GetChildItems(ss, openApiDocument.Components.Schemas.Values.Select(z => z), true);
                            filteredModel.AddRange(childSchemas);
                        }
                    }

                }
            }

            return filteredModel;
        }

        public static string GenerateDataAnnotations(KeyValuePair<string, OpenApiSchema> item)
        {
            StringBuilder stringBuilder = new StringBuilder();

            //Required attribute
            if (item.Value.Required.Count > 0)
            {
                stringBuilder.Append($"[Required]");
            }

            //String Min and Max handling.
            if (item.Value.MaxLength != null && item.Value.MinLength != null)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append($"     [StringLength({item.Value.MaxLength}, MinimumLength={item.Value.MinLength})]");
                }
                else
                {
                    stringBuilder.Append($"[StringLength({item.Value.MaxLength}, MinimumLength={item.Value.MinLength})]");
                }
            }

            if(item.Value.MaxLength == null && item.Value.MinLength != null)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append($"     [MinLength({item.Value.MinLength})]");
                }
                else
                {
                    stringBuilder.Append($"[MinLength({item.Value.MinLength})]");
                }
            }

            if (item.Value.MaxLength != null && item.Value.MinLength == null)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append($"     [MaxLength({item.Value.MaxLength})]");
                }
                else
                {
                    stringBuilder.Append($"[MaxLength({item.Value.MaxLength})]");
                }
            }


            return stringBuilder.ToString();

        }
        public class OperationModel
        {
            public string verb { get; set; }
            public string path { get; set; }
            public OpenApiOperation Value { get; set; }
        }
    }
    
}

using APIGenerator.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Readers;
using System.Text;


//Entry point
Generator generator = new Generator();
generator.BuilderType = "netcore";
generator.Version = "6.0";
//Input variables
generator.UseSampelInput = false;
generator.OpeAPISpecPath = @"C:\api.yaml";
//Output variables
generator.OutputPath = @"C:\src02";
//Project variables
generator.SolutionName = "New.Service";
generator.ProjectName = "Pets.API";
generator.BreakProjectsByTags = true;
//Genrator
generator.ResolveBuilder();
generator.Generate();


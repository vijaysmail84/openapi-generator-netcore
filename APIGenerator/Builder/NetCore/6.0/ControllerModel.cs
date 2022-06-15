using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIGenerator.Builder.NetCore._6._0
{
    public class ControllerModel
    {
        /// <summary>
        /// Service layer params without [FromXXX]
        /// </summary>
        public string ServiceParameters { get; set; }
        /// <summary>
        /// Action name in the controller
        /// </summary>
        public string? OperationName { get; set; }
        /// <summary>
        /// Action verb
        /// </summary>
        public string? OperationVerb { get; set; }
        /// <summary>
        /// Action summary
        /// </summary>
        public string? OperationSummary { get; set; }
        /// <summary>
        /// Action path like \action\{id}
        /// </summary>
        public string? OperationPath { get; set; }
        /// <summary>
        /// Action input parameters coming as query string, url, cookie
        /// </summary>
        public string OperationParameters { get; set; }

        /// <summary>
        /// Action respone status code details
        /// </summary>
        public List<ResponseModel> OperationResponses { get; set; }
    }

    public class ResponseModel
    {
        /// <summary>
        /// Status code like 200
        /// </summary>
        public string statuscode { get; set; }
        /// <summary>
        /// Model name to bind with name+model
        /// </summary>
        public string modelname { get; set; }
        /// <summary>
        /// Description of the response
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Can be object or array
        /// </summary>
        public string modeltype { get; set; }
        /// <summary>
        /// Return object on the response.
        /// </summary>
        public string returns { get; set; }

    }

    public class ProjectModel
    {
        public string solutionguid1 { get; set; }
        public string projectname { get; set; }

        public string projectfoldername { get; set; }

        public string projectguid { get; set; }

    }

    public class ModelModel
    {
        public string modelnameinherited { get; set; }
        public string name { get; set; }
        public string datatype { get; set; }
        public string required { get; set; }
        public string min { get; set; }
        public string max { get; set; }
        public string length { get; set; }
        public string format { get; set; }
        public string isreadonly { get; set; }
        public string description { get; set; }
        public string datamembername { get; set; }
        public string dataannotations { get; set; }
    }
}
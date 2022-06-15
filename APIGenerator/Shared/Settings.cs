using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIGenerator.Shared
{
    public class Settings
    {
        public string? basepath { get; set; }
        public string? controllertemplate { get; set; }
        public string? modeltemplate { get; set; }
        public string? csprjtemplate { get; set; }
        public string? slntemplate { get; set; }
        public string? programtemplate { get; set; }

        public string? dockerfiletemplate { get; set; }

    }
}

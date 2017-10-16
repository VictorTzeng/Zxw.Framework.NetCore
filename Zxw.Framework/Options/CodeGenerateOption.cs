using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zxw.Framework.Options
{
    public class CodeGenerateOption
    {
        public string ModelsNamespace { get; set; }
        public string IRepositoriesNamespace { get; set; }
        public string RepositoriesNamespace { get; set; }
        public string IServicsNamespace { get; set; }
        public string ServicesNamespace { get; set; }
    }
}

namespace Zxw.Framework.NetCore.Options
{
    public class CodeGenerateOption
    {
        public string OutputPath { get; set; }
        public string ModelsNamespace { get; set; }
        public string ViewModelsNamespace { get; set; }
        public string ControllersNamespace { get; set; }
        public string IRepositoriesNamespace { get; set; }
        public string RepositoriesNamespace { get; set; }
        public string IServicesNamespace { get; set; }
        public string ServicesNamespace { get; set; }
    }
}

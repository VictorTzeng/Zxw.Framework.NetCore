namespace Zxw.Framework.NetCore.Options
{
    public class CodeGenerateOption
    {
        public virtual string OutputPath { get; set; }
        public virtual string ModelsNamespace { get; set; }
        public virtual string ViewModelsNamespace { get; set; }
        public virtual string ControllersNamespace { get; set; }
        public virtual string IRepositoriesNamespace { get; set; }
        public virtual string RepositoriesNamespace { get; set; }
        public virtual string IServicesNamespace { get; set; }
        public virtual string ServicesNamespace { get; set; }
        public bool IsPascalCase { get; set; } = true;
    }
}

using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Cqrs
{
    public interface ICommand
    {

    }
    /// <summary>
    /// Marker interface to mark a command
    /// </summary>
    public interface ICommand<T, TKey>: ICommand where T : BaseModel<TKey>
    {
        T Instance { get; set; }
    }
}
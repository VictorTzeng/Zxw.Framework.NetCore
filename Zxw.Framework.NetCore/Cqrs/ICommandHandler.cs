using System.Threading.Tasks;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Cqrs
{
    public interface ICommandHandler<in TParameter> where TParameter :ICommand
    {
        /// <summary>
        /// Executes a command handler
        /// </summary>
        /// <param name="command">The command to be used</param>
        void Execute(TParameter command);

        Task ExecuteAsync(TParameter command);

    }
    /// <summary>
    /// Base interface for command handlers
    /// </summary>
    /// <typeparam name="TParameter"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface ICommandHandler<in TParameter,T,TKey>: ICommandHandler<TParameter>
        where TParameter : ICommand<T,TKey> 
        where T : BaseModel<TKey>
    {
    }

}

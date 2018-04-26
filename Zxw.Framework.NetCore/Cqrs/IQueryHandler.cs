namespace Zxw.Framework.NetCore.Cqrs
{
    /// <summary>
    /// Base interface for query handlers
    /// </summary>
    /// <typeparam name="TParameter">Query type</typeparam>
    /// <typeparam name="TResult">Query Result type</typeparam>
    public interface IQueryHandler<in TParameter, out TResult> where TResult : IQueryResult where TParameter : IQuery
    {
        /// <summary>
        /// Retrieve a query result from a query
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns>Retrieve Query Result</returns>
        TResult Retrieve(TParameter query);
    }
}

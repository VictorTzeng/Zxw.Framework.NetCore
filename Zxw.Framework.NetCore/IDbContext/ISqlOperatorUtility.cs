using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.IDbContext
{
    public interface ISqlOperatorUtility: IScopedDependency
    {
        DataTable SqlQuery(string sql, CommandType cmdType = CommandType.Text, int cmdTimeout = 30, params DbParameter[] parameters);
        DataSet SqlQuery(List<string> sqlList, int cmdTimeout = 30, params DbParameter[] parameters);
        IList<T> SqlQuery<T>(string sql, CommandType cmdType = CommandType.Text, int cmdTimeout = 30, params DbParameter[] parameters);
        int ExecuteSqlCommand(string sql, CommandType cmdType = CommandType.Text, int cmdTimeout = 30, params DbParameter[] parameters);
        int ExecuteSqlCommandWithTransaction(List<string> sqlList, int cmdTimeout = 30,
            params DbParameter[] parameters);

        IList<T> ExecuteStoredProcedure<T>(string sql, int cmdTimeout = 30, DbParameter[] sqlParams = null)
            where T : new();
        object ExecuteScalar(string sql, CommandType cmdType = CommandType.Text, int cmdTimeout = 30, params DbParameter[] parameters);
        void ClearDataTables(params string[] tables);
    }
}

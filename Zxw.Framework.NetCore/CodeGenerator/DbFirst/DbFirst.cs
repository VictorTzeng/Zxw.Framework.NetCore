using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.CodeGenerator.DbFirst
{
    internal sealed class DbFirst : IDbFirst
    {
        private CodeGenerator Instance { get; set; }

        private IDbContextCore _dbContext;
        public IDbContextCore DbContext
        {
            get => _dbContext;
            set
            {
                _dbContext = value;
                AllTables = _dbContext.GetCurrentDatabaseTableList().ToList();
            }
        }

        private List<DbTable> AllTables { get; set; }

        public DbFirst(IOptions<CodeGenerateOption> option)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));
            Instance = new CodeGenerator(option.Value);
        }

        public void GenerateAll(bool ifExistCovered = false)
        {
            Instance.Generate(AllTables,ifExistCovered);
        }

        public IDbFirst Generate(Func<DbTable, bool> selector, bool ifExistCovered = false)
        {
            if (selector == null)
                selector = m => true;
            Instance.Generate(AllTables.Where(selector).ToList(), ifExistCovered);
            return this;
        }

        public IDbFirst GenerateEntity(Func<DbTable, bool> selector, bool ifExistCovered = false)
        {
            var tables = AllTables.Where(selector).ToList();
            foreach (var table in tables)
            {
                Instance.GenerateEntity(table, ifExistCovered);                
            }
            return this;
        }

        public IDbFirst GenerateSingle(Func<DbTable, bool> selector, bool ifExistCovered = false)
        {
            GenerateEntity(selector, ifExistCovered);
            GenerateIRepository(selector, ifExistCovered);
            GenerateRepository(selector, ifExistCovered);
            GenerateIService(selector, ifExistCovered);
            GenerateService(selector, ifExistCovered);
            GenerateController(selector, ifExistCovered);
            GenerateApiController(selector, ifExistCovered);

            return this;
        }

        public IDbFirst GenerateIRepository(Func<DbTable, bool> selector, bool ifExistCovered = false)
        {
            var tables = AllTables.Where(selector).ToList();
            foreach (var table in tables)
            {
                if (table.Columns.Any(c => c.IsPrimaryKey))
                {
                    var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                    Instance.GenerateIRepository(table.TableName, pkTypeName, ifExistCovered);
                }
            }

            return this;
        }

        public IDbFirst GenerateRepository(Func<DbTable, bool> selector, bool ifExistCovered = false)
        {
            var tables = AllTables.Where(selector).ToList();
            foreach (var table in tables)
            {
                if (table.Columns.Any(c => c.IsPrimaryKey))
                {
                    var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                    Instance.GenerateRepository(table.TableName, pkTypeName, ifExistCovered);
                }
            }

            return this;
        }

        public IDbFirst GenerateIService(Func<DbTable, bool> selector, bool ifExistCovered = false)
        {
            var tables = AllTables.Where(selector).ToList();
            foreach (var table in tables)
            {
                if (table.Columns.Any(c => c.IsPrimaryKey))
                {
                    var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                    Instance.GenerateIService(table.TableName, pkTypeName, ifExistCovered);
                }
            }

            return this;
        }

        public IDbFirst GenerateService(Func<DbTable, bool> selector, bool ifExistCovered = false)
        {
            var tables = AllTables.Where(selector).ToList();
            foreach (var table in tables)
            {
                if (table.Columns.Any(c => c.IsPrimaryKey))
                {
                    var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                    Instance.GenerateService(table.TableName, pkTypeName, ifExistCovered);
                }
            }

            return this;
        }

        public IDbFirst GenerateController(Func<DbTable, bool> selector, bool ifExistCovered = false)
        {
            var tables = AllTables.Where(selector).ToList();
            foreach (var table in tables)
            {
                if (table.Columns.Any(c => c.IsPrimaryKey))
                {
                    var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                    Instance.GenerateController(table.TableName, pkTypeName, ifExistCovered);
                }
            }

            return this;
        }

        public IDbFirst GenerateApiController(Func<DbTable, bool> selector, bool ifExistCovered = false)
        {
            var tables = AllTables.Where(selector).ToList();
            foreach (var table in tables)
            {
                if (table.Columns.Any(c => c.IsPrimaryKey))
                {
                    var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                    Instance.GenerateApiController(table.TableName, pkTypeName, ifExistCovered);
                }
            }

            return this;
        }

        public IDbFirst GenerateViewModel(string viewName, bool ifExistCovered = false)
        {
            var sql = $"select top 1 * from {viewName}";
            var dt = DbContext.GetDataTable(sql);
            GenerateViewModel(dt, viewName, ifExistCovered);
            return this;
        }

        public IDbFirst GenerateViewModel(DataTable dt, string className, bool ifExistCovered = false)
        {
            Instance.GenerateViewModel(dt, className, ifExistCovered);
            return this;
        }

        public IDbFirst GenerateViewModel(DataSet ds, bool ifExistCovered = false)
        {
            Instance.GenerateViewModel(ds, ifExistCovered);
            return this;
        }
    }
}

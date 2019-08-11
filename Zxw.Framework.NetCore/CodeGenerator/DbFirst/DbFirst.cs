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

        public void GenerateAll(bool ifExsitedCovered = false)
        {
            Instance.GenerateAllCodesFromDatabase(ifExsitedCovered);
        }

        public IDbFirst Generate(Func<DbTable, bool> selector, bool ifExsitedCovered = false)
        {
            Instance.GenerateAllCodesFromDatabase(ifExsitedCovered, selector);
            return this;
        }

        public IDbFirst GenerateEntity(Func<DbTable, bool> selector, bool ifExsitedCovered = false)
        {
            var tables = AllTables.Where(selector).ToList();
            foreach (var table in tables)
            {
                Instance.GenerateEntity(table, ifExsitedCovered);                
            }
            return this;
        }

        public IDbFirst GenerateSingle(Func<DbTable, bool> selector, bool ifExsitedCovered = false)
        {
            GenerateEntity(selector, ifExsitedCovered);
            GenerateIRepository(selector, ifExsitedCovered);
            GenerateRepository(selector, ifExsitedCovered);
            GenerateIService(selector, ifExsitedCovered);
            GenerateService(selector, ifExsitedCovered);
            GenerateController(selector, ifExsitedCovered);
            GenerateApiController(selector, ifExsitedCovered);

            return this;
        }

        public IDbFirst GenerateIRepository(Func<DbTable, bool> selector, bool ifExsitedCovered = false)
        {
            var tables = AllTables.Where(selector).ToList();
            foreach (var table in tables)
            {
                if (table.Columns.Any(c => c.IsPrimaryKey))
                {
                    var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                    Instance.GenerateIRepository(table.TableName.ToPascalCase(), pkTypeName, ifExsitedCovered);
                }
            }

            return this;
        }

        public IDbFirst GenerateRepository(Func<DbTable, bool> selector, bool ifExsitedCovered = false)
        {
            var tables = AllTables.Where(selector).ToList();
            foreach (var table in tables)
            {
                if (table.Columns.Any(c => c.IsPrimaryKey))
                {
                    var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                    Instance.GenerateRepository(table.TableName.ToPascalCase(), pkTypeName, ifExsitedCovered);
                }
            }

            return this;
        }

        public IDbFirst GenerateIService(Func<DbTable, bool> selector, bool ifExsitedCovered = false)
        {
            var tables = AllTables.Where(selector).ToList();
            foreach (var table in tables)
            {
                if (table.Columns.Any(c => c.IsPrimaryKey))
                {
                    var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                    Instance.GenerateIService(table.TableName.ToPascalCase(), pkTypeName, ifExsitedCovered);
                }
            }

            return this;
        }

        public IDbFirst GenerateService(Func<DbTable, bool> selector, bool ifExsitedCovered = false)
        {
            var tables = AllTables.Where(selector).ToList();
            foreach (var table in tables)
            {
                if (table.Columns.Any(c => c.IsPrimaryKey))
                {
                    var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                    Instance.GenerateService(table.TableName.ToPascalCase(), pkTypeName, ifExsitedCovered);
                }
            }

            return this;
        }

        public IDbFirst GenerateController(Func<DbTable, bool> selector, bool ifExsitedCovered = false)
        {
            var tables = AllTables.Where(selector).ToList();
            foreach (var table in tables)
            {
                if (table.Columns.Any(c => c.IsPrimaryKey))
                {
                    var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                    Instance.GenerateController(table.TableName.ToPascalCase(), pkTypeName, ifExsitedCovered);
                }
            }

            return this;
        }

        public IDbFirst GenerateApiController(Func<DbTable, bool> selector, bool ifExsitedCovered = false)
        {
            var tables = AllTables.Where(selector).ToList();
            foreach (var table in tables)
            {
                if (table.Columns.Any(c => c.IsPrimaryKey))
                {
                    var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                    Instance.GenerateApiController(table.TableName.ToPascalCase(), pkTypeName, ifExsitedCovered);
                }
            }

            return this;
        }

        public IDbFirst GenerateViewModel(string viewName, bool ifExsitedCovered = false)
        {
            var sql = $"select top 1 * from {viewName}";
            var dt = DbContext.GetDataTable(sql);
            GenerateViewModel(dt, viewName, ifExsitedCovered);
            return this;
        }

        public IDbFirst GenerateViewModel(DataTable dt, string className, bool ifExsitedCovered = false)
        {
            Instance.GenerateViewModel(dt, className, ifExsitedCovered);
            return this;
        }

        public IDbFirst GenerateViewModel(DataSet ds, bool ifExsitedCovered = false)
        {
            Instance.GenerateViewModel(ds, ifExsitedCovered);
            return this;
        }
    }
}

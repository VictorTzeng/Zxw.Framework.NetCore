using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public class DbContextFactory
    {
        public static DbContextFactory Instance => new DbContextFactory();

        private Dictionary<string, Dictionary<string, object>> dicDbContexts { get; set; } =
            new Dictionary<string, Dictionary<string, object>>();

        public void AddDbContext<TContext>(string tagName, TContext context) where TContext : DbContext, IDbContextCore
        {
            if(context == null)return;
            var typeName = context.GetType().Name;
            if (dicDbContexts.ContainsKey(typeName))
            {
                var contexts = dicDbContexts[typeName];
                if (contexts.ContainsKey(tagName))
                {
                    contexts[tagName] = context;
                }
                else
                {
                    contexts.Add(tagName, context);
                }
            }
            else
            {
                dicDbContexts.Add(typeName, new Dictionary<string, object>()
                {
                    {tagName, context}
                });
            }
        }

        public void AddDbContext<TContext>(string tagName, DbContextOption option)
            where TContext : BaseDbContext, IDbContextCore
        {
            TContext context = (TContext) Activator.CreateInstance(typeof(TContext), option);
            AddDbContext(tagName, context);
        }

        public IDbContextCore GetDbContext(string tagName)
        {
            foreach (var dic in dicDbContexts)
            {
                foreach (var x in dic.Value)
                {
                    if (x.Key == tagName)
                        return (IDbContextCore) x.Value;
                }
            }

            return null;
        }

        public IDbContextCore GetDbContext<TContext>(string tagName) where TContext : DbContext, IDbContextCore
        {
            var typeName = typeof(TContext).Name;
            if (dicDbContexts.ContainsKey(typeName))
            {
                return (IDbContextCore) (dicDbContexts[typeName].ContainsKey(tagName)
                    ? dicDbContexts[typeName][tagName]
                    : null);
            }

            return (IDbContextCore) null;
        }
    }
}

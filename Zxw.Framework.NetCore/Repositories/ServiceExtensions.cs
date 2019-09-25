using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Repositories
{
    public static class ServiceExtensions
    {
        public static IServiceCollection RegisterDefaultRepositories<T>(this IServiceCollection services) where T:DbContext,new()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var list = assembly.GetTypes().Where(t => t.GetCustomAttributes<DbContextAttribute>()
                                                          .Any(a => a.ContextType == typeof(T))
                                                      && !t.GetCustomAttributes<MigrationAttribute>().Any() &&
                                                      !t.FullName.Contains("Migrations")).ToList();
            if (list.Any())
            {
                foreach (var type in list)
                {
                    var pkType = GetPrimaryKeyType(type);
                    var implType = GetRepositoryType(type, pkType);
                    if (pkType != null)
                    {
                        services.TryAddScoped(typeof(IRepository<,>).MakeGenericType(type, pkType), implType);
                    }
                }
            }
            return services;
        }

        private static Type GetRepositoryType(Type entityType, Type primaryKeyType)
        {
            return typeof(BaseRepository<,>).MakeGenericType(entityType, primaryKeyType);
        }

        private static Type GetPrimaryKeyType(Type entityType)
        {
            foreach (var interfaceType in entityType.GetTypeInfo().GetInterfaces())
            {
                if (interfaceType.GetTypeInfo().IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(BaseModel<>))
                {
                    return interfaceType.GenericTypeArguments[0];
                }
            }

            return null;
        }

    }
}

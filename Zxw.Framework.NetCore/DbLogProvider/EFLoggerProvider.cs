using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Zxw.Framework.NetCore.DbLogProvider
{
    public class EntityFrameworkCommandLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new EntityFrameworkCommandLogger(categoryName); //创建EFLogger类的实例
        }

        public void Dispose()
        {

        }
    }

    public class EntityFrameworkCommandLogger : ILogger
    {
        private readonly string categoryName;

        public EntityFrameworkCommandLogger(string categoryName) => this.categoryName = categoryName;

        public virtual bool IsEnabled(LogLevel logLevel) => true;

        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            //ef core执行数据库查询时的categoryName为Microsoft.EntityFrameworkCore.Database.Command,日志级别为Information
            if (categoryName == DbLoggerCategory.Database.Command.Name
                && logLevel == LogLevel.Information)
            {
                var logContent = formatter(state, exception);
                //TODO: 拿到日志内容想怎么玩就怎么玩吧
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(logContent);
                Console.ResetColor();

                this.Log(LogLevel.Trace, logContent);
            }
        }

        public virtual IDisposable BeginScope<TState>(TState state) => null;
    }
}

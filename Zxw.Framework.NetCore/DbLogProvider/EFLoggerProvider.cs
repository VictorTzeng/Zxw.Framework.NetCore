using System;
using Microsoft.Extensions.Logging;
using Zxw.Framework.NetCore.Helpers;

namespace Zxw.Framework.NetCore.DbLogProvider
{
public class EFLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new EFLogger(categoryName);//创建EFLogger类的实例
        }

        public void Dispose()
        {

        }
    }

    public class EFLogger : ILogger
    {
        private readonly string categoryName;

        public EFLogger(string categoryName) => this.categoryName = categoryName;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            //ef core执行数据库查询时的categoryName为Microsoft.EntityFrameworkCore.Database.Command,日志级别为Information
            if (categoryName == "Microsoft.EntityFrameworkCore.Database.Command"
                && logLevel == LogLevel.Information) {
                var logContent = formatter(state, exception);
                //TODO: 拿到日志内容想怎么玩就怎么玩吧
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(logContent);
                Console.ResetColor();

                Log4NetHelper.WriteInfo(GetType(), logContent);
            }
        }

        public IDisposable BeginScope<TState>(TState state) => null;
    }
}

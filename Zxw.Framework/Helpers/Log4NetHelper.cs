using System;
using System.Diagnostics;
using System.IO;
using log4net;
using log4net.Config;

namespace Zxw.Framework.Helpers
{
    /// <summary>
    /// log4net封装类
    /// *********************************使用说明**********************************
    /// 1.首先将配置文件(log4net.config或App.config)放置在程序运行目录
    /// 2.调用SetConfig方法，并传入配置文件的全路径
    /// 3.调用WriteError、WriteInfo、WriteFatal、WriteDebug等方法
    /// </summary>
    public class Log4NetHelper
    {
        /// <summary>
        /// 读取配置文件，并使其生效。如果未找到配置文件，则抛出异常
        /// </summary>
        /// <param name="configFilePath">配置文件全路径</param>
        public static void SetConfig(string configFilePath)
        {
            var fileInfo = new FileInfo(configFilePath);
            if (!fileInfo.Exists)
            {
                throw new Exception("未找到配置文件" + configFilePath);
            }
            XmlConfigurator.ConfigureAndWatch(fileInfo);
        }

        #region static void WriteError(Type t, Exception ex)

        /// <summary>
        /// 输出错误日志到Log4Net
        /// </summary>
        /// <param name="t"></param>
        /// <param name="ex"></param>
        public static void WriteError(Type t, Exception ex)
        {
            var log = LogManager.GetLogger(t);
            log.Error("Error", ex);
        }

        #endregion static void WriteError(Type t, Exception ex)

        #region static void WriteError(Type t, string msg)

        /// <summary>
        /// 输出错误日志到Log4Net
        /// </summary>
        /// <param name="t"></param>
        /// <param name="msg"></param>
        public static void WriteError(Type t, string msg)
        {
            var log = LogManager.GetLogger(t);
            var stackTrace = new StackTrace();
            var stackFrame = stackTrace.GetFrame(1);
            var methodBase = stackFrame.GetMethod();
            var message = "方法名称：" + methodBase.Name + "\r\n日志内容：" + msg;
            log.Error(message);
        }

        #endregion static void WriteError(Type t, string msg)

        /// <summary>
        /// 记录消息日志
        /// </summary>
        /// <param name="t"></param>
        /// <param name="msg"></param>
        public static void WriteInfo(Type t, string msg)
        {
            var log = LogManager.GetLogger(t);
            var stackTrace = new StackTrace();
            var stackFrame = stackTrace.GetFrame(1);
            var methodBase = stackFrame.GetMethod();
            var message = "方法名称：" + methodBase.Name + "\r\n日志内容：" + msg;
            log.Info(message);
        }

        /// <summary>
        /// 记录消息日志
        /// </summary>
        /// <param name="t"></param>
        /// <param name="exception"></param>
        public static void WriteInfo(Type t, Exception exception)
        {
            var log = LogManager.GetLogger(t);
            log.Info("系统消息", exception);
        }

        /// <summary>
        /// 记录致命错误日志
        /// </summary>
        /// <param name="t"></param>
        /// <param name="msg"></param>
        public static void WriteFatal(Type t, string msg)
        {
            var log = LogManager.GetLogger(t);
            var stackTrace = new StackTrace();
            var stackFrame = stackTrace.GetFrame(1);
            var methodBase = stackFrame.GetMethod();
            var message = "方法名称：" + methodBase.Name + "\r\n日志内容：" + msg;
            log.Fatal(message);
        }

        /// <summary>
        /// 记录致命错误日志
        /// </summary>
        /// <param name="t"></param>
        /// <param name="exception"></param>
        public static void WriteFatal(Type t, Exception exception)
        {
            var log = LogManager.GetLogger(t);
            log.Fatal("系统致命错误", exception);
        }

        /// <summary>
        /// 记录Debug日志
        /// </summary>
        /// <param name="t"></param>
        /// <param name="msg"></param>
        public static void WriteDebug(Type t, string msg)
        {
            var log = LogManager.GetLogger(t);
            var stackTrace = new StackTrace();
            var stackFrame = stackTrace.GetFrame(1);
            var methodBase = stackFrame.GetMethod();
            var message = "方法名称：" + methodBase.Name + "\r\n日志内容：" + msg;
            log.Debug(message);
        }

        /// <summary>
        /// 记录Debug日志
        /// </summary>
        /// <param name="t"></param>
        /// <param name="exception"></param>
        public static void WriteDebug(Type t, Exception exception)
        {
            var log = LogManager.GetLogger(t);
            log.Debug("系统调试信息", exception);
        }

        /// <summary>
        /// 记录警告信息
        /// </summary>
        /// <param name="t"></param>
        /// <param name="msg"></param>
        public static void WriteWarn(Type t, string msg)
        {
            var log = LogManager.GetLogger(t);
            log.Warn(msg);
        }

        /// <summary>
        /// 记录警告信息
        /// </summary>
        /// <param name="t"></param>
        /// <param name="exception"></param>
        public static void WriteWarn(Type t, Exception exception)
        {
            var log = LogManager.GetLogger(t);
            log.Warn("系统警告信息", exception);
        }
    }
}
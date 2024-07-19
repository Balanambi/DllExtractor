using log4net;
using log4net.Config;
using System;
using System.Reflection;

public static class Logger
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    static Logger()
    {
        XmlConfigurator.Configure();
    }

    public static void Info(string message)
    {
        log.Info(message);
    }

    public static void Error(string message, Exception ex = null)
    {
        log.Error(message, ex);
    }
}

using System;
using System.Data;
using log4net;
using log4net.Appender;
using Stavadlo22.Data;
using Stavadlo22.Infrastructure.Enums;


namespace Stavadlo22.Infrastructure
{
    /// <summary>
    /// Trieda definujuca rozsirujuce metody pre log for net.
    /// </summary>
    public static class Log4NetExtensions
    {
        static StavadloGlobalData GlobalData;
        static Log4NetExtensions()
        {
            GlobalData = StavadloGlobalData.Instance;
        }
        /// <summary>
        /// Metoda pre informativny vypis, okrem standardu, doplni nazov klienta na zaciatku spravy, ktory nasledne pri logovani do db sa zapise do stlpca client_name, aby bolo mozne rozlisit od koho dany zaznam je.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="message"></param>
        public static void CustomInfo(this ILog log, string message)
        {

            log.Info(message);
            // USER_ROLE currentRole = App.currentUserRole.GetValueOrDefault();
            USER_ROLE currentRole = StavadloGlobalData.Instance.CurrentUser.Role;
            if (GlobalData.WebServiceLogger != null && (currentRole == USER_ROLE.DISPECER || currentRole == USER_ROLE.UDRZBA || currentRole==USER_ROLE.ADMIN) && GlobalData.ApplicationMode == APPLICATION_MODE.MASTER)
            {
                GlobalData.WebServiceLogger.EnqueLogData(message + "#INFO#" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff"));
            }
        }

        public static void CustomError(this ILog log, string message)
        {
            //App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            //    {
            //log4net.LogicalThreadContext.Properties[":client_name"] = App.clientName;
            //log4net.LogicalThreadContext.Properties[":message2"] = DBNull.Value;
            //log.Info(message.Substring(0, 4000));
            log.Error(message);
            //USER_ROLE currentRole = App.currentUserRole.GetValueOrDefault();
            USER_ROLE currentRole = StavadloGlobalData.Instance.CurrentUser.Role;
            if (GlobalData.WebServiceLogger != null && (currentRole == Enums.USER_ROLE.DISPECER || currentRole == USER_ROLE.UDRZBA) && GlobalData.ApplicationMode == APPLICATION_MODE.MASTER)
            {
                GlobalData.WebServiceLogger.EnqueLogData(message + "#ERROR#" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff"));
            }
            //    }));
        }

        public static void CustomDebug(this ILog log, string message)
        {
            //App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            //    {
            //log4net.LogicalThreadContext.Properties[":client_name"] = GlobalData.HMIClientName;
            //log4net.LogicalThreadContext.Properties[":message2"] = DBNull.Value;
            //log.Info(message.Substring(0, 4000));
            log.Debug(message);
            //USER_ROLE currentRole = App.currentUserRole.GetValueOrDefault();
            USER_ROLE currentRole = StavadloGlobalData.Instance.CurrentUser.Role;
            if (GlobalData.WebServiceLogger != null && (currentRole == USER_ROLE.DISPECER || currentRole == USER_ROLE.UDRZBA) && GlobalData.ApplicationMode == APPLICATION_MODE.MASTER)
            {
                GlobalData.WebServiceLogger.EnqueLogData(message + "#DEBUG#" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff"));
            }
            //    }));
        }
    }

    public class ClientNameAppenderParameter : AdoNetAppenderParameter
    {
        public override void FormatValue(System.Data.IDbCommand command, log4net.Core.LoggingEvent loggingEvent)
        {
            string[] data = loggingEvent.RenderedMessage.Split('~');
            string clientName = string.Empty;

            if (data != null && data.Length > 1)
                clientName = data[0];

            IDbDataParameter param = (IDbDataParameter)command.Parameters[ParameterName];

            object formattedValue = clientName;

            if (formattedValue == null)
            {
                formattedValue = DBNull.Value;
            }

            param.Value = formattedValue;

            // Try to get property value
            //object propertyValue = null;
            //var propertyName = ParameterName.Replace("@", "");

            //var messageObject = loggingEvent.MessageObject;
            //if (messageObject != null)
            //{
            //    var property = messageObject.GetType().GetProperty(propertyName);
            //    if (property != null)
            //    {
            //        propertyValue = property.GetValue(messageObject, null);
            //    }
            //}

            //// Insert property value (or db null) into parameter
            //var dataParameter = (IDbDataParameter)command.Parameters[ParameterName];
            //dataParameter.Value = propertyValue ?? DBNull.Value;

        }
    }
}

using System.IO;
using System;
using System.Text;
using System.Xml;
using System.Web;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

namespace PullFinanceData.Util
{
    /// <summary>
    ///     ExceptionUtil is the central location for log/handle exceptions.
    ///     Currently it use the Microsoft Enterprise Library. The configuration files is
    ///     exceptionHandlingConfiguration.config, loggingConfiguration.config and
    ///     loggingDistributorConfiguration.config.
    /// </summary>
    public class ExceptionUtil
    {
        public static void DefaultHandleException(Exception ex, AWDEnvironment env)
        {
            ex.HelpLink += Environment.NewLine + "***RequestId: " + env?.RequestId;
            DefaultHandleException(ex);
            if (env != null)
            {
                env.WriteMessage(ex.ToString());
                if (env.TrackingLog != null)
                    env.TrackingLog.AddLog("Exception", null, 0, ex.Message + ex.StackTrace);
            }
        }

        public static void DefaultHandleException(Exception ex)
        {
            HandleException(ex, "Log Only Policy");
        }

        public static void DefaultHandleWarning(Exception ex)
        {
            HandleException(ex, "Log Warning Policy");
        }

        public static void AppendException(Exception ex, string message)
        {
            if (message.Length > 20000)
                message = message.Substring(0, 20000);
            ex.HelpLink = message;
            DefaultHandleException(ex);
        }

        public static void DefaultHandleException(Exception ex, AWDEnvironment env, string message)
        {
            ex.HelpLink += Environment.NewLine + "***RequestId: " + env?.RequestId;
            DefaultHandleException(ex);
            if (env != null)
            {
                env.WriteMessage(ex.ToString());
                if (env.TrackingLog != null)
                    env.TrackingLog.AddLog("Exception", null, 0, ex.Message + ex.StackTrace);
            }
        }

        internal static void RethrowException(Exception ex, AWDEnvironment env, string message)
        {
            message = ex.Message + Environment.NewLine + message;
            if (env != null)
            {
                env.WriteMessage(ex.ToString());
                if (env.TrackingLog != null)
                    env.TrackingLog.AddLog("Exception", null, 0, ex.Message + ex.StackTrace);
            }

            var newException = new Exception(message, ex);
            throw newException;
        }

        private static string GetRequestInfo()
        {
            var requestInfo = "URL:{0}" + Environment.NewLine
                                        + "Post Data:" + Environment.NewLine + "{1}";
            var httpContext = HttpContext.Current;
            //var httpContext = HttpContext.Current;
            //if (httpContext == null)
            //    return string.Empty;
            var request = httpContext.Request;
            var url = request.Url.ToString();
            var sb = new StringBuilder(1024);
            foreach (var key in request.Form.AllKeys)
            {
                sb.Append(key + ":");
                var value = request.Form[key];
                if (value.Length > 1000)
                    value = value.Substring(0, 1000) + "...";
                sb.Append(value);
                sb.Append(Environment.NewLine);
            }

            var inputXml = httpContext.Items["InputXml"] as XmlDocument;
            if (inputXml != null)
            {
                var outerXml = inputXml.OuterXml;
                if (outerXml.Length > 10000)
                    outerXml = outerXml.Substring(0, 10000) + "---" + outerXml.Length;
                sb.Append(outerXml);
            }

            requestInfo = string.Format(requestInfo, url, sb);
            return requestInfo;
        }


        private static void HandleException(Exception ex, string policyName)
        {
            var rethrow = false;
            try
            {
                if (ex.HelpLink == null || !ex.HelpLink.StartsWith("\r\n***"))
                {
                    var requestInfo = GetRequestInfo();
                    if (!string.IsNullOrEmpty(requestInfo))
                    {
                        if (!string.IsNullOrEmpty(ex.HelpLink))
                            ex.HelpLink += Environment.NewLine;
                        ex.HelpLink += requestInfo;
                    }
                }

                rethrow = ExceptionPolicy.HandleException(ex, policyName);
            }
            catch (Exception e)
            {
                try
                {
                    // NOTE(byuan): no permission to write C:
                    var path = Path.Combine(Environment.GetEnvironmentVariable("TEMP") ?? "c:\\temp", "exception.txt");
                    File.AppendAllText(path, DateTime.Now + ": " + ex + Environment.NewLine + Environment.NewLine
                                             + e + Environment.NewLine +
                                             "-----------------------------------------------------------------------------------------------" +
                                             Environment.NewLine + Environment.NewLine);
                }
                catch
                {
                }
            }

            if (rethrow) throw ex;
        }
    }
}


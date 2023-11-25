using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PullFinanceData.DataObj;


namespace PullFinanceData.Log4net
{
    public class BaseLogger
    {
        private const string logFormat =
            "{0:yyyy-MM-ddTHH:mm:ss.fff} client_ip=\"{1}\" event_description=\"{2}\" event_severity=\"{3}\" event_status=\"{4}\" event_type=\"{5}\" service_name=\"{6}\" user_name=\"{7}\" real_user_name=\"{8}\" hostname=\"{9}\" inst_id=\"{10}\" request_id=\"{11}\" service_version=\"{12}\" session_id=\"{13}\" user_id=\"{14}\" real_user_id=\"{15}\" env=\"{16}\" technical_service=\"{17}\" product_context=\"{18}\"";

        private readonly ILog log;
        private DateTime _timestamp;

        private BaseLogger(string loggerName)
            : this(loggerName, null)
        {
        }

        protected BaseLogger(string loggerName, string serviceName)
        {
            service_name = serviceName;
            log = LogManager.GetLogger(loggerName);
        }

        /// <summary>
        ///     requied
        /// </summary>
        public DateTime timestamp
        {
            get
            {
                if (_timestamp > DateTime.MinValue) return _timestamp;
                return DateTime.Now;
            }
            set => _timestamp = value;
        }

        /// <summary>
        ///     requied
        /// </summary>
        public string client_ip { get; set; }

        /// <summary>
        ///     requied
        /// </summary>
        public string service_name { get; set; }

        /// <summary>
        ///     requied
        /// </summary>
        public string user_name { get; set; }

        /// <summary>
        ///     requied
        /// </summary>
        public string real_user_name { get; set; }

        //optional
        public string hostname { get; set; }
        public string inst_id { get; set; }
        public string request_id { get; set; }
        public string service_version { get; set; }
        public string session_id { get; set; }
        public string user_id { get; set; }
        public string real_user_id { get; set; }

        public string technical_service { get; set; }
        public string product_context { get; set; }

        /// <summary>
        ///     since jobName is a user input and will be used as a log file name, sanitize before use.
        ///     TBD: BaseLogger should require caller type instead of loggerName, so this method can be hidden, because loggerName
        ///     still needs to be sanitized
        ///     TBD: BaseLogger is not necessary as log4net recommends using type hierarchy to configure loggers
        /// </summary>
        /// <typeparam name="T">in case jobName is invalid, use a type name instead</typeparam>
        /// <param name="jobName">user input that will be used as a file name</param>
        /// <returns>not null, use jobName if it is a valid file name, otherwise use type name</returns>
        public static string GetFileName<T>(string jobName)
        {
            // remove all characters unwanted http://stackoverflow.com/questions/146134/how-to-remove-illegal-characters-from-path-and-filenames
            var hs = new HashSet<char>(Path.GetInvalidFileNameChars()).Concat(Path.GetInvalidPathChars());
            var fileName = jobName;
            if (fileName != null) fileName = new string(fileName.Where(c => !hs.Contains(c)).ToArray());
            //if (string.IsNullOrWhiteSpace(jobName))
            //{
            //    jobName = new string(loggerName.Where(c => !hs.Contains(c)).ToArray());
            //}
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = new string(typeof(T).Name.Where(c => !hs.Contains(c)).ToArray());
            fileName = !string.IsNullOrWhiteSpace(fileName)
                ? fileName
                : typeof(BaseLogger).Name; // this.GetType() if this method could be hidden
                                           // LogicalThreadContext.Properties["FileName"] = fileName; // if this method could be hidden
            return fileName;
        }

        protected virtual string ToLogString(string eventDescription, EventSeverity eventSeverity, EventStatus eventStatus,
            EventType eventType)
        {
            return string.Format(logFormat,
                timestamp, Format(client_ip), Format(eventDescription), eventSeverity, eventStatus, eventType,
                Format(service_name), Format(user_name), Format(real_user_name),
                Format(hostname), Format(inst_id), Format(request_id),
                Format(service_version), Format(session_id), Format(user_id), Format(real_user_id),
                Format(ServerConfiguration.ProductMode.ToString()),
                Format(technical_service), Format(product_context));
        }

        public virtual void Log(string eventDescription, EventSeverity eventSeverity, EventStatus eventStatus,
            EventType eventType)
        {
            if (log == null)
                return;

            var msg = ToLogString(eventDescription, eventSeverity, eventStatus, eventType);
            switch (eventSeverity)
            {
                case EventSeverity.critical:
                    log.Error(msg);
                    break;
                case EventSeverity.warn:
                case EventSeverity.high:
                    log.Warn(msg);
                    break;
                case EventSeverity.medium:
                    log.Info(msg);
                    break;
                default:
                    log.Debug(msg);
                    break;
            }
        }

        internal string Format(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return string.Empty;
            str = str.Replace('"', '\'');
            return str;
        }

        public void SetTestUserName()
        {
            user_name = "test_user";
        }
    }

    public enum EventSeverity
    {
        none,
        critical,
        warn,
        high,
        medium,
        low,
        debug
    }

    public enum EventStatus
    {
        none,
        success,
        failure,
        info
    }

    //please add other types if necessary
    public enum EventType
    {
        none,
        authentication,
        read,
        update,
        delete,
        insert,
        administrative,
        info,
        other
    }
}
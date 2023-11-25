using System;
using System.Web;
using System.Xml;
using DataPuller.Util;

namespace DataPuller.Log4net;

public class EventLogger : BaseLogger
{
    private readonly AWDEnvironment m_environment;
    private HttpRequest m_req = null;

    public EventLogger()
        : base("EventLogger", "EventLogger")
    {
    }

    public EventLogger(AWDEnvironment env, HttpRequest req)
        : this()
    {
        m_environment = env;
        m_req = req;
        SetBaseLogInfo();
    }

    private string ETag { get; set; }
    private string ProductCode { get; set; }
    private Guid ProductID { get; set; }
    private string URL { get; set; }

    public void LogAuthentication(string eventDescription, EventStatus eventStatus)
    {
        Log(eventDescription, EventSeverity.high, eventStatus, EventType.authentication);
    }

    public void LogNormal(string eventDescription, EventStatus eventStatus)
    {
        Log(eventDescription, EventSeverity.medium, eventStatus, EventType.other);
    }


    private void SetBaseLogInfo()
    {
        if (m_req == null || m_environment == null) return;

        ETag = m_req.Headers["ETag"];
        ProductCode = m_environment.ProductCode;
        ProductID = m_environment.ProductId;
        URL = m_req.RawUrl;


        var clientIP = GetClientIP();

        string username = m_req.Headers["email"]; //m_environment.UserLoginEmail;
        var useid = m_environment.UserId.ToString();
        var realUserName = "";
        var realUserId = "";

        Guid impersonateId = m_req.QueryString["IMPID"].ToGuid();
        if (!DataTypeUtil.IsNull(impersonateId)) // is impersonare
        {
            realUserId = m_environment.CreatorId.ToString();
            realUserName = m_environment.UserLoginEmail;

            var profile = new UserProfile(m_environment);
            var xml = "";
            if (profile.GetUserProfile(m_environment.UserId, out xml)) //get impersonate user profile
            {
                var SelNode = xml.ToXml().SelectSingleNode(@"/NewDataSet/Table/Email");
                if (SelNode != null && SelNode.InnerText.Length > 0) username = SelNode.InnerText;
            }
        }

        string hostName = m_req.Url.Host;

        this.client_ip = clientIP;
        this.user_name = username;
        this.real_user_name = realUserName;
        this.request_id = m_environment.RequestId;
        this.user_id = useid;
        this.real_user_id = realUserId;

        this.hostname = hostName;
        this.session_id = m_environment.SessionId.ToString();
        this.inst_id = m_environment.InstitutionId;
    }

    protected string GetClientIP()
    {
        if (m_req == null) return "";

        // Look for a proxy address first
        string ip = m_req.ServerVariables["HTTP_X_FORWARDED_FOR"];
        // If there is no proxy, get the standard remote address
        if (string.IsNullOrEmpty(ip) || ip.ToLower() == "unknown")
            ip = m_req.ServerVariables["REMOTE_ADDR"];
        ip = StringUtil.CheckScriptInjection(ip);
        return ip;
    }
}
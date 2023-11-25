using System.Collections.Generic;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PullFinanceData.Util;

namespace PullFinanceData.DataObj
{
    public class ServerConfiguration
    {
        //private static readonly Dictionary<string, string> Settings;

        //public static readonly string RHSAccount;
        //public static readonly string RHSUser;
        //public static readonly string RHSPSW;

        //public static readonly string BlazFTPUserName;
        //public static readonly string BlazFTPUserPassword;
        //public static readonly bool BlazFTPEncryptFile;

        //public static readonly int UnPostTimeoutSeconds;

        //public static readonly string ServerName = Environment.MachineName;
        //public static readonly int ThreadNumber = 4;
        //public static readonly int RSReportThreadNumber = 15;
        //public static readonly string WebServer = @"http://localhost";
        //public static readonly string AccountRequestXmlFolder;
        public static readonly ProductMode ProductMode = ProductMode.LIVE; //default as LIVE Web Server
        //public static readonly string PEReportWebLink;
        //public static readonly string ReportFrameworkServer;
        //public static readonly bool IsReportAuditEnabled;
        //public static readonly bool IsCloudMVBatchEnabled;
        //public static readonly bool IsUKEnvironment;
        //public static readonly bool IsBusinessEntityVersionEnabled;
        //public static readonly bool IsSchwabEnabled;

        //private static readonly Func<XElement, string, string> AttrValue = (xEle, name) =>
        //{
        //    if (xEle != null && xEle.Attribute(name) != null)
        //        return xEle.Attribute(name).Value;
        //    return null;
        //};

        //public readonly string InstanceId = string.Empty;
        //public readonly string ServerInstance;
        public readonly string ServiceName;

        //public readonly string TempFileFolder;
        //public readonly bool UseNewInterface;
        //public readonly bool UseNewIrebalInterface;

        static ServerConfiguration()
        {
            //try
            //{
            //    var root = XElement.Load(ConfigFileLocation);
            //    var serverSetting = root.Descendants("Server").FirstOrDefault(x => AttrValue(x, "Name") == ServerName);
            //    if (serverSetting != null)
            //    {
            //        ProductMode = (ProductMode)Enum.Parse(typeof(ProductMode), AttrValue(serverSetting, "ProductMode"));
            //    }
            //    else
            //    {
            //        var productMode = ConfigurationManager.AppSettings["ProductMode"];
            //        if (!string.IsNullOrEmpty(productMode))
            //            ProductMode = (ProductMode)Enum.Parse(typeof(ProductMode), productMode);
            //    }

            //    var batchServerItem = root.Descendants("BatchServers").FirstOrDefault();
            //    if (batchServerItem != null)
            //    {
            //        IsBatch =
            //            batchServerItem.Descendants("Server").FirstOrDefault(x => AttrValue(x, "Name") == ServerName) !=
            //            null;
            //        if (IsBatch) AWDEnvironment.IsTestPool = IsTP;
            //    }

            //    var serviceSetting = root.Descendants("BatchServerSetting")
            //        .FirstOrDefault(x => AttrValue(x, "ProductMode") == ProductMode.ToString());
            //    Settings = serviceSetting.Descendants("Setting")
            //        .ToDictionary(x => AttrValue(x, "Name"), x => AttrValue(x, "Value"));


            //    //DOPA-3781:The AWDEnvironment initialize calls AWDServerConfiguartion, and AWDServerConfiguration calls AWDEnvironment in CryptographyHelper.DESDecrypt, it's dangerous cross reference in static and initialize.
            //    //The DESDecrypt will crash here in the above scenario. Use ETF-8 instead to avoid cross-calling.
            //    //RHS Settings For RSReport
            //    RHSAccount = CryptographyHelper.DESDecrypt(GetSetting("RHSAccount"), Encoding.UTF8);
            //    RHSUser = CryptographyHelper.DESDecrypt(GetSetting("RHSUser"), Encoding.UTF8);
            //    RHSPSW = CryptographyHelper.DESDecrypt(GetSetting("RHSPSW"), Encoding.UTF8);

            //    //IRebal Blaz FTP user name / pwd
            //    BlazFTPUserName = CryptographyHelper.DESDecrypt(GetSetting("BlazFTPUserName"), Encoding.UTF8);
            //    BlazFTPUserPassword = CryptographyHelper.DESDecrypt(GetSetting("BlazFTPUserPwd"), Encoding.UTF8);
            //    BlazFTPEncryptFile = GetSetting("EncryptBlazFTPFile").ToBool();

            //    UnPostTimeoutSeconds = GetSetting("UnPostTimeoutSeconds").ToInt(1800);

            //    ThreadNumber = GetSetting("ThreadNumber").ToInt(ThreadNumber);
            //    RSReportThreadNumber = GetSetting("RSReportThreadNumber").ToInt(RSReportThreadNumber);
            //    PEReportWebLink = GetSetting("PEReportWebLink");
            //    AccountRequestXmlFolder = GetSetting("AccountRequestXml");
            //    ReportFrameworkServer = GetSetting("ReportFrameworkServer") ?? @"http://localhost:3310";
            //    IsReportAuditEnabled = GetSetting("IsReportAuditEnabled").ToBool();
            //    IsCloudMVBatchEnabled = GetSetting("IsCloudMVBatchEnabled").ToBool();
            //    IsUKEnvironment = GetSetting("IsUKEnvironment").ToBool();
            //    IsBusinessEntityVersionEnabled = GetSetting("IsBusinessEntityVersionEnabled").ToBool();
            //    IsSchwabEnabled = GetSetting("IsSchwabEnabled").ToBool();

            //    var batchAdminSetting = root.Descendants("BatchAdminSetting")
            //        .Descendants("Setting").FirstOrDefault(x => AttrValue(x, "ProductMode") == ProductMode.ToString());
            //    BatchAdminServicePort = AttrValue(batchAdminSetting, "Port").ToInt(8999);
            //    BatchAdminServiceUrl = AttrValue(batchAdminSetting, "Url");

            //    var reportMonitorSetting = root.Descendants("ReportMonitorSetting").FirstOrDefault();
            //    ReportMonitorTimeOut = AttrValue(reportMonitorSetting, "TimeOut").ToInt(240);
            //    ReportMonitorSleep = AttrValue(reportMonitorSetting, "Sleep").ToInt(10);
            //    MonitorProcess = AttrValue(reportMonitorSetting, "ProcessName");
            //    EmailsTo = AttrValue(reportMonitorSetting, "EmailsTo");
            //    ReportAlertEmailsTo = AttrValue(reportMonitorSetting, "ReportAlertEmailsTo");
            //    EmailFrom = AttrValue(reportMonitorSetting, "EmailFrom");
            //    Kill = AttrValue(reportMonitorSetting, "Kill").ToBool(false);

            //    var B13FTaskSetting = root.Descendants("B13FSecuritySetting").FirstOrDefault();
            //    B13FParserJSPath = AttrValue(B13FTaskSetting, "B13FParserJSPath");
            //    B13FExchanges = AttrValue(B13FTaskSetting, "Exchanges");
            //    if (B13FTaskSetting != null)
            //        B13FPDFAddr = B13FTaskSetting.Descendants("Last13FPDFLink")
            //            .ToDictionary(x => AttrValue(x, "i"), x => AttrValue(x, "v"));

            //    var posDBSetting = root.Descendants("PosDBServerSetting").FirstOrDefault();
            //    NewPosDBServerId = AttrValue(posDBSetting, "NewServerId").ToInt(102);

            //    PosDBServerIds = new List<int>();
            //    var posDBServerIds = AttrValue(posDBSetting, "PosServerIds").ToStr();
            //    if (!DataTypeUtil.IsNull(posDBServerIds))
            //        posDBServerIds.Split(',').Where(id => !DataTypeUtil.IsNull(id)).ToList().ForEach(id =>
            //        {
            //            var posDBServerId = int.MinValue;
            //            if (int.TryParse(id, out posDBServerId)
            //                && !PosDBServerIds.Contains(posDBServerId))
            //                PosDBServerIds.Add(posDBServerId);
            //        });

            //    #region BatchTaskProducer service

            //    var batchTaskProducerSetting = root.Descendants("BatchTaskProducerSetting").FirstOrDefault();
            //    BatchTaskProducerFilterByPaidUser = AttrValue(batchTaskProducerSetting, "FilterByPaidUser").ToBool(false);

            //    var startTime = TimeSpan.Zero;
            //    var endTime = TimeSpan.Zero;

            //    BatchTaskStartTime = TimeSpan.TryParse(
            //        AttrValue(batchTaskProducerSetting, "StartTime").ToStr(), out startTime)
            //        ? startTime
            //        : TimeSpan.MinValue;
            //    BatchTaskEndTime = TimeSpan.TryParse(
            //        AttrValue(batchTaskProducerSetting, "EndTime").ToStr(), out endTime)
            //        ? endTime
            //        : TimeSpan.MinValue;

            //    BatchTaskTimeInterval =
            //        TimeSpan.FromMinutes(AttrValue(batchTaskProducerSetting, "TimeInterval").ToInt(30)); // minutes

            //    #endregion

            //    var paConversionSettingNode = root.Descendants("PAConversionSetting").FirstOrDefault();
            //    var mopAccountCacheSetting = root.Descendants("MOPAccountCacheSetting").FirstOrDefault();
            //    var dataProductionSettingNode = root.Descendants("DataProductionSetting").FirstOrDefault();
            //    var wwwrootFolder = GetSetting("BatchReportWWWrootFolder");
            //    if (wwwrootFolder != null)
            //    {
            //        var reportTempFolder = wwwrootFolder.Trim('\\') + "\\Temp";
            //        try
            //        {
            //            if (!Directory.Exists(reportTempFolder))
            //                Directory.CreateDirectory(reportTempFolder);
            //        }
            //        catch
            //        {
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    AWDEnvironment.s_DefaultEnviroment.WriteMessage(ex.ToString());
            //}
        }

        /// <summary>
        ///     Init AWDServerConfiguration
        /// </summary>
        /// <param name="env"></param>
        /// <param name="serviceName"></param>
        /// <param name="hardPostFix">
        ///     The hardPostFix is MUST for the clone services such as BatchReportRunBOX,
        ///     BatchReportMstarRerun. It's to aovid the temp folder conflicts including user logo/footer files and temp sub pdf
        ///     files
        /// </param>
        public ServerConfiguration(AWDEnvironment env, string serviceName, string hardPostFix = null)
        {
            //try
            //{
            //    ServiceName = serviceName;
            //    var root = XElement.Load(ConfigFileLocation);
                //var serverSetting = root.Descendants("Server").FirstOrDefault(x => AttrValue(x, "Name") == ServerName);
                //if (serverSetting == null)
                //    serverSetting = root.Descendants("Server")
                //        .FirstOrDefault(x => AttrValue(x, "ProductMode") == ProductMode.ToString());

                //if (serverSetting != null)
                //{
                //    var serviceList = serverSetting.Descendants("Service").ToList();
                //    var service = serviceList.Where(x => AttrValue(x, "Name") == ServiceName).FirstOrDefault();
                //    UseNewInterface = AttrValue(service, "UseNewInterface").ToBool(UseNewInterface);
                //    UseNewIrebalInterface = AttrValue(service, "UseNewIrebalInterface").ToBool(UseNewIrebalInterface);
                //}

                //ServerInstance = Environment.MachineName;
                //TempFileFolder = GetSetting("BatchReportTempFileFolder");

            //    foreach (var ins in MultipleServiceInstances)
            //        for (var i = 1; i <= ins.Value; i++)
            //            if (serviceName == ins.Key.ToString() + i)
            //            {
            //                InstanceId = i.ToString();
            //                ServerInstance += "_" + InstanceId;
            //            }

            //    if (!string.IsNullOrEmpty(hardPostFix))
            //    {
            //        var index = 100;
            //        if (PostFixInstances.ContainsKey(hardPostFix)) index = PostFixInstances[hardPostFix];
            //        InstanceId = index.ToString();
            //        ServerInstance += "_" + index;
            //    }

            //    if (TempFileFolder != null)
            //        TempFileFolder = TempFileFolder.Trim('\\') + "{0}{1}".FormatTo(InstanceId, "\\");
            //}
            //catch (Exception ex)
            //{
            //    env.WriteMessage(ex.ToString());
            //}
        }

        //static Func<XElement, string, string> AttrValue = (xEle, name) =>
        //{
        //    if (xEle != null && xEle.Attribute(name) != null)
        //        return xEle.Attribute(name).Value;
        //    return null;
        //};

        //public static string CodeVersion => IsTP ? ProductMode.ToString() : null;
        public static ProductMode DBProductMode => IsTP ? ProductMode.LIVE : ProductMode;

        //public static string ConfigFileLocation => ConfigurationManager.AppSettings["ServerConfigFile"] ??
        //                                           @"C:\ServerConfig\ServerConfiguration.Config";

        public static bool IsTP => ProductMode == ProductMode.TP || ProductMode == ProductMode.PatchTP ||
                                   ProductMode == ProductMode.UAT;

        //public static bool IsUAT => ProductMode == ProductMode.UAT;
        //public static bool IsLive => ProductMode == ProductMode.LIVE && !AWDEnvironment.IsTestPool;
        //public static bool IsDBLive => DBProductMode == ProductMode.LIVE;
        //public static bool IsDev => ProductMode == ProductMode.DEV;
        //public static bool IsQA => ProductMode == ProductMode.QA;
        //public static bool IsSTG => ProductMode == ProductMode.STG;
        //public static bool IsSTG2 => ProductMode == ProductMode.STG2;
        //public static bool IsLowEnv => ProductMode == ProductMode.QA || ProductMode == ProductMode.STG;
        //public static bool IsBatch { get; }

        //public static int BatchAdminServicePort { get; private set; }
        //public static string BatchAdminServiceUrl { get; private set; }

        //public static int ReportMonitorTimeOut { get; private set; }
        //public static int ReportMonitorSleep { get; private set; }
        //public static string MonitorProcess { get; private set; }
        //public static string EmailsTo { get; private set; }
        //public static string ReportAlertEmailsTo { get; private set; }
        //public static string EmailFrom { get; private set; }
        //public static bool Kill { get; private set; }
        //public static int NewPosDBServerId { get; set; }

        //public static List<int> PosDBServerIds { get; set; }

        //public static PAConversionSetting PAConversionSetting { get; private set; }
        //public static MOPAccountCacheSetting MOPAccountCacheSetting { get; private set; }
        //public static DataProductionSetting DataProductionSetting { get; private set; }

        //public static bool IsStopped
        //{
        //    get
        //    {
        //        var root = XElement.Load(ConfigFileLocation);
        //        var serverSetting = root.Descendants("BatchServers").FirstOrDefault();
        //        if (serverSetting == null || serverSetting.Attribute("Stop") == null)
        //            return false;
        //        return serverSetting.Attribute("Stop").Value.ToBool();
        //    }
        //}

        //public static bool SetStopped(string serviceName)
        //{
        //    try
        //    {
        //        if (GetStoppedServices().Contains(serviceName)) return true;

        //        var root = XElement.Load(ConfigFileLocation);
        //        var serverSetting = root.Descendants("BatchServers").FirstOrDefault();
        //        var obj = serverSetting.Attribute("StoppedServices");
        //        obj.Value = obj.Value.Trim(',') + "," + serviceName;
        //        root.Save(ConfigFileLocation);
        //    }
        //    catch
        //    {
        //    }

        //    return false;
        //}

        //public static List<string> GetStoppedServices()
        //{
        //    var root = XElement.Load(ConfigFileLocation);
        //    var serverSetting = root.Descendants("BatchServers").FirstOrDefault();
        //    var stoppedServices = serverSetting.Attribute("StoppedServices");
        //    return stoppedServices.Value.Trim(',').Split(',').ToList();
        //}

        //public static string GetSetting(string name)
        //{
        //    if (Settings != null && Settings.ContainsKey(name)) return Settings[name];
        //    return null;
        //}

        //public static List<string> GetServiceListByServer(string serverName)
        //{
        //    var root = XElement.Load(ConfigFileLocation).Element("BatchServers");
        //    var serverSetting = root.Descendants("Server").FirstOrDefault(x => AttrValue(x, "Name") == serverName);
        //    if (serverSetting == null)
        //        serverSetting = root.Descendants("Server")
        //            .FirstOrDefault(x => AttrValue(x, "ProductMode") == ProductMode.ToString());
        //    return serverSetting.Descendants("Service").Select(x => AttrValue(x, "Name"))
        //        .Where(s => !s.StartsWith("ServerAPP")).ToList();
        //}

        //public static Dictionary<string, ReportType> GetAllServices()
        //{
        //    var serviceList = Enum.GetValues(typeof(ReportType)).Cast<ReportType>().ToDictionary(s => s.ToString(), s => s);
        //    foreach (var ins in MultipleServiceInstances)
        //        for (var i = 1; i <= ins.Value; i++)
        //            serviceList.Add(ins.Key.ToString() + i, ins.Key);
        //    return serviceList;
        //}

        //public static DataTable GetBatchServicesDistribution()
        //{
        //    var dt = new DataTable();
        //    try
        //    {
        //        dt.Columns.Add("Server");
        //        dt.Columns.Add("Mode");

        //        var root = XElement.Load(ConfigFileLocation);
        //        var item = root.Descendants("BatchServers").FirstOrDefault();
        //        item.Descendants("Service").Select(d => AttrValue(d, "Name")).Distinct().ToList()
        //            .ForEach(s => dt.Columns.Add(s));
        //        foreach (var server in item.Descendants("Server"))
        //        {
        //            var name = AttrValue(server, "Name") ?? "";
        //            if (name.StartsWith("SZPC")) continue;
        //            var dr = dt.NewRow();
        //            dr["Server"] = name;
        //            dr["Mode"] = AttrValue(server, "ProductMode");
        //            server.Descendants("Service").ToList().ForEach(s => dr[AttrValue(s, "Name")] = "Y");
        //            dt.Rows.Add(dr);
        //        }

        //        var dv = new DataView(dt);
        //        dv.Sort = "Mode";
        //        dt = dv.ToTable();
        //    }
        //    catch
        //    {
        //    }

        //    return dt;
        //}

        #region B13FSecuritySetting

        //public static string B13FParserJSPath { get; set; }
        //public static string B13FExchanges { get; set; }
        //public static Dictionary<string, string> B13FPDFAddr { get; set; }

        #endregion


        #region BatchTaskProducer service

        //public static TimeSpan
        //    BatchTaskTimeInterval
        //{ get; private set; } // task check time interval(for settled time point)

        //public static bool BatchTaskProducerFilterByPaidUser { get; private set; }
        //public static TimeSpan BatchTaskStartTime { get; private set; }
        //public static TimeSpan BatchTaskEndTime { get; private set; }

        #endregion
    }

    //from Leon: TP for previous version, like 3.15, and staging(STG) for current version, like 3.16.
    public enum ProductMode
    {
        QA,
        TP,
        PatchTP,
        STG,
        LIVE,
        UAT,
        LOCAL,
        DEV,
        STG2
    }
}
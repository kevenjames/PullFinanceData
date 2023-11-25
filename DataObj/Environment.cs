using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using PullFinanceData.Util;

/// <summary>
///     AWDEnvironment is a setting container for current page or application. This object need to
///     be create when load page or application start and pass it to other objects or functions.
/// </summary>
[Serializable]
public class AWDEnvironment
{
    public delegate KeyValuePair<Guid, string> GetUserCompanyCallback(Guid userId, Guid productId);

    public delegate string GetUserNameCallback(string userId);

    public delegate IDictionary<Guid, Guid> RoleUnitCallback(AWDEnvironment environment);

    public const string ProductCode_DIRECT = "DIRECT";
    public const string ProductCode_AWSOE = "AWSOE";
    public const string ProductCode_CWP = "CWP";
    public const string ProductCode_PS = "PS";
    public const string ProductCode_EPI = "EPI";
    public const string ProductCode_PSWEB = "PSWEB";
    public const string ProductCode_SRT = "SRT";
    public const string ProductCode_QS = "QS";
    public const string ProductCode_ERT = "ERT"; //europe retirement
    public const string ProductCode_MPS = "MPS"; //mps.morningstar.com
    public const string ProductCode_PRBatch = "TBB";
    public const int T2PConvertionTimeout = 300;
    public const int T2PConvertionBatchInsertMax = 2000;
    public const int s_GetCustomFundTypeBatchMax = 100;
    public const int CloneHoldingTimeout = 600;
    public static Encoding s_DefaultEncoding = Encoding.UTF8;
    public static Encoding s_DefaultEncodingNoBOM = new UTF8Encoding(false, true);

    public static XmlWriterSettings s_DefaultXmlWriterSettings = new()
    {
#if DEBUG
        Indent = true,
#else
            Indent = false,
#endif
        OmitXmlDeclaration = true,
        Encoding = s_DefaultEncoding,
        CloseOutput = true,
        ConformanceLevel = ConformanceLevel.Auto
    };

    public static Guid s_AdvProductId = new("00000001-F18A-47B0-B8AE-9CD50DB2D0A0");
    public static Guid s_DirectProductId = new("2262E3B5-8F49-49C2-9A39-5099FC3B1435");
    public static Guid s_MISProductId = new("1D4F3431-AEE0-44E0-A317-7DE0EC0D861F");
    public static Guid s_EPIProductId = new("CCF496E4-5DBA-4C25-A71D-B4F882997D47");
    public static Guid s_TBBProductId = new("8952E02E-8A53-4DDB-A80B-3813D6C24BEC");
    public static Guid s_ITASubProductId = new("BEF6938A-304B-4002-B791-E7E2ED5262D1");
    public static Guid s_MOPSubProductId = new("aa2e72c9-a7b9-4a6d-a1b4-d4b99404d3bd");
    public static Guid s_AUSSubProductId = new("EF33150A-0DAF-45D4-B80F-73B778BFCC47");
    public static Guid s_NZLSubProductId = new("DD3E2A5B-90B0-4417-BE42-C242CB9CA293");
    public static Guid s_UKSubProductId = new("FD1F4A3D-E3A5-497F-BDF0-1F758DBCA2D7");
    public static Guid s_MYSubProductId = new("DC410A95-699D-4141-AC8C-372D2C3708FE");
    public static Guid s_DirectSubProductId = new("2262E3B5-8F49-49C2-9A39-5099FC3B1435");
    public static Guid s_TBBSubProductId = new("8952E02E-8A53-4DDB-A80B-3813D6C24BEC");
    public static Guid s_CHNSubProductId = new("E175B51B-E6A9-470B-93D0-C18AA928E8F8");
    public static Guid s_MITProductId = new("2FD703DA-849A-4E34-91A1-3829F2A3C3D7");
    public static Guid s_PSProductId = new("D57A4FE9-1EE8-48EE-BDEE-1DF0357FB6A0");
    public static Guid s_PSWebProductId = new("EF8E5F45-223F-4E1C-8AAD-F4BFA414B5C4");
    public static Guid s_CWPProductId = new("EE32A9B5-8112-499B-BA5E-DE0E796BFFA1");
    public static Guid s_INDSubProductId = new("441645DF-FF48-48FD-A2DC-61E8FEBAC974");

    public static readonly Dictionary<string, Guid> s_ProductDictionary = new()
    {
        { ProductCode_DIRECT, s_DirectProductId },
        { ProductCode_AWSOE, s_AdvProductId },
        { ProductCode_CWP, s_CWPProductId },
        { ProductCode_PS, s_PSProductId },
        { ProductCode_EPI, s_EPIProductId },
        { ProductCode_PSWEB, s_PSWebProductId }
    };

    public static List<string> s_SupportLangList = new()
        { "ENU", "CHS", "CHT", "ITA", "DEU", "ENA", "FRA", "FRC", "JPN", "ZHH", "KOR", "PTG", "ESP", "ENZ" };

    public static string s_DataPath = "data";
    public static string s_LogPath = "requestlog";
    public static bool s_CanLog = true;
    public static int s_SqlCommandTimeout = 60;
    public static int s_SqlBigCommandTimeout = 120;
    public static int s_ThreadTimeout = 60;
    public static int s_runtimeCacheTimeout = 10;
    public static bool s_BorrowRate = false;

    private static bool s_isApplication;
    private static readonly string s_NoMsgFlag = "0";
    private static string s_ApplicationPath = "";
    private static readonly AWDEnvironment s_DefaultEnv;
    private static readonly bool s_EnableUniverseCache;
    private static readonly bool s_IsOffline;
    private static bool s_SmartSeachClosed;

    //private static readonly string s_CodeVersion;
    private static readonly bool s_Use3xMVAsCloudMV;

    public EventLogger EventLogger = new EventLogger();

    // NOTE(wqin): It is a hardcode logic for ADS userfund, this attribute should be removed after new ADS user fund module released. Request By Jason #109,280
    public string HardCodeVersionInfo;

    private List<int> m_AdminRoles;

    private string m_awdVersion;

    private CommonBatchLogger m_CommonBatchLogger;

    private CommonLogger m_commonLogger;

    private Dictionary<string, bool> m_convertedFlags;
    private string m_CSRFToken;
    private int m_defaultPASDBServerId;
    private int m_defaultPositionDBServerId;
    private int m_defaultPositionDBServerIdDraft;

    private string m_doVersion;

    private DynamicCache m_DynamicCache;
    private string m_EnableExtendedPerformance;
    private Guid m_FirmId;
    private Guid m_HostProductId;
    private string m_InstitutionId;

    private int m_InstitutionUserId;
    private bool m_IsShareLibrary;
    private object m_lock = new();
    private int m_NewPosDBServerId;
    private int m_NewPosDBServerIdDraft;
    private Guid m_OfficeId;
    private string m_PrePostReturn;
    private PrivateIndexPermission m_PrivateIndexPermission;
    private string m_ProductCode;
    private Guid m_ProductId;
    private ReclassificationCache m_ReclassificationCache;
    private string m_RegionId;
    private string m_replaceProductCode;
    private string m_requestId;
    private ResearchDataPermission m_ResearchDataPermission;

    [XmlIgnore] private IDictionary<Guid, Guid> m_roleIds;

    private Guid m_SessionId;
    private bool m_skipIDCChecking;
    private int m_StorageType;

    private string m_subProductCode;
    private Guid m_SubProductId;
    private string m_targetCurrency;
    private bool? m_UseFirmSetting;
    private KeyValuePair<Guid, string> m_userCompany;

    private string m_userLoginEmail;
    public Dictionary<string, bool> MChBmkIdMap = new();
    public Dictionary<string, bool> MChRfIdMap = new();

    public Dictionary<string, bool> MChSecIdMap = new();

    private CacheControlObject s_CacheControlObject;

    private bool s_NoCache;

    public SharedServiceLogger SharedSvcLogger = new SharedServiceLogger();

    static AWDEnvironment()
    {
        STripleKey = ConfigurationManager.AppSettings["STripleKey"];
        s_NoMsgFlag = ConfigurationManager.AppSettings["NoMsg"];
        Guid userId = ConfigurationManager.AppSettings["TestUserId"].ToGuid();
        string productCode = ConfigurationManager.AppSettings["TestProductCode"];
        IsTestPool = ConfigurationManager.AppSettings["IsTestPool"].ToBool();
        Guid subProductId = ConfigurationManager.AppSettings["TestSubProductId"].ToGuid();
        HostName = ConfigurationManager.AppSettings["HostName"];
        s_SmartSeachClosed = ConfigurationManager.AppSettings["SmartSeachClosed"].ToBool();

        s_IsOffline = ConfigurationManager.AppSettings["IsOffline"].ToBool();
        IsSecurityCheck = ConfigurationManager.AppSettings["IsSecurityCheck"].ToBool();
        UsePortSP = true; // DataTypeUtil.ObjectBoolValue(ConfigurationManager.AppSettings["UsePortSP"]);
        if (DataTypeUtil.IsNull(productCode) || productCode == "MSTAR2")
            productCode = "AWSOE";
        var productId = s_AdvProductId;
        if (productCode == "DIRECT")
            productId = s_DirectProductId;
        else if (productCode == "MIS")
            productId = s_MISProductId;
        else if (productCode == "EPI")
            productId = s_EPIProductId;
        else if (productCode == "CWP")
            productId = s_CWPProductId;
        else if (productCode == "PSWEB")
            productId = s_PSWebProductId;
        Guid creatorId = ConfigurationManager.AppSettings["TestCreatorId"].ToGuid();
        if (DataTypeUtil.IsNull(creatorId))
            creatorId = userId;
        AssetAllocationCountry = ConfigurationManager.AppSettings["AssetAllocationCountry"];
        s_EnableUniverseCache = ConfigurationManager.AppSettings["EnableUniverseCache"] == "1";
        Paymentech = ConfigurationManager.AppSettings["Paymentech"];
        TestCardNum = ConfigurationManager.AppSettings["TestCardNum"];

        s_DefaultEnv = new AWDEnvironment(userId, userId, productId, subProductId, productCode, "3", "", creatorId);
        s_CanLog = Directory.Exists(ApplicationPath + s_LogPath);
        AWSOEFeedbackEmail = ConfigurationManager.AppSettings["AWSOEFeedbackEmail"];
        CommonConnString = ConfigurationManager.AppSettings["CommonConnString"];
        if (DataTypeUtil.IsNull(AWSOEFeedbackEmail))
            AWSOEFeedbackEmail = CRMConst.sAWSOEFeedbackEmail;
        AnnCryptKey = ConfigurationManager.AppSettings["AnnuityIntelligenceEncrypt"];
        AnnHashKey = ConfigurationManager.AppSettings["AnnuityIntelligenceHash"];
        ServerFarm = Environment.GetEnvironmentVariable("farm");
        s_Use3xMVAsCloudMV = ConfigurationManager.AppSettings["Use3xMVAsCloudMV"].ToBool(false);
    }

    public AWDEnvironment()
    {
    }

    public AWDEnvironment(Guid userId, Guid officeId, Guid productId, string productCode,
        string secode, string languageId, Guid creatorId)
    {
        UserId = userId;
        m_OfficeId = officeId;
        m_ProductId = productId;
        m_ProductCode = productCode;
        Secode = secode;
        LanguageId = languageId;
        CreatorId = creatorId;
        m_InstitutionUserId = 0;
        m_InstitutionId = "MSTAR";
    }

    public AWDEnvironment(Guid userId, Guid officeId, Guid productId, string productCode, Guid subproductId,
        string subproductCode, string secode, string languageId, Guid creatorId)
    {
        UserId = userId;
        m_OfficeId = officeId;
        m_ProductId = productId;
        m_ProductCode = productCode;
        m_SubProductId = subproductId;
        m_subProductCode = subproductCode;
        Secode = secode;
        LanguageId = languageId;
        CreatorId = creatorId;
        m_InstitutionUserId = 0;
        m_InstitutionId = "MSTAR";
    }

    public AWDEnvironment(Guid userId, Guid officeId, Guid productId, Guid subProductId, string productCode,
        string secode, string languageId, Guid creatorId)
        : this(userId, officeId, productId, productCode, secode, languageId, creatorId)
    {
        m_SubProductId = subProductId;
    }

    public AWDEnvironment(Guid userId, Guid officeId, Guid productId, string productCode,
        string secode, string languageId, Guid creatorId, int institutionUserId,
        string institutionId)
    {
        UserId = userId;
        m_OfficeId = officeId;
        m_ProductId = productId;
        m_ProductCode = productCode;
        Secode = secode;
        LanguageId = languageId;
        CreatorId = creatorId;
        m_InstitutionUserId = institutionUserId;
        m_InstitutionId = institutionId;
    }

    public static string HostName { get; }

    public static string CommonConnString { get; set; }

    public static bool IsTestPool { get; set; }

    public static bool IsAdcalcdataBatchFarm => IsFarm("adcalcdata-batch");

    public static bool IsAdcalcdataAltFarm => IsFarm("adcalcdata-alt");

    public static bool IsAdmainFarm => IsFarm("admain");

    public static bool IsAdpasFarm => IsFarm("adpas");

    public static bool IsAwdprocFarm => IsFarm("awdproc");

    public static bool IsAdtoolFarm => IsFarm("adtool");

    public static bool IsAwgmainFarm => IsFarm("awgmain");


    public bool IsPRSeparateGroup { get; set; }

    public static bool IsLive => ServerConfiguration.DBProductMode == ProductMode.LIVE;

    public static bool UsePortSP { get; }

    public static bool IsSecurityCheck { get; }

    public static bool s_IsApplication
    {
        get => s_isApplication;
        set
        {
            s_isApplication = value;
            s_SqlCommandTimeout = 180;
        }
    }

    public static string Paymentech { get; }

    public static string TestCardNum { get; }

    public static string AnnCryptKey { get; }

    public static string AnnHashKey { get; }

    public static string STripleKey { get; }

    public static bool IsSmartSeachClosed
    {
        get => s_SmartSeachClosed;
        set => s_SmartSeachClosed = value;
    }

    public static bool DisableTrackingLog
    {
        get
        {
            try
            {
                return CacheUtil.GetCache("DisableTrackingLog", LoadDynamicAppSetting, "DisableTrackingLog", 10)
                    .ToBool();
            }
            catch (Exception ex)
            {
                ExceptionHandler.DefaultHandleException(ex);
                return false;
            }
        }
    }

    public static bool DisableMsgQueue
    {
        get
        {
            try
            {
                return CacheUtil.GetCache("DisableMsgQueue", LoadDynamicAppSetting, "DisableMsgQueue", 10).ToBool();
            }
            catch (Exception ex)
            {
                ExceptionHandler.DefaultHandleException(ex);
                return false;
            }
        }
    }

    public static bool DisableUsageTracking
    {
        get
        {
            try
            {
                return CacheUtil.GetCache("DisableUsageTracking", LoadDynamicAppSetting, "DisableUsageTracking", 10)
                    .ToBool();
            }
            catch (Exception ex)
            {
                ExceptionHandler.DefaultHandleException(ex);
                return false;
            }
        }
    }

    public static bool EnableTransitPage
    {
        get
        {
            try
            {
                return CacheUtil.GetCache("EnableTransitPage", LoadDynamicAppSetting, "EnableTransitPage", 10).ToBool();
            }
            catch (Exception ex)
            {
                ExceptionHandler.DefaultHandleException(ex);
                return false;
            }
        }
    }

    public static string AWSOEFeedbackEmail { get; }

    public static string ApplicationPath
    {
        get
        {
            if (DataTypeUtil.IsNull(s_ApplicationPath))
            {
                s_ApplicationPath = @"d:\wwwroot\";
                if (!Directory.Exists(s_ApplicationPath + "xml"))
                    s_ApplicationPath = @"d:\inetpub\wwwroot\";
            }

            return s_ApplicationPath;
        }
        set
        {
            s_ApplicationPath = value;
            if (!s_ApplicationPath.EndsWith("\\"))
                s_ApplicationPath += "\\";
            if (!Directory.Exists(s_ApplicationPath + "xml"))
                s_ApplicationPath = @"d:\wwwroot\";
            if (!Directory.Exists(s_ApplicationPath + "xml"))
                s_ApplicationPath = @"d:\inetpub\wwwroot\";
        }
    }

    public static string AssetAllocationCountry { get; } = "USA";

    public static string ServerFarm { get; }

    public Guid IID { get; set; }

    public int CopyRightYear => DateTime.Now.Year;

    public static AWDEnvironment s_DefaultEnviroment
    {
        get
        {
            var env = new AWDEnvironment(s_DefaultEnv.UserId,
                s_DefaultEnv.UserId,
                s_DefaultEnv.ProductId,
                s_DefaultEnv.ProductCode,
                s_DefaultEnv.Secode,
                s_DefaultEnv.LanguageId,
                s_DefaultEnv.CreatorId);
            env.RequestId = s_DefaultEnv.RequestId;
            env.NoCache = true;
            return env;
        }
    }

    public static AWDEnvironment DefaultDirectEnviroment
    {
        get
        {
            var env = new AWDEnvironment(s_DefaultEnv.UserId,
                s_DefaultEnv.UserId,
                s_DirectProductId,
                ProductCode_DIRECT,
                s_DefaultEnv.Secode,
                s_DefaultEnv.LanguageId,
                s_DefaultEnv.CreatorId);
            env.RequestId = s_DefaultEnv.RequestId;
            return env;
        }
    }

    public string UserLoginEmail
    {
        get
        {
            if (m_userLoginEmail.IsNullOrEmpty() && UserId != Guid.Empty)
                try
                {
                    var db = new UsersReadOnlyDB(this);
                    db.CreateStoredProcCommand("dbo.GetUserEmail");
                    db.AddInParameter("@UserId", DbType.Guid, UserId);
                    db.ExecuteReader(reader => m_userLoginEmail = reader[1] as string);
                }
                catch (Exception ex)
                {
                    ExceptionHandler.DefaultHandleException(ex, this);
                    m_userLoginEmail = "";
                }

            return m_userLoginEmail;
        }
    }

    public bool IsMorningstarUser
    {
        get
        {
            var email = UserLoginEmail;
            if (email == null || email.StartsWith("mis") || email.StartsWith("mop"))
                return false;
            return email.ToLower().EndsWith("morningstar.com");
        }
    }

    public bool FromBatch { get; set; }

    public bool UseFirmSetting
    {
        get
        {
            if (m_UseFirmSetting == null)
            {
                UserProfileDB db = new UserProfileDB(this);
                bool result;
                db.Execute_GetFlagUseFirmLevelSettings(UserId, out result);
                m_UseFirmSetting = result;
            }

            return (bool)m_UseFirmSetting;
        }
        set => m_UseFirmSetting = value;
    }

    public string SubProductCode
    {
        get
        {
            if (DataTypeUtil.IsNull(m_subProductCode))
            {
                var db = new UsersReadOnlyDB(this);
                db.SetCommand_GetSubProductCode(SubProductId);
                m_subProductCode = db.ExecuteScalar().ToStr();
            }

            return m_subProductCode;
        }
    }

    public Guid UserId { get; private set; }

    public Guid FirmId
    {
        get
        {
            if (DataTypeUtil.IsNull(m_FirmId) && !DataTypeUtil.IsNull(UserId) && !DataTypeUtil.IsNull(ProductId))
            {
                var fid = CacheUtil.GetCache(UserId + "-" + ProductId + "-firmid", p =>
                {
                    var firmId = Guid.Empty;
                    var o = p as AWDEnvironment;
                    var db = new UsersReadOnlyDB(o);
                    db.SetCommand_Usr_GetCompanyInfoByUserId(UserId, ProductId);
                    db.ExecuteReader(
                        delegate(IDataReader reader) { firmId = reader["CompanyId"].ToGuid(); }
                    );
                    return firmId;
                }, this, s_runtimeCacheTimeout);

                m_FirmId = DataTypeUtil.ObjectGuidValue(fid);
            }

            return m_FirmId;
        }
    }

    public string ProductCode
    {
        get => m_ProductCode;
        set => m_ProductCode = value;
    }

    public bool IsPresentationStudio =>
        m_ProductCode == ProductCode_PS && m_ProductId == s_PSProductId &&
        !DataTypeUtil.IsNull(m_HostProductId) && m_HostProductId == s_DirectProductId;

    public bool IsReportStudio =>
        m_ProductCode == ProductCode_PS && m_ProductId == s_PSProductId &&
        !DataTypeUtil.IsNull(m_HostProductId) && m_HostProductId == s_AdvProductId;

    public bool IsPRBatch => m_ProductCode == ProductCode_PRBatch && m_ProductId == s_TBBProductId;

    public bool IsShareLibrary
    {
        get => m_IsShareLibrary;
        set => m_IsShareLibrary = value;
    }

    public Guid ProductId
    {
        get
        {
            if (m_ProductCode == ProductCode_PS && m_ProductId == s_PSProductId &&
                !DataTypeUtil.IsNull(m_HostProductId) && m_HostProductId == s_AdvProductId)
                return m_HostProductId;

            if (m_ProductCode == ProductCode_EPI
                && m_ProductId == s_EPIProductId
                && !DataTypeUtil.IsNull(m_HostProductId)
                && m_HostProductId == s_DirectProductId
               )
                return m_HostProductId;

            return m_ProductId == s_TBBProductId ? s_DirectProductId : m_ProductId;
        }
        set => m_ProductId = value;
    }

    public Guid SubProductId
    {
        get
        {
            if (m_ProductCode == ProductCode_PS && !DataTypeUtil.IsNull(m_HostProductId) &&
                m_HostProductId == s_AdvProductId && m_SubProductId != s_UKSubProductId)
                m_SubProductId = m_HostProductId;

            return m_SubProductId == s_TBBSubProductId ? s_DirectSubProductId : m_SubProductId;
        }
        set => m_SubProductId = value;
    }

    public string ReplaceProductCode
    {
        get => m_replaceProductCode;
        set => m_replaceProductCode = value;
    }

    public Guid CreatorId { get; private set; }

    public string Secode { get; }

    public Guid OfficeId
    {
        get => m_OfficeId;
        set => m_OfficeId = value;
    }

    public string LanguageId { get; private set; }

    public string ClientRegionId { get; set; }

    public string RegionIdForLanguageId { get; private set; }

    public string RegionId
    {
        get => m_RegionId;
        set
        {
            ClientRegionId = value;
            var id = value;

            //Enable limited language support for DIRECT
            if (IsDirect() && id != null && !s_SupportLangList.Contains(id.ToUpper()))
                id = "ENU";
            m_RegionId = DataTypeUtil.IsNull(id) ? "ENU" : id.ToUpper();
            LanguageId = LangIdMapping.Instance.GetLangId(m_RegionId);
            RegionIdForLanguageId = LangIdMapping.Instance.GetRegionId(m_RegionId);
        }
    }

    public Guid SessionId
    {
        get => m_SessionId;
        set => m_SessionId = value;
    }

    public string CSRFToken
    {
        get => m_CSRFToken;
        set => m_CSRFToken = value;
    }

    public string TargetCurrency
    {
        get => m_targetCurrency;
        set => m_targetCurrency = value;
    }

    public string EnableExtendedPerformance
    {
        get => m_EnableExtendedPerformance;
        set => m_EnableExtendedPerformance = value;
    }

    public int InstitutionUserId
    {
        get => m_InstitutionUserId;
        set => m_InstitutionUserId = value;
    }

    public string InstitutionId
    {
        get => m_InstitutionId;
        set => m_InstitutionId = value;
    }

    public Guid HostProductId
    {
        get
        {
            if (m_HostProductId == s_TBBProductId || m_HostProductId == s_EPIProductId)
                return s_DirectProductId;
            return m_HostProductId;
        }
        set => m_HostProductId = value;
    }

    public int ProductBit => m_ProductId == s_DirectProductId ? 2 : 1;

    public bool SkipIDCCheck
    {
        get => m_skipIDCChecking;
        set => m_skipIDCChecking = value;
    }

    public string RequestId
    {
        get => m_requestId;
        set => m_requestId = value;
    }

    public PriceProviderType PriceProviderType { get; set; }
    public bool IsMARTPriceProvider => PriceProviderType == PriceProviderType.Mart;
    public Dictionary<Guid, Guid> CompanyIds { get; } = new();

    private string m_userName { get; set; }

    [XmlIgnore]
    public PrivateIndexPermission PrivateIndexPermission
    {
        get
        {
            if (m_PrivateIndexPermission == null)
                m_PrivateIndexPermission = new PrivateIndexPermission(this);
            return m_PrivateIndexPermission;
        }
    }

    [XmlIgnore]
    public ResearchDataPermission ResearchDataPermission
    {
        get
        {
            if (m_ResearchDataPermission == null)
                m_ResearchDataPermission = new ResearchDataPermission(this);
            return m_ResearchDataPermission;
        }
    }

    [XmlIgnore] public ResearchDPPermission ResearchDPPermission { get; set; }

    [XmlIgnore]
    public ReclassificationCache ReclassificationCache
    {
        get
        {
            if (m_ReclassificationCache == null)
                m_ReclassificationCache = new ReclassificationCache(this);
            return m_ReclassificationCache;
        }
    }

    public string PrePostReturn
    {
        get => m_PrePostReturn;
        set => m_PrePostReturn = value ?? (m_SubProductId == s_ITASubProductId ? "2" : "1");
    }

    private List<int> AdminRoles
    {
        get
        {
            if (DataTypeUtil.ObjectIsNull(m_AdminRoles))
            {
                m_AdminRoles = new List<int>();

                try
                {
                    var db = new AWDDBBase(this, AWDConnection.Users);
                    db.CreateSqlStringCommand("dbo.getAdvisors '" + UserId + "'");
                    db.ExecuteReader(delegate(IDataReader reader)
                    {
                        var roleId = reader["RoleId"].ToInt();
                        if (!m_AdminRoles.Contains(roleId))
                            m_AdminRoles.Add(roleId);
                    });
                }
                catch (Exception ex)
                {
                    ExceptionHandler.DefaultHandleException(ex);
                }
            }

            return m_AdminRoles;
        }
    }

    public string AWDVersion
    {
        get => m_awdVersion;
        set => m_awdVersion = value;
    }

    public string DOVersion
    {
        get => m_doVersion;
        set => m_doVersion = value;
    }

    public TrackingLog TrackingLog { get; set; }

    public UsageLoggerObject UsageLoggerObject { get; set; }

    /// <summary>
    ///     System default user currency. Now is from SubProduct level.
    /// </summary>
    public string UserDefaultCurrency { get; set; }

    public string Module { get; set; }

    [XmlIgnore] public Dictionary<string, string> AppliedUniverse { get; set; }

    [XmlIgnore]
    public DynamicCache DynamicCache
    {
        get
        {
            if (m_DynamicCache == null)
                m_DynamicCache = new DynamicCache(this);
            return m_DynamicCache;
        }
    }

    public string Ms_sso2 { get; set; }


    public bool IsShowAllAUSHolding { get; set; }
    public bool ByPassDataPermission { get; set; }
    public Guid PermissionUserId { private get; set; }
    public Guid PermissionProductId { private get; set; }

    public bool IsPSBatchRequest { get; set; }

    public bool DisableQuotePrice { get; set; }

    public Dictionary<string, bool> ConvertedFlags
    {
        get
        {
            if (m_convertedFlags == null)
            {
                m_convertedFlags = new Dictionary<string, bool>
                {
                    { EnvConst.CustomSecurityConverted, false },
                    { EnvConst.StrategyConverted, false },
                    { EnvConst.AggregateConverted, false },
                    { EnvConst.DirectNoteConverted, false }
                };

                if (DataTypeUtil.IsNull(UserId)) return m_convertedFlags;

                try
                {
                    var db = new UsersReadOnlyDB(this);
                    db.SetCommand_Usr_GetConvertFlag(UserId);
                    db.ExecuteReader(
                        delegate(IDataReader reader)
                        {
                            m_convertedFlags[EnvConst.CustomSecurityConverted] =
                                reader[EnvConst.CustomSecurityConverted].ToBool();
                            m_convertedFlags[EnvConst.StrategyConverted] = reader[EnvConst.StrategyConverted].ToBool();
                            m_convertedFlags[EnvConst.AggregateConverted] =
                                reader[EnvConst.AggregateConverted].ToBool();
                        });
                }
                catch (Exception ex)
                {
                    ExceptionHandler.DefaultHandleException(ex);
                }

                try
                {
                    if (s_DirectProductId.Equals(HostProductId))
                    {
                        var userProfileDb = new UserProfileDB(this);
                        userProfileDb.SetCommand_Usr_GetDirectNoteConvertFlag(UserId);
                        userProfileDb.ExecuteReader(
                            delegate(IDataReader reader)
                            {
                                m_convertedFlags[EnvConst.DirectNoteConverted] =
                                    reader[EnvConst.DirectNoteConverted].ToBool();
                            });

                        if (!m_convertedFlags[EnvConst.DirectNoteConverted].ToBool())
                        {
                            //  if not converted, check if there are any old Direct notes.
                            int count;
                            var userNoteAdaptorDb = new UserNoteAdaptorDB(this);
                            userNoteAdaptorDb.Execute_UserNote_Statistics(UserId, HostProductId, out count);
                            if (count <= 0)
                            {
                                //  if there isn't any old Direct notes, think it's converted.
                                if (userProfileDb.ReadOnly)
                                    userProfileDb = new UserProfileDB(this, explicitWrite: true);
                                userProfileDb.Execute_Usr_UpdateDirectNoteConvertFlag(UserId, true);
                                m_convertedFlags[EnvConst.DirectNoteConverted] = true;
                            }
                        }
                    }
                    else
                    {
                        m_convertedFlags[EnvConst.DirectNoteConverted] = false;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionHandler.DefaultHandleException(ex);
                }
            }

            return m_convertedFlags;
        }
    }

    public object CacheSecurityInfoProviderObj { get; set; }

    public CommonLogger CommonLogger
    {
        get
        {
            if (m_commonLogger == null)
            {
                m_commonLogger = new CommonLogger();
                m_commonLogger.request_id = RequestId;
                m_commonLogger.user_id = UserId.ToStr();
            }

            return m_commonLogger;
        }
    }

    public CommonBatchLogger CommonBatchLogger
    {
        get
        {
            if (m_CommonBatchLogger == null)
                m_CommonBatchLogger = new CommonBatchLogger(CommonBatchLogger.BatchServiceName);
            return m_CommonBatchLogger;
        }
    }

    public bool IncludeCloudList { get; set; }
    public bool IsFromADS { get; set; }
    public bool IsFromAppStream { get; set; }

    public bool NoCache
    {
        get => CacheControl.GlobalCacheDisabled;
        set => s_NoCache = value;
    }

    public CacheControlObject CacheControl
    {
        get
        {
            if (s_CacheControlObject == null) s_CacheControlObject = GetCacheControlObject(s_NoCache);
            return s_CacheControlObject;
        }
    }

    private static bool IsFarm(string farm)
    {
        return farm.Equals(ServerFarm, StringComparison.InvariantCultureIgnoreCase);
    }

    //public static string CodeVersion
    //{
    //    get { return s_CodeVersion; }
    //}

    private static object LoadDynamicAppSetting(string key)
    {
        try
        {
            if (ConfigurationManager.AppSettings.Get(key) != null)
                return new AppSettingsReader().GetValue(key, typeof(string));
            return null;
        }
        catch
        {
            return null;
        }
    }

    public static List<int> GetPasDBServers(AWDEnvironment env)
    {
        var serverIds = new List<int>();
        var db = new VerifySessionDB(env);
        db.SetCommand_GetPasDBServers(!AWDServerConfiguration.IsLive);
        db.ExecuteReader(reader => serverIds.Add(reader[0].ToInt()));
        return serverIds;
    }

    public void SetMap(Dictionary<string, bool> mchmap)
    {
        if (mchmap.Count > 0)
        {
            var keys = new List<string>();
            keys.AddRange(mchmap.Keys);
            foreach (var kv in keys) mchmap[kv] = true;
        }
    }

    public static AWDEnvironment GetEnviroment(Guid userId)
    {
        return GetEnviroment(userId, s_AdvProductId, false);
    }

    public static AWDEnvironment GetEnviroment(Guid userId, Guid productId)
    {
        return GetEnviroment(userId, productId, false);
    }

    public static AWDEnvironment GetEnviroment(Guid userId, Guid productId, bool setSubProductInfo)
    {
        var productCode = "AWSOE";
        if (productId == s_DirectProductId)
            productCode = "DIRECT";
        else if (productId == s_MISProductId)
            productCode = "MIS";
        var obj = setSubProductInfo
            ? new AWDEnvironment(userId, userId, productId, productCode, productId, productCode,
                s_DefaultEnviroment.Secode, s_DefaultEnviroment.LanguageId,
                userId)
            : new AWDEnvironment(userId, userId, productId, productCode,
                s_DefaultEnviroment.Secode, s_DefaultEnviroment.LanguageId,
                userId);
        obj.SetMissingInfo();
        if (DataTypeUtil.IsNull(obj.RequestId))
            obj.RequestId = Guid.NewGuid().ToStr();
        return obj;
    }

    public void SetMissingInfo(bool correctUnitIdForDirect = false)
    {
        var paraTuple = new Tuple<AWDEnvironment, bool>(this, correctUnitIdForDirect);
        var spInfo = CacheUtil.GetCache(
            UserId.ToString().ToLower() + "-" + ProductId + "-" + correctUnitIdForDirect + "-spid", p =>
            {
                var para = p as Tuple<AWDEnvironment, bool>;
                var o = para.Item1;
                var correctUnitId = para.Item2;

                var info = new SubProductInfo();
                UsersReadOnlyDB db = new UsersReadOnlyDB(o);
                db.SetCommand_Usr_GetSubProductId(o.UserId, o.ProductId);
                db.ExecuteReader(delegate(IDataReader reader)
                {
                    info.SubProductId = DataTypeUtil.ObjectGuidValue(reader[0]);
                    info.OfficeId = DataTypeUtil.ObjectGuidValue(reader[1]);
                    info.SubProductCode = DataTypeUtil.ObjectStringValue(reader[2]);
                    info.ClientRegionId = DataTypeUtil.ObjectStringValue(reader[3]);
                });

                if (correctUnitId)
                {
                    var unitId = Guid.Empty;
                    UserProfileDB profileDB = new UserProfileDB(o);
                    profileDB.SetCommand_GetUserProductCode(o.UserId, ProductCode_DIRECT);
                    profileDB.ExecuteReader(delegate(IDataReader reader)
                    {
                        var nIndex = reader.GetOrdinal("UnitId");
                        if (!reader.IsDBNull(nIndex))
                            unitId = reader.GetGuid(nIndex);
                    });

                    info.OfficeId = unitId;
                }

                return info;
            }, paraTuple, s_runtimeCacheTimeout) as SubProductInfo;

        if (spInfo != null)
        {
            SubProductId = spInfo.SubProductId;
            OfficeId = spInfo.OfficeId;
            m_subProductCode = spInfo.SubProductCode;
            ClientRegionId = spInfo.ClientRegionId;
            m_RegionId = ClientRegionId;
        }
        else
        {
            //There is exception in CacheUtil.GetCache.
            ExceptionHandler.DefaultHandleException(
                new Exception("Error to get SPID, UNID from database when SetMissingInfo."));
        }

        if (UserId == s_ITASubProductId || UserId == s_MOPSubProductId || UserId == s_AUSSubProductId
            || UserId == s_NZLSubProductId || UserId == s_UKSubProductId || UserId == s_MYSubProductId
            || UserId == s_CHNSubProductId || UserId == s_TBBSubProductId || UserId == s_INDSubProductId)
            SubProductId = UserId;
        if (DataTypeUtil.IsNull(SubProductId))
            SubProductId = UserId;
        if (DataTypeUtil.IsNull(OfficeId))
            OfficeId = UserId;
    }

    public static void SetRequestIdOnDefaultEnvironment(string newRequestId)
    {
        s_DefaultEnv.RequestId = newRequestId;
    }

    public string GetUserName(GetUserNameCallback callback, string userId)
    {
        if (!UserId.ToString().Compare(userId, true)) return callback(userId);

        if (string.IsNullOrEmpty(m_userName)) m_userName = callback(userId);

        return m_userName;
    }

    public KeyValuePair<Guid, string> GetUserCompany(GetUserCompanyCallback callback, Guid userId, Guid productId)
    {
        if (UserId != userId || ProductId != productId) return callback(userId, productId);

        if (DataTypeUtil.IsNull(m_userCompany.Key) && DataTypeUtil.IsNull(m_userCompany.Value))
            m_userCompany = callback(userId, productId);

        return m_userCompany;
    }

    public bool IsAdmin()
    {
        return AdminRoles != null && AdminRoles.Contains(21);
    }

    public Guid GetPermissionUserId()
    {
        return DataTypeUtil.IsNull(PermissionUserId) ? UserId : PermissionUserId;
    }

    public Guid GetPermissionProductId()
    {
        return DataTypeUtil.IsNull(PermissionProductId) ? ProductId : PermissionProductId;
    }

    public bool IsOwner()
    {
        if (CreatorId == UserId)
            return true;
        return false;
    }

    public bool IsAWSOE()
    {
        return m_ProductId == s_AdvProductId;
    }

    public bool IsAWSOE_USA()
    {
        return m_ProductId == s_AdvProductId && m_SubProductId == s_AdvProductId;
    }

    public bool IsDirect()
    {
        return m_ProductId == s_DirectProductId;
    }

    public bool IsPSOrPSWEB()
    {
        return m_ProductId == s_PSProductId || m_ProductId == s_PSWebProductId;
    }

    public bool IsTBBProduct()
    {
        return m_ProductId == s_TBBProductId;
    }

    public bool IsMIS()
    {
        return m_ProductId == s_MISProductId;
    }

    public bool IsNoMsg()
    {
        return s_NoMsgFlag == "1";
    }

    public bool IsAWSOE_AU()
    {
        return m_ProductId == s_AdvProductId && m_SubProductId == s_AUSSubProductId;
    }

    public bool IsAWSOE_NZL()
    {
        return m_ProductId == s_AdvProductId && m_SubProductId == s_NZLSubProductId;
    }

    public bool IsUKOrEng()
    {
        return m_SubProductId == s_UKSubProductId || (m_RegionId != null && m_RegionId.ToUpper() == "ENG");
    }

    public bool IsIND()
    {
        return m_SubProductId == s_INDSubProductId;
    }

    public bool IsAWSOE_UK()
    {
        return m_ProductId == s_AdvProductId && m_SubProductId == s_UKSubProductId;
    }

    public bool IsAWSOE_MYS()
    {
        return m_ProductId == s_AdvProductId && m_SubProductId == s_MYSubProductId;
    }

    public bool IsAWSOE_CHN()
    {
        return m_ProductId == s_AdvProductId && m_SubProductId == s_CHNSubProductId;
    }

    public bool IsAWSOE_ITA()
    {
        return m_ProductId == s_AdvProductId && m_SubProductId == s_ITASubProductId;
    }

    public bool IsEPI()
    {
        return m_ProductId == s_EPIProductId;
    }

    public bool IsMIT()
    {
        return m_ProductId == s_MITProductId;
    }

    public bool IsDirect_GLOBAL()
    {
        return m_SubProductId == s_DirectSubProductId;
    }

    public bool IsTBB()
    {
        return m_SubProductId == s_TBBSubProductId;
    }

    public bool IsCWP()
    {
        return m_ProductId == s_CWPProductId;
    }

    public bool IsCWP_US()
    {
        return m_ProductId == s_CWPProductId && m_RegionId != null && m_RegionId.ToUpper() == "ENU";
    }

    public bool IsCWP_UK()
    {
        return m_ProductId == s_CWPProductId && m_RegionId != null && m_RegionId.ToUpper() == "ENG";
    }

    public bool IsCWP_ITA()
    {
        return m_ProductId == s_CWPProductId && m_RegionId != null && m_RegionId.ToUpper() == "ITA";
    }

    public bool IsAU()
    {
        return m_SubProductId == s_AUSSubProductId;
    }

    public bool IsNZL()
    {
        return m_SubProductId == s_NZLSubProductId;
    }

    public bool IsUK()
    {
        return m_SubProductId == s_UKSubProductId;
    }

    public bool IsITA()
    {
        return m_SubProductId == s_ITASubProductId;
    }

    public bool IsMOP()
    {
        return m_SubProductId == s_MOPSubProductId;
    }

    public int GetDefaultPASDBServerId()
    {
        if (m_defaultPASDBServerId == 0) LoadDBServerId();
        return m_defaultPASDBServerId;
    }

    public int GetDefaultPositionDBServerId()
    {
        if (m_defaultPositionDBServerId == 0) LoadDBServerId();
        return m_defaultPositionDBServerId;
    }

    public int GetDefaultPositionDBServerIdDraft()
    {
        if (m_defaultPositionDBServerIdDraft == 0) LoadDBServerId();
        return m_defaultPositionDBServerIdDraft;
    }

    public int GetNewPositionDBServerId()
    {
        if (m_NewPosDBServerId == 0) LoadDBServerId();

        return m_NewPosDBServerId;
    }

    public int getNewPositionDBServerIdDraft()
    {
        if (m_NewPosDBServerIdDraft == 0) LoadDBServerId();
        return m_NewPosDBServerIdDraft;
    }

    public int GetStorageType()
    {
        if (DataTypeUtil.IsNull(m_StorageType)) LoadDBServerId();

        return m_StorageType;
    }

    public bool GetDisableConversionFlag()
    {
        UserProfileDB db = new UserProfileDB(this);
        db.SetCommand_Usr_GetPSLimit_V3(UserId, m_ProductId, Guid.Empty);
        using (IDataReader reader = db.ExecuteReader())
        {
            if (reader.Read()) return reader["PSDisableConvertPositionPortfolioFlag"].ToBool(false);
        }

        return false;
    }

    private void LoadDBServerId()
    {
        UserProfileDB db = new UserProfileDB(this);
        db.SetCommand_Usr_GetDBLink(UserId, m_ProductId);
        db.ExecuteReader(
            delegate(IDataReader reader)
            {
                m_defaultPASDBServerId = reader.GetValue(0).ToInt();
                if (GetDisableConversionFlag())
                {
                    m_defaultPositionDBServerId = reader["PosDBServerId"].ToInt();
                }
                else
                {
                    var status = reader["PSConvertPositionPortfolioStatus"].ToInt();
                    if (status == (int)PASConst.ConversionStatus.Default ||
                        status == (int)PASConst.ConversionStatus.Success)
                        m_defaultPositionDBServerId = reader["PorDBServerId"].ToInt();
                    else
                        m_defaultPositionDBServerId = reader["PosDBServerId"].ToInt();
                }

                m_NewPosDBServerId = reader["NewPosDBServerId"].ToInt();
                var storageType = reader["StorageType"].ToInt();
                if (!DataTypeUtil.IsNull(storageType)) m_StorageType = storageType;
            });

        const int DEFAULT_DRAFT_POS_DB_ID = 210;
        m_NewPosDBServerIdDraft = AppSettingsConfiguratoin.Instance
            .GetValue(AWDServerConfiguration.ProductMode, "DraftPosDBServerId").ToInt(DEFAULT_DRAFT_POS_DB_ID);
        m_defaultPositionDBServerIdDraft = AppSettingsConfiguratoin.Instance
            .GetValue(AWDServerConfiguration.ProductMode, "DraftPosDBServerId").ToInt(DEFAULT_DRAFT_POS_DB_ID);
        if (m_defaultPASDBServerId == 0)
            m_defaultPASDBServerId = 1;
        if (m_defaultPositionDBServerId == 0)
            m_defaultPASDBServerId = 1;
    }

    public bool IsRegion_CHS()
    {
        return m_RegionId == "CHS";
    }

    public IDictionary<Guid, Guid> GetDefaultRoles(RoleUnitCallback callback)
    {
        if (m_roleIds == null)
            m_roleIds = callback(this);

        return m_roleIds;
    }

    public void SetOfficeId(Guid officeId)
    {
        m_OfficeId = officeId;
    }

    public void SetUserId(Guid userId)
    {
        UserId = userId;
    }

    public void SetCreatorId(Guid creatorId)
    {
        CreatorId = creatorId;
    }

    public void HandleSecurityCheck()
    {
        throw new SvrException(ErrorInfo.s_SecurityCheckError, "Security Check Error");
    }

    public void LogOther(string msg, string subType = null, double ms = 0, params string[] additionalInfo)
    {
        if (TrackingLog != null) TrackingLog.AddLog("Others", subType, ms, msg, additionalInfo);
    }

    private static CacheControlObject GetCacheControlObject(bool noCache)
    {
        // disable cache for UK and non-admain/non-adtool servers
        if (IsAwgmainFarm || (!IsAdmainFarm && !IsAdtoolFarm)) return new CacheControlObject(true, true, 120);

        var globalCacheDisabled = false;
        var dpCacheDisabled = false;
        var dpCacheMinutes = 60;
        CacheControlObject obj = null;
        try
        {
            obj = CacheUtil.GetCache("ProductCacheControlObject", () =>
            {
                var cacheControl =
                    new Guid("767bfe60-4052-4880-83aa-1f9ecede0629"); // AMS ProductCacheControl EmailOwner
                var userDB = new UsersReadOnlyDB(s_DefaultEnviroment);
                userDB.SetCommand_GetMailBox(cacheControl);
                userDB.ExecuteReader(dr =>
                {
                    string fieldName = dr["MailboxName"].ToStr();
                    object fieldValue = dr["Email"];
                    switch (fieldName)
                    {
                        case "cache.global.disabled":
                            globalCacheDisabled = fieldValue.ToBool(globalCacheDisabled);
                            break;
                        case "cache.dp.disabled":
                            dpCacheDisabled = fieldValue.ToBool(dpCacheDisabled);
                            break;
                        case "cache.dp.minutes":
                            dpCacheMinutes = fieldValue.ToInt(dpCacheMinutes);
                            break;
                    }
                });
                return new CacheControlObject(globalCacheDisabled, dpCacheDisabled, dpCacheMinutes);
            }, 5) as CacheControlObject;
        }
        catch (Exception ex)
        {
            ExceptionHandler.DefaultHandleException(ex);
        }

        if (obj != null)
        {
            globalCacheDisabled = noCache || obj.GlobalCacheDisabled;
            dpCacheDisabled = noCache || obj.DpCacheDisabled;
            dpCacheMinutes = obj.DpCacheMinutes;
        }

        return new CacheControlObject(globalCacheDisabled, dpCacheDisabled, dpCacheMinutes);
    }

    #region Debug region

    public void SetDataPath()
    {
        var path = string.Format("{0}{1}", ApplicationPath, s_DataPath);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public void SetErrorMessage(float millseconds, string msg)
    {
        if (s_CanLog) WriteRawMessage(string.Format("{0}Total time spend {1} ms.", msg ?? "", millseconds));
    }

    private void WriteRawMessage(string message)
    {
        // In lieu of Console.WriteLine here, add console appender to logging config as done in ServerApp and set s_CanLog = true.
        if (s_CanLog) CommonLogger.LogMessage(message, EventStatus.info, EventType.info);
    }

    [Conditional("DEBUG")]
    public void WriteDebugMessage(string traceMessage)
    {
        WriteMessage(traceMessage);
    }

    public void LockAndWriteMessage(string traceMessage)
    {
        lock (m_lock)
        {
            WriteMessage(traceMessage);
        }
    }

    public void LockAndWriteTimer(float millSeconds, string info)
    {
        lock (m_lock)
        {
            WriteTimer(millSeconds, info);
        }
    }

    public void WriteMessage(string traceMessage)
    {
        var message = string.Format("[{0} TRACE] - {1}", Timer.GetExactNowString(), traceMessage);
        WriteRawMessage(message);
    }

    [Conditional("DEBUG")]
    public void WriteDebugTimer(float millSeconds, string info)
    {
        WriteTimer(millSeconds, info);
    }

    public void WriteTimer(float millSeconds, string info)
    {
        var message = string.Format("[{0} TIMER] - {1} ms. {2}", Timer.GetExactNowString(),
            DataTypeUtil.Format(millSeconds, 0, 5), info);
        WriteRawMessage(message);
    }

    public void WriteMemoryInfo(string message)
    {
        WriteMessage(string.Format("{0} : Total Virtual Memory Size {1} KB", message,
            Process.GetCurrentProcess().PagedMemorySize64 / 1024));
    }

    public void WriteNewLine()
    {
        WriteRawMessage(string.Empty);
    }

    public static void SaveTextToFile(string filePathName, string data, FileMode mode)
    {
        var datas = s_DefaultEncoding.GetBytes(data);
        using (Stream s = new FileStream(filePathName, mode, FileAccess.Write, FileShare.ReadWrite))
        {
            s.Write(datas, 0, datas.Length);
        }
    }

    #endregion
}

public class TrackingLog : IDisposable
{
    public const int s_postXmlLimit = 4096;
    private readonly AWDEnvironment m_env;
    private readonly object m_lock = new();
    private readonly MemoryStream m_mSteam;
    private readonly Guid m_requestId;
    private readonly string m_startTime;
    private readonly string m_url;
    private double m_DBTime;
    private ServerSiteLogger m_ServerSiteLogger;
    private XmlTextWriter m_writer;
    private GZOutputStream m_zipSteam;

    public TrackingLog(AWDEnvironment env, HttpRequest req, byte[] postData, bool isLogRequest)
    {
        try
        {
            var requestId = DataTypeUtil.ObjectGuidValue(req.Headers["ETag"]);
            if (DataTypeUtil.IsNull(requestId))
            {
                var enableETag = DataTypeUtil.ObjectBoolValue(req.QueryString["EnableETag"]);
                if (enableETag)
                    requestId = req.QueryString["RequestId"].ToGuid();
            }

            if (!DataTypeUtil.IsNull(requestId) && (!AWDEnvironment.DisableTrackingLog || isLogRequest))
                EnableTracking = true;
            else
                EnableTracking = false;

            m_env = env;
            m_requestId = requestId;
            m_url = StringUtil.CheckScriptInjection(req.Url.PathAndQuery);
            m_startTime = Timer.GetExactNowString();
            m_ServerSiteLogger = new ServerSiteLogger(env, req);

            if (EnableTracking)
            {
                m_mSteam = new MemoryStream(4096);
                m_zipSteam = new GZOutputStream(m_mSteam);
                m_writer = new XmlTextWriter(m_zipSteam, AWDEnvironment.s_DefaultEncoding);

                m_writer.WriteStartElement("req");
                m_writer.WriteAttributeString("id", m_requestId.ToString());
                m_writer.WriteAttributeString("machine", Environment.MachineName);
                m_writer.WriteElementString("url", m_url);
                if (postData != null)
                    m_writer.WriteElementString(
                        "post",
                        postData.Length > s_postXmlLimit
                            ? AWDEnvironment.s_DefaultEncoding.GetString(postData, 0, s_postXmlLimit)
                            : AWDEnvironment.s_DefaultEncoding.GetString(postData));
            }

            m_ServerSiteLogger.LogAsync("RequestStart", "", m_url);
        }
        catch (Exception ex)
        {
            ExceptionHandler.DefaultHandleException(ex);
        }
    }

    public TrackingLog(AWDEnvironment env, Guid requestId, string objectId)
    {
        try
        {
            EnableTracking = true;

            m_url = objectId;
            m_env = env;
            m_requestId = requestId;

            m_ServerSiteLogger = new ServerSiteLogger(env, null);
            m_ServerSiteLogger.LogAsync("RequestStart", "", m_url);
        }
        catch (Exception ex)
        {
            ExceptionHandler.DefaultHandleException(ex);
        }
    }

    public TrackingLog(AWDEnvironment environment, bool enableTracking)
    {
        try
        {
            var requestId = environment.RequestId.ToGuid();
            EnableTracking = enableTracking;

            m_env = environment;
            m_requestId = requestId;
            m_url = string.Empty;
            m_startTime = Timer.GetExactNowString();

            if (EnableTracking)
            {
                m_mSteam = new MemoryStream(4096);
                m_zipSteam = new GZOutputStream(m_mSteam);
                m_writer = new XmlTextWriter(m_zipSteam, AWDEnvironment.s_DefaultEncoding);

                m_writer.WriteStartElement("req");
                m_writer.WriteAttributeString("id", m_requestId.ToString());
                m_writer.WriteAttributeString("machine", Environment.MachineName);
                m_writer.WriteElementString("url", m_url);
            }
        }
        catch (Exception ex)
        {
            ExceptionHandler.DefaultHandleException(ex);
        }
    }

    public Guid RequestId => m_requestId;

    public bool EnableTracking { get; set; }

    public void Dispose()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (m_writer != null)
        {
            try
            {
                m_writer.Flush();
                m_writer.Close();
            }
            catch
            {
            }

            m_writer = null;
        }

        if (m_zipSteam != null)
        {
            try
            {
                m_zipSteam.Dispose();
            }
            catch
            {
            }

            m_zipSteam = null;
        }
    }


    public void AddLog(string type, string subtype, double millSeconds, string msg, params string[] additionalInfo)
    {
        if (type == "DB")
            m_DBTime += millSeconds;
        try
        {
            lock (m_lock)
            {
                if (m_writer != null)
                {
                    m_writer.WriteStartElement("log");
                    XmlUtil.WriteAttribute(m_writer, "id", Guid.NewGuid());
                    XmlUtil.WriteAttribute(m_writer, "time", Timer.GetExactNowString());
                    XmlUtil.WriteAttribute(m_writer, "type", type);
                    XmlUtil.WriteAttribute(m_writer, "subtype", subtype);
                    XmlUtil.WriteAttribute(m_writer, "ms", (int)millSeconds);
                    msg = msg.Replace("\0",
                        ""); //fix bug for Unhandled Exception: System.Xml.XmlException: '.', hexadecimal value 0x00, is an invalid character.
                    XmlUtil.WriteAttribute(m_writer, "msg", msg);
                    if (additionalInfo != null)
                        for (var i = 0; i < additionalInfo.Length; i++)
                            XmlUtil.WriteAttribute(m_writer, "a" + i, additionalInfo[i]);
                    m_writer.WriteEndElement();
                }

                if (m_ServerSiteLogger != null) m_ServerSiteLogger.LogAsync(type, subtype, msg);
            }
        }
        catch (Exception ex)
        {
            ExceptionHandler.DefaultHandleException(ex);
        }
    }

    public void Finish(double totalMillSeconds, bool enablePerformanceBaseLine)
    {
        try
        {
            lock (m_lock)
            {
                if (m_writer != null)
                {
                    m_writer.WriteStartElement("timer");
                    m_writer.WriteAttributeString("starttime", m_startTime);
                    m_writer.WriteAttributeString("endtime", Timer.GetExactNowString());
                    m_writer.WriteAttributeString("ms", DataTypeUtil.Format((int)totalMillSeconds));
                    m_writer.WriteEndElement();
                    m_writer.WriteEndElement();
                    m_writer.Flush();
                    m_writer.Close();
                    m_zipSteam.Close();
                    m_mSteam.Close();
                    m_writer = null;
                    m_zipSteam = null;
                    var datas = m_mSteam.ToArray();
                    datas = CryptographyHelper.DESEncrypt(datas);
                    m_env.TrackingLog = null; // no more logging

                    var db = new TrackingDataDB(m_env);
                    db.Execute_RQT_SaveRequestLog(RequestId, m_env.UserId, DateTime.Now, m_url, datas);
                }

                if (m_ServerSiteLogger != null) m_ServerSiteLogger.LogAsync("RequestEnd", null, null);
            }

            if (enablePerformanceBaseLine)
            {
                var url = m_url;
                var index = url.IndexOf('?');
                if (index > 0)
                    url = url.Substring(0, index);

                new PerformanceBaseLineDB(m_env)
                    .Execute_Per_InsertPerformanceBaseLine(RequestId, url, Environment.MachineName, m_DBTime,
                        totalMillSeconds, 0);
            }
        }
        catch (Exception ex)
        {
            ExceptionHandler.DefaultHandleException(ex);
        }
    }
}
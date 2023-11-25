using System;
using System.Text;
using System.Xml;

namespace PullFinanceData.Util
{
    public class ErrorInfo
    {
        #region register error 650000 - 659999

        public const int s_NoPromotionInfo = 650001;

        #endregion

        public static XmlDocument s_noErrorDom = GetBaseErrorInfoDom();

        public static XmlDocument GetBaseErrorInfoDom()
        {
            var dom = new XmlDocument();
            XmlUtil.AddElement(dom, "awd");
            return dom;
        }

        public static XmlElement AddError(XmlDocument doc, int errCode)
        {
            if (doc.DocumentElement == null)
                XmlUtil.AddElement(doc, "awd");
            var errorElem = XmlUtil.AddElement(doc.DocumentElement, "error");
            XmlUtil.SetAttribute(errorElem, "code", errCode.ToString());
            return errorElem;
        }

        public static XmlElement AddError(XmlDocument doc, int errCode, string message)
        {
            if (doc.DocumentElement == null)
                XmlUtil.AddElement(doc, "awd");
            var errorElem = XmlUtil.AddElement(doc.DocumentElement, "error");
            XmlUtil.SetAttribute(errorElem, "code", errCode.ToString());
            XmlUtil.SetAttribute(errorElem, "debugmsg", message);
            return errorElem;
        }

        public static void AddError(XmlWriter writer, int errCode, string message)
        {
            writer.WriteStartElement("error");
            XmlUtil.WriteAttribute(writer, "code", errCode);
            XmlUtil.WriteAttribute(writer, "debugmsg", message);
            writer.WriteEndElement(); //error
        }

        public static void AddError(XmlWriter writer, string errCode, string message)
        {
            writer.WriteStartElement("error");
            XmlUtil.WriteAttribute(writer, "code", errCode);
            XmlUtil.WriteAttribute(writer, "codemsg", message);
            writer.WriteEndElement(); //error
        }

        public static void SetClientId(XmlElement errorElem, Guid clientId)
        {
            XmlUtil.SetAttribute(errorElem, "clientid", clientId);
        }

        public static void SetAccountId(XmlElement errorElem, Guid accountId)
        {
            XmlUtil.SetAttribute(errorElem, "accoutid", accountId);
        }

        public static void SetHoldingId(XmlElement errorElem, Guid holdingId)
        {
            XmlUtil.SetAttribute(errorElem, "holdingid", holdingId);
        }

        public static void SetTransactionId(XmlElement errorElem, int transactionId)
        {
            XmlUtil.SetAttribute(errorElem, "tid", transactionId);
        }

        public static string ReportError(int ErrNumber, string ErrDescription)
        {
            return ReportError(ErrNumber, DataTypeUtil.IntNullValue, ErrDescription);
        }

        public static string ReportError(int ErrNumber, int BusErrNumber, string ErrDescription)
        {
            var sErrXml = new StringBuilder();
            sErrXml.Append("<Error>");
            sErrXml.Append("<Code>");
            sErrXml.Append(ErrNumber);
            sErrXml.Append("</Code>");
            sErrXml.Append("<ErrMsg>");
            sErrXml.Append(XmlUtil.EncodeXmlString(ErrDescription, true, false, false));
            sErrXml.Append("</ErrMsg>");
            if (!DataTypeUtil.IsNull(BusErrNumber))
            {
                sErrXml.Append("<BusinessCode>");
                sErrXml.Append(BusErrNumber);
                sErrXml.Append("</BusinessCode>");
            }

            sErrXml.Append("</Error>");

            return sErrXml.ToString();
        }

        public static string GetErrorXml(int nErrorCode)
        {
            var sErrXml = new StringBuilder();
            sErrXml.Append("<error code='").Append(DataTypeUtil.ObjectStringValue(nErrorCode)).Append("' codemsg=''/>");
            return sErrXml.ToString();
        }

        public static string GetCodeMsg(int nErrorCode)
        {
            return "";
        }

        public static void SetPositionId(XmlElement errorElem, long positionId)
        {
            XmlUtil.SetAttribute(errorElem, "positionid", positionId);
        }

        #region application error 600000  - 699999

        public const int s_GenericException = 600000;
        //public const int s_GenericError = 600001;
        //public const int s_SecurityCheckError = 600002;
        //public const int s_NoCacheData = 600003;
        //public const int s_PrimaryClassException = 600004;
        //public const int s_PrivateIndexHoldingPermission = 600254;
        //public const int s_InvalidLoginPassword = 600255;
        //public const int s_NoSrcIdInUsageLog = 600303;
        //public const int s_NoFirmIdByUserId = 600304;
        //public const int s_BadBinary = 600305;
        //public const int s_NotMatchViewType = 600306;
        //public const int s_NoConfiguration = 600307;
        //public const int s_NoSecurityId = 600308;
        //public const int s_NoRoleId = 600309;
        //public const int s_InvalidTokenError = 600310;
        //public const int s_InvalidDate = 600311;

        //public const int s_InvalidUserDefaultSetting = 600401;

        //public const int s_MQMessageSendError = 600909;
        //public const int s_NoEmail = 600911;

        #region Account/Holding/Transaction/Group

        //public const int s_NoAccount = 610001;
        //public const int s_NotTaxAccount = 610002;
        //public const int s_LOTMATCHERROR = 610003;
        //public const int s_SHAREMATCHERROR = 610004;
        //public const int s_DATEMATCHERROR = 610005;
        //public const int s_NoTransaction = 610006;
        //public const int s_NoBuyTrans = 610007;
        //public const int s_SpecifiedMatching = 610008;
        //public const int s_DividendNoShares = 610009;
        //public const int s_ZeroQuantity = 610010;
        //public const int s_MissingSplit = 610011;
        //public const int s_LongShortUpdate = 610012;
        //public const int s_FMCashNotEnough = 610013;
        //public const int s_DuplicateTrans = 610014;
        //public const int s_SecTranFailed = 610015;
        //public const int s_NoWritePermission = 610016;
        //public const int s_UpdateTranAcctHlg = 610017;
        //public const int s_RequireTranAcct = 610018;
        //public const int s_DeleteDefaultSetting = 610019;
        //public const int s_DuplicateGroupName = 610020;
        //public const int s_CFAGMapping = 610021;
        //public const int s_ManuallUpdateCashTrans = 610022;
        //public const int s_LinkTransactionError = 610023;
        //public const int s_RelinkAccountFail = 610024;
        //public const int s_FeePercentageInvalid = 610025;
        //public const int s_RaiseCashExceedsTotal = 610026;
        //public const int s_RaiseCashNoAction = 610027;
        //public const int s_HasSpecialTaxlotMatching = 610028;
        //public const int s_EditDateIsTooEarly = 610029;
        //public const int s_EditDateIsTooLate = 610030;
        //public const int s_NoReverseTransaction = 610031;
        //public const int s_ReverseNotMatch = 610032;
        //public const int s_CanNotConvertAccount = 610033;
        //public const int s_UpdateCloseTransaction = 610034;
        //public const int s_SaveSecurityExclusion = 610035;
        //public const int s_DeleteSecurityExclusion = 610036;
        //public const int s_SaveRepMap = 610037;
        //public const int s_DeleteRepMap = 610038;
        //public const int s_DuplicateAccountGroupName = 610039;
        //public const int s_SaveBlockAccountSetting = 610040;
        //public const int s_DeleteBlockAccountSetting = 610041;
        //public const int s_DuplicateBlockAccountSetting = 610042;
        //public const int s_DuplicatePortfolioID = 610043;
        //public const int s_RemoveCompositAccountAfterCloseDate = 610044;
        //public const int s_InvalidImportedTransactionDate = 610045;
        //public const int s_NoOpenTransToSplit = 610046;
        //public const int s_InvalidGroupTypeId = 610047;

        #endregion


        #region Billing

        //public const int s_BillingAssetLess0 = 620001;
        //public const int s_NoManagementFeeSetting = 620002;
        //public const int s_NotIncludeManagementFee = 620003;
        //public const int s_NoBillingBasedOnTypeId = 620004;
        //public const int s_UpdateBillingStorageIdError = 620005;
        //public const int s_DuplicateManagementFeeSetting = 620006;

        #endregion

        #region CRM

        //public const int s_ARGUMENTERROR = 630001;
        //public const int s_MailBoxSizeExceeded = 630002;
        //public const int s_NoDeletePermission = 630003;
        //public const int s_ItemNotFound = 630004;
        //public const int s_ErrorSpouse = 630005;
        //public const int s_ErrorTransferClient = 630006;
        //public const int s_EmailNotFound = 630007;
        //public const int s_CWPOEUser = 630008;
        //public const int s_CWPClientLoginExist = 630009;
        //public const int s_CWPFailCreateLogin = 630010;
        //public const int s_InvalidClientId = 630011;
        //public const int s_InvalidEmail = 630012;
        //public const int s_ErrorSaveCWPMessage = 630013;
        //public const int s_ErrorPostReportPermission = 630014;
        //public const int s_NoMergeClientPermission = 630015;
        //public const int s_ErrorLinkReport = 630016;
        //public const int s_SingleMemberAccountOwner = 630017;
        //public const int s_MultipleMemberAccountOwner = 630018;
        //public const int s_DeleteMultipleMemberAccountOwner = 630036;
        //public const int s_FailDeletePrimaryMember = 630019;
        //public const int s_InvalidPasswordFormat = 630020;

        //public const int s_InvalidPasswordBackList = 630120;

        //copy from BPO server side, it is duplicate case
        //public const int s_ClientHasAccount = 630021;
        //public const int s_CWPEmailExist = 630021;
        //public const int s_ErrorClientDeleted = 630022;
        //public const int s_PostAccountBeforeClient = 630023;
        //public const int s_FailToLocateUser = 630024;

        //public const int s_ErrorSaveReportFile = 630025;
        //public const int s_ErrorGetReportFile = 630026;
        //public const int s_ErrorDeleteReportFile = 630027;
        //public const int s_ErrorNoPermission = 630028;
        //public const int s_ErrorInvalidUser = 630029;
        //public const int s_ErrorMaxAssistants = 630030;
        //public const int s_ErrorNoOffice = 630031;
        //public const int s_ErrorNotInSameOffice = 630032;
        //public const int s_ErrorAddAddAssistant = 630033;
        //public const int s_ErrorDuplicateClientName = 630034;
        //public const int s_ErrorMoveMember2Client = 630035;

        //  udf
        //public const int s_ErrorExccedUdfLimitation = 630040;
        //public const int s_ErrorFailedToUpdateUdf = 630041;
        //public const int s_ErrorFailedToAddNewUdf = 630042;
        //public const int s_ErrorFailedToDeleteUdf = 630043;
        //public const int s_ErrorUpdateNonExistingUdf = 630044;
        //public const int s_ErrorChangeUdfType = 630045;
        //public const int s_ErrorDeleteNonExistingUdf = 630046;
        //public const int s_ErrorUpdateUndefinedUdf = 630047;
        //public const int s_ErrorDuplicateUdfName = 630048;
        //public const int s_ErrorInvalidInputUdf = 630049;
        //public const int s_ErrorInvalidInputUdfCategory = 630050;
        //public const int s_ErrorFailedToUpdateUdfCategory = 630051;
        //public const int s_ErrorFailedToAddNewUdfCategory = 630052;
        //public const int s_ErrorFailedToDeleteUdfCategory = 630053;
        //public const int s_ErrorDeleteNonExistingUdfCategory = 630054;
        //public const int s_ErrorUpdateNonExistingUdfCategory = 630055;
        //public const int s_ErrorInvalidInputUdfValue = 630056;
        //public const int s_ErrorUpdateNonExistingUdfTask = 630057;

        //public const int s_ErrorUpdateNonConsistentTask = 630058;

        //  duplicate CWP email, used to escape 65001 that returned by Users.Member_InsertLogin
        //public const int s_ErrorCWPDuplicate = 630059;
        //public const int s_ErrorDeleteNonExistingUdfValue = 630060;

        //public const int s_ErrorMergeSourceCWPTargetNoCWP = 630061;
        //public const int s_ErrorMergeSourceCWPTargetCWP = 630062;
        //public const int s_ErrorUpdateDuplicateNameUdf = 630063;
        //public const int s_ErrorUpdateDuplicateNameCategory = 630064;
        //public const int s_ErrorExccedUdfCategoryItemLimitation = 630065;

        //Smart Search
        //public const int s_ErrorSmartSearchAddUpdateException = 630200;

        // Global login 
        //public const int s_GLErrorStart = 630100;
        //public const int s_GLErrorEmptyPwd = 630101;
        //public const int s_GLErrorEmptyLogin = 630102;
        //public const int s_GLErrorLoginType = 630103;
        //public const int s_GLErrorEmailAddr = 630107;
        //public const int s_GLErrorInvalidUser = 630108;
        //public const int s_GLErrorInvalidServiceClient = 630118;
        //public const int s_GLErrorResetPassword = 630119;

        //public const int s_ErrorAssignClientToPostedReport = 10860;

        #endregion

        //public const int s_PrimaryMember1 = 652001;
        //public const int s_PrimaryMember2 = 652002;
        //public const int s_UpdateMember = 652003;
        //public const int s_MergeClient = 652004;
        //public const int s_MaxAssetAllocation = 656001;
        //public const int s_AssetForRiskPlan = 656010;


        #region calculation error 670000 - 679999

        //public const int s_CalcTotalReturnIndex = 670001;
        //public const int s_AccumulateSplitFactor = 670002;
        //public const int s_CalcAdjustedPrice = 670002;

        #endregion

        #endregion

        #region database error 60000  - 69999

        #region PasAccounts database error 60000 - 64999

        //public const int s_DelHldRefSI = 60000;
        //public const int s_DelHlgRefReb = 60001;
        //public const int s_AccountNotInDB = 60002;
        //public const int s_NoAveragePriceAccount = 60003;
        //public const int s_DatabaseError = 60004;
        //public const int s_PermissionDenied = 60005;
        //public const int s_StoredProcedureFailed = 60006;

        #endregion

        #region PasSecurities database error 65000 - 69999

        //public const int s_DupCode = 65001;
        //public const int s_InvBondSchType = 65002;
        //public const int s_DeleteReferenceCheck = 65003;
        //public const int s_MergeReferenceCheck = 65004;

        #endregion

        #region database generic error 690000  - 699999

        //public const int s_DBGenericException = 699999;

        #endregion

        #endregion

        #region research error 640000 - 649999

        //public const int s_NoSupportMI = 640001;
        //public const int s_NoWhereClause = 640002;
        //public const int s_NoDataTransit = 640003;

        #endregion

        #region time series data error 660000 - 669999

        //public const int s_NoSecurityInfo = 660000;
        //public const int s_DeleteTransactionFailed = 660001;
        //public const int s_AddTransactionFailed = 660002;
        //public const int s_ExistedRelatedAccount = 660003;
        //public const int s_UpdateTransactionFailed = 660004;

        #endregion

        #region Quote Speed Error 680000 - 689999

        //public const int s_GetQSHoldingFailed = 680001;
        //public const int s_NoSecIdAndTicker = 680002;
        //public const int s_ErrorFormat = 680003;
        //public const int s_DeleteQSHoldingFailed = 680004;
        //public const int s_UpdateQSHoldingFailed = 680005;

        #endregion

        #region Share With OE Error 690000-699999

        //public const int s_SomeUserNotExist = 690001;
        //public const int s_AllUserNotExist = 690002;
        //public const int s_GetHoldingFailed = 690003;
        //public const int s_SomeHoldingNotExist = 690004;
        //public const int s_AllHoldingNotExist = 690005;
        //public const int s_SomeShareFailed = 690006;
        //public const int s_AllShareFailed = 690007;
        //public const int s_HasUserDefineScurity = 690008;

        #endregion

        #region MOP data error

        //public const int s_MOPAccountCacheJob_ExceedMaxThread = 700101;
        //public const int s_MOPAccountCacheJob_InProgress = 700102;
        //public const int s_MOPAccountCacheJob_RefreshError = 700110;
        //public const int s_MOPAccountCacheQuery_GetAccountIdByAccountNumber = 700201;
        //public const int s_MOPAccountCacheQuery_NoAccountIdByAccountNumber = 700202;
        //public const int s_MOPAccountCacheQuery_MulitpleAccountIdByAccountNumber = 700203;
        //public const int s_MOPAccountCacheQuery_UpdateMOPAccountCacheByAccountId = 700204;
        //public const int s_MOPAccountLink_UpdateReferenceIdError = 700301;
        //public const int s_MOPAccountLink_GetMISDataError = 700302;

        #endregion

        #region Model publish error

        //public const int s_ModelPublish_BadRequest = 701000;
        //public const int s_ModelPublish_DuplicatedInitialJob = 701001;
        //public const int s_ModelPublish_TheModelPublishHasBeenApproved = 701002;
        //public const int s_ModelPublish_TheModelPublishHasBeenDeactivated = 701003;
        //public const int s_ModelPublish_TheModelPublishHasNotBeenInitialied = 701004;
        //public const int s_ModelPublish_ThePortfolioTypeIsNotModel = 701005;

        #endregion

        #region Direct Lens errors 702000-703000

        //public const int s_UnsupportedStorageType = 702000;
        //public const int s_SaveDraftSecurityFailed = 702001;
        //public const int s_SaveDraftAccountFailed = 702002;
        //public const int s_DeleteRealPortfolioViaInvalidEndpoint = 702003;
        //public const int s_DraftAccountNotSupported = 702004;

        #endregion
    }
}
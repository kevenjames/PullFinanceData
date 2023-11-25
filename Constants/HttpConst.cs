using System.Collections.Generic;

namespace PullFinanceData.Constants
{
    public static class HttpConst
    {
        public const int ScriptTimeout = 300;
        //public const string LoginPage = "/login/awsLogin.aspx?retXML=yes&ccSession=1&impadvisorid={0}&skout=1&redirectsrc=BasePage";
        //public const string MITLoginPage = "/login/awsLogin.aspx?retXML=yes&ccSession=1&productcode={0}&redir={1}&redirectsrc=BasePage";
        //public const string WebLoginPage = "/login/awsLogin.aspx?ProductCode={0}&redirectsrc=BasePage&redir={1}";
        //public const string WebLoginKickoutPage = "/login/awsLogin.aspx?ccSession=2&ProductCode={0}&redirectsrc=BasePage&redir={1}";
        public const string ContentLengthHeader = "Content-Length";
        public const string TransferEncodingHeader = "transfer-encoding";
        public const string AcceptEncodingHeader = "Accept-Encoding";

        public const string ContentTypeBinary = "application/pdf";
        public const string ContentTypeZip = "application/zip";
        public const string ContentTypeXml = "text/xml";
        public const string ContentTypeHtml = "text/html";
        public const string ContentTypeJson = "text/json";
        public const string ContentTypeCSS = "text/css";

        public const string ContentTypeText = "text/plain";
        public const string ContentTypeXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string ContentTypeCsv = "text/csv";
        public const string ContentTypeDownload = "application/octet-stream";

        public const string CharsetUTF8 = "utf-8";

        public const string CSRFReferer = "HTTP_REFERER";
        public const string RefererErrorMessageFormat = "The CSRF attack came from: {0}, Time is {1}; ";
        //public static readonly List<string> RefererDirectURLs = new List<string>() { "morningstar.com", "localhost:49992" };
        //public static readonly List<string> RefererOfficeURLs = new List<string>() { "morningstar.com", "localhost:49991" };

        public const int APIGeeTokenTimeout = 720;
    }
}
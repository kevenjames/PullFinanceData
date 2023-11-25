using log4net;
using PullFinanceData.DataObj;
using PullFinanceData.Log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using PullFinanceData.Constants;
using PullFinanceData.DataObj;

namespace PullFinanceData.Util
{
    public class HttpUtil
    {
        /// <summary>
        /// HttpResponseBase is easier to mock. For HttpResponse, only Output can be replaced
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="response"></param>
        /// <param name="bArr"></param>
        public static void WriteBinaryData(AWDEnvironment environment, HttpResponse response, byte[] bArr)
        {
            WriteBinaryData(environment, new HttpResponseWrapper(response), bArr);
        }

        public static void WriteBinaryData(AWDEnvironment environment, HttpResponseBase response, byte[] bArr)
        {
            if (bArr != null && bArr.Length > 0)
            {
                var timer = new Timer();
                response.AddHeader(HttpConst.ContentLengthHeader, bArr.Length.ToString());

                if (response.Output != null && response.Output is StringWriter)
                {
                    string text = AWDEnvironment.s_DefaultEncodingNoBOM.GetString(bArr);
                    response.Output.Write(text);
                }
                else
                {
                    response.OutputStream.Write(bArr, 0, bArr.Length);
                }

                timer.Stop();
                if (environment != null)
                {
                    environment.WriteDebugTimer(
                        timer.ElapsedMillSeconds,
                        "Finish Response Data " + bArr.Length + " in " + (int)timer.ElapsedMillSeconds + " ms.");
                    if (environment.TrackingLog != null)
                        environment.TrackingLog.AddLog("Response", "Complete", timer.ElapsedMillSeconds, bArr.Length.ToString());
                }
            }
        }

        public static byte[] GetResponseData(AWDEnvironment environment, string url)
        {
            return GetResponseData(environment, url, 0);
        }

        public static byte[] GetResponseData(AWDEnvironment environment, string url, int timeout)
        {
            return GetResponseData(environment, url, null, null, timeout);
        }

        public static byte[] GetResponseData(AWDEnvironment environment, string url, object postData)
        {
            return GetResponseData(environment, url, postData, null);
        }

        public static byte[] GetResponseData(AWDEnvironment environment, string url, object postData, bool unzipdata)
        {
            byte[] gzipheader;
            return GetResponseData(environment, url, postData, null, out gzipheader, unzipdata);
        }

        public static byte[] GetResponseData(AWDEnvironment environment, string url, object postData,
                                             IHttpSession httpSession)
        {
            return GetResponseData(environment, url, postData, httpSession, 0);
        }

        public static byte[] GetResponseData(AWDEnvironment environment, string url, object postData,
                                             IHttpSession httpSession, int timeout)
        {
            byte[] gzipheader;
            return GetResponseData(environment, url, postData, httpSession, out gzipheader, true, timeout);
        }

        private static readonly CommonLogger commonLogger = new CommonLogger();
        public static XmlDocument GetDataByURL(AWDEnvironment environment, String url, string requestData = null, string httpMethod = "POST", string contentType = null)
        {
            var apiKey = AppConfiguration.Instance.GetValue(ServerConfiguration.DBProductMode, "CustomerAPIKey");
            var apiSecret = AppConfiguration.Instance.GetValue(ServerConfiguration.DBProductMode, "CustomerAPISecret");
            string apiToken = GetAPIGeeAccessToken(apiKey, apiSecret);

            Dictionary<string, string> header = new Dictionary<string, string>()
            {
                {"X-API-UserId", environment.UserId.ToStr()},
                {"IID", environment.IID.ToStr()},
                {"PCODE", environment.ProductCode},
                {"accept", "application/xml"},
                {"Authorization", "Bearer " + apiToken}
            };

            if (!string.IsNullOrEmpty(contentType))
            {
                header.Add("content-type", contentType);
            }

            if (environment.TrackingLog != null && environment.TrackingLog.EnableTracking)
            {
                string requestId = Guid.NewGuid().ToString();

                environment.TrackingLog.AddLog("LoadWebPage", "Url", 0, url + "#RequestId=" + requestId);

                header.Add("RequestId", requestId);
                header.Add("EnableTracking", environment.TrackingLog.EnableTracking.ToString());

                commonLogger.LogMessage(string.Format("Call CustomerApi Key requestId='{0}',EnableTracking='{1}'", requestId, environment.TrackingLog.EnableTracking.ToStr()), EventStatus.info, EventType.info);
            }

            byte[] bytes = HttpUtil.GetResponseDataWithHeader(environment, url, requestData, header, httpMethod);

            XmlDocument xmlDoc = XmlUtil.LoadXml(bytes);
            return xmlDoc;
        }

        public static XmlDocument GetResponseDataWithApiKey(AWDEnvironment environment, string url, string apiKey, string postData = null, string httpMethod = "POST")
        {
            Dictionary<string, string> header = new Dictionary<string, string>()
            {
                {"X-API-UserId", environment.UserId.ToStr()},
                {"X-API-ProductId", environment.ProductId.ToStr()},
                {"X-API-RequestId", Guid.NewGuid().ToStr()},
                {"accept", "application/xml"},
                {"content-type", "application/xml"},
                {"ApiKey", apiKey}
            };

            if (environment.TrackingLog != null && environment.TrackingLog.EnableTracking)
            {
                header.Add("EnableTracking", environment.TrackingLog.EnableTracking.ToString());
            }

            byte[] bytes = GetResponseDataWithHeader(environment, url, postData, header, httpMethod);
            XmlDocument xmlDoc = XmlUtil.LoadXml(bytes);

            return xmlDoc;
        }

        public static XmlDocument GetAPIGeeDataByURL(AWDEnvironment environment, String url, string requestData = null, string httpMethod = "POST")
        {
            bool isLiveKeyOnTP = false;
            var mode = ServerConfiguration.ProductMode;
            if (mode == ProductMode.PatchTP)
                mode = ProductMode.TP;

            if ((mode == ProductMode.TP || mode == ProductMode.PatchTP) && isLiveKeyOnTP)
                mode = ProductMode.LIVE;

            var apikey = AppConfiguration.Instance.GetValue(mode, "DirectAPIKey");
            var apisecret = AppConfiguration.Instance.GetValue(mode, "DirectAPISecret");
            string api_token = GetAPIGeeAccessToken(apikey, apisecret);


            Dictionary<string, string> header = new Dictionary<string, string>()
            {
                {"UID", environment.UserId.ToStr()},
                {"IID", environment.IID.ToStr()},
                {"PCODE", environment.ProductCode},
                {"accept", "application/xml"},
                {"Authorization", "Bearer " + api_token}
            };

            byte[] bytes = HttpUtil.GetResponseDataWithHeader(environment, url, requestData, header, httpMethod);
            XmlDocument xmlDoc = XmlUtil.LoadXml(bytes);
            return xmlDoc;
        }

        public static Dictionary<string, string> GetTokenHeader(string key = "DirectAPIKey", string secret = "DirectAPISecret")
        {
            bool isLiveKeyOnTP = false;
            var mode = ServerConfiguration.ProductMode;
            if (mode == ProductMode.PatchTP)
                mode = ProductMode.TP;

            if ((mode == ProductMode.TP || mode == ProductMode.PatchTP) && isLiveKeyOnTP)
                mode = ProductMode.LIVE;

            var apikey = AppConfiguration.Instance.GetValue(mode, key);
            var apisecret = AppConfiguration.Instance.GetValue(mode, secret);
            string api_token = HttpUtil.GetAPIGeeAccessToken(apikey, apisecret);


            Dictionary<string, string> header = new Dictionary<string, string>()
            {
                {"Authorization", "Bearer " + api_token}
            };

            return header;
        }

        /// <summary>
        /// Get binary data from web link.
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="httpSession"></param>
        /// <param name="gzipHeader"></param>
        /// <param name="unzipdata"></param>
        /// <returns></returns>
        public static byte[] GetResponseData(AWDEnvironment environment, string url, object postData,
                                             IHttpSession httpSession, out byte[] gzipHeader, bool unzipdata)
        {
            return GetResponseData(environment, url, postData, httpSession, out gzipHeader, unzipdata, 0);
        }

        public static byte[] GetResponseDataWithHeader(AWDEnvironment environment, string url, object requestData, Dictionary<string, string> header,
             string httpMethod = "POST", int timeOut = 0)
        {
            HttpWebRequest request = GetWebRequest(url, null, false, timeOut, header);

            if (httpMethod == "POST")
            {
                AddPostData(environment, request, requestData);
            }
            else if (httpMethod == "PUT")
            {
                AddPutData(environment, request, requestData);
            }

            return GetResponseData(environment, request, null);
        }

        /// <summary>
        /// Get binary data from web link.
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="httpSession"></param>
        /// <param name="gzipHeader"></param>
        /// <param name="unzipdata"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static byte[] GetResponseData(AWDEnvironment environment, string url, object postData,
                                             IHttpSession httpSession, out byte[] gzipHeader, bool unzipdata, int timeout)
        {
            HttpWebRequest request = GetWebRequest(url, httpSession, false, timeout);
            AddPostData(environment, request, postData);
            return GetResponseData(environment, request, httpSession, out gzipHeader, unzipdata);
        }

        /// <summary>
        /// V2 version of GetResponseData() uses C# implementation(GZipStream) instead of C++ zlib1.dll to unzip response.
        /// </summary>        
        public static byte[] GetResponseData_V2(AWDEnvironment environment, string url, object postData,
                                           IHttpSession httpSession, int timeout)
        {
            HttpWebRequest request = GetWebRequest(url, httpSession, false, timeout);
            AddPostData(environment, request, postData);
            return GetResponseData(request, httpSession, environment);
        }

        public static HttpWebRequest GetWebRequest(string url, IHttpSession httpSession)
        {
            return GetWebRequest(url, httpSession, false, 0);
        }

        public static HttpWebRequest GetWebRequest(string url, IHttpSession httpSession, bool keepLive)
        {
            return GetWebRequest(url, httpSession, keepLive, 0);
        }

        public static HttpWebRequest GetWebRequest(string url, IHttpSession httpSession, bool keepLive, int timeout, Dictionary<string, string> header = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.KeepAlive = keepLive;
            request.UserAgent = "MorningstarAWD";
            request.Headers[HttpConst.AcceptEncodingHeader] = "gzip";
            if (header != null)
            {
                if (header.ContainsKey("accept"))
                {
                    request.Accept = header["accept"];
                    header.Remove("accept");
                }

                if (header.ContainsKey("content-type"))
                {
                    request.ContentType = header["content-type"];
                    header.Remove("content-type");
                }

                foreach (string key in header.Keys)
                {
                    request.Headers.Add(key, header[key]);
                }
            }
            if (timeout > 0)
                request.Timeout = timeout;
            if (httpSession != null)
                httpSession.HandleRequest(request);
            return request;
        }

        public static void AddPostData(HttpWebRequest request, object postData)
        {
            AddPostData(AWDEnvironment.s_DefaultEnviroment, request, postData);
        }

        public static void AddPostData(AWDEnvironment environment, HttpWebRequest request, object postData)
        {
            if (!UnavailableResourceUtil.CheckWebResource(request.RequestUri.ToString()))
                return;
            if (postData != null)
            {
                try
                {
                    request.Method = "POST";
                    byte[] data;
                    var stringData = postData as string;
                    if (stringData != null)
                    {
                        environment.WriteDebugMessage("Post:" + stringData);
                        if (environment.TrackingLog != null)
                            environment.TrackingLog.AddLog("LoadWebPage", "Post", 0, stringData);
                        data = AWDEnvironment.s_DefaultEncoding.GetBytes(stringData);
                    }
                    else
                    {
                        var bArr = postData as byte[];
                        if (bArr != null)
                        {
                            data = bArr;
                        }
                        else
                        {
                            var s = postData as Stream;
                            if (s != null)
                            {
                                data = new byte[s.Length];
                                StreamUtil.ReadAll(s, data, 0, (int)s.Length);
                                s.Close();
                            }
                            else
                            {
                                throw new NotSupportedException("The type of postData in AddPostData is not supported.");
                            }
                        }
                        environment.WriteDebugMessage("Post:" + bArr.Length + " bytes");
                        if (environment.TrackingLog != null)
                            environment.TrackingLog.AddLog("LoadWebPage", "Post", 0, bArr.Length + " bytes");
                    }

                    request.ContentLength = data.Length;
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(data, 0, data.Length);
                        //requestStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    ExceptionUtil.DefaultHandleException(ex, environment);
                }
            }
        }

        public static void AddPutData(AWDEnvironment environment, HttpWebRequest request, object putData)
        {
            if (putData != null)
            {
                try
                {
                    request.Method = "PUT";
                    byte[] data;
                    var stringData = putData as string;
                    if (stringData != null)
                    {
                        environment.WriteDebugMessage("Put:" + stringData);
                        if (environment.TrackingLog != null)
                            environment.TrackingLog.AddLog("LoadWebPage", "Put", 0, stringData);
                        data = AWDEnvironment.s_DefaultEncoding.GetBytes(stringData);
                    }
                    else
                    {
                        var bArr = putData as byte[];
                        if (bArr != null)
                        {
                            data = bArr;
                        }
                        else
                        {
                            var s = putData as Stream;
                            if (s != null)
                            {
                                data = new byte[s.Length];
                                StreamUtil.ReadAll(s, data, 0, (int)s.Length);
                                s.Close();
                            }
                            else
                            {
                                throw new NotSupportedException("The type of putData in AddPostData is not supported.");
                            }
                        }
                        environment.WriteDebugMessage("Put:" + bArr.Length + " bytes");
                        if (environment.TrackingLog != null)
                            environment.TrackingLog.AddLog("LoadWebPage", "Put", 0, bArr.Length + " bytes");
                    }

                    request.ContentLength = data.Length;
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(data, 0, data.Length);
                        //requestStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    ExceptionUtil.DefaultHandleException(ex, environment);
                }
            }
        }

        public static byte[] GetResponseData(AWDEnvironment environment, HttpWebRequest request,
                                             IHttpSession httpSession)
        {
            byte[] gzipheader;
            return GetResponseData(environment, request, httpSession, out gzipheader, true);
        }

        public static Stream GetResponseStream(AWDEnvironment environment, string url, object postData, int timeout)
        {
            Stream outputStream = null;
            HttpWebRequest request = null;
            Timer timer = new Timer();

            try
            {
                request = GetWebRequest(url, null, false, timeout);
                SetRequestIdHeaders(request, environment);
                AddPostData(environment, request, postData);
                var response = (HttpWebResponse)request.GetResponse();
                outputStream = response.GetResponseStream();
            }
            catch (WebException wex)
            {
                if (wex.Status != WebExceptionStatus.ProtocolError)
                    ExceptionUtil.AppendException(wex, request.RequestUri.ToString());
            }
            catch (Exception ex)
            {
                ExceptionUtil.AppendException(ex, request.RequestUri.ToString());
            }
            finally
            {
                timer.Stop();
                environment.WriteDebugTimer(timer.ElapsedMillSeconds, "Load Url " + request.RequestUri);
                if (environment.TrackingLog != null)
                {
                    environment.TrackingLog.AddLog("LoadWebPage", "Url", timer.ElapsedMillSeconds, request.RequestUri.ToString());
                }
            }

            return outputStream;
        }

        public static byte[] GetResponseData(AWDEnvironment environment, HttpWebRequest request,
                                             IHttpSession httpSession, out byte[] gzipHeader, bool unzipdata)
        {
            gzipHeader = null;
            var timer = new Timer();
            byte[] bArr = null;
            try
            {
                SetRequestIdHeaders(request, environment);

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (httpSession != null)
                        httpSession.HandleResponse(response);
                    using (Stream os = response.GetResponseStream())
                    {
                        bArr = StreamUtil.ReadAll(os);
                        if (response.ContentEncoding == "gzip" && unzipdata)
                        {
                            bArr = ZipUtil.DoUnGzip(bArr, out gzipHeader);
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                environment.LogOther(wex.Message);
                if (wex.Status != WebExceptionStatus.ProtocolError)
                    ExceptionUtil.AppendException(wex, request.RequestUri.ToString());
            }
            catch (Exception ex)
            {
                environment.LogOther(ex.Message);
                ExceptionUtil.AppendException(ex, request.RequestUri.ToString());
            }
            finally
            {
                timer.Stop();
                environment.WriteDebugTimer(timer.ElapsedMillSeconds, "Load Url " + request.RequestUri);
                if (environment.TrackingLog != null)
                {
                    environment.TrackingLog.AddLog("LoadWebPage", "Url", timer.ElapsedMillSeconds, request.RequestUri.ToString());
                }
            }
            return bArr;
        }

        public static byte[] GetResponseData(
            HttpWebRequest request,
            IHttpSession httpSession,
            AWDEnvironment environment)
        {
            if (!UnavailableResourceUtil.CheckWebResource(request.RequestUri.ToString()))
            {
                commonLogger.LogMessage($"Request id: {environment.RequestId}, Unavailable Resource: {request.RequestUri}.", EventStatus.failure, EventType.info);
                return null;
            }

            byte[] bArr = null;
            try
            {
                SetRequestIdHeaders(request, environment);

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (httpSession != null)
                        httpSession.HandleResponse(response);
                    using (Stream os = response.GetResponseStream())
                    {
                        bArr = StreamUtil.ReadAll(os);
                        if (response.ContentEncoding == "gzip")
                        {
                            bArr = ZipUtil.DoUnGzip(bArr);
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                commonLogger.LogMessage($"Request id: {environment.RequestId}, Catch WebException: {wex} .", EventStatus.failure, EventType.info);
                throw wex;
            }
            catch (Exception ex)
            {
                commonLogger.LogMessage($"Request id: {environment.RequestId}, Catch Exception: {ex} .", EventStatus.failure, EventType.info);
                throw ex;
            }

            return bArr;
        }

        public static XElement GetResponseXElemnetData(AWDEnvironment environment, HttpWebRequest request,
                                             IHttpSession httpSession)
        {
            if (!UnavailableResourceUtil.CheckWebResource(request.RequestUri.ToString()))
                return null;

            var timer = new Timer();
            try
            {
                SetRequestIdHeaders(request, environment);

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (httpSession != null)
                        httpSession.HandleResponse(response);
                    using (Stream os = response.GetResponseStream())
                    {
                        return XElement.Load(os);
                    }
                }
            }
            catch (WebException wex)
            {
                if (wex.Status != WebExceptionStatus.ProtocolError)
                    ExceptionUtil.AppendException(wex, request.RequestUri.ToString());
            }
            catch (Exception ex)
            {
                ExceptionUtil.AppendException(ex, request.RequestUri.ToString());
            }
            finally
            {
                timer.Stop();
                environment.WriteDebugTimer(timer.ElapsedMillSeconds, "Load Url " + request.RequestUri);
                if (environment.TrackingLog != null)
                    environment.TrackingLog.AddLog("LoadWebPage", "Url", timer.ElapsedMillSeconds, request.RequestUri.ToString());
            }
            return null;
        }

        public static void UnLockUserInCookie()
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.Cookies.Add(new HttpCookie("loginfail", ""));
            }
        }

        public static bool IsExchangeTestUser()
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Items.Contains("IsExchangeTestUser");
            }
            return false;
        }

        public static string RefreshAPIGeeAccessToken(object APIclient)
        {
            Dictionary<string, string> dictClient = (Dictionary<string, string>)APIclient;

            string host = ServerConfigSetting.Instance.GetConfig("APIGee");
            string url = host + "/oauth2/accesstoken?grant_type=client_credentials&amp;scope=read";
            string clientId;
            dictClient.TryGetValue("key", out clientId);
            string clientSecret;
            dictClient.TryGetValue("secret", out clientSecret);

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientId))
                return null;

            string result;
            using (WebClient client = new WebClient())
            {
                string credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(clientId
               + ":" + clientSecret));
                client.Headers.Add("Authorization", "Basic " + credentials);
                result = client.DownloadString(url);

                var json = JObject.Parse(result);
                result = json["access_token"].ToString();
            }
            return result;
        }
        public static string GetAPIGeeAccessToken(string apikey, string secret)
        {
            Dictionary<string, string> APIclient = new Dictionary<string, string>
            {
                { "key" , apikey },
                { "secret" , secret }
            };

            return (string)CacheUtil.GetCache(apikey, RefreshAPIGeeAccessToken, APIclient, HttpConst.APIGeeTokenTimeout);
        }

        public static void SetAPIGeeHeaders(HttpWebRequest request, AWDEnvironment environment, bool isLiveKeyOnTP)
        {
            var mode = ServerConfiguration.ProductMode;
            if (mode == ProductMode.PatchTP)
                mode = ProductMode.TP;

            if ((mode == ProductMode.TP || mode == ProductMode.PatchTP) && isLiveKeyOnTP)
                mode = ProductMode.LIVE;

            var apikey = AppSettingsConfiguratoin.Instance.GetValue(mode, "DirectAPIKey");
            var apisecret = AppSettingsConfiguratoin.Instance.GetValue(mode, "DirectAPISecret");
            string api_token = GetAPIGeeAccessToken(apikey, apisecret);

            request.Headers.Add("Authorization", "Bearer " + api_token);
            request.Headers.Add("UID", environment.UserId.ToString());
            request.Headers.Add("PID", environment.ProductId.ToString());
            request.Headers.Add("PCODE", environment.ProductCode);
        }

        public static void SetRequestIdHeaders(HttpWebRequest request, AWDEnvironment environment)
        {
            if (request != null && request.Headers["RequestId"] == null)
                request.Headers.Add("RequestId", environment.RequestId);
        }
    }
}
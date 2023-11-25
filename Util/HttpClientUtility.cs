using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace PullFinanceData
{
    public class HttpClientUtility
    {
        public HttpClientUtility()
        {

        }
        public string HttpClientPost(string url, object datajson)
        {
            using (HttpClient httpClient = new HttpClient()) //http对象
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.Timeout = new TimeSpan(0, 0, 5);
                //转为链接需要的格式
                HttpContent httpContent = new JsonContent(datajson);
                //请求
                HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    Task<string> t = response.Content.ReadAsStringAsync();
                    return t.Result;
                }
                throw new Exception("调用失败");
            }

        }
    }
}
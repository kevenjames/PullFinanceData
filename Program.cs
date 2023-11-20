using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace PullFinanceData
{
    internal class Program
    {
        public class TushareRequest
        { 
            public string api_name { get; set; }
            public string token { get; set; }
            public Dictionary<string, string> Params { get; set; }
        }

        public class Data
        {
            public string ts_code { get; set; }
            public string trade_date { get; set; }
            public double open { get; set; }
            public double high { get; set; }
            public double low { get; set; }
            public double close { get; set; }
            public double pre_close { get; set; }
            public double change { get; set; }
            public double pct_chg { get; set; }
            public double vol { get; set; }
            public double amount { get; set; }
        }

        public class Root
        {
            public List<string> fields { get; set; }
            public List<List<object>> items { get; set; }
            public bool has_more { get; set; }
        }

        static async Task Main()
        {
            string tushareToken = "1c8555886c5074eabe1f6856cca2a9d5d5aafc8bed978dae43080a0a";
            string stockCode = "600000.SH";  // 以浦发银行为例
            string tradeDate = "";
            string startDate = "";
            string endDate = "";

            // 构建请求 URL
            string apiUrl = $"https://api.tushare.pro";


            // 构建请求参数
            Dictionary<string, string> postBody = new Dictionary<string, string>();
            postBody.Add("ts_code", stockCode);
            postBody.Add("trade_date", tradeDate);
            postBody.Add("start_date", startDate);
            postBody.Add("end_date", endDate);

            var requestData = new TushareRequest
            {
                api_name = "daily",
                token = tushareToken,
                Params = postBody
            };

            using (HttpClient client = new HttpClient())
            {
                // 设置 Tushare 接口令牌
                client.DefaultRequestHeaders.Add("token", tushareToken);
                string jsonRequestData = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
                HttpContent content = new StringContent(jsonRequestData, System.Text.Encoding.UTF8, "application/json");

                // 发送请求并获取响应
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                // 读取响应内容
                string responseData = await response.Content.ReadAsStringAsync();

                //反序列化响应内容
                Root root = JsonSerializer.Deserialize<Root>(responseData);

                // 打印日线数据
                Console.WriteLine(responseData);
            }

            return;
        }
    }
}

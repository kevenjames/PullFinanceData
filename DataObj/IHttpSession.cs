using System.Net;

namespace PullFinanceData.DataObj
{
    public interface IHttpSession
    {
        void HandleRequest(HttpWebRequest request);
        void HandleResponse(HttpWebResponse response);
    }
}
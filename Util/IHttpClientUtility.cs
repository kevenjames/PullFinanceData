namespace PullFinanceData
{
    public interface IHttpClientUtility
    {
        string HttpClientPost(string url, object datajson);
    }
}
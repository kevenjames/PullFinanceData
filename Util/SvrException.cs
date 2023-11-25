using System;

namespace PullFinanceData.Util
{
    public class SvrException : Exception
    {
        public SvrException(int errorCode) : this(errorCode, "Server Exception:" + errorCode)
        {
        }

        public SvrException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
            ErrorMessage = message;
        }

        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
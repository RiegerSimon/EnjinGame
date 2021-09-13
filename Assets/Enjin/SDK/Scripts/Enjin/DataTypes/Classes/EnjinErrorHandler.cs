namespace EnjinSDK
{
    public class EnjinErrorHandler    {
        // Public Properties
        public string Message { get; set; }
        public string Code { get; set; }
        public string Detail { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public EnjinErrorHandler()
        {
            Message = string.Empty;
            Code = "0";
            Detail = string.Empty;
        }
    }
}
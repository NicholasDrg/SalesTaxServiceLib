/// <summary>
/// 
/// </summary>


namespace SalesTaxServiceLib
{
    /// <summary>
    /// Temporary logging class.
    /// </summary>
    public static class Log
    {
        static public void Error(string errorMsg)
        {
            Console.WriteLine("[Error] " + errorMsg);
        }

        static public void Info(string errorMsg)
        {
            Console.WriteLine("[Info] " + errorMsg);
        }

        static public void Warn(string errorMsg)
        {
            Console.WriteLine("[Warning] " + errorMsg);
        }
    }
}

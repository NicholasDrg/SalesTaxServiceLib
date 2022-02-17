/// <summary>
/// 
/// </summary>


namespace SalesTaxServiceLib
{
    /// <summary>
    /// Superclass of sales tax request results classes (e.g. SalesTaxRateResults).
    /// </summary>
    public class SalesTaxRequestResults
    {
        public SalesTaxService.TaxRequestStatusEnum RequestStatus { get; set; }

        public string? ProviderRequestErrorData { get; set; }
    }
}

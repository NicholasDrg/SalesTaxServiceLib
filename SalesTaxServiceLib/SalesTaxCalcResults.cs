/// <summary>
/// 
/// </summary>


namespace SalesTaxServiceLib
{
    /// <summary>
    /// 
    /// </summary>
    public struct SalesTaxCalcData
    {
        public double AmountToCollect { get; set; }
        public double? Rate { get; set; }
        public bool? FreightTaxable = false;
        public double? OrderTotalAmount { get; set; }
        public double? Shipping { get; set; }
        public double? TaxableAmount { get; set; }
    }

    public class SalesTaxCalcResults : SalesTaxRequestResults
    {
        public SalesTaxCalcData? TaxCalcData { get; set; }

        public SalesTaxCalcResults()
        {
            RequestStatus = SalesTaxService.TaxRequestStatusEnum.FAIL;
            ProviderRequestErrorData = null;
            TaxCalcData = null;
        }
    }
}

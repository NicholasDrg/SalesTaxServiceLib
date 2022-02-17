/// <summary>
/// 
/// </summary>


namespace SalesTaxServiceLib
{
    /// <summary>
    /// 
    /// </summary>
    public struct SalesTaxRateData
    {
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? County { get; set; }
        public string? City { get; set; }
        public string? Zip { get; set; }

        public double? CombinedRate { get; set; }
        public double? CombinedDistrictRate { get; set; }
        public double? CountryRate { get; set; }
        public double? StateRate { get; set; }
        public double? CountyRate { get; set; }
        public double? CityRate { get; set; }

        public bool? FreightTaxable = false;
    }

    public class SalesTaxRateResults : SalesTaxRequestResults
    {
        /// <summary>
        /// Contains the common/unified format for Tax Rate data.
        /// Note: For simplicity, this is the JSON string in the TaxJar format.
        /// </summary>
        public SalesTaxRateData? TaxRateData { get; set; }

        public SalesTaxRateResults()
        {
            RequestStatus = SalesTaxService.TaxRequestStatusEnum.FAIL;
            TaxRateData = null;
        }
    }
}

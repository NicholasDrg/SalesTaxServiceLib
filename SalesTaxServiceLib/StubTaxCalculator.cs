/// <summary>
/// 
/// </summary>


namespace SalesTaxServiceLib
{
    /// <summary>
    /// Mocking class.
    /// </summary>
    public class StubTaxCalculator : ISalesTaxCalculator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taxCalcRequestInput"></param>
        /// <returns></returns>
        public SalesTaxCalcResults CalculateTax(SalesTaxCalcRequestInput taxCalcRequestInput)
        {
            SalesTaxCalcResults requestResults = new SalesTaxCalcResults();
            if(taxCalcRequestInput.to_zip == "01463")
            {
                double rate = 0.065;
                double totalOrderAmount = taxCalcRequestInput.amount + taxCalcRequestInput.shipping;
                double amountToCollect = taxCalcRequestInput.amount * rate;

                SalesTaxCalcData taxCalcData = new SalesTaxCalcData()
                {
                    TaxableAmount = taxCalcRequestInput.amount,
                    Shipping = taxCalcRequestInput.shipping,
                    Rate = rate,
                    OrderTotalAmount = totalOrderAmount,
                    FreightTaxable = false,
                    AmountToCollect = amountToCollect,
                };
                requestResults.TaxCalcData = taxCalcData;
                requestResults.RequestStatus = SalesTaxService.TaxRequestStatusEnum.SUCCESS;
            }
            return requestResults;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="salesTaxRateRequestInput"></param>
        /// <returns></returns>
        public SalesTaxRateResults GetTaxRate(SalesTaxRateRequestInput salesTaxRateRequestInput)
        {
            SalesTaxRateResults requestResults = new SalesTaxRateResults();

            if (salesTaxRateRequestInput.ZipCode == "01463")
            {
                SalesTaxRateData taxRateData = new SalesTaxRateData()
                {
                    CombinedRate = 0.0625,
                    CountryRate = 0.0,
                    StateRate = 0.0625,
                    CountyRate = 0.0,
                    CityRate = 0.0,
                    Country = "US",
                    State = "MA",
                    County = null,
                    City = null,
                    Zip = "01463",
                    FreightTaxable = false,
                };
                requestResults.TaxRateData = taxRateData;
                requestResults.RequestStatus = SalesTaxService.TaxRequestStatusEnum.SUCCESS;
            }
            return requestResults;
        }
    }
}

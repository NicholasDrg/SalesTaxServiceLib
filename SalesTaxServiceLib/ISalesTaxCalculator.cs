/// <summary>
/// Interface for Sales Tax Calculator implementations.
/// </summary>

namespace SalesTaxServiceLib
{
    /// <summary>
    /// Interface for sales tax calculator implementations.
    /// </summary>
    /// <remarks>
    /// Note1: Using a synchronous interface to keep things simple for now. See comments below.
    public interface ISalesTaxCalculator
    {
        /// <summary>
        /// Calculates the sales tax amount for a given order.
        /// </summary>
        /// <param name="salesTaxCalcRequestInput"></param>
        /// <returns>The order sales tax.</returns>
        public SalesTaxCalcResults CalculateTax(SalesTaxCalcRequestInput salesTaxCalcRequestInput);
        //public Task<SalesTaxCalcResults> CalculateTax(SalesTaxCalcRequestInput orderSalesTaxCalcRequestInput);

        /// <summary>
        /// Retrieves the sales tax rates for a given geographical location.
        /// </summary>
        /// <param name="salesTaxRateRequestInput"></param>
        /// <returns></returns>
        public SalesTaxRateResults GetTaxRate(SalesTaxRateRequestInput salesTaxRateRequestInput);
        //public Task<SalesTaxRateResults> GetTaxRate(SalesTaxRateRequestInput salesTaxRateInput);
    }


    /// Note: If we want TaxCalculator objects to perform requests asynchronously
    /// (allow the client to use 'await'), we'll need an async interface like the
    /// one below.  We can either use only an async interface to replace the above
    /// or have the async interface inherit from the above interface.  Then, the
    /// implementations can implement both sync and async versions of each method.
    /// I am not doing that now to keep things simple.
    /*
    public interface ISalesTaxCalculatorAsync : ISalesTaxCalculator
    {
        public Task<SalesTaxCalcResults> CalculateTaxAsync(SalesTaxCalcRequestInput salesTaxCalcRequestInput);
        public Task<SalesTaxRateResults> GetTaxRateAsync(SalesTaxRateRequestInput salesTaxRateInput);
    }
    */

}

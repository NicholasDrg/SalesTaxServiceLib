/// <summary>
/// 
/// </summary>


namespace SalesTaxServiceLib.Tests
{
    /// <summary>
    /// Class to allow unit testing TaxJarTaxCalculator protected methods.
    /// </summary>
    /// <remarks>
    /// Note: While trying to figure out how to create unit tests for protected or
    /// private methods, I found that this is not recommended according to various
    /// online sources.  They recommend unit tests for public methods only.  I get
    /// some of the rationale but I still find unit testing specific protected
    /// methods very useful.  In past C++ projects we unit tested protected methods
    /// by creating test subclasses.  I find this useful in this situation.  Sorry.
    /// </remarks>
    internal class TaxJarTaxCalculatorTester : TaxJarTaxCalculator
    {
        public object GenerateCalculateTaxRequestInputTester(SalesTaxCalcRequestInput taxCalcRequestInput)
        {
            return GenerateCalculateTaxRequestInput(taxCalcRequestInput);
        }

        public object GenerateTaxRateRequestInputTester(SalesTaxRateRequestInput taxRateRequestInput)
        {
            return GenerateTaxRateRequestInput(taxRateRequestInput);
        }

        public SalesTaxCalcData? TransformCalculateTaxResponseOutputTester(string? taxSourceOrderSalesTaxResponseData)
        {
            return TransformCalculateTaxResponseOutput(taxSourceOrderSalesTaxResponseData);
        }

        public SalesTaxRateData? TransformGetTaxRatesRequestOutputTester(string taxRateResponseOutput)
        {
            return TransformGetTaxRatesRequestOutput(taxRateResponseOutput);
        }
    }
}

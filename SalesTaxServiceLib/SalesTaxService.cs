/// <summary>
/// 
/// </summary>


namespace SalesTaxServiceLib
{
    /// <summary>
    /// Sales tax service class providing sales tax information.  Class is agnostic to
    /// specific tax service providers / data retrieval sources.
    /// </summary>
    /// <remarks>
    /// Calls in the SalesTaxService API defined here return error status.  Alternatively,
    /// we could have used exceptions for error conditions.
    /// </remarks>
    public class SalesTaxService
    {
        protected ISalesTaxCalculator _taxCalculator;


        /// <summary>
        /// 
        /// </summary>
        public enum TaxRequestStatusEnum
        {
            FAIL,
            SUCCESS,
        };


        /// <summary>
        /// Calculates the tax amount for a given order.
        /// </summary>
        /// <param name="orderData"></param>
        /// <returns></returns>
        public SalesTaxCalcResults CalculateTax(SalesTaxCalcRequestInput taxCalcRequestInput)
        {
            SalesTaxCalcResults requestResults = new SalesTaxCalcResults();
            if (IsValidTaxCalculateRequestInput(taxCalcRequestInput))
            {
                requestResults = _taxCalculator.CalculateTax(taxCalcRequestInput);
                // Note: _taxCalculator should never be NULL.
            }
            return requestResults;
        }


        /// <summary>
        /// Retrieves the sales tax rates for a given geographical location.
        /// </summary>
        /// <param name="taxRateRequestInput"></param>
        /// <returns></returns>
        public SalesTaxRateResults GetTaxRate(SalesTaxRateRequestInput taxRateRequestInput)
        {
            SalesTaxRateResults requestResults = new SalesTaxRateResults();

            if (IsValidTaxRateRequestInput(taxRateRequestInput))
            {
                requestResults = _taxCalculator.GetTaxRate(taxRateRequestInput);
                // Note: _taxCalculator should never be NULL.
            }
            return requestResults;
        }


        /// <summary>
        /// Retrieves the sales tax rates for a given geographical location.
        /// </summary>
        /// <param name="zip"></param>
        /// <returns></returns>
        public SalesTaxRateResults GetTaxRate( string zip)
        {
            SalesTaxRateRequestInput queryInput = new SalesTaxRateRequestInput()
            {
                ZipCode = zip,
            };
            return GetTaxRate(queryInput);
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="taxCalculator"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SalesTaxService( ISalesTaxCalculator taxCalculator )
        {
            if(taxCalculator == null)
            {
                throw new ArgumentNullException("TaxCalculator object is NULL");
            }
            _taxCalculator = taxCalculator;
        }


        /// <summary>
        /// Validates tax calculation request input data.
        /// </summary>
        /// <param name="taxCalcRequestInput"></param>
        /// <returns>True if valid, otherwise, False.</returns>
        /// <remarks>
        /// Note: Has minimum validation.  It is more of a placeholder to put more validation.
        /// </remarks>
        private bool IsValidTaxCalculateRequestInput(SalesTaxCalcRequestInput taxCalcRequestInput)
        {
            bool isValidRequestInput = true;
            if ((taxCalcRequestInput.from_zip == null) || (taxCalcRequestInput.from_zip.Length < 1))
            {
                Log.Error("'From' zip code in calculate sales tax request input data is empty or NULL");
                isValidRequestInput = false;
            }
            if ((taxCalcRequestInput.to_zip == null) || (taxCalcRequestInput.to_zip.Length < 1))
            {
                Log.Error("'To' zip code in calculate sales tax request input data is empty or NULL");
                isValidRequestInput = false;
            }
            return isValidRequestInput;
        }


        /// <summary>
        /// Validates tax rate request input data.
        /// </summary>
        /// <param name="taxRateRequestInput"></param>
        /// <returns>True if valid, otherwise, False.</returns>
        /// <remarks>
        /// Note: Has minimum validation.  It is more of a placeholder to put more validation.
        /// </remarks>
        private bool IsValidTaxRateRequestInput(SalesTaxRateRequestInput taxRateRequestInput)
        {
            bool isValidRequestInput = true;
            if ((taxRateRequestInput.ZipCode == null) || (taxRateRequestInput.ZipCode.Length < 1))
            {
                Log.Error("Zip code in sales tax rate request input data is empty or NULL");
                isValidRequestInput = false;
            }
            return isValidRequestInput;
        }

    }
}

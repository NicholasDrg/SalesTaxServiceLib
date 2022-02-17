/// <summary>
/// 
/// </summary>

namespace SalesTaxServiceLib
{
    /// <summary>
    /// Simple class to provide sales tax rate query input.
    /// </summary>
    /// <remarks>
    /// Note: Using this struct for now to keep things simple.  In the future, it
    /// could be converterd into a class that handles validation of the input.
    ///
    /// Note: We could use a dictionary or JSON string to define the input.  However,
    /// that would require validation to ensure every field name string is correct.
    /// JSON would be the right approach if we think SalesTaxService will end up being a
    /// microservice with a REST interface.  The struct is simpler for now.
    /// </remarks>
    public readonly struct SalesTaxRateRequestInput
    {
        public string ZipCode { get; init; }
        public string? Country { get; init; }
        public string? City { get; init; }
        public string? StateProvince { get; init; }
        public string? Street { get; init; }
    }

}

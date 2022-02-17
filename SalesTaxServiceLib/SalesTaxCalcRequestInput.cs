/// <summary>
/// 
/// </summary>

namespace SalesTaxServiceLib
{
    /// <summary>
    /// Class to provide common sales order tax calculation request input data.
    /// 
    /// Note: For simplicity, we kept the field names the same as in the TaxJar request JSON
    /// data.  However, in the future common field names should be used an code added to
    /// transform the common field names to the names that TaxJar expects.
    /// </summary>
    /// <remarks>
    /// Note: Not doing any input validation 
    /// 
    /// Note: We could use a dictionary or JSON string to define the input.  However,
    /// that would require validation to ensure every field name string is correct.
    /// JSON would be the right approach if we think SalesTaxService will end up being a
    /// microservice with a REST interface.
    /// </remarks>
    public class SalesTaxCalcRequestInput
    {
        public struct NexusAddress
        {
            public string id { get; init; }
            public string country { get; init; }
            public string zip { get; init; }
            public string state { get; init; }
            public string city { get; init; }
            public string street { get; init; }
        }

        public struct LineItem
        {
            public string id { get; init; }
            public int quantity { get; init; }
            public string product_tax_code { get; init; }
            public decimal unit_price { get; init; }
            public decimal discount { get; init; }
        }

        public string from_country { get; init; }
        public string from_zip { get; init; }
        public string from_state { get; init; }
        public string from_city { get; init; }
        public string from_street { get; init; }
        public string to_country { get; init; }
        public string to_zip { get; init; }
        public string to_state { get; init; }
        public string to_city { get; init; }
        public string to_street { get; init; }
        public double amount { get; init; }
        public double shipping { get; init; }

        public List<NexusAddress> nexus_addresses = new List<NexusAddress>();
        public List<LineItem> line_items = new List<LineItem>();

        public void AddNexusAddress(NexusAddress nexusAddress)
        {
            nexus_addresses.Add(nexusAddress);
        }

        public void AddLineItem(LineItem lineItem)
        {
            line_items.Add(lineItem);
        }
    }
}

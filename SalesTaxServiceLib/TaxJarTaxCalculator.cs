using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Text;


namespace SalesTaxServiceLib
{
    /// <summary>
    /// Class handling requests to TaxJar service provider.
    /// </summary>
    public class TaxJarTaxCalculator : ISalesTaxCalculator
    {
        private const string URL_BASE = "https://api.taxjar.com/v2/";
        private const string URL_TAX_CALC = URL_BASE + "taxes/";
        private const string URL_TAX_RATES = URL_BASE + "rates/";


        private const string TAX_JAR_API_TOKEN = "5da2f821eee4035db4771edab942a4cc";

        private const string COUNTRY_FLD = "country";
        private const string STATE_PROVINCE_FLD = "state";
        private const string CITY_FLD = "city";
        private const string STREET_FLD = "street";

        private readonly HttpClient _httpClient = new HttpClient();


        /// <summary>
        /// Struct for HTTP response data.
        /// </summary>
        struct HttpResponse
        {
            public string? Content { get; set; }
            public string? HttpStatusMessage { get; set; }
        }


        public TaxJarTaxCalculator()
        {
            SetupHttpClient();
        }

        ///////////////////////////////////////////////////////////////////////////
        /// ISalesTaxCalculator Implementation

        /// <summary>
        /// Retrieves sales tax data for given order from TaxJar service, extracts the
        /// sales tax amount and returns it.
        /// </summary>
        /// <param name="taxCalcRequestInput"></param>
        /// <returns></returns>
        public SalesTaxCalcResults CalculateTax(SalesTaxCalcRequestInput taxCalcRequestInput)
        {
            SalesTaxCalcResults requestResults = new SalesTaxCalcResults();

            string requestJsonStr = (string) GenerateCalculateTaxRequestInput(taxCalcRequestInput);

            Task<HttpResponse> httpResponseTask = SendCalculateTaxRequest(requestJsonStr);

            requestResults.ProviderRequestErrorData = httpResponseTask.Result.HttpStatusMessage;
            string? responseContent = httpResponseTask.Result.Content;
            if ((requestResults.ProviderRequestErrorData == null) && (responseContent != null))
            {
                requestResults.RequestStatus = SalesTaxService.TaxRequestStatusEnum.SUCCESS;
                requestResults.TaxCalcData = TransformCalculateTaxResponseOutput(responseContent);
            }
            return requestResults;
        }


        /// <summary>
        /// Interface method implementation to retrieve sales tax rate for a location.
        /// </summary>
        /// <param name="taxRateRequestInput"></param>
        /// <returns></returns>
        public SalesTaxRateResults GetTaxRate(SalesTaxRateRequestInput taxRateRequestInput)
        {
            SalesTaxRateResults requestResults = new SalesTaxRateResults();
            string taxRateRequestUrl = (string) GenerateTaxRateRequestInput(taxRateRequestInput);

            Task<HttpResponse> httpResponseTask = SendGetTaxRateRequest(taxRateRequestUrl);

            string? responseContent = httpResponseTask.Result.Content;
            if (responseContent != null)
            {
                // TODO: Parse HTTP response content for errors to set error status.

                requestResults.TaxRateData = TransformGetTaxRatesRequestOutput(responseContent);
                if (requestResults.TaxRateData != null)
                {
                    requestResults.RequestStatus = SalesTaxService.TaxRequestStatusEnum.SUCCESS;
                }
            }

            // Note: To implement an 'async' version of this method you may use the following.
            // (See remarks in the ISalesTaxCalculator interface.)
            //Task<HttpResponse> httpResponseTask = SendGetTaxRateRequest(taxRateRequestUrl);
            //HttpResponse httpResponse = awat httpResponseTask;  ...

            return requestResults;
        }

        /// ISalesTaxCalculator Implementation End
        ///////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Sets up the HTTP client object by adding to the header authorization and user agent info.
        /// </summary>
        private void SetupHttpClient()
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + TAX_JAR_API_TOKEN);

            string platform = RuntimeInformation.OSDescription;
            string arch = RuntimeInformation.OSArchitecture.ToString();
            string framework = RuntimeInformation.FrameworkDescription;
            string userAgentValue = $"TaxJar Client/.NET ({platform}; {arch}; {framework})";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", userAgentValue);

            // TODO: The following may not be necessary.
            //_httpClient.DefaultRequestHeaders.Accept.Add(
            //    new MediaTypeWithQualityHeaderValue("application/json"));
        }


        /// <summary>
        /// Sends TaxJar request to calculate sales tax for given order.
        /// </summary>
        /// <param name="salesOrderTaxCalcRequestInputJsonStr"></param>
        /// <returns>HTTP response status and contents.</returns>
        private async Task<HttpResponse> SendCalculateTaxRequest(string salesOrderTaxCalcRequestInputJsonStr)
        {
            HttpResponse httpResponse = new HttpResponse();

            // TODO: Is the following needed?
            //_httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

            var httpContent = new StringContent(salesOrderTaxCalcRequestInputJsonStr, Encoding.UTF8, "application/json");

            HttpResponseMessage httpResponseTask = await _httpClient.PostAsync(URL_TAX_CALC, httpContent);

            System.Net.HttpStatusCode? httpStatusCode = httpResponseTask.StatusCode;
            // Future: Maybe handle HTTP response codes (determine if response should be a 200
            // always and if not, handle it).
            if (httpStatusCode != System.Net.HttpStatusCode.OK)
            {
                string httpErrorStatus = "HTTP response code: " + ((int?)httpStatusCode).ToString() + " - " + httpStatusCode.ToString();
                httpResponse.HttpStatusMessage = httpErrorStatus;
                Log.Error(httpErrorStatus);
            }

            // If the response contains content, read it.
            if (httpResponseTask.Content == null)
            {
                Log.Error("HTTP response content was NULL!");
            }
            else
            {
                httpResponse.Content = await httpResponseTask.Content.ReadAsStringAsync();
                //Console.WriteLine(httpResponse.Content);

                // If the HTTP response code is not 200-OK, log the response content.
                if (httpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    Log.Error("HTTP error. Response content: " + httpResponse.Content);
                }
            }
            return httpResponse;
        }


        /// <summary>
        /// Sends TaxJar HTTP request for tax rate query.
        /// </summary>
        /// <param name="taxRateRequestUrl"></param>
        /// <returns>TaxJar tax rate JSON output</returns>
        private async Task<HttpResponse> SendGetTaxRateRequest(string taxRateRequestUrl)
        {
            HttpResponse httpResponse = new HttpResponse();
            httpResponse.Content = await _httpClient.GetStringAsync(taxRateRequestUrl);
            // Future: Parse the content for error messages.  Need to know what to expect as errors.
            //Console.WriteLine(httpResponse.Content);
            return httpResponse;
        }


        /// <summary>
        /// Generates TaxJar specific request input data for calculating sales tax from
        /// common request data objects used for every tax provider request.
        /// Specifically, receives common order sales tax calculation request data and
        /// generates the JSON data TaxJar expects for the calculate tax request.
        /// </summary>
        /// <param name="taxCalcRequestInput"></param>
        /// <returns></returns>
        protected object GenerateCalculateTaxRequestInput(SalesTaxCalcRequestInput taxCalcRequestInput)
        {
            string orderTaxCalcRequestJson = JsonConvert.SerializeObject(taxCalcRequestInput);
            // Console.WriteLine(orderTaxCalcRequestJson);
            return orderTaxCalcRequestJson;
        }


        /// <summary>
        /// Generates the input data needed for a tax rate request from TaxJar.  The input data
        /// is expected in the form of URL query parameters.
        /// </summary>
        /// <param name="taxRateRequestInput"></param>
        /// <returns>TaxJar URL with query parameters needed to make the tax rate request.</returns>
        protected object GenerateTaxRateRequestInput(SalesTaxRateRequestInput taxRateRequestInput)
        {
            string url = URL_TAX_RATES + taxRateRequestInput.ZipCode;

            // Create an HTTP query string with the sales tax rate query input values.
            UriBuilder uriBuilder = new UriBuilder(url);
            uriBuilder.Port = -1;
            NameValueCollection urlQuery = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            if (taxRateRequestInput.Country != null)
            {
                urlQuery[COUNTRY_FLD] = taxRateRequestInput.Country;
            }
            if (taxRateRequestInput.StateProvince != null)
            {
                urlQuery[STATE_PROVINCE_FLD] = taxRateRequestInput.StateProvince;
            }
            if (taxRateRequestInput.City != null)
            {
                urlQuery[CITY_FLD] = taxRateRequestInput.City;
            }
            if (taxRateRequestInput.Street != null)
            {
                urlQuery[STREET_FLD] = taxRateRequestInput.Street;
            }
            //Console.WriteLine(urlQuery.ToString());

            uriBuilder.Query = urlQuery.ToString();
            // Return the full URL with the query string.
            return uriBuilder.ToString();
        }


        /// <summary>
        /// Transforms the order sales tax calculation response data received from
        /// TaxJar into a common format.  Specifically, parses the calculate sales
        /// tax data and returns it.
        /// </summary>
        /// <param name="taxSourceOrderSalesTaxResponseData"></param>
        /// <returns>Calculated tax data or NULL if parsing failed.</returns>
        protected SalesTaxCalcData? TransformCalculateTaxResponseOutput(string? taxSourceOrderSalesTaxResponseData)
        {
            SalesTaxCalcData? salesTaxCalcData = null;
            // Sample Response
            /*
            {
                "tax":{
                    "amount_to_collect":1.43,
                    "breakdown":{
                        "city_tax_collectable":0.0,
                        "city_tax_rate":0.0,
                        "city_taxable_amount":0.0,
                        "combined_tax_rate":0.095,
                        "county_tax_collectable":0.15,
                        "county_tax_rate":0.01,"county_taxable_amount":15.0,
                        "line_items":[{
                            "city_amount":0.0,
                            "city_tax_rate":0.0,
                            "city_taxable_amount":0.0,
                            "combined_tax_rate":0.095,
                            "county_amount":0.15,
                            "county_tax_rate":0.01,
                            "county_taxable_amount":15.0,"id":"1",
                            "special_district_amount":0.34,
                            "special_district_taxable_amount":15.0,"special_tax_rate":0.0225,"state_amount":0.94,
                            "state_sales_tax_rate":0.0625,"state_taxable_amount":15.0,
                            "tax_collectable":1.43,"taxable_amount":15.0}],
                        "special_district_tax_collectable":0.34,
                        "special_district_taxable_amount":15.0,
                        "special_tax_rate":0.0225,"state_tax_collectable":0.94,"state_tax_rate":0.0625,
                        "state_taxable_amount":15.0,"tax_collectable":1.43,"taxable_amount":15.0
                    },
                    "freight_taxable":false,
                    "has_nexus":true,
                    "jurisdictions":{ "city":"LOS ANGELES","country":"US","county":"LOS ANGELES COUNTY","state":"CA"},
                    "order_total_amount":16.5,
                    "rate":0.095,
                    "shipping":1.5,
                    "tax_source":"destination",
                    "taxable_amount":15.0
                }
            } */

            // Parse the calculated tax data only if it is not NULL.  If it is NULL,
            // the caller will log an error.
            if (taxSourceOrderSalesTaxResponseData != null)
            {
                try
                {
                    // Parse amount to collect.
                    JObject jsonOrderSalesTaxData = JObject.Parse(taxSourceOrderSalesTaxResponseData);
                    // Console.WriteLine(jsonOrderSalesTaxData.ToString());

                    JToken? taxDataJson = jsonOrderSalesTaxData["tax"];
                    if (taxDataJson != null)
                    {
                        JToken? amountToCollectJson = taxDataJson["amount_to_collect"];
                        if (amountToCollectJson != null)
                        {
                            SalesTaxCalcData taxCalcData = new SalesTaxCalcData()
                            {
                                AmountToCollect = (double)amountToCollectJson,
                            };

                            JToken? rateJson = taxDataJson["rate"];
                            if(rateJson != null)
                            {
                                taxCalcData.Rate = (double)rateJson;
                            }
                            JToken? freightTaxableJson = taxDataJson["freight_taxable"];
                            if (freightTaxableJson != null)
                            {
                                taxCalcData.FreightTaxable = (bool)freightTaxableJson;
                            }
                            JToken? orderTotalAmountJson = taxDataJson["order_total_amount"];
                            if (orderTotalAmountJson != null)
                            {
                                taxCalcData.OrderTotalAmount = (double)orderTotalAmountJson;
                            }
                            JToken? shippingJson = taxDataJson["shipping"];
                            if (shippingJson != null)
                            {
                                taxCalcData.Shipping = (double)shippingJson;
                            }
                            JToken? taxableAmountJson = taxDataJson["taxable_amount"];
                            if (taxableAmountJson != null)
                            {
                                taxCalcData.TaxableAmount = (double)taxableAmountJson;
                            }
                            salesTaxCalcData = taxCalcData;
                        }
                        else
                        {
                            Log.Error("Did not find tax amount to collect in tax calculation JSON response.");
                        }
                    }
                }
                catch (JsonReaderException ex)
                {
                    Log.Error("Failed to read calculated sales tax JSON (Exc:" + ex.Message + ")");
                }
            }
            return salesTaxCalcData;
        }


        /// <summary>
        /// Transforms tax rate TaxJar JSON output into common/unified tax rate structure.
        /// </summary>
        /// <param name="taxRateResponseOutput"></param>
        /// <returns>The common tax rate output, or NULL if unable to parse data.</returns>
        protected SalesTaxRateData? TransformGetTaxRatesRequestOutput(string taxRateResponseOutput)
        {
            SalesTaxRateData? salesTaxRateData = null;

            // Expected result.
            /*
            {"rate":{
                "city":null,
                "city_rate":"0.0",
                "combined_district_rate":"0.0",
                "combined_rate":"0.0625",
                "country":"US","country_rate":"0.0",
                "county":null,"county_rate":"0.0",
                "freight_taxable":false,
                "state":"MA",
                "state_rate":"0.0625",
                "zip":"01463"}}
            */

            // Parse the calculated tax rate data only if it is not NULL.  If it is NULL,
            // the caller will log an error.
            if (taxRateResponseOutput != null)
            {
                try
                {
                    // Parse tax rate data.
                    JObject jsonTaxRateData = JObject.Parse(taxRateResponseOutput);
                    // Console.WriteLine(jsonTaxRateData.ToString());

                    JToken? taxRateJson = jsonTaxRateData["rate"];
                    if (taxRateJson != null)
                    {
                        JToken? combinedRateJson = taxRateJson["combined_rate"];

                        JToken? combinedDistrictRateJson = taxRateJson["combined_district_rate"];

                        JToken? countryRateJson = taxRateJson["country_rate"];

                        JToken? stateRateJson = taxRateJson["state_rate"];

                        JToken? countyRateJson = taxRateJson["county_rate"];

                        JToken? cityRateJson = taxRateJson["city_rate"];

                        JToken? freightTaxableJson = taxRateJson["freight_taxable"];

                        JToken? countryJson = taxRateJson["country"];

                        JToken? zipJson = taxRateJson["zip"];

                        JToken? stateJson = taxRateJson["state"];

                        JToken? countyJson = taxRateJson["county"];

                        JToken? cityJson = taxRateJson["city"];

                        salesTaxRateData = new SalesTaxRateData()
                        {
                            CombinedRate = (double?)combinedRateJson,
                            CountryRate = (double?)countryRateJson,
                            StateRate = (double?)stateRateJson,
                            CountyRate = (double?)countyRateJson,
                            CityRate = (double?)cityRateJson,
                            Country = (string?)countryJson,
                            State = (string?)stateJson,
                            County = (string?)countyJson,
                            City = (string?)cityJson,
                            Zip = (string?)zipJson,
                            FreightTaxable = (bool?)freightTaxableJson,
                        };
                    }
                }
                catch (JsonReaderException ex)
                {
                    Log.Error("Failed to parse sales tax rate JSON data (Exc:" + ex.Message + ")");
                }
            }
            return salesTaxRateData;
        }
    }

}

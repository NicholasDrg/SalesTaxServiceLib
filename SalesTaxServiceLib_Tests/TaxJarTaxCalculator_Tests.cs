using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Newtonsoft.Json.Linq;


namespace SalesTaxServiceLib.Tests
{
    [TestClass()]
    public class TaxJarTaxCalculator_Tests
    {
        /// <summary>
        /// Test TaxJar request for tax calculation of valid order.
        /// </summary>
        [TestMethod()]
        public void CalculateTax_SingleItemOrder_Test()
        {
            SalesTaxCalcRequestInput taxCalcRequestInput = new SalesTaxCalcRequestInput()
            {
                from_country = "US",
                from_zip = "92093",
                from_state = "CA",
                from_city = "La Jolla",
                from_street = "9500 Gilman Drive",
                to_country = "US",
                to_zip = "90002",
                to_state = "CA",
                to_city = "Los Angeles",
                to_street = "1335 E 103rd St",
                amount = 15,
                shipping = 1.5,
            };

            taxCalcRequestInput.AddNexusAddress(new SalesTaxCalcRequestInput.NexusAddress()
            {
                id = "Main Location",
                country = "US",
                zip = "92093",
                state = "CA",
                city = "La Jolla",
                street = "9500 Gilman Drive",
            });

            taxCalcRequestInput.AddLineItem(new SalesTaxCalcRequestInput.LineItem()
            {
                id = "1",
                quantity = 1,
                product_tax_code = "20010",
                unit_price = 15,
                discount = 0
            });

            TaxJarTaxCalculator taxJarCalculatorObj = new TaxJarTaxCalculator();
            SalesTaxCalcResults requestResults = taxJarCalculatorObj.CalculateTax(taxCalcRequestInput);

            Assert.AreEqual(SalesTaxService.TaxRequestStatusEnum.SUCCESS, requestResults.RequestStatus);
            Assert.IsNull(requestResults.ProviderRequestErrorData);
            Assert.IsNotNull(requestResults.TaxCalcData);
            if (requestResults.TaxCalcData != null)
            {
                SalesTaxCalcData taxCalcData = (SalesTaxCalcData)requestResults.TaxCalcData;
                Console.WriteLine("From/To Zip: " + taxCalcRequestInput.from_zip + " / " + taxCalcRequestInput.from_zip);
                Console.WriteLine("Taxable Amount: " + taxCalcData.TaxableAmount);
                Console.WriteLine("Tax Rate: " + taxCalcData.Rate);
                Console.WriteLine("Tax Amount to Collect: " + taxCalcData.AmountToCollect);

                Assert.AreEqual((double)1.43, taxCalcData.AmountToCollect);
                Assert.AreEqual(false, taxCalcData.FreightTaxable);
                Assert.AreEqual(16.5, taxCalcData.OrderTotalAmount);
                Assert.AreEqual(0.095, taxCalcData.Rate);
                Assert.AreEqual(15, taxCalcData.TaxableAmount);
                Assert.AreEqual(1.5, taxCalcData.Shipping);
            }
        }


        /// <summary>
        /// Test TaxJar request for tax calculation of invalid order request input.
        /// </summary>
        [TestMethod()]
        public void CalculateTax_IncompleteRequestInput_Test()
        {
            SalesTaxCalcRequestInput salesOrderTaxCalcQueryInput = new SalesTaxCalcRequestInput()
            {
                from_country = "US",
                from_zip = "92093",
                to_country = "US",
                to_zip = "90002",
                amount = 15,
                shipping = 1.5,
            };

            TaxJarTaxCalculator taxJarCalculatorObj = new TaxJarTaxCalculator();
            SalesTaxCalcResults salesTaxCalcResults = taxJarCalculatorObj.CalculateTax(salesOrderTaxCalcQueryInput);

            Assert.AreEqual(SalesTaxService.TaxRequestStatusEnum.FAIL, salesTaxCalcResults.RequestStatus);
            Assert.IsNotNull(salesTaxCalcResults.ProviderRequestErrorData);
            Assert.AreEqual("HTTP response code: 400 - BadRequest", salesTaxCalcResults.ProviderRequestErrorData);
            Assert.IsNull(salesTaxCalcResults.TaxCalcData);
        }


        /// <summary>
        /// Tests retrieving data rates from TaxJar for zip code only.
        /// </summary>
        /// <param name="zipCode">Zip Code</param>
        /// <param name="expState">Expected state</param>
        /// <param name="expStateRate">Expected state rate</param>
        /// <param name="expCombinedRate">Expected combined rate</param>
        [DataTestMethod()]
        [DataRow("01463", "MA", 0.0625, 0.0625)]
        [DataRow("90404-3370", "CA", 0.0625, 0.1025)]
        public void GetTaxRate_ZipOnly_Tests(string zipCode, string expState, double expStateRate, double expCombinedRate)
        {
            SalesTaxRateRequestInput salesTaxRateQueryInput = new SalesTaxRateRequestInput()
            {
                ZipCode = zipCode,
            };
            TaxJarTaxCalculator taxJarCalculatorObj = new TaxJarTaxCalculator();
            SalesTaxRateResults taxRateResults = taxJarCalculatorObj.GetTaxRate(salesTaxRateQueryInput);
            Assert.AreEqual(SalesTaxService.TaxRequestStatusEnum.SUCCESS, taxRateResults.RequestStatus);
            Assert.IsNull(taxRateResults.ProviderRequestErrorData);
            Assert.IsNotNull(taxRateResults.TaxRateData);
            if (taxRateResults.TaxRateData != null)
            {
                SalesTaxRateData taxRateData = (SalesTaxRateData)taxRateResults.TaxRateData;
                Assert.IsNotNull(taxRateData.Zip);
                Assert.AreEqual(zipCode, taxRateData.Zip);

                Assert.IsNotNull(taxRateData.State);
                Assert.AreEqual(expState, taxRateData.State);

                Assert.IsNotNull(taxRateData.StateRate);
                Assert.AreEqual(expStateRate, taxRateData.StateRate);

                Assert.IsNotNull(taxRateData.CombinedRate);
                Assert.AreEqual(expCombinedRate, taxRateData.CombinedRate);
            }
        }


        /// <summary>
        /// Tests retrieving data rates from TaxJar for complete location data (not just Zip).
        /// </summary>
        /// <param name="zipCode"></param>
        /// <param name="expState"></param>
        /// <param name="expCity"></param>
        /// <param name="expStateRate"></param>
        /// <param name="expCombinedRate"></param>
        /// <remarks>
        /// Note: Not sure testing actual tax rate values from TaxJar in every case would
        /// work well in the long term since tests will fail with any tax rate changes.
        /// (See Williston, VT note below).  Maybe testing against TaxJar should be limited
        /// and confined to TaxJarTaxCalculator testing in order to pick up issues with the
        /// service.  However, more extensive data testing could be done with mocks.
        /// </remarks>
        [DataTestMethod()]
        [DataRow("90404", "CA", "Santa Monica", null, 0.0625, 0.1025)]
        [DataRow("05452", "VT", "Essex Junction", null, 0.06, 0.06)]
        [DataRow("92093", "CA", "La Jolla", "9500 Gilman Drive", 0.0625, 0.0775)]
        // Note: TaxJar now returns a NULL city for Williston, VT (one of their sample data).
        // So, tax rate data does change now and then.
        //[DataRow("05495", "VT", "Williston", "0.06", "0.07")]
        public void GetTaxRate_SpecificAddress_Tests(string zipCode, string expState, string expCity, string? expStreet, double expStateRate, double expCombinedRate)
        {
            SalesTaxRateRequestInput salesTaxRateQueryInput = new SalesTaxRateRequestInput()
            {
                ZipCode = zipCode,
                Country = "US",
                StateProvince = expState,
                City = expCity,
                Street = expStreet,
            };
            TaxJarTaxCalculator taxJarCalculatorObj = new TaxJarTaxCalculator();
            SalesTaxRateResults taxRateResults = taxJarCalculatorObj.GetTaxRate(salesTaxRateQueryInput);
            Assert.AreEqual(SalesTaxService.TaxRequestStatusEnum.SUCCESS, taxRateResults.RequestStatus);
            Assert.IsNull(taxRateResults.ProviderRequestErrorData);
            Assert.IsNotNull(taxRateResults.TaxRateData);
            if (taxRateResults.TaxRateData != null)
            {
                SalesTaxRateData taxRateData = (SalesTaxRateData)taxRateResults.TaxRateData;

                Assert.IsNotNull(taxRateData.Zip);
                Assert.AreEqual(zipCode, taxRateData.Zip);

                Assert.IsNotNull(taxRateData.State);
                Assert.AreEqual(expState, taxRateData.State);

                Assert.IsNotNull(taxRateData.City);
                Assert.AreEqual(expCity.ToUpper(), taxRateData.City.ToUpper());

                Assert.IsNotNull(taxRateData.StateRate);
                Assert.AreEqual(expStateRate, taxRateData.StateRate);

                Assert.IsNotNull(taxRateData.CombinedRate);
                Assert.AreEqual(expCombinedRate, taxRateData.CombinedRate);

                Console.WriteLine(taxRateData.Zip);
                Console.WriteLine(taxRateData.State);
                Console.WriteLine(taxRateData.City);
                Console.WriteLine("State Rate: " + taxRateData.StateRate);
                Console.WriteLine("Combined Rate: " + taxRateData.CombinedRate);
            }
        }


        /// <summary>
        /// Tests the generation of the JSON data needed by a TaxJar tax calculation request.
        /// </summary>
        [TestMethod()]
        public void GenerateCalculateTaxRequestInput_Test()
        {
            SalesTaxCalcRequestInput taxCalcRequestInput = new SalesTaxCalcRequestInput()
            {
                from_country = "US",
                from_zip = "92093",
                from_state = "CA",
                from_city = "La Jolla",
                from_street = "9500 Gilman Drive",
                to_country = "US",
                to_zip = "90002",
                to_state = "CA",
                to_city = "Los Angeles",
                to_street = "1335 E 103rd St",
                amount = 15,
                shipping = 1.5,
            };

            taxCalcRequestInput.AddNexusAddress(new SalesTaxCalcRequestInput.NexusAddress()
            {
                id = "Main Location",
                country = "US",
                zip = "92093",
                state = "CA",
                city = "La Jolla",
                street = "9500 Gilman Drive",
            });

            taxCalcRequestInput.AddLineItem(new SalesTaxCalcRequestInput.LineItem()
            {
                id = "1",
                quantity = 1,
                product_tax_code = "20010",
                unit_price = 15,
                discount = 0
            });

            TaxJarTaxCalculatorTester taxJarCalculatorObj = new TaxJarTaxCalculatorTester();
            string taxCalcReqInputStr = (string)taxJarCalculatorObj.GenerateCalculateTaxRequestInputTester(taxCalcRequestInput);
            //Console.WriteLine(taxCalcReqInputStr);
            const string EXP_TAXCALC_REQ_INPUT_STR = @"{""nexus_addresses"":[{""id"":""Main Location"",""country"":""US"",""zip"":""92093"",""state"":""CA"",""city"":""La Jolla"",""street"":""9500 Gilman Drive""}],""line_items"":[{""id"":""1"",""quantity"":1,""product_tax_code"":""20010"",""unit_price"":15.0,""discount"":0.0}],""from_country"":""US"",""from_zip"":""92093"",""from_state"":""CA"",""from_city"":""La Jolla"",""from_street"":""9500 Gilman Drive"",""to_country"":""US"",""to_zip"":""90002"",""to_state"":""CA"",""to_city"":""Los Angeles"",""to_street"":""1335 E 103rd St"",""amount"":15.0,""shipping"":1.5}";
            Assert.AreEqual(EXP_TAXCALC_REQ_INPUT_STR, taxCalcReqInputStr);
        }


        /// <summary>
        /// Tests the creation of a TaxJar URL with query parameters based on input.
        /// </summary>
        /// <param name="zipCode"></param>
        /// <param name="expState"></param>
        /// <param name="expCity"></param>
        /// <param name="expStreet"></param>
        /// <param name="expStateRate"></param>
        /// <param name="expCombinedRate"></param>
        /// <remarks>
        /// Note: I understand that testing protected and private methods is not recommended
        /// according to various online sources.  I can see only part of the benefit. In this
        /// case it was very useful to test how input was transformed correctly.
        /// </remarks>
        [DataTestMethod()]
        [DataRow("90404", "US", "CA", "Santa Monica", null, "https://api.taxjar.com/v2/rates/90404?country=US&state=CA&city=Santa+Monica")]
        [DataRow("01463", "US", "MA", null, null, "https://api.taxjar.com/v2/rates/01463?country=US&state=MA")]
        [DataRow("01463", null, null, null, null, "https://api.taxjar.com/v2/rates/01463")]
        public void GenerateTaxRateRequestInput_Test(string zipCode, string expCountry, string expState, string expCity, string? expStreet, string expUrl)
        {
            SalesTaxRateRequestInput salesTaxRateRequestInput = new SalesTaxRateRequestInput()
            {
                ZipCode = zipCode,
                Country = expCountry,
                StateProvince = expState,
                City = expCity,
                Street = expStreet,
            };
            TaxJarTaxCalculatorTester taxJarCalculatorObj = new TaxJarTaxCalculatorTester();
            string taxRateRequestUrl = (string)taxJarCalculatorObj.GenerateTaxRateRequestInputTester(salesTaxRateRequestInput);
            //Console.WriteLine(taxRateRequestUrl);
            Assert.IsNotNull(taxRateRequestUrl);
            Assert.AreEqual(expUrl, taxRateRequestUrl);
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void TransformCalculateTaxRequestOutput_EmptyResponse_Test()
        {
            const string requestOutput = "";
            TaxJarTaxCalculatorTester taxJarCalculatorObj = new TaxJarTaxCalculatorTester();
            SalesTaxCalcData? taxCalcData = taxJarCalculatorObj.TransformCalculateTaxResponseOutputTester(requestOutput);
            Assert.IsNull(taxCalcData);
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void TransformCalculateTaxRequestOutput_InvalidResponse_Test()
        {
            const string invalidJsonResponseOutput = @"
                {
                    'tax':{
                        'amount_to_collect':1.43,
                        'breakdown':{
                            'city_tax_collectable':0.0,
                            'city_tax_rate':0.0,
            ";
            TaxJarTaxCalculatorTester taxJarCalculatorObj = new TaxJarTaxCalculatorTester();
            SalesTaxCalcData? taxCalcData = taxJarCalculatorObj.TransformCalculateTaxResponseOutputTester(invalidJsonResponseOutput);
            Assert.IsNull(taxCalcData);
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void TransformCalculateTaxRequestOutput_Test()
        {
            const string requestOutput = @"
            {
                'tax':{
                    'amount_to_collect':1.43,
                    'breakdown':{
                        'city_tax_collectable':0.0,
                        'city_tax_rate':0.0,
                        'city_taxable_amount':0.0,
                        'combined_tax_rate':0.095,
                        'county_tax_collectable':0.15,
                        'county_tax_rate':0.01,'county_taxable_amount':15.0,
                        'line_items':[{
                            'city_amount':0.0,
                            'city_tax_rate':0.0,
                            'city_taxable_amount':0.0,
                            'combined_tax_rate':0.095,
                            'county_amount':0.15,
                            'county_tax_rate':0.01,
                            'county_taxable_amount':15.0,'id':'1',
                            'special_district_amount':0.34,
                            'special_district_taxable_amount':15.0,'special_tax_rate':0.0225,'state_amount':0.94,
                            'state_sales_tax_rate':0.0625,'state_taxable_amount':15.0,
                            'tax_collectable':1.43,'taxable_amount':15.0}],
                        'special_district_tax_collectable':0.34,
                        'special_district_taxable_amount':15.0,
                        'special_tax_rate':0.0225,'state_tax_collectable':0.94,'state_tax_rate':0.0625,
                        'state_taxable_amount':15.0,'tax_collectable':1.43,'taxable_amount':15.0
                    },
                    'freight_taxable':false,
                    'has_nexus':true,
                    'jurisdictions':{ 'city':'LOS ANGELES','country':'US','county':'LOS ANGELES COUNTY','state':'CA'},
                    'order_total_amount':16.5,
                    'rate':0.095,
                    'shipping':1.5,
                    'tax_source':'destination',
                    'taxable_amount':15.0
                }
            }";

            TaxJarTaxCalculatorTester taxJarCalculatorObj = new TaxJarTaxCalculatorTester();
            SalesTaxCalcData? taxCalcData = taxJarCalculatorObj.TransformCalculateTaxResponseOutputTester(requestOutput);
            Assert.IsNotNull(taxCalcData);
            if (taxCalcData != null)
            {
                SalesTaxCalcData salesTaxCalcData = (SalesTaxCalcData)taxCalcData;
                Assert.AreEqual(1.43, salesTaxCalcData.AmountToCollect);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void TransformGetTaxRatesRequestOutput_Test()
        {
            const string requestOutput = @"
                {'rate':{
                    'city':null,
                    'city_rate':'0.0',
                    'combined_district_rate':'0.0',
                    'combined_rate':'0.0625',
                    'country':'US','country_rate':'0.0',
                    'county':null,'county_rate':'0.0',
                    'freight_taxable':false,
                    'state':'MA',
                    'state_rate':'0.0625',
                    'zip':'01463'}}";

            TaxJarTaxCalculatorTester taxJarCalculatorObj = new TaxJarTaxCalculatorTester();
            SalesTaxRateData? taxRateData = taxJarCalculatorObj.TransformGetTaxRatesRequestOutputTester(requestOutput);
            Assert.IsNotNull(taxRateData);
            if (taxRateData != null)
            {
                SalesTaxRateData salesTaxRateData = (SalesTaxRateData)taxRateData;
                Assert.IsNotNull(salesTaxRateData.CombinedRate);
                Assert.AreEqual(0.0625, salesTaxRateData.CombinedRate);
            }
        }
    }
}

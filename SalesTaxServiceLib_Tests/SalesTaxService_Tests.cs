using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace SalesTaxServiceLib.Tests
{
    [TestClass()]
    public class SalesTaxService_Tests
    {
        /// <summary>
        /// Test passing NULL calculator object to SalesTaxService constructor.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException), "Null TaxCalculator object passed.")]
        public void SalesTaxServiceConstructorNullTaxCalculatorArg_Test()
        {
            ISalesTaxCalculator? taxCalculatorObj = null;
            SalesTaxService taxServiceObj = new SalesTaxService(taxCalculatorObj);
        }


        /// <summary>
        /// Test TaxJar request for tax calculation of valid order.
        /// </summary>
        [TestMethod()]
        public void CalculateTax_SingleItemOrder_TaxJar_Test()
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

            ISalesTaxCalculator taxCalcObj = new TaxJarTaxCalculator();
            SalesTaxService taxServiceObj = new SalesTaxService(taxCalcObj);

            SalesTaxCalcResults requestResults = taxServiceObj.CalculateTax(taxCalcRequestInput);
            Assert.AreEqual(SalesTaxService.TaxRequestStatusEnum.SUCCESS, requestResults.RequestStatus);
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
        /// Test TaxJar request for tax calculation of order with invalid request input.
        /// </summary>
        [TestMethod()]
        public void CalculateTax_IncompleteRequestInput_TaxJar_Test()
        {
            SalesTaxCalcRequestInput salesTaxCalcRequestInput = new SalesTaxCalcRequestInput()
            {
                from_country = "US",
                from_zip = "92093",
                to_country = "US",
                to_zip = "90002",
                amount = 15,
                shipping = 1.5,
            };

            ISalesTaxCalculator taxCalcObj = new TaxJarTaxCalculator();
            SalesTaxService taxServiceObj = new SalesTaxService(taxCalcObj);

            SalesTaxCalcResults requestResults = taxServiceObj.CalculateTax(salesTaxCalcRequestInput);
            Assert.AreEqual(SalesTaxService.TaxRequestStatusEnum.FAIL, requestResults.RequestStatus);
            Assert.IsNull(requestResults.TaxCalcData);
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void CalculateTax_StubTaxCalc_Test()
        {
            SalesTaxCalcRequestInput salesTaxCalcRequestInput = new SalesTaxCalcRequestInput()
            {
                from_country = "US",
                from_zip = "01463",
                to_country = "US",
                to_zip = "01463",
                amount = 100,
                shipping = 1.5,
            };
            ISalesTaxCalculator taxCalcObj = new StubTaxCalculator();
            SalesTaxService taxServiceObj = new SalesTaxService(taxCalcObj);

            SalesTaxCalcResults requestResults = taxServiceObj.CalculateTax(salesTaxCalcRequestInput);
            Assert.AreEqual(SalesTaxService.TaxRequestStatusEnum.SUCCESS, requestResults.RequestStatus);
            Assert.IsNotNull(requestResults.TaxCalcData);
            if (requestResults.TaxCalcData != null)
            {
                SalesTaxCalcData taxCalcData = (SalesTaxCalcData)requestResults.TaxCalcData;
                Assert.AreEqual((double)6.5, taxCalcData.AmountToCollect);
                Assert.AreEqual(false, taxCalcData.FreightTaxable);
                Assert.AreEqual(101.5, taxCalcData.OrderTotalAmount);
                Assert.AreEqual(0.065, taxCalcData.Rate);
                Assert.AreEqual(100, taxCalcData.TaxableAmount);
                Assert.AreEqual(1.5, taxCalcData.Shipping);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void GetTaxRate_TaxJar_Test()
        {
            const string EXP_ZIP = "01463";
            const string EXP_STATE = "MA";
            const double EXP_STATE_RATE = 0.0625;
            const double EXP_COMBINED_RATE = 0.0625;

            ISalesTaxCalculator taxCalcObj = new TaxJarTaxCalculator();
            SalesTaxService taxServiceObj = new SalesTaxService(taxCalcObj);
            SalesTaxRateResults requestResults = taxServiceObj.GetTaxRate("01463");
            Assert.AreEqual(SalesTaxService.TaxRequestStatusEnum.SUCCESS, requestResults.RequestStatus);
            Assert.IsNotNull(requestResults.TaxRateData);
            //Console.WriteLine(requestResults.TaxRateData);

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

            if(requestResults.TaxRateData != null)
            {
                SalesTaxRateData taxRateData = (SalesTaxRateData)requestResults.TaxRateData;
                Assert.IsNotNull(taxRateData.Zip);
                Assert.AreEqual(EXP_ZIP, taxRateData.Zip);

                Assert.IsNotNull(taxRateData.State);
                Assert.AreEqual(EXP_STATE, taxRateData.State);

                Assert.IsNotNull(taxRateData.StateRate);
                Assert.AreEqual(EXP_STATE_RATE, taxRateData.StateRate);

                Assert.IsNotNull(taxRateData.CombinedRate);
                Assert.AreEqual(EXP_COMBINED_RATE, taxRateData.CombinedRate);

                Console.WriteLine(taxRateData.Zip);
                Console.WriteLine(taxRateData.State);
                Console.WriteLine("State Rate: " + taxRateData.StateRate);
                Console.WriteLine("Combined Rate: " + taxRateData.CombinedRate);
            }
        }


        /// <summary>
        /// Test retrieving TaxJar tax rates for invalid zip code.
        /// </summary>
        [TestMethod()]
        public void GetTaxRate_InvalidZip_TaxJar_Test()
        {
            SalesTaxRateRequestInput taxRateRequestInput = new SalesTaxRateRequestInput()
            {
                ZipCode = "",
            };
            ISalesTaxCalculator taxCalcObj = new TaxJarTaxCalculator();
            SalesTaxService taxServiceObj = new SalesTaxService(taxCalcObj);
            SalesTaxRateResults requestResults = taxServiceObj.GetTaxRate(taxRateRequestInput);
            Assert.AreEqual(SalesTaxService.TaxRequestStatusEnum.FAIL, requestResults.RequestStatus);
            Assert.IsNull(requestResults.TaxRateData);
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void GetTaxRate_StubTaxCalc_Test()
        {
            SalesTaxRateRequestInput salesTaxRateQueryInput = new SalesTaxRateRequestInput()
            {
                ZipCode = "01463",
            };
            ISalesTaxCalculator taxCalcObj = new StubTaxCalculator();
            SalesTaxService taxServiceObj = new SalesTaxService(taxCalcObj);
            SalesTaxRateResults requestResults = taxServiceObj.GetTaxRate(salesTaxRateQueryInput);
            Assert.AreEqual(SalesTaxService.TaxRequestStatusEnum.SUCCESS, requestResults.RequestStatus);
            Assert.IsNotNull(requestResults.TaxRateData);

            const string EXP_ZIP = "01463";
            const string EXP_STATE = "MA";
            const double EXP_STATE_RATE = 0.0625;
            const double EXP_COMBINED_RATE = 0.0625;

            if (requestResults.TaxRateData != null)
            {
                SalesTaxRateData taxRateData = (SalesTaxRateData)requestResults.TaxRateData;
                Assert.IsNotNull(taxRateData.Zip);
                Assert.AreEqual(EXP_ZIP, taxRateData.Zip);

                Assert.IsNotNull(taxRateData.State);
                Assert.AreEqual(EXP_STATE, taxRateData.State);

                Assert.IsNotNull(taxRateData.StateRate);
                Assert.AreEqual(EXP_STATE_RATE, taxRateData.StateRate);

                Assert.IsNotNull(taxRateData.CombinedRate);
                Assert.AreEqual(EXP_COMBINED_RATE, taxRateData.CombinedRate);
            }
        }

    }
}

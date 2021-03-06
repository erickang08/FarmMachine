using System;
using FarmMachine.Domain.Services;
using Xunit;

namespace FarmMachine.Tests.Tests
{
  public class CurrencyConverterTest
  {
    [Fact]
    public void TestConvertUSDToBTC()
    {
      var tradeCalc = new TradeCalcService();
      var amountUSD = 18m;
      var price = 10491.94100000m;

      var result = decimal.Round(tradeCalc.GetBuyAmount(amountUSD, price), 8);

      Assert.Equal(result, 0.00171131m);
    }
    
    [Fact]
    public void TestConvertBTCToUSD()
    {
      var tradeCalc = new TradeCalcService();
      var amountBTC = 0.00171131m;
      var price = 10491.94100000m;

      var result = decimal.Round(tradeCalc.GetSellAmount(amountBTC, price), 8);

      Assert.Equal(result, 17.91007614m);
    }
  }
}
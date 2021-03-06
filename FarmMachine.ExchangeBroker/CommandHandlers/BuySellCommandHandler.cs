using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FarmMachine.Domain.Commands.Exchange;
using FarmMachine.Domain.Services;
using FarmMachine.ExchangeBroker.Exchanges;
using MassTransit;
using MongoDB.Driver;
using Serilog;

namespace FarmMachine.ExchangeBroker.CommandHandlers
{
  public class BuySellCommandHandler : IConsumer<BuyCurrency>, IConsumer<SellCurrency>
  {
//    public IMongoCollection<BsonDocument> _protocol;
    private IBittrexExchange _exchange;
    private IProtocolService _protocolService;
    
    public BuySellCommandHandler(IMongoDatabase database, IBittrexExchange exchange, IProtocolService protocolService)
    {
      _exchange = exchange;
//      _protocol = database.GetCollection<BsonDocument>("protocol");
      _protocolService = protocolService;
    }
    
    public async Task Consume(ConsumeContext<BuyCurrency> context)
    {
      var amountInCurrency = await _exchange.RiskManager.GetActualBuyAmount();
      var rate = await _exchange.GetActualBuyPrice();
      var converter = new TradeCalcService();
      var amount = decimal.Round(converter.GetBuyAmount(amountInCurrency, rate), 8);
      var balanceMinLimit = _exchange.RiskManager.GetBalanceMinLimit();
      
      Log.Information($"GET[BUY] => USD amount [{amount} / {amountInCurrency}] rate [{rate}]");

      if (amountInCurrency <= balanceMinLimit)
      {
        Log.Warning($"Balance equal {amountInCurrency}. Risk manager stopping FarmMachine.ExchangeBroker");
        
        return;
      }

      try
      {
        await _protocolService.WriteAsync(new Dictionary<string, object>
        {
          {"_id", context.Message.Id},
          {"amount", amount},
          {"bid", context.Message.Bid},
          {"rate", rate},
          {"timestamp", context.Message.Created},
          {"type", "buy"}
        }, "BuyCurrency");
      }
      catch (Exception ex)
      {
        Log.Warning($"Invalid write to db: {ex}");
      }

//      await _exchange.PlaceOrderOnBuy(amount, rate);
    }

    public async Task Consume(ConsumeContext<SellCurrency> context)
    {
      var amountInCurrency = await _exchange.RiskManager.GetActualSellAmount();
      var rate = await _exchange.GetActualSellPrice();
      var converter = new TradeCalcService();
      var amount = decimal.Round(converter.GetSellAmount(amountInCurrency, rate), 8);
      var balanceMinLimit = _exchange.RiskManager.GetBalanceMinLimit();
      
      Log.Information($"GET[SELL] => USD amount [{amount} / {amountInCurrency}] rate [{rate}]");

      if (amount <= balanceMinLimit)
      {
        Log.Warning($"Balance equal {amountInCurrency}. Risk manager stopping FarmMachine.ExchangeBroker");

        return;
      }
      
      try
      {
        await _protocolService.WriteAsync(new Dictionary<string, object>
        {
          {"_id", context.Message.Id},
          {"amount", context.Message.Amount},
          {"ask", context.Message.Ask},
          {"rate", rate},
          {"timestamp", context.Message.Created},
          {"type", "sell"}
        }, "SellCurrency");
      }
      catch (Exception ex)
      {
        Log.Warning($"Invalid write to db: {ex}");
      }

//      await _exchange.PlaceOrderOnSell(amountInCurrency, rate);
    }
  }
}
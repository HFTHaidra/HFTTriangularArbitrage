using Binance.Net.Clients;
using Bybit.Net.Clients;
using CryptoExchange.Net.Authentication;
using Io.Gate.GateApi.Client;
using Io.Gate.GateApi.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Websocket.Client;

namespace Futures
{
    internal class BinanceFuture
    {
        public string Key = "";
        public string SecretKey = "";
        public List<String> contracts = new List<String>();
        public string symbol = "";
        public Book book = new Book();
        public decimal balance = 0;
        public decimal BalanceRunVale = 0;
        public bool contractsOK = false;
        public BinanceSocketClient socketClient = new BinanceSocketClient();
        public dynamic tickerSubscriptionResult;
        public WebsocketClient client = new WebsocketClient(new Uri("wss://fx-ws.gateio.ws/v4/ws/usdt"));
        public bool clientWebsocket = false;
        public static List<string> msg = new List<string>();

        public double SellSize = 0;
        public double BuySize = 0;
        BinanceRestClient binance = new BinanceRestClient(options => { options.ApiCredentials = new ApiCredentials(key: "2ed2ed2e", secret: "d2ed3wefd3ed3"); });

        public static void c(string msg)
        {
            Fast.msg.Add(msg);
        }
        public BinanceFuture() { }
        public BinanceFuture(string key, string SecretKey_)
        {
            this.Key = key;
            this.SecretKey = SecretKey_;
            this.binance = new BinanceRestClient(options => {
                options.ApiCredentials = new ApiCredentials(key: Key, secret: SecretKey);
            });
        }
        public decimal Balance(string Coin = "USDT")
        {
            decimal res = 0;
            BalanceRunVale = 0;
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                await BalanceRun();

            }).Start();
            int i = 50;
            while (i >= 1)
            {
                if (BalanceRunVale != 0)
                {
                    res = BalanceRunVale;
                    i = -1;
                    break;
                }
                else
                {
                    i--;
                    Thread.Sleep(100);
                }

            }

            return res;
        }
        public async Task BalanceRun(string Coin = "USDT")
        {

            try
            {
                try
                {
                    var binanceClient = new BinanceRestClient(options => {
                        options.ApiCredentials = new ApiCredentials(key: Key, secret: SecretKey);
                    });

                    var result = await binanceClient.UsdFuturesApi.Account.GetBalancesAsync(receiveWindow: 50000);

                    if (result.Success)
                    {
                        var bb = result.Data;
                        foreach (var item in bb)
                        {
                            //c(" BinanceSpot:"+item.Asset+" : "+item.Available); 
                            if (item.Asset == Coin)
                            {
                                BalanceRunVale = item.AvailableBalance;
                            }
                        }
                    }
                    else
                    {
                        c("BalanceRun:" + result.Error.Message);
                    }
                }
                catch (Exception e) { c(e.Message); }
            }
            catch (Exception ex) { c("C:" + ex.Message); }
        }
        public int Contracts()
        {
            int res = 0;
            contractsOK = false;

            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                await GetInsuranceAsync();

            }).Start();
            int i = 50;
            while (i >= 1)
            {
                if (contractsOK)
                {
                    res = contracts.Count;
                    i = -1;
                    break;
                }
                else
                {
                    i--;
                    Thread.Sleep(100);
                }

            }



            return res;
        }
        public async Task GetInsuranceAsync()
        {
            try
            {
                contracts = new List<String>();


                var binanceClient = new BinanceRestClient(options => {
                    options.ApiCredentials = new ApiCredentials(key: Key, secret: SecretKey);
                });

                var result = await binanceClient.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync();

                if (result.Success)
                {
                    var bb = result.Data.Symbols;
                    foreach (var b in bb)
                    {
                        if (b != null)
                        {
                            contracts.Add(b.Name);
                        }
                    }
                }
                else
                {
                    c(result.Error.Message);
                }




            }
            catch (Exception ex) { c(ex.Message); }
        }
        public async Task StartStreams(string sy)
        {
            try
            {
                if (clientWebsocket == false)
                {
                    if (sy.Length > 3)
                    {
                        clientWebsocket = true;
                        this.socketClient = new BinanceSocketClient();
                        this.tickerSubscriptionResult = socketClient.UsdFuturesApi.ExchangeData.SubscribeToBookTickerUpdatesAsync(sy, async (update) =>
                        {
                            await MessageReceived(update.Data.BestAskPrice, update.Data.BestBidPrice);
                        });
                    }

                }
                else
                {
                    c(" Binance Socket  already send the subscription request ");
                }

            }
            catch (Exception ex) { c(ex.Message); }
        }
        public async Task MessageReceived(decimal ask_, decimal bid_)
        {
            try
            {
                new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    if (ask_ != 0 && bid_ != 0)
                    {
                        book.ask = ask_;
                        book.bid = bid_;
                        book.spread = ask_ - bid_;
                        book.spread_ = Math.Round((book.spread * 100) / book.bid, 3);
                        book.time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        if (book.spread_ > book.MaxSpread_)
                            book.MaxSpread_ = book.spread_;
                        book.done = true;

                        new Thread(async () => { Thread.CurrentThread.IsBackground = true; await Processor.Run(); }).Start();
                    }
                }).Start();
            }
            catch (Exception ex) { c(ex.Message); }
        }
        public async Task UnStartStreams(string sy)
        {
            try
            {

                if (clientWebsocket)
                {
                    c(" Don't  unsubscribe this Binance, Binance Socket  already send the subscription request  ");
                }
            }
            catch (Exception ex) { c(ex.Message); }
        }
        public async Task Sell()
        {
            try
            {
                long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();


                var result = await binance.UsdFuturesApi.Trading.PlaceOrderAsync(
                    symbol,
                    Binance.Net.Enums.OrderSide.Sell,
                    Binance.Net.Enums.FuturesOrderType.Market,
                    (decimal)SellSize
                    );

                if (result.Success)
                {
                    var price = result.Data.Price;
                    var avprice = result.Data.Price;
                    long ms = DateTimeOffset.Now.ToUnixTimeMilliseconds() - milliseconds;
                    c("Fast Buy Done On: " + ms + "ms , FillPrice: " + price + " avg:" + avprice);
                }
                else
                {
                    c(result.Error.Message);
                }


            }
            catch (Exception e) { c(e.Message); }
        }
        public async Task Buy()
        {
            try
            {
                long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();


                var result = await binance.UsdFuturesApi.Trading.PlaceOrderAsync(
                    symbol,
                    Binance.Net.Enums.OrderSide.Buy,
                    Binance.Net.Enums.FuturesOrderType.Market,
                    (decimal)SellSize
                    );

                if (result.Success)
                {
                    var price = result.Data.Price;
                    var avprice = result.Data.Price;
                    long ms = DateTimeOffset.Now.ToUnixTimeMilliseconds() - milliseconds;
                    c("Fast Sell Done On: " + ms + "ms , FillPrice: " + price + " avg:" + avprice);
                }
                else
                {
                    c(result.Error.Message);
                }


            }
            catch (Exception e) { c(e.Message); }
        }

        public async Task SendUpdate()
        {
            try
            {
                //long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();


                //var result = await binance.SpotApi.Trading.PlaceOrderAsync(
                //    "BTCUSDT",
                //    Binance.Net.Enums.OrderSide.Buy,
                //    Binance.Net.Enums.SpotOrderType.Market,
                //    (decimal)1000
                //    );

                //if (result.Success)
                //{
                //    //var price = result.Data.Price;
                //    //var avprice = result.Data.AverageFillPrice;
                //    long ms = DateTimeOffset.Now.ToUnixTimeMilliseconds() - milliseconds;
                //    //c("Fast Sell Done On: " + ms + "ms , FillPrice: " + price + " avg:" + avprice);
                //}
                //else
                //{
                //    //c(result.Error.Message);
                //}


            }
            catch (Exception e) { c(e.Message); }
        }
    }
}

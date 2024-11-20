using Io.Gate.GateApi.Api;
using Io.Gate.GateApi.Client;
using Io.Gate.GateApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client; 
using Newtonsoft.Json.Linq;

namespace Futures
{
    internal class GateFuture
    {
        public string Key = "";
        public string SecretKey = "";
        public List<String> contracts = new List<String>();
        public string symbol = "";
        public Book book = new Book();
        public decimal balance = 0;


        public string ordershosturl = "/futures/usdt/orders";
        public string leverage = "1";
        public Configuration config = new Configuration();
        public string settle = "usdt";
        public FuturesApi futuresApi;
        public WebsocketClient client = new WebsocketClient(new Uri("wss://fx-ws.gateio.ws/v4/ws/usdt"));
        public static List<string> msg = new List<string>();


        public double SellSize = 0;
        public double BuySize = 0;
        public FuturesApi futuresApi_ = new FuturesApi();
        public FuturesOrder futuresOrder_ = new FuturesOrder("BTCUSDT");
        public SpotApi spotApi = new SpotApi();
        public GateFuture() { }

        public GateFuture(string key, string secretkey)
        {
            this.Key = key;
            this.SecretKey = secretkey;
            config.BasePath = "https://api.gateio.ws/api/v4";
            config.SetGateApiV4KeyPair(Key, SecretKey);
            futuresApi = new FuturesApi(config);
            futuresApi_ = new FuturesApi(config);
            spotApi = new SpotApi(config);

        }
        public static void c(string msg)
        {
            Fast.msg.Add(msg);
        }
        public decimal Balance()
        {
            decimal res = 0;
            try
            {

                List<SpotAccount> result = spotApi.ListSpotAccounts("USDT");

                res = decimal.Parse( result[0].Available);
                
            }
            catch (Exception e) { c(e.Message); }
            return res;
        }
        public List<string> Contracts()
        {
            List<string> res = new List<string>();
            try
            { 
                List<CurrencyPair> result = spotApi.ListCurrencyPairs(); 
                foreach (CurrencyPair c in result)
                {
                    res.Add(c.Id);
                } 

            }
            catch (Exception ex) { c(ex.Message); }

            return res;
        }
        public async Task StartStreams(string sy)
        {
            try
            {
                futuresOrder_ = new FuturesOrder(symbol);
                string urls = "wss://api.gateio.ws/ws/v4/";
                Uri url = new Uri(urls);
                long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                string Subscribe = "{\"time\" : " + milliseconds + ", \"channel\" : \"spot.book_ticker\", \"event\": \"subscribe\", \"payload\" : [\"" + sy + "\",\"1\",\"0\"]}";
                client = new WebsocketClient(url);
                client.MessageReceived.Subscribe(async msg => await MessageReceived(msg));
                client.Start();
                client.Send(Subscribe);

            }
            catch (Exception ex) { c(ex.Message); }
        }
        public async Task MessageReceived(ResponseMessage msg)
        {
            try
            {
                new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    string text = msg.Text ?? "";

                    if (text != "" && text.Contains("fail"))
                    {
                        c(text);
                    }

                    if (text != "" && text.Contains("update"))
                    {
                        JObject json = JObject.Parse(text);
                        var data = json["result"];
                        if (data != null)
                        {
                            var a = data["a"];
                            var b = data["b"];
                            var contractm = data["t"];

                            decimal ASK = (decimal)(a ?? 0);
                            decimal BID = (decimal)(b ?? 0);
                            long time = (long)contractm;
                            if (ASK != 0 && BID != 0)
                            {
                                book.ask = ASK;
                                book.bid = BID;
                                book.spread = ASK - BID;
                                book.spread_ = Math.Round((book.spread * 100) / book.bid, 3);
                                book.time = time;
                                if (book.spread_ > book.MaxSpread_)
                                    book.MaxSpread_ = book.spread_;
                                book.done = true;
                                new Thread(async () => { Thread.CurrentThread.IsBackground = true; await Processor.Run(); }).Start();
                            }
                        }
                    }
                }).Start();
            }
            catch (Exception ex) { c(ex.Message); }
        }
        public async Task UnStartStreams(string sy)
        {
            try
            {

                client.Dispose();
                this.book = new Book();
                this.book.done = true;
            }
            catch (Exception ex) { c(ex.Message); }
        }


        public async Task Sell()
        {

            try
            {
                long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                //futuresOrder_ = new FuturesOrder(symbol)
                //{
                //    Size = -1 * (long)SellSize,
                //    Tif = FuturesOrder.TifEnum.Ioc,
                //    Price = "0",
                //};
                var order = new Order(null, currencyPair: symbol, Order.TypeEnum.Market, account: "spot", Order.SideEnum.Sell, amount: SellSize.ToString(), timeInForce: Order.TimeInForceEnum.Ioc);

                //FuturesOrder orderResponse;
                try
                {
                    //orderResponse = futuresApi_.CreateFuturesOrder(settle, futuresOrder_);
                    Order result = spotApi.CreateOrder(order);
                    long ms = DateTimeOffset.Now.ToUnixTimeMilliseconds() - milliseconds;
                    c("Slow Sell Done On: " + ms + "ms , FillPrice: " + result.FillPrice);

                }
                catch (GateApiException e)
                {
                    c(e.Message);
                    return;
                }

            }
            catch (Exception e) { c(e.Message); }


        }
        public async Task Buy()
        {

            try
            {
                long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                futuresOrder_ = new FuturesOrder(symbol)
                {
                    Size = (long)SellSize,
                    Tif = FuturesOrder.TifEnum.Ioc,
                    Price = "0",
                };

                FuturesOrder orderResponse;
                try
                {
                    orderResponse = futuresApi_.CreateFuturesOrder(settle, futuresOrder_);
                    long ms = DateTimeOffset.Now.ToUnixTimeMilliseconds() - milliseconds;
                    c("Slow Buy Done On: " + ms + "ms , FillPrice: " + orderResponse.FillPrice);

                }
                catch (GateApiException e)
                {
                    c(e.Message);
                    return;
                }

            }
            catch (Exception e) { c(e.Message); }


        }
    }
}

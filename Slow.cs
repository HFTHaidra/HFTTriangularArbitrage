
using Io.Gate.GateApi.Api;
using Io.Gate.GateApi.Client;
using System.Diagnostics.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks; 
using Websocket.Client;
using Newtonsoft.Json.Linq;
using System.Security.Policy;
using System.Net.WebSockets; 
using GateIo.Net.Clients;
using CryptoExchange.Net.Authentication;
using Io.Gate.GateApi.Model;

namespace Futures
{
    internal class Slow
    {
        public string Key = "";
        public string SecretKey = "";
        public List<String> contracts = new List<String>();
        public string symbol = "";
        public Book book =new Book();
        public decimal balance = 0;


        public  string ordershosturl = "/futures/usdt/orders";
        public  string leverage = "1";
        public  Configuration config = new Configuration();
        public  string settle = "usdt";
        public  FuturesApi futuresApi;
        public  WebsocketClient client = new WebsocketClient(new Uri("wss://fx-ws.gateio.ws/v4/ws/usdt"));
        public  static List<string> msg = new List<string>();


        public double SellSize = 0;
        public double BuySize = 0;
        public FuturesApi futuresApi_ = new FuturesApi();
        public FuturesOrder futuresOrder_ = new FuturesOrder("BTCUSDT");
        public Slow() { }

        public Slow(string key, string secretkey) 
        {
            this.Key = key;
            this.SecretKey = secretkey;
            config.BasePath = "https://api.gateio.ws/api/v4"; 
            config.SetGateApiV4KeyPair(Key, SecretKey);
            futuresApi = new FuturesApi(config);
            futuresApi_ = new FuturesApi(config);

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
                var apiInstance = new FuturesApi(config);
                Io.Gate.GateApi.Model.FuturesAccount result = apiInstance.ListFuturesAccounts(settle);
                res = decimal.Parse(result.Total);
               
            }
            catch (Exception e) { c(e.Message); }
            return res;
        }
        public List<string> Contracts()
        {
            List<string> res = new List<string>();
            try
            {
                Configuration config = new Configuration();
                config.BasePath = "https://api.gateio.ws/api/v4";
                var apiInstance = new FuturesApi(config);

                var settle = "usdt";  // string | Settle currency
                var limit = 100;  // int? | Maximum number of records to be returned in a single list (optional)  (default to 100)
                var offset = 0;  // int? | List offset, starting from 0 (optional)  (default to 0)
                List<Io.Gate.GateApi.Model.Contract> result = apiInstance.ListFuturesContracts(settle, limit, offset);
                foreach (Io.Gate.GateApi.Model.Contract contract in result) 
                {
                    res.Add(contract.Name);
                }

                limit = 100;  // int? | Maximum number of records to be returned in a single list (optional)  (default to 100)
                offset = 101;  // int? | List offset, starting from 0 (optional)  (default to 0)
                result = apiInstance.ListFuturesContracts(settle, limit, offset);
                foreach (Io.Gate.GateApi.Model.Contract contract in result)
                {
                    res.Add(contract.Name);
                }

                limit = 100;  // int? | Maximum number of records to be returned in a single list (optional)  (default to 100)
                offset = 201;  // int? | List offset, starting from 0 (optional)  (default to 0)
                result = apiInstance.ListFuturesContracts(settle, limit, offset);
                foreach (Io.Gate.GateApi.Model.Contract contract in result)
                {
                    res.Add(contract.Name);
                }

                limit = 100;  // int? | Maximum number of records to be returned in a single list (optional)  (default to 100)
                offset = 301;  // int? | List offset, starting from 0 (optional)  (default to 0)
                result = apiInstance.ListFuturesContracts(settle, limit, offset);
                foreach (Io.Gate.GateApi.Model.Contract contract in result)
                {
                    res.Add(contract.Name);
                }

                limit = 100;  // int? | Maximum number of records to be returned in a single list (optional)  (default to 100)
                offset = 401;  // int? | List offset, starting from 0 (optional)  (default to 0)
                result = apiInstance.ListFuturesContracts(settle, limit, offset);
                foreach (Io.Gate.GateApi.Model.Contract contract in result)
                {
                    res.Add(contract.Name);
                }

            }
            catch(Exception ex) { c(ex.Message); }

            return res;
        }
        public async Task StartStreams(string sy)
        {
            try
            { 
                futuresOrder_ = new FuturesOrder(symbol);
                string urls = "wss://fx-ws.gateio.ws/v4/ws/usdt";
                Uri url = new Uri(urls);
                long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                string Subscribe = "{\"time\" : " + milliseconds + ", \"channel\" : \"futures.order_book\", \"event\": \"subscribe\", \"payload\" : [\"" + sy + "\",\"1\",\"0\"]}";
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

                    if (text != "" && text.Contains("asks"))
                    {
                        JObject json = JObject.Parse(text);
                        var data = json["result"];
                        if (data != null)
                        {
                            var asks = data["asks"][0];
                            var bids = data["bids"][0];
                            var contractm = data["t"];
                            
                            decimal ASK = (decimal)(asks["p"] ?? 0);
                            decimal BID = (decimal)(bids["p"] ?? 0);
                            long time = (long) contractm  ;
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
                futuresOrder_ = new FuturesOrder(symbol)
                {
                    Size = -1 * (long)SellSize,
                    Tif = FuturesOrder.TifEnum.Ioc,
                    Price = "0",

                };

                FuturesOrder orderResponse;
                try
                {
                    orderResponse = futuresApi_.CreateFuturesOrder(settle, futuresOrder_);
                    long ms = DateTimeOffset.Now.ToUnixTimeMilliseconds() - milliseconds;
                    c("Slow Sell Done On: " + ms + "ms , FillPrice: " + orderResponse.FillPrice);

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
                    Size =  (long)SellSize,
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

        public async Task SendUpdate()
        {

            try
            {
                long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                futuresOrder_ = new FuturesOrder("BTC_USDT")
                {
                    Size = (long)900000,
                    Tif = FuturesOrder.TifEnum.Ioc,
                    Price = "0",
                };

                FuturesOrder orderResponse;
                try
                {
                    orderResponse = futuresApi_.CreateFuturesOrder(settle, futuresOrder_);
                    long ms = DateTimeOffset.Now.ToUnixTimeMilliseconds() - milliseconds;
                    //c("Slow Buy Done On: " + ms + "ms , FillPrice: " + orderResponse.FillPrice);

                }
                catch (GateApiException e)
                {
                    //c(e.Message);
                    return;
                }

            }
            catch (Exception e) { c(e.Message); }


        }
    }
}

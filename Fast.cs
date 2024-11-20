using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
//using Websocket.Client;
using Newtonsoft.Json.Linq;

using static System.Net.Mime.MediaTypeNames;
using System.Net;
using Bybit.Net.Clients;
using CryptoExchange.Net.Authentication;
using System.Threading;
using Io.Gate.GateApi.Api;
using Io.Gate.GateApi.Client;

namespace Futures
{
    internal class Fast
    {
        public string Key = "";
        public string SecretKey = "";
        public List<String> contracts = new List<String>();
        public string symbol = "";
        public Book book = new Book();
        public decimal balance = 0;
        public decimal BalanceRunVale = 0;
        public bool contractsOK = false;


        public static List<string> msg  = new List<string>();
        public Fast() { }
        public Fast(string key, string secretkey)
        {
            this.Key = key;
            this.SecretKey = secretkey;
        }
        public static void c(string msg)
        {
            Fast.msg.Add(msg);
        }
        public decimal Balance()
        {
            decimal res = 0;
            BalanceRunVale = 0;
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                await BalanceRun();
                
            }).Start();
            int i = 50;
            while (i>=1)
            {
                if (BalanceRunVale!=0)
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
        public async Task BalanceRun()
        {
           
            try
            {
                try
                {
                    var restClient = new BybitRestClient(options => {
                        // Options can be configured here, for example:
                        options.ApiCredentials = new ApiCredentials(key: Key, secret: SecretKey);
                    });
                    var b = await restClient.V5Api.Account.GetAllAssetBalancesAsync(accountType: Bybit.Net.Enums.AccountType.Unified, asset: "USDT");
                    if (b.Success)
                    {
                        var bb = b.Data.Balances.First().TransferBalance;
                        BalanceRunVale = bb;
                    }
                    else
                    {
                        c(b.Error.Message);
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
                var restClient = new BybitRestClient(options => {
                    // Options can be configured here, for example:
                    options.ApiCredentials = new ApiCredentials(key: Key, secret: SecretKey);
                });
                var cc = await restClient.V5Api.ExchangeData.GetInsuranceAsync(asset: "USDT");
                var bb = cc.Data.List;
                foreach (var b in bb)
                {
                    if (b != null)
                    {
                        contracts.Add(b.Symbols);
                    }
                }
                contracts.RemoveAt(0);
                 

            }
            catch (Exception ex) { c(ex.Message); }
        }

    }
}

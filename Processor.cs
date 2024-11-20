using Binance.Net.Objects.Models.Spot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Futures
{
    internal class Processor
    {
        //public static BanaceSpot FastExchange = new BanaceSpot();
        public static Slow SlowExchange = new Slow();
        public static GateFuture FastExchange = new GateFuture();
        //public static BinanceFuture SlowExchange = new BinanceFuture();

        public static List<string> msg = new List<string>();
        public static decimal Gap = 0;
        public static decimal Gap_ = 0;
        public static decimal Spread = 0;



        public static decimal MaxGap = 0;
        public static decimal MaxGap_ = 0;
        public static decimal MaxSpread = 0;

        public static decimal MinGap = 0;
        public static decimal MinGap_ = 0;
        public static decimal MinSpread = 0;

        public static Level Level1 = new Level();
        public static Level Level2 = new Level();
        public static Level Level3 = new Level();
        public static Level Level4 = new Level();

        public static double MinGapForOpen = 1;
        public static double MaxGapForClose = -1;
        public static double MaxSpreadForOpen = 0.6;
        public static string Stutus = "Step1";
        public static bool StartTrading = false;


        public Processor() { }
        public static void c(string msg)
        {
            Processor.msg.Add(msg);
        }
        public async static Task Run()
        {
            try
            {
                new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    if (SlowExchange.book.bid!=0&& FastExchange.book.bid!=0)
                    {
                        Gap = SlowExchange.book.bid - FastExchange.book.bid;
                        decimal min = Math.Min(SlowExchange.book.bid, FastExchange.book.bid); 
                        Gap_ = Math.Round((Gap * 100) / min, 3);
                        Spread = SlowExchange.book.spread_ + FastExchange.book.spread_;


                        if (StartTrading==true)
                        {
                            double gap_ = (double)Gap_;
                            double spread_ = (double) Spread;
                            // Trading Open or Close
                            if ( Stutus == "Step1" && gap_ >= MinGapForOpen && spread_< MaxSpreadForOpen )
                            {
                                Stutus = "Step2";
                                new Thread(async () =>
                                {
                                    Thread.CurrentThread.IsBackground = true;
                                    await Processor.FastExchange.Buy();
                                }).Start();
                                new Thread(async () =>
                                {
                                    Thread.CurrentThread.IsBackground = true; 
                                    await Processor.SlowExchange.Sell();
                                }).Start();
                                



                                // Send Message ....
                                c("We Find Step1 Arbitrage Send ....");
                                string m = " ::Step1-> Fast:" + Processor.FastExchange.book.ask + "," + Processor.FastExchange.book.bid + "," + Processor.FastExchange.book.spread_ + "% - ";
                                m += "Slow:" + Processor.SlowExchange.book.ask + "," + Processor.SlowExchange.book.bid + "," + Processor.SlowExchange.book.spread_ + "% - ";
                                m += " Gap:" + Processor.Gap_ + " AllSpread: " + Processor.Spread + "%";
                                c(m);

                            }


                            if (Stutus == "Step2" && gap_ <= MaxGapForClose && spread_ < MaxSpreadForOpen)
                            {
                                Stutus = "Step1";
                                new Thread(async () =>
                                {
                                    Thread.CurrentThread.IsBackground = true;
                                    await Processor.FastExchange.Sell();
                                }).Start();
                                new Thread(async () =>
                                {
                                    Thread.CurrentThread.IsBackground = true; 
                                    await Processor.SlowExchange.Buy();
                                }).Start();
                                



                                // Send Message ....
                                c("We Find Step1 Arbitrage Send ....");
                                string m = " ::Step2 -> Fast:" + Processor.FastExchange.book.ask + "," + Processor.FastExchange.book.bid + "," + Processor.FastExchange.book.spread_ + "% - ";
                                m += "Slow:" + Processor.SlowExchange.book.ask + "," + Processor.SlowExchange.book.bid + "," + Processor.SlowExchange.book.spread_ + "% - ";
                                m += " Gap:" + Processor.Gap_ + " AllSpread: " + Processor.Spread + "%";
                                c(m);

                            }
                        }



                        if (Gap>MaxGap)
                        {
                            MaxGap = Gap;
                            MaxGap_ = Gap_;
                            MaxSpread = Spread;
                        }
                        if (Gap<MinGap)
                        {
                            MinGap = Gap;
                            MinSpread = Spread;
                            MinGap_ = Gap_;
                        }
                        Level1.Updates();
                        Level2.Updates();
                        Level3.Updates();
                        Level4.Updates();
                    } 
                }).Start();
            }
            catch (Exception ex) { c(ex.Message); }
        }// fin Run ....
    
        public static double GetSize(double usdt,int digit,decimal bid,double constact,int type)
        {
            double res = 0;
            double BID = (double)bid;
            if (type==1)
            {
                res = Math.Round( usdt, digit);
            }
            if (type == 2)
            {
                res = Math.Round( (usdt * BID), digit);
            }
            if (type == 3)
            {
                res = Math.Round((usdt / BID), digit);
            }
            if (type == 4)
            {
                res = Math.Round(( BID / usdt ), digit);
            }
            
            res = res*constact;
            return res;
        }
    
        
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Futures
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static int iConsole = 0;
        bool StartStreams = false;
        bool StartTrading = false;
        string BinanceKeySubAccount = "iH8hJQsNLseQnvUyrijfcFM33woSEQzwdImTQJ0Z6A0nE9K5xgX4j7QldLOFvIdr";
        string BinanceSecretKeySubAccount = "j5jMS68RPtjFm1CMcPayyXzTWhwySE7QK7UGXlOoY8jnqqJkhowTuouEPk5xnWnE";
        string GateKey = "29c24494754daa9f20fc0b87eaa93ef7";
        string GateSecretKey = "cc8aec2c2caf996c75857ee0973dae5779acf6e3e624c45778da9ff37f6f8a22";
        public void c(string msg)
        { 
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    string date = DateTime.Now.ToString("dd-MM-yyyy hh.mm.ss.fff") + ": "+msg+ "\r\n";
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        Console.AppendText(date);
                        iConsole++;
                        if (iConsole>2000)
                        {
                            Console.Text = "date";
                            iConsole = 1;
                        }
                    });

                    
                }
                catch (Exception ex) { c("C:" + ex.Message); }
            }).Start(); 

        }
        private async void button1_Click(object sender, EventArgs e)
        {
            //Processor.FastExchange = new BanaceSpot(textBox1.Text, textBox2.Text);
            //Processor.SlowExchange = new Slow(textBox3.Text, textBox4.Text);
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                Processor.FastExchange.balance = Processor.FastExchange.Balance();
                c("Fast Balance: " + Processor.FastExchange.balance + "USDT");

            }).Start();
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                Processor.SlowExchange.balance = Processor.SlowExchange.Balance();
                c("Slow Balance: " + Processor.SlowExchange.balance + " USDT");

            }).Start();
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.CurrentThread.IsBackground = true;
                //Processor.SlowExchange.contracts = Processor.SlowExchange.Contracts();
                c("Slow Contracts: " + Processor.SlowExchange.contracts.Count);

            }).Start();
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                Processor.FastExchange.Contracts();
                c("Fast Contracts: " + Processor.FastExchange.contracts.Count); 

            }).Start();

           
               
               
                
                
                
                

           

           
           


         
        }

        private void ConsoleTimer_Tick(object sender, EventArgs e)
        {
            try
            { 
                if (BanaceSpot.msg.Count >= 1)
                {
                    c(BanaceSpot.msg[0]);
                    BanaceSpot.msg.RemoveAt(0);
                }
                if (Fast.msg.Count >= 1)
                {
                    c(Fast.msg[0]);
                    Fast.msg.RemoveAt(0);
                }
                if (Slow.msg.Count >= 1)
                {
                    c(Slow.msg[0]);
                    Slow.msg.RemoveAt(0);
                }
                if (BinanceFuture.msg.Count >= 1)
                {
                    c(BinanceFuture.msg[0]);
                    BinanceFuture.msg.RemoveAt(0);
                }
                if (Processor.msg.Count >= 1)
                {
                    c(Processor.msg[0]);
                    Processor.msg.RemoveAt(0);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        private void comboBox1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                comboBox1.Items.Clear();
                foreach (var item in Processor.FastExchange.contracts)
                {
                    comboBox1.Items.Add(item);
                }
            }
            catch (Exception ex) { c(ex.Message); }
        }

        private void comboBox2_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                comboBox2.Items.Clear();
                foreach (var item in Processor.SlowExchange.contracts)
                {
                    comboBox2.Items.Add(item);
                }
            }
            catch (Exception ex) { c(ex.Message); }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (StartStreams!=true)
            {
                StartStreams = true;
                await Processor.SlowExchange.StartStreams(comboBox2.Text);
                await Processor.FastExchange.StartStreams(comboBox1.Text);
            }


            try
            {
                Processor.Level1 = new Level(double.Parse(textBox6.Text), double.Parse(textBox7.Text), "max");
                Processor.Level2 = new Level(double.Parse(textBox13.Text), double.Parse(textBox12.Text), "max");
                Processor.Level3 = new Level(double.Parse(textBox18.Text), double.Parse(textBox17.Text), "max");
                Processor.Level4 = new Level(double.Parse(textBox23.Text), double.Parse(textBox22.Text), "max");

            }
            catch (Exception ex) { c(ex.Message); }
           
            
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (StartStreams!=false)
            {
                StartStreams = false;
                await Processor.SlowExchange.UnStartStreams(comboBox2.Text);
                await Processor.FastExchange?.UnStartStreams(comboBox1.Text);
            }
            
        }

        private void ASKBIDUpdates_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Processor.FastExchange != null && Processor.FastExchange.book.done)
                {
                    label7.Text = (double)Processor.FastExchange.book.ask + "";
                    label10.Text = (double)Processor.FastExchange.book.bid + "";
                    label12.Text = (double)Processor.FastExchange.book.spread + "";
                    label11.Text = Processor.FastExchange.book.spread_ + "%";
                    label42.Text = Processor.FastExchange.book.MaxSpread_ + "%";
                    Processor.FastExchange.book.done = false;
                }

                if (Processor.SlowExchange != null && Processor.SlowExchange.book.done)
                {
                    label16.Text = Processor.SlowExchange.book.ask + "";
                    label15.Text = Processor.SlowExchange.book.bid + "";
                    label14.Text = Processor.SlowExchange.book.spread + "";
                    label13.Text = Processor.SlowExchange.book.spread_ + "%";
                    label41.Text = Processor.SlowExchange.book.MaxSpread_ + "%";
                    Processor.SlowExchange.book.done = false;
                }
                if (Processor.SlowExchange.book.bid != 0 && Processor.FastExchange.book.bid != 0)
                {
                    label24.Text = Processor.Gap + "";
                    label25.Text = Processor.Gap_ + "%";
                    label27.Text = Processor.Spread + "%";


                    label33.Text = Processor.MaxGap + "";
                    label31.Text = Processor.MaxGap_ + "%";
                    label29.Text = Processor.MaxSpread + "%";

                    label39.Text = Processor.MinGap + "";
                    label37.Text = Processor.MinGap_ + "%";
                    label35.Text = Processor.MinSpread + "%";

                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
           
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private async void Balance_Tick(object sender, EventArgs e)
        {
            if (Processor.FastExchange.balance!=0&& Processor.SlowExchange.balance!=0)
            {
                new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                     

                    //Processor.FastExchange.balance = Processor.FastExchange.Balance();
                    //this.Invoke((MethodInvoker)delegate ()
                    //{
                    //    label21.Text = Math.Round(Processor.FastExchange.balance,4) + " USDT";
                    //});
                    //Processor.SlowExchange.balance = Processor.SlowExchange.Balance();
                    //this.Invoke((MethodInvoker)delegate ()
                    //{
                    //    label22.Text = Math.Round(Processor.SlowExchange.balance,4) + " USDT";
                    //});
                    if (Processor.FastExchange.book.bid != 0 && Processor.SlowExchange.book.bid != 0)
                    {
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            try
                            {
                                //----- FAST ----------------------------------------
                                if (comboBox1.Text != textBox25.Text)
                                {
                                    textBox25.Text = comboBox1.Text;
                                }

                                Processor.FastExchange.SellSize = Processor.GetSize(
                                    double.Parse(textBox26.Text), int.Parse(textBox27.Text),
                                   Processor.FastExchange.book.bid, double.Parse(textBox31.Text),
                                   int.Parse(textBox28.Text));
                                label63.Text = Processor.FastExchange.SellSize + "";
                                Processor.FastExchange.BuySize = Processor.GetSize(
                                    double.Parse(textBox26.Text), int.Parse(textBox27.Text),
                                   Processor.FastExchange.book.bid, double.Parse(textBox29.Text),
                                   int.Parse(textBox30.Text));

                                label64.Text = Processor.FastExchange.BuySize + "";
                                //-------- SLOW ------------------------------------------------
                                if (comboBox2.Text != textBox38.Text)
                                {
                                    textBox38.Text = comboBox2.Text;
                                }
                                Processor.SlowExchange.SellSize = Processor.GetSize(
                                   double.Parse(textBox37.Text), int.Parse(textBox36.Text),
                                  Processor.SlowExchange.book.bid, double.Parse(textBox33.Text),
                                  int.Parse(textBox32.Text));
                                label77.Text = Processor.SlowExchange.SellSize + "";

                                Processor.SlowExchange.BuySize = Processor.GetSize(
                                   double.Parse(textBox37.Text), int.Parse(textBox36.Text),
                                  Processor.SlowExchange.book.bid, double.Parse(textBox35.Text),
                                  int.Parse(textBox34.Text));
                                label75.Text = Processor.SlowExchange.BuySize + "";
                                //-----------------
                                groupBox6.Text = "Fast ask:" + (double)Processor.FastExchange.book.ask;
                                groupBox9.Text = "Slow ask:" + (double)Processor.SlowExchange.book.ask;

                                Processor.FastExchange.symbol = textBox25.Text;
                                Processor.SlowExchange.symbol = textBox38.Text;


                            } catch (Exception ex) { c(ex.Message); }
                      
                        });
                    }
                }).Start();

                 
            }
        }

        private void Data_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Processor.Level1.msgModify)
                {
                    Processor.Level1.msgModify = false;
                    textBox5.Text = "";
                    foreach (var item in Processor.Level1.msg)
                        textBox5.Text += item + "\r\n";
                    textBox8.Text = Processor.Level1.status;
                    textBox9.Text = Processor.Level1.Loop.ToString();
                }

                if (Processor.Level2.msgModify)
                {
                    Processor.Level2.msgModify = false;
                    textBox14.Text = "";
                    foreach (var item in Processor.Level2.msg)
                        textBox14.Text += item + "\r\n";
                    textBox11.Text = Processor.Level2.status;
                    textBox10.Text = Processor.Level2.Loop.ToString();
                }

                if (Processor.Level3.msgModify)
                {
                    Processor.Level3.msgModify = false;
                    textBox19.Text = "";
                    foreach (var item in Processor.Level3.msg)
                        textBox19.Text += item + "\r\n";
                    textBox16.Text = Processor.Level3.status;
                    textBox15.Text = Processor.Level3.Loop.ToString();
                }

                if (Processor.Level4.msgModify)
                {
                    Processor.Level4.msgModify = false;
                    textBox24.Text = "";
                    foreach (var item in Processor.Level4.msg)
                        textBox24.Text += item + "\r\n";
                    textBox21.Text = Processor.Level4.status;
                    textBox20.Text = Processor.Level4.Loop.ToString();
                }
            }
            catch (Exception ex) { c(ex.Message); }
            
        }

        private void groupBox8_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox13_Enter(object sender, EventArgs e)
        {

        }

        private async void button6_Click(object sender, EventArgs e)
        {
            c("Symbol:"+ Processor.SlowExchange.symbol);
            c("Start Selling on ask,bid=("+ Processor.SlowExchange.book.ask+", "+ Processor.SlowExchange.book.bid+") Pricess.");
            await Processor.SlowExchange.Sell();

            c("Done Selling on ask,bid=(" + Processor.SlowExchange.book.ask + ", " + Processor.SlowExchange.book.bid + ") Pricess.");
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            c("Symbol:" + Processor.SlowExchange.symbol);
            c("Start buying on ask,bid=(" + Processor.SlowExchange.book.ask + ", " + Processor.SlowExchange.book.bid + ") Pricess.");
            await Processor.SlowExchange.Buy();
            c("Done buying on ask,bid=(" + Processor.SlowExchange.book.ask + ", " + Processor.SlowExchange.book.bid + ") Pricess.");
        }

        private async void GateUpdate_Tick(object sender, EventArgs e)
        {
            if (Processor.FastExchange.balance != 0 && Processor.SlowExchange.balance != 0)
            {
                new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true; 
                    try
                    {
                        //await Processor.SlowExchange.SendUpdate();
                        //await Processor.FastExchange.SendUpdate();
                    }
                    catch (Exception ex)
                    {
                        c(ex.Message);
                    } 
                }).Start();

               
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            c("Symbol:" + Processor.FastExchange.symbol);
            c("Start Selling on ask,bid=(" + Processor.FastExchange.book.ask + ", " + Processor.FastExchange.book.bid + ") Pricess.");
             
            Thread.CurrentThread.IsBackground = true;
            await Processor.FastExchange.Sell();
             
            c("Done Selling on ask,bid=(" + Processor.FastExchange.book.ask + ", " + Processor.FastExchange.book.bid + ") Pricess.");


           
         }

        private async void button5_Click(object sender, EventArgs e)
        {
            c("Symbol:" + Processor.FastExchange.symbol);
            c("Start buying on ask,bid=(" + Processor.FastExchange.book.ask + ", " + Processor.FastExchange.book.bid + ") Pricess.");
            await Processor.FastExchange.Buy();
            c("Done buying on ask,bid=(" + Processor.SlowExchange.book.ask + ", " + Processor.SlowExchange.book.bid + ") Pricess.");

        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                if (StartTrading == false)
                {
                    Processor.MinGapForOpen = (double)double.Parse(textBox39.Text);
                    Processor.MaxGapForClose = (double)double.Parse(textBox40.Text);
                    Processor.MaxSpreadForOpen = (double)double.Parse(textBox41.Text);
                    Processor.Stutus = "Step1";
                    StartTrading = true;
                    Processor.StartTrading = true;
                    button9.Text = "Stop?";
                    c("Strating Trading Processor ....");
                }
                else
                {
                    StartTrading = false;
                    Processor.StartTrading = false;
                    button9.Text = "Starting ...";
                    c("Stop Trading ....");
                }
            }
            catch (Exception ex) { c(ex.Message); }
           
        }

        private void button10_Click(object sender, EventArgs e)
        {
           
            
        }

        private async void button10_Click_1(object sender, EventArgs e)
        {
            //Processor.FastExchange = new BanaceSpot(BinanceKeySubAccount, BinanceSecretKeySubAccount);
            //Processor.SlowExchange = new BinanceFuture(BinanceKeySubAccount, BinanceSecretKeySubAccount);
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                Processor.FastExchange.balance = Processor.FastExchange.Balance();
                c("Fast Balance: " + Processor.FastExchange.balance + "USDT");

            }).Start();
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                Processor.SlowExchange.balance = Processor.SlowExchange.Balance();
                c("Slow Balance: " + Processor.SlowExchange.balance + " USDT");

            }).Start();
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                Processor.SlowExchange.Contracts();
                c("Slow Contracts: " + Processor.SlowExchange.contracts.Count);

            }).Start();
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                Processor.FastExchange.Contracts();
                c("Fast Contracts: " + Processor.FastExchange.contracts.Count);

            }).Start();
        }

        private async void button11_Click(object sender, EventArgs e)
        {
            if (StartStreams != true)
            {
                StartStreams = true;
                await Processor.SlowExchange.StartStreams(comboBox2.Text);
                await Processor.FastExchange.StartStreams(comboBox1.Text);
            }


            try
            {
                Processor.Level1 = new Level(double.Parse(textBox6.Text), double.Parse(textBox7.Text), "max");
                Processor.Level2 = new Level(double.Parse(textBox13.Text), double.Parse(textBox12.Text), "max");
                Processor.Level3 = new Level(double.Parse(textBox18.Text), double.Parse(textBox17.Text), "max");
                Processor.Level4 = new Level(double.Parse(textBox23.Text), double.Parse(textBox22.Text), "max");

            }
            catch (Exception ex) { c(ex.Message); }
        }

        private async void button16_Click(object sender, EventArgs e)
        {
            c("Symbol:" + Processor.FastExchange.symbol);
            c("Start Selling on ask,bid=(" + Processor.FastExchange.book.ask + ", " + Processor.FastExchange.book.bid + ") Pricess.");

            Thread.CurrentThread.IsBackground = true;
            await Processor.FastExchange.Sell();

            c("Done Selling on ask,bid=(" + Processor.FastExchange.book.ask + ", " + Processor.FastExchange.book.bid + ") Pricess.");


        }

        private async void button15_Click(object sender, EventArgs e)
        {
            c("Symbol:" + Processor.FastExchange.symbol);
            c("Start buying on ask,bid=(" + Processor.FastExchange.book.ask + ", " + Processor.FastExchange.book.bid + ") Pricess.");
            await Processor.FastExchange.Buy();
            c("Done buying on ask,bid=(" + Processor.SlowExchange.book.ask + ", " + Processor.SlowExchange.book.bid + ") Pricess.");

        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private async void button14_Click(object sender, EventArgs e)
        {
            c("Symbol:" + Processor.SlowExchange.symbol);
            c("Start Selling on ask,bid=(" + Processor.SlowExchange.book.ask + ", " + Processor.SlowExchange.book.bid + ") Pricess.");
            await Processor.SlowExchange.Sell();

            c("Done Selling on ask,bid=(" + Processor.SlowExchange.book.ask + ", " + Processor.SlowExchange.book.bid + ") Pricess.");
        }

        private async void button13_Click(object sender, EventArgs e)
        {
            c("Symbol:" + Processor.SlowExchange.symbol);
            c("Start buying on ask,bid=(" + Processor.SlowExchange.book.ask + ", " + Processor.SlowExchange.book.bid + ") Pricess.");
            await Processor.SlowExchange.Buy();
            c("Done buying on ask,bid=(" + Processor.SlowExchange.book.ask + ", " + Processor.SlowExchange.book.bid + ") Pricess.");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Processor.FastExchange = new GateFuture(GateKey, GateSecretKey);
            Processor.SlowExchange = new Slow(GateKey, GateSecretKey);
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                Processor.FastExchange.balance = Processor.FastExchange.Balance();
                c("Fast Balance: " + Processor.FastExchange.balance + "USDT");

            }).Start();
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                Processor.SlowExchange.balance = Processor.SlowExchange.Balance();
                c("Slow Balance: " + Processor.SlowExchange.balance + " USDT");

            }).Start();
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.CurrentThread.IsBackground = true;
                Processor.SlowExchange.contracts = Processor.SlowExchange.Contracts();
                c("Slow Contracts: " + Processor.SlowExchange.contracts.Count);

            }).Start();
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                Processor.FastExchange.contracts = Processor.FastExchange.Contracts();
                c("Fast Contracts: " + Processor.FastExchange.contracts.Count);

            }).Start();
        }

        private async void button8_Click_1(object sender, EventArgs e)
        {
            
                
                await Processor.SlowExchange.StartStreams(comboBox2.Text);
                await Processor.FastExchange.StartStreams(comboBox1.Text);
            


            try
            {
                Processor.Level1 = new Level(double.Parse(textBox6.Text), double.Parse(textBox7.Text), "max");
                Processor.Level2 = new Level(double.Parse(textBox13.Text), double.Parse(textBox12.Text), "max");
                Processor.Level3 = new Level(double.Parse(textBox18.Text), double.Parse(textBox17.Text), "max");
                Processor.Level4 = new Level(double.Parse(textBox23.Text), double.Parse(textBox22.Text), "max");

            }
            catch (Exception ex) { c(ex.Message); }
        }

        private async void button18_Click(object sender, EventArgs e)
        {
            c("Symbol:" + Processor.SlowExchange.symbol);
            c("Start Selling on ask,bid=(" + Processor.SlowExchange.book.ask + ", " + Processor.SlowExchange.book.bid + ") Pricess.");
            await Processor.SlowExchange.Sell();

            c("Done Selling on ask,bid=(" + Processor.SlowExchange.book.ask + ", " + Processor.SlowExchange.book.bid + ") Pricess.");

        }

        private async void button17_Click(object sender, EventArgs e)
        {
            c("Symbol:" + Processor.SlowExchange.symbol);
            c("Start buying on ask,bid=(" + Processor.SlowExchange.book.ask + ", " + Processor.SlowExchange.book.bid + ") Pricess.");
            await Processor.SlowExchange.Buy();
            c("Done buying on ask,bid=(" + Processor.SlowExchange.book.ask + ", " + Processor.SlowExchange.book.bid + ") Pricess.");

        }

        private async void button20_Click(object sender, EventArgs e)
        {
            c("Symbol:" + Processor.SlowExchange.symbol);
            c("Start Selling on ask,bid=(" + Processor.FastExchange.book.ask + ", " + Processor.FastExchange.book.bid + ") Pricess.");
            await Processor.FastExchange.Sell();

            c("Done Selling on ask,bid=(" + Processor.FastExchange.book.ask + ", " + Processor.FastExchange.book.bid + ") Pricess.");


        }
    }
}

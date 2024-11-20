using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futures
{
    internal class Level
    {
        public decimal max = 0;
        public decimal min = 0;
        public string status = "max"; // max -> min (1-loop)
        public int Loop = 0;

        public List<string> msg = new List<string>();
        public bool msgModify = false;
        public Level() { }
        public Level(double max_=1, double min_=-1,string status_="max") 
        {
            this.max = (decimal) max_; 
            this.min = (decimal) min_;
            this.status = status_;
            Loop = 0;
            msg = new List<string>();
            msgModify = false;
        }
        public void Updates()
        {

            if (status == "max")
            {
                if ( Processor.Gap_ > max )
                {
                    status = "min";
                    string date = DateTime.Now.ToString("dd-MM-yyyy hh.mm.ss.ffffff");
                    string m = date+" ::MAX-> Fast:" + Processor.FastExchange.book.ask + "," + Processor.FastExchange.book.bid + "," + Processor.FastExchange.book.spread_ + "% - ";
                    m += "Slow:" + Processor.SlowExchange.book.ask + "," + Processor.SlowExchange.book.bid + "," + Processor.SlowExchange.book.spread_ + "% - ";
                    m += " Gap:" + Processor.Gap_ + " AllSpread: "+Processor.Spread+"%";
                    msg.Add(m);
                    msgModify = true;
                }
            }

            if (status == "min")
            {
                if (Processor.Gap_ < min)
                {
                    status = "max";
                    Loop++;
                    string date = DateTime.Now.ToString("dd-MM-yyyy hh.mm.ss.ffffff");
                    string m =date+ " ::MIN-> Fast:" + Processor.FastExchange.book.ask + "," + Processor.FastExchange.book.bid + "," + Processor.FastExchange.book.spread_ + "% - ";
                    m += "Slow:" + Processor.SlowExchange.book.ask + "," + Processor.SlowExchange.book.bid + "," + Processor.SlowExchange.book.spread_ + "% - ";
                    m += " Gap:" + Processor.Gap_ + " AllSpread: " + Processor.Spread+"%";
                    msg.Add(m);
                    msgModify = true;
                }
            }

        }
    }
}

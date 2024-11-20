using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futures
{
    internal class Book
    {
        public string symbol = "";
        public decimal ask = 0;
        public decimal bid =0;
        public long time = 0;
        public decimal spread = -1;
        public decimal spread_ = -1;
        public bool done = false;
        public decimal MaxSpread_ = 0;
        public Book() { }
    }
}

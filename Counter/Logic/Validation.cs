using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Counter.Logic
{
    public class Validation
    {
        public bool IsIntNumeric(string s)
        {
            int output;
            return int.TryParse(s, out output);
        }
    }
}

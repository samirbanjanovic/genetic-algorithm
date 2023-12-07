using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrac.Extensions.Byte
{
    public static class Helpers
    {
        public static string Text(this byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}

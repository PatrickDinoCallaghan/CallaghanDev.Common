using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.Web
{
    public interface IGetCookies
    {

        public void ExtractCookies(string outputPath = "");
    }
}

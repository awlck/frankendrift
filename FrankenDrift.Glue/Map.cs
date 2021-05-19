using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adravalon.Glue
{
    public interface Map
    {
        public void RecalculateNode(object node);
        public void SelectNode(string key);
    }
}

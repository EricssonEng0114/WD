using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UBPC.Web.Collections
{
    public class WebClientInfo : ClientInfo 
    {
        public WebClientInfo()
            : base()
        {
            // Base class does it all
        }

        public WebClientInfo(String clientCode, String clientModule, String clientName, String clientAlias, String clientDBName, String clientDBServerName)
            : base(clientCode, clientModule, clientName, clientAlias, clientDBName, clientDBServerName)
        {
            // Base class does it all
        }
    }
}

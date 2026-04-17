using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace UBPC.Web.Collections
{
    public class WebClientCollection : ClientCollection<WebClientInfo>
    {
        /// <summary>
        /// Create the list of all Web Client from TBL_ClientInfo table
        /// </summary>
        public WebClientCollection()
            : base()
        {
            // Base class does it all
        }

        /// <summary>
        /// Create the list of available Web Client for the user
        /// </summary>
        /// <param name="userID"></param>     
        public WebClientCollection(String userID)
            : base(userID)
        {
            // Base class does it all
        }

        /// <summary>
        /// Create the list of available RLPS Client for the user
        /// </summary>
        /// <param name="connectionString"></param>     
        /// <param name="userID"></param>     
        public WebClientCollection(String connectionString, String userID)
            : base(connectionString, userID)
        {
            // Base class does it all
        }

    }
}

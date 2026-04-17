using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UBPC.Web.Collections
{
    public class RLWorksourceInfo
    {
        private String _worksourceID;
        private String _worksourceName;
        private String _worksourceAlias;
        private String _comments;
        private String _centerBSB;
        private String _acctZone;
        private Boolean _pvAssist;
        private Boolean _forchqclearing;

        private WebClientInfo _client;
        private String _formatString;

        public RLWorksourceInfo(String worksourceID, String worksourceName, String worksourceAlias, String comments, String centerBSB, String acctZone, Boolean pvAssist, Boolean forChqClearing, WebClientInfo client)
        {
            _worksourceID = worksourceID;
            _worksourceName = worksourceName;
            _worksourceAlias = worksourceAlias;
            _comments = comments;
            _centerBSB = centerBSB;
            _acctZone = acctZone;
            _pvAssist = pvAssist;
            _forchqclearing = forChqClearing;
            _client = client;
            _formatString = null;
        }

        public String WorksourceID
        {
            get { return _worksourceID; }
        }

        public String WorksourceName
        {
            get { return _worksourceName; }
        }

        public String WorksourceAlias
        {
            get { return _worksourceAlias; }
        }

        public String Comments
        {
            get { return _comments; }
        }

        public String CenterBSB
        {
            get { return _centerBSB; }
        }

        public String AcctZone
        {
            get { return _acctZone; }
        }

        public Boolean PVAssist
        {
            get { return _pvAssist; }
        }

        public Boolean ForChqClearing
        {
            get { return _forchqclearing; }
        }

        public WebClientInfo Client
        {
            get { return _client; }
        }

        /// <summary>
        /// FormatString used by ToString to return a string representation of this object:
        /// {0} = WorksourceID
        /// {1} = WorksourceName
        /// {2} = ClientCode
        /// {3} = ClientName
        /// </summary>
        public String FormatString
        {
            get { return _formatString; }
            set
            {
                if (value == String.Empty)
                    _formatString = null;
                else
                    _formatString = value;
            }
        }

        public override string ToString()
        {
            if (this.FormatString == null)
            {
                return base.ToString();
            }
            else
            {
                String str = String.Empty;

                try
                {
                    str = String.Format(this.FormatString, this.WorksourceID, this.WorksourceName, this.Client.Code, this.Client.Name);
                }
                catch
                {
                    str = base.ToString();
                }

                return str;
            }
        }
    }
}

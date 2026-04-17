using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UBPCWeb.Model
{
    public class jQueryDataTableGridModel
    {
        private Object _data;
        private int _totalRecords;
        private int _totalDisplayRecords;

        public Object aaData
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        public int iTotalRecords
        {
            get
            {
                return _totalRecords;
            }
            set
            {
                _totalRecords = value;
            }
        }

        public int iTotalDisplayRecords
        {
            get
            {
                return _totalDisplayRecords;
            }
            set
            {
                _totalDisplayRecords = value;
            }
        }

        public jQueryDataTableGridModel()
        {
            _data = null;
            _totalRecords = 0;
            _totalDisplayRecords = 0;
        }
    }    
}
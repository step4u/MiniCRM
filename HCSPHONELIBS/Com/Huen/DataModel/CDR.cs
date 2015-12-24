using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Huen.DataModel
{
    public class CDR
    {
        public int index = -1;
        public int inner_fseq = -1;
        public string office_name = string.Empty;
        public DateTime startdate;
        public DateTime enddate;
        public int caller_type = -1;
        public string caller = string.Empty;
        public string caller_ipn_number = string.Empty;
        public string caller_group_code = string.Empty;
        public string caller_group_name = string.Empty;
        public string caller_human_name = string.Empty;
        public string callee = string.Empty;
        public int callee_type = -1;
        public string callee_ipn_number = string.Empty;
        public string callee_group_code = string.Empty;
        public string callee_group_name = string.Empty;
        public string callee_human_name = string.Empty;
        public int result = -1;
        public int seq = -1;
    }
}

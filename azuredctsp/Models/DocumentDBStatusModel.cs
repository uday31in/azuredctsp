using System;
using System.Collections.Generic;
using System.Text;

namespace azuredctsp
{
    public class DocumentDBStatus
    { 
        public long docs { get; set; }
        public double writes { get; set; }
        public double rus { get; set; }
        public double reads { get; set; }
        public string Time { get; set; }
    }

    public class DocumentDBControllerModel
    {
       
        public string Guid { get;  set; }
    }
    
}

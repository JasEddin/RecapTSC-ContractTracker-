using System;
using System.Collections.Generic;
using System.Text;

namespace ContractChecker.Core.Models
{
    public class ContractFile
    {
        public string Name { get; set; }
        public string PathInApis { get; set; }
        public int LatestVersion { get; set; }
        public string Server { get; set; }
    }
}

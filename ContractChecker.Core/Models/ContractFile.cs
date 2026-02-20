using System;
using System.Collections.Generic;
using System.Text;

namespace ContractChecker.Core.Models
{
    public class ContractFile
    {
        public string Name { get; set; }
        public string FilePathinApisFolder { get; set; }
        public string[] Servers { get; set; }
    }
}

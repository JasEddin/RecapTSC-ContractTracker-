using System;
using System.Collections.Generic;
using System.Text;

namespace OpenApi.ContractGuard.Configurations
{
    internal class TrackedApiConfig
    {
        public string Url { get; set; }
        public string LocalContractPath { get; set; }
    }
}

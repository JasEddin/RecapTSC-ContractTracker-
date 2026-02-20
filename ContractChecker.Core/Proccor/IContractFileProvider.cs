using ContractChecker.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

public interface IContractFileProvider
{
    public List<ContractFile> LoadAllA();
}


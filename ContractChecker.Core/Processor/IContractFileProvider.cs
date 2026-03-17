using ContractChecker.Core.Models;

public interface IContractFileProvider
{
    public List<ContractFile> LoadAllLocalFiles();
}


using Kori.Models;

namespace Kori.Contracts;

public interface ISecureStorageManager
{
    bool Save(string key, string name, string value);

    Credential? Load(string key);
    
    bool Delete(string key, string? name=null);

}
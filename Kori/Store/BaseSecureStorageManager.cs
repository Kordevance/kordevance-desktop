using Kori.Contracts;
using Kori.Models;

namespace Kori.Store;

public abstract class BaseSecureStorageManager(string keyPrefix) : ISecureStorageManager
{
    protected string GetIdentifier(string key) => $"{keyPrefix}.{key}";

    public abstract bool Save(string key, string name, string value);
    public abstract Credential? Load(string key);
    public abstract bool Delete(string key, string? name=null);
    
}
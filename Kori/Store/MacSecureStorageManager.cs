using System.Text;
using KeychainVault;
using KeychainVault.Contracts;
using Kori.Models;

namespace Kori.Store;

public sealed class MacSecureStorageManager(string keyPrefix, bool isProd) : BaseSecureStorageManager(keyPrefix)
{
    private readonly IKeychain _keychain = isProd ? new MacOSKeychain() : new MacOSKeychain(false);
    
    public override bool Save(string key, string name, string value)
    {
        var id = GetIdentifier(key);
        var secret = Encoding.UTF8.GetBytes(value);
        try
        {
            _keychain.AddGenericPasswordItem(id, name, secret);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override Credential? Load(string key)
    {
        try
        {
            var id = GetIdentifier(key);
            var result = _keychain.LoadGenericPasswordItem(id);
            if (result is null)
            {
                return null;
            }
            
            var value = Encoding.UTF8.GetString(result.Secret);
            var property = result.Account;

            if (string.IsNullOrEmpty(property) || string.IsNullOrEmpty(value))
            {
                return null;
            }
                
            return new Credential(property, value);

        }
        catch
        {
            return null;
        }
    }

    public override bool Delete(string key, string? name = null)
    {
        try
        {
            return _keychain.DeleteGenericPasswordItem(GetIdentifier(key), name);
        }
        catch
        {
            return false;
        }
    }
}
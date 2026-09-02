using System.Net;
using AdysTech.CredentialManager;
using Kori.Models;

namespace Kori.Store;

public sealed class WindowsSecureStorageManager(string keyPrefix) : BaseSecureStorageManager(keyPrefix)
{
    public override bool Save(string key, string name, string value)
    {
        var id = GetIdentifier(key);
        var cred = new NetworkCredential(name, value);

        try
        {
            CredentialManager.SaveCredentials(id, cred);
            return true;
        }
        catch
        {
            return false;
        }
        
    }

    public override Credential? Load(string key)
    {
        var id = GetIdentifier(key);
        var cred = CredentialManager.GetCredentials(id);
        return cred is not null ? new Credential(cred.UserName, cred.Password) : null;
    }

    public override bool Delete(string key, string? name=null)
    {
        var id = GetIdentifier(key);
        try
        {
            return CredentialManager.RemoveCredentials(id);
        }
        catch
        {
            return false;
        }
    }
}
using System.Threading.Tasks;
using Kori.Contracts;
using Kori.Models;

namespace Kori;

public sealed class SessionContext
{
    private readonly ISecureStorageManager _store;
    
    private const string GatewayKey = "gateway";

    public Gateway? Gateway { get; private set; }
    public Profile? Profile { get; set; }
    
    public bool IsConnectedToGateway => Gateway != null;

    public SessionContext(ISecureStorageManager store)
    {
        _store = store;
    }
    
    public Task Initialize()
    {
        var gateway = _store.Load(GatewayKey);
        if (gateway == null){ return Task.CompletedTask; }

        Gateway = new Gateway
        {
            Address = gateway.Name,
            Token = gateway.Value
        };
        
        return Task.CompletedTask;
    }

    public void SaveGateway(Gateway gateway)
    {
        _store.Save(GatewayKey, gateway.Address, gateway.Token);
        Gateway = gateway;
    }

    public void Logout()
    {   
        _store.Delete(GatewayKey);
        Gateway = null;
        Profile = null;
    }
}
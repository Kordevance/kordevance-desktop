using System.Collections.Generic;
using System.Threading.Tasks;
using Kori.Models;

namespace Kori.Contracts;

public interface IGatewayHandler
{
    Task<bool> HandleTestGatewayConnection(string address);
    
    Task<Gateway?> HandleRegisterDeviceToGateway(string address, string code);
    Task<string> HandleCreatePairingInvite(Gateway gateway);
    Task<List<Device>> HandleRegisteredDevices(Gateway gateway);
    Task<bool> HandlePromoteDeviceToOwner(Gateway gateway, Device device);
    
    Task<List<Profile>?> HandleGetAllProfiles(Gateway gateway);
    Task<Profile?> HandleCreateProfile(Gateway gateway, string name);
    Task<bool> HandleUpdateProfile(Gateway gateway, Profile profile);
    Task<bool> HandleDeleteProfile(Gateway gateway, Profile profile);
    
    Task<Provider?> HandleAddProvider(Gateway gateway, Provider provider, string profileId, string? displayName=null);
    Task<bool> HandleDeleteProvider(Gateway gateway, Provider provider, string profileId);
    Task<List<Provider>> HandleGetAllProviders(Gateway gateway, string profileId);
    Task<List<string>> HandleGetProviderModels(Gateway gateway,  Provider provider, string profileId);
    
    Task<bool> HandleAssignModel(Gateway gateway, string providerId, string modelId, ModelAssignmentType assignment, string profileId);
    Task<bool> HandleDeleteModelAssignment(Gateway gateway, ModelAssignmentType assignment, string profileId);
    Task<List<ModelAssignment>> HandleGetAllModelAssignments(Gateway gateway, string profileId);
    
    Task<Chat?> HandleChat(Gateway gateway, string profileId, Chat chat);
    Task<List<Conversation>> HandleGetAllChats(Gateway gateway, string profileId);
    
    Task<List<Connector>> HandleGetAllConnectors(Gateway gateway, string profileId);
    Task<bool> HandleDeleteConnector(Gateway gateway, Connector connector, string profileId);
    Task<string> HandleAddConnector(Gateway gateway, Connector connector, string profileId);
    
    Task<List<Goal>> HandleGetAllGoals(Gateway gateway, string profileId);
    Task<bool> HandleDeleteGoal(Gateway gateway, Goal goal, string profileId);
}
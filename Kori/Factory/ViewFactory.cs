using System;
using Kori.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Kori.Factory;

public sealed class ViewFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public ViewFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T GetView<T>() where T : ViewModelBase
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public T GetView<T>(params object[] parameters) where T : class
    {
        return ActivatorUtilities.CreateInstance<T>(_serviceProvider, parameters);
    }
    
}
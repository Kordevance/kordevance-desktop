using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kori.ViewModels;

public class ViewModelBase : ObservableValidator
{
    public virtual Task Initialize() { return Task.CompletedTask; }
}
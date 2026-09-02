using System.ComponentModel.DataAnnotations;

namespace Kori.Validators;

public class LocalizedRequiredAttribute : RequiredAttribute
{
    public LocalizedRequiredAttribute(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public override string FormatErrorMessage(string name)
    {
        if (ErrorMessage == null)
        {
            return base.FormatErrorMessage(name);
        }
        
        return Localizer.Instance?[ErrorMessage] ?? ErrorMessage;
    }
    
}
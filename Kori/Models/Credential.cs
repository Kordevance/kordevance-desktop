namespace Kori.Models;

public sealed class Credential
{
    public string Name { get; }
    public string Value { get; }

    public Credential(string name, string value)
    {
        Name = name;
        Value = value;
    }
}
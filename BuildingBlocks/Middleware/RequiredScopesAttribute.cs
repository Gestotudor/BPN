namespace BuildingBlocks.Middleware;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequiredScopesAttribute : Attribute
{
    public RequiredScopesAttribute(params string[] scopes)
    {
        Scopes = scopes ?? Array.Empty<string>();
    }

    public IReadOnlyCollection<string> Scopes { get; }
}

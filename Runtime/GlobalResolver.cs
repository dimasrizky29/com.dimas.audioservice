using VContainer;

public static class GlobalResolver
{
    // Ini akan menampung Resolver dari ProjectLifetimeScope
    public static IObjectResolver Instance { get; set; }
}
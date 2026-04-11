using UnityEngine;
using VContainer;
using VContainer.Unity;

public class AudioLifetimeScope : LifetimeScope
{
    [SerializeField] private AudioLibrary audioLibrary;

    protected override void Configure(IContainerBuilder builder)
    {
        // --- Audio Service ---
        builder.RegisterInstance(audioLibrary);
        builder.Register<IAudioService, AudioService>(Lifetime.Singleton);

        // Set Resolver (Danger, only for button sound or small utility)
        builder.RegisterBuildCallback(resolver =>
        {
            GlobalResolver.Instance = resolver;
        });
    }
}

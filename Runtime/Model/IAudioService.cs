using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IAudioService
{
    // Fire and Forget (untuk SFX/UI)
    void PlayOneShot(string id);

    // Play dengan kontrol posisi (3D Sound)
    void PlayAtPosition(string id, Vector3 position);

    // Play BGM (menggantikan BGM sebelumnya)
    UniTask PlayBGM(string id, float fadeDuration = 1f);

    // Settings
    void SetMute(AudioType type, bool isMuted);
    void SetVolume(AudioType type, float volume);

    bool IsBgmMuted { get; }
    bool IsSfxMuted { get; }
}
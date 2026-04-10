# 🔊 AudioService

Modular Audio Service for Unity — Addressable-based audio system dengan VContainer DI, object pooling, dan persistent settings.

## Requirements

| Dependency | Version |
|---|---|
| Unity | `6000.3.7f1+` |
| UniTask | `2.5.10` |
| VContainer | `1.17.0` |
| Addressables | `2.8.1` |

## Installation

**Package Manager** → **+** → **Add package from git URL:**

```
https://github.com/dimasrizky29/com.dimas.audioservice.git
```

## Quick Start (Drag & Drop)

1. Buka **Package Manager** → pilih **AudioService** → tab **Samples** → **Import** "Example Sample".
2. Drag prefab `AudioService` dan `Button (Legacy)` ke **Hierarchy**.
3. Tekan **Play** ▶️ → klik tombol untuk mendengar suara.

## Setup Guide

### 1. Buat AudioLibrary

**Right Click** di Project → **Create → Service → Game → Audio Library**, lalu isi entries:

| Field | Description |
|---|---|
| `id` | ID unik, misal `"ButtonGeneral"` |
| `type` | Kategori: `BGM`, `SFX`, `UI` |
| `clipReference` | Addressable reference ke AudioClip |
| `volume` | Volume default (0–1) |
| `loop` | Loop toggle |

> Pastikan AudioClip sudah ditandai **Addressable** di Inspector.

### 2. Register di VContainer

```csharp
public class ProjectLifetimeScope : LifetimeScope
{
    [SerializeField] private AudioLibrary audioLibrary;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(audioLibrary);
        builder.Register<IAudioService, AudioService>(Lifetime.Singleton);

        // Wajib jika menggunakan UIButtonSound
        builder.RegisterBuildCallback(resolver =>
        {
            GlobalResolver.Instance = resolver;
        });
    }
}
```

### 3. Gunakan AudioService

```csharp
// Via Injection (recommended)
[Inject] private IAudioService _audioService;

_audioService.PlayOneShot("ButtonGeneral");          // SFX one-shot
_audioService.PlayAtPosition("Explosion", position); // 3D positional
await _audioService.PlayBGM("BGM_MainMenu", 1.5f);  // BGM dengan fade

// Via GlobalResolver (untuk MonoBehaviour tanpa injection)
var audio = GlobalResolver.Instance.Resolve<IAudioService>();
audio.PlayOneShot("ButtonGeneral");
```

## API Reference

### IAudioService

| Method / Property | Description |
|---|---|
| `PlayOneShot(string id)` | Mainkan SFX/UI sekali putar |
| `PlayAtPosition(string id, Vector3 pos)` | Mainkan audio pada posisi 3D |
| `PlayBGM(string id, float fade = 1f)` | Mainkan BGM dengan fade transition |
| `SetMute(AudioType type, bool muted)` | Toggle mute per kategori (auto-save ke PlayerPrefs) |
| `SetVolume(AudioType type, float vol)` | Atur volume per kategori (0.0–1.0) |
| `IsBgmMuted` / `IsSfxMuted` | Cek status mute |

### UIButtonSound

Komponen untuk suara tombol UI. Tambahkan ke GameObject Button, atur `Sound Id` di Inspector (default: `"ButtonGeneral"`). Menggunakan `IPointerDownHandler` untuk respons yang lebih cepat.

> Membutuhkan `GlobalResolver.Instance` yang sudah di-set di LifetimeScope.

## License

Lihat [LICENSE](LICENSE) untuk informasi lisensi.

**Author:** [Dimas Rizky](https://github.com/dimasrizky29)
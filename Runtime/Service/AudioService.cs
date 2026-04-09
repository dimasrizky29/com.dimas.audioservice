using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AudioService : IAudioService, System.IDisposable
{
    private readonly AudioLibrary _library;
    private readonly Transform _poolRoot; // Wadah untuk AudioSource di scene

    // Pooling System
    private readonly Queue<AudioSource> _sfxPool = new();
    private readonly List<AudioSource> _activeLoopingSources = new();
    private AudioSource _bgmSource;

    // State
    private bool _isSfxMuted;
    private bool _isBgmMuted;
    private float _sfxVolume = 1f;
    private float _bgmVolume = 1f;

    public bool IsBgmMuted => _isBgmMuted;
    public bool IsSfxMuted => _isSfxMuted;

    // Cache handle addressables agar bisa direlease
    private Dictionary<string, AsyncOperationHandle<AudioClip>> _loadedClips = new();

    // Constructor Injection via VContainer
    public AudioService(AudioLibrary library)
    {
        _library = library;
        _library.Initialize();

        // Setup Root Object di Scene (Hidden)
        var go = new GameObject("[AudioService_Runtime]");
        Object.DontDestroyOnLoad(go);
        _poolRoot = go.transform;

        InitializeSources();
        LoadSettings();
    }

    private void InitializeSources()
    {
        // Setup BGM Source
        _bgmSource = CreateNewSource("BGM_Source");
        _bgmSource.loop = true;

        // Pre-warm Pool (buat 10 audio source di awal)
        for (int i = 0; i < 10; i++)
        {
            var source = CreateNewSource($"SFX_Pool_{i}");
            source.gameObject.SetActive(false);
            _sfxPool.Enqueue(source);
        }
    }

    private AudioSource CreateNewSource(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_poolRoot);
        return go.AddComponent<AudioSource>();
    }

    // --- MAIN LOGIC ---
    public async void PlayOneShot(string id)
    {
        if (_isSfxMuted) return;

        var entry = _library.GetEntry(id);
        if (entry == null)
        {
            Debug.LogWarning($"[AudioService] Audio ID not found: {id}");
            return;
        }

        AudioClip clip = await LoadClipAsync(entry);
        if (clip == null) return;

        // Ambil source dari pool
        AudioSource source = GetPooledSource();
        source.gameObject.SetActive(true);
        source.volume = entry.volume * _sfxVolume;
        source.spatialBlend = 0f; // 2D Sound

        source.PlayOneShot(clip);

        // Return to pool logic (manual simple delay)
        ReturnToPoolDelayed(source, clip.length).Forget();
    }

    public async void PlayAtPosition(string id, Vector3 position)
    {
        if (_isSfxMuted) return;

        var entry = _library.GetEntry(id);
        if (entry == null) return;

        AudioClip clip = await LoadClipAsync(entry);
        if (clip == null) return;

        AudioSource source = GetPooledSource();
        source.transform.position = position;
        source.gameObject.SetActive(true);
        //source.spatialBlend = 1f; // 3D Sound
        source.spatialBlend = .8f; // semi 3D Sound
        source.minDistance = 2f;
        source.maxDistance = 20f;
        source.volume = entry.volume * _sfxVolume;

        source.PlayOneShot(clip);
        ReturnToPoolDelayed(source, clip.length).Forget();
    }

    public async UniTask PlayBGM(string id, float fadeDuration = 1f)
    {
        var entry = _library.GetEntry(id);
        if (entry == null) return;

        AudioClip clip = await LoadClipAsync(entry);
        if (clip == null) return;

        if (_bgmSource.isPlaying)
        {
            // Simple fade out
            float startVol = _bgmSource.volume;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                _bgmSource.volume = Mathf.Lerp(startVol, 0, t / fadeDuration);
                await UniTask.Yield();
            }
        }

        _bgmSource.clip = clip;
        _bgmSource.volume = _isBgmMuted ? 0 : entry.volume * _bgmVolume;
        _bgmSource.Play();
    }

    // --- HELPER METHODS ---
    private AudioSource GetPooledSource()
    {
        if (_sfxPool.Count > 0)
            return _sfxPool.Dequeue();

        return CreateNewSource("SFX_Pool_Expand"); // Auto expand pool if empty
    }

    private async UniTaskVoid ReturnToPoolDelayed(AudioSource source, float delay)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
        if (source != null)
        {
            source.gameObject.SetActive(false);
            source.transform.SetParent(_poolRoot); // Reset parent
            _sfxPool.Enqueue(source);
        }
    }

    // Addressables Loader Wrapper
    private async UniTask<AudioClip> LoadClipAsync(AudioEntry entry)
    {
        // Cek apakah clip sudah di-load sebelumnya?
        if (entry.clipReference.Asset != null)
            return entry.clipReference.Asset as AudioClip;

        // Jika belum, load dari Addressables
        var op = entry.clipReference.LoadAssetAsync<AudioClip>();
        await op.ToUniTask();

        if (op.Status == AsyncOperationStatus.Succeeded)
        {
            // Simpan handle jika perlu release manual nanti
            // Untuk audio kecil/sering dipakai, kita biarkan di memory. 
            // Untuk optimasi lebih lanjut, gunakan Reference Counting.
            return op.Result;
        }

        Debug.LogError($"Failed to load audio: {entry.id}");
        return null;
    }

    // --- SETTINGS & MUTE ---
    public void SetMute(AudioType type, bool isMuted)
    {
        if (type == AudioType.BGM)
        {
            _isBgmMuted = isMuted;
            _bgmSource.mute = isMuted;
            PlayerPrefs.SetInt("Mute_BGM", isMuted ? 1 : 0);
        }
        else
        {
            _isSfxMuted = isMuted;
            // Logic untuk mematikan semua SFX yang sedang jalan (opsional)
            PlayerPrefs.SetInt("Mute_SFX", isMuted ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    public void SetVolume(AudioType type, float volume)
    {
        if (type == AudioType.BGM)
        {
            _bgmVolume = volume;
            _bgmSource.volume = volume;
        }
        else
        {
            _sfxVolume = volume;
        }
    }

    private void LoadSettings()
    {
        SetMute(AudioType.BGM, PlayerPrefs.GetInt("Mute_BGM", 0) == 1);
        SetMute(AudioType.SFX, PlayerPrefs.GetInt("Mute_SFX", 0) == 1);
    }

    public void Dispose()
    {
        // Cleanup Addressables handles jika perlu
        foreach (var handle in _loadedClips.Values)
        {
            Addressables.Release(handle);
        }
        _loadedClips.Clear();
    }
}
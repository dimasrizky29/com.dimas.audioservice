using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class AudioEntry
{
    public string id; // ID unik, misal "Button_Click"
    public AudioType type;
    public AssetReferenceT<AudioClip> clipReference; // Addressable Reference
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop;
}

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Service/Game/Audio Library")]
public class AudioLibrary : ScriptableObject
{
    public List<AudioEntry> entries;

    // Dictionary untuk pencarian cepat O(1)
    private Dictionary<string, AudioEntry> _lookup;

    public void Initialize()
    {
        _lookup = new Dictionary<string, AudioEntry>();
        foreach (var entry in entries)
        {
            if (!_lookup.ContainsKey(entry.id))
                _lookup.Add(entry.id, entry);
        }
    }

    public AudioEntry GetEntry(string id)
    {
        if (_lookup == null) Initialize();
        return _lookup.TryGetValue(id, out var entry) ? entry : null;
    }
}
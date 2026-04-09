using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

public class UIButtonSound : MonoBehaviour, IPointerDownHandler
{
    // Biarkan desainer memilih suara lewat Inspector, default ke UI_Click
    [SerializeField] private string _soundId = "ButtonGeneral";

    private IAudioService _audioService;

    // Menggunakan IPointerDownHandler agar lebih responsif daripada onClick
    public void OnPointerDown(PointerEventData eventData)
    {
        // Ambil service hanya jika belum ada (Lazy Loading)
        if (_audioService == null)
        {
            // Mengambil dari "Pintu Akses" Statis
            _audioService = GlobalResolver.Instance.Resolve<IAudioService>();
        }

        _audioService.PlayOneShot(_soundId);
    }
}
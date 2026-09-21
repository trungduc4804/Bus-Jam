using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AudioToggleButton : MonoBehaviour
{
    [Header("Sprite Icon Âm Thanh")]
    [Tooltip("Icon khi bật âm thanh (AudioOn)")]
    public Sprite iconAudioOn;
    [Tooltip("Icon khi tắt âm thanh (AudioMute)")]
    public Sprite iconAudioMute;

    [Header("Image hiển thị (Tự lấy nếu để trống)")]
    public Image targetImage;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
    }

    private void Start()
    {
        // Gán sự kiện OnClick
        button.onClick.RemoveListener(OnClickToggle);
        button.onClick.AddListener(OnClickToggle);

        // Lắng nghe sự kiện đổi trạng thái từ AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnAudioMuteChanged += UpdateIcon;
            UpdateIcon(AudioManager.Instance.IsMuted);
        }
        else
        {
            // Đọc trực tiếp từ PlayerPrefs nếu AudioManager chưa kịp khởi tạo
            bool isMuted = PlayerPrefs.GetInt("Sound_Muted", 0) == 1;
            UpdateIcon(isMuted);
        }
    }

    private void OnDestroy()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnAudioMuteChanged -= UpdateIcon;
        }
    }

    public void OnClickToggle()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleAudio();
            // Phát tiếng click nhẹ nếu âm thanh vừa được bật lên
            AudioManager.Instance.PlayButtonClick();
        }
        else
        {
            // Fallback hoạt động độc lập kể cả khi chưa kéo AudioManager vào Scene
            bool isMuted = PlayerPrefs.GetInt("Sound_Muted", 0) == 1;
            isMuted = !isMuted;
            PlayerPrefs.SetInt("Sound_Muted", isMuted ? 1 : 0);
            PlayerPrefs.Save();
            AudioListener.volume = isMuted ? 0f : 1f;
            UpdateIcon(isMuted);
        }
    }

    public void UpdateIcon(bool isMuted)
    {
        if (targetImage == null) return;

        if (isMuted && iconAudioMute != null)
        {
            targetImage.sprite = iconAudioMute;
        }
        else if (!isMuted && iconAudioOn != null)
        {
            targetImage.sprite = iconAudioOn;
        }
    }
}

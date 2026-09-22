using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources (Tự động thêm nếu để trống)")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Nhạc Nền (Background Music)")]
    [Tooltip("Nhạc nền vui tươi, nhẹ nhàng ở màn hình Menu")]
    public AudioClip bgmMenu;
    [Tooltip("Nhạc nền thư giãn, nhịp điệu khi chơi game")]
    public AudioClip bgmGameplay;

    [Header("Âm Thanh Khách & Tương Tác (Passenger SFX)")]
    [Tooltip("Tiếng 'Pop/Plop' giòn tan khi chạm vào khách thành công")]
    public AudioClip sfxTapKhach;
    [Tooltip("Tiếng 'Cục/Thump' báo lỗi khi khách bị người khác chặn lối")]
    public AudioClip sfxKhachBiChan;
    [Tooltip("Tiếng 'Ting/Ding' thanh thoát khi khách nhảy lên xe bus")]
    public AudioClip sfxKhachLenXe;
    [Tooltip("Tiếng 'Buzzer/Cảnh báo' khi tất cả các ô hàng chờ đều đầy")]
    public AudioClip sfxHangChoDay;

    [Header("Âm Thanh Xe Bus (Bus SFX)")]
    [Tooltip("Tiếng phanh/xì hơi khí nén (Air brake) khi xe bus vào bến đỗ")]
    public AudioClip sfxXeDen;
    [Tooltip("Tiếng còi 'Bíp bíp' vui nhộn + rồ ga khi xe đầy khách và rời đi")]
    public AudioClip sfxXeDi;

    [Header("Âm Thanh Kết Thúc Màn & UI")]
    [Tooltip("Tiếng kèn fanfare / pháo giấy chúc mừng khi vượt qua màn chơi")]
    public AudioClip sfxChienThang;
    [Tooltip("Tiếng nhạc buồn nhẹ / thất bại khi hết chỗ kẹt đường")]
    public AudioClip sfxThatBai;
    [Tooltip("Tiếng 'Click' ngắn khi bấm các nút trên giao diện")]
    public AudioClip sfxClickButton;

    [Header("Cài Đặt Âm Lượng")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;

    // Sự kiện thông báo khi trạng thái Mute thay đổi (để các nút Setting cập nhật icon)
    public event System.Action<bool> OnAudioMuteChanged;

    public bool IsMuted { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ AudioManager không bị hủy khi chuyển Scene
            KhoiTaoAudioSources();
            TaiCaiDatAmThanh();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        PhatNhacTheoScene(SceneManager.GetActiveScene().name);
    }

    private void KhoiTaoAudioSources()
    {
        // Tự tạo AudioSource cho Nhạc nền nếu chưa có
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        // Tự tạo AudioSource cho Sound Effects nếu chưa có
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    private void TaiCaiDatAmThanh()
    {
        // Đọc trạng thái từ PlayerPrefs (0: bật âm thanh, 1: tắt âm thanh)
        IsMuted = PlayerPrefs.GetInt("Sound_Muted", 0) == 1;
        CapNhatTrangThaiAmThanh();
    }

    // ==========================================
    // BẬT / TẮT ÂM THANH (DÙNG CHO NÚT SETTING)
    // ==========================================
    public void ToggleAudio()
    {
        IsMuted = !IsMuted;
        PlayerPrefs.SetInt("Sound_Muted", IsMuted ? 1 : 0);
        PlayerPrefs.Save();

        CapNhatTrangThaiAmThanh();

        // Kích hoạt event cho các UI button tự đổi sprite (AudioOn / AudioMute)
        OnAudioMuteChanged?.Invoke(IsMuted);

        Debug.Log($"[AudioManager] Trạng thái âm thanh: {(IsMuted ? "TẮT (Mute)" : "BẬT (Unmute)")}");
    }

    private void CapNhatTrangThaiAmThanh()
    {
        if (musicSource != null) musicSource.mute = IsMuted;
        if (sfxSource != null) sfxSource.mute = IsMuted;

        // Đồng thời cập nhật toàn cục AudioListener
        AudioListener.volume = IsMuted ? 0f : 1f;
    }

    // ==========================================
    // QUẢN LÝ NHẠC NỀN THEO SCENE
    // ==========================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ngắt ngay toàn bộ âm thanh hiệu ứng còn dở từ scene trước (như tiếng thua game dài)
        StopSFX();
        PhatNhacTheoScene(scene.name);
    }

    private void PhatNhacTheoScene(string sceneName)
    {
        if (sceneName.ToLower().Contains("menu"))
        {
            PlayMusic(bgmMenu);
        }
        else
        {
            PlayMusic(bgmGameplay);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        // Nếu clip đang phát chính là bài này rồi thì không phát lại từ đầu
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    // ==========================================
    // PHÁT HIỆU ỨNG ÂM THANH (SFX)
    // ==========================================
    public void PlaySFX(AudioClip clip, float pitchVariation = 0.08f)
    {
        if (clip == null || sfxSource == null || IsMuted) return;

        // Thay đổi nhẹ Pitch (+- 8%) giúp âm thanh sống động, bấm nhiều lần không bị chói tai
        sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void StopSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.Stop();
            sfxSource.clip = null;
        }
    }

    public void StopAllAudio()
    {
        StopMusic();
        StopSFX();
    }

    // Các hàm gọi nhanh tiện lợi từ bất kỳ script nào
    public void PlayTapKhach() => PlaySFX(sfxTapKhach, 0.12f);
    public void PlayKhachBiChan() => PlaySFX(sfxKhachBiChan, 0.05f);
    public void PlayKhachLenXe() => PlaySFX(sfxKhachLenXe, 0.06f);
    public void PlayHangChoDay() => PlaySFX(sfxHangChoDay, 0f);
    public void PlayXeDen() => PlaySFX(sfxXeDen, 0.04f);
    public void PlayXeDi() => PlaySFX(sfxXeDi, 0.04f);
    
    // Khi Chiến Thắng: Dừng nhạc nền và phát tiếng kèn/pháo hoa mừng chiến thắng
    public void PlayChienThang()
    {
        StopMusic();
        PlaySFX(sfxChienThang, 0f);
    }

    // Khi Thất Bại: Dừng nhạc nền và phát tiếng thất bại
    public void PlayThatBai()
    {
        StopMusic();
        PlaySFX(sfxThatBai, 0f);
    }

    public void PlayButtonClick() => PlaySFX(sfxClickButton, 0.05f);
}

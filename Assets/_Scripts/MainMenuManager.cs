using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [Header("Cài đặt Chuyển Scene")]
    [Tooltip("Tên chính xác của Scene chơi game")]
    public string gameplaySceneName = "MainGameplay";

    [Header("Bảng Chọn Level (Level Select)")]
    [Tooltip("Panel chọn màn chơi (nếu có). Khi bấm nút Level sẽ bật panel này")]
    public GameObject panelLevelSelect;

    [Header("Tùy chọn: Tự Động Sinh Nút Level")]
    [Tooltip("Khung chứa danh sách các nút level (Grid Layout Group hoặc Content của ScrollView)")]
    public Transform levelButtonsContainer;

    [Tooltip("Prefab nút bấm level")]
    public GameObject levelButtonPrefab;

    [Tooltip("Tổng số level hiện có trong game")]
    public int tongSoLevel = 5;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ẩn panel Level lúc mới vào menu (nếu đang bật)
        if (panelLevelSelect != null)
        {
            panelLevelSelect.SetActive(false);
        }

        // Nếu bạn có kéo thả Container và Prefab thì tự sinh nút, không thì bỏ qua
        TaoDanhSachNutLevel();
    }

    // ==========================================
    // 1. NÚT PLAY (CHƠI TIẾP HOẶC BẮT ĐẦU MỚI)
    // ==========================================
    public void PlayGame()
    {
        Debug.Log("[MainMenu] Bắt đầu chơi game!");
        // Lấy level người chơi đang chơi dở (mặc định là level 0)
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 0);
        LevelManager.levelIndex = currentLevel;

        ChuyenVaoGame();
    }

    // ==========================================
    // 2. NÚT LEVEL (BẬT / TẮT BẢNG CHỌN LEVEL)
    // ==========================================
    public void OpenLevelSelect()
    {
        if (panelLevelSelect != null)
        {
            panelLevelSelect.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[MainMenuManager] Bạn chưa kéo Panel Level Select vào ô panelLevelSelect trong Inspector!");
        }
    }

    public void CloseLevelSelect()
    {
        if (panelLevelSelect != null)
        {
            panelLevelSelect.SetActive(false);
        }
    }

    // ==========================================
    // 3. CHỌN 1 LEVEL CỤ THỂ
    // ==========================================
    /// <summary>
    /// Chọn level theo số hiển thị (Level 1, 2, 3...)
    /// </summary>
    /// <param name="levelNumber">Số thứ tự màn (1, 2, 3...)</param>
    public void SelectLevel(int levelNumber)
    {
        int index = Mathf.Max(0, levelNumber - 1);
        Debug.Log($"[MainMenu] Chọn Level {levelNumber} (index {index})");

        LevelManager.levelIndex = index;
        PlayerPrefs.SetInt("CurrentLevel", index);
        PlayerPrefs.Save();

        ChuyenVaoGame();
    }

    /// <summary>
    /// Chọn level theo chỉ số mảng (0, 1, 2...)
    /// </summary>
    public void SelectLevelIndex(int index)
    {
        index = Mathf.Max(0, index);
        Debug.Log($"[MainMenu] Chọn Level Index: {index}");

        LevelManager.levelIndex = index;
        PlayerPrefs.SetInt("CurrentLevel", index);
        PlayerPrefs.Save();

        ChuyenVaoGame();
    }

    // ==========================================
    // 4. NÚT QUIT (THOÁT GAME)
    // ==========================================
    public void QuitGame()
    {
        Debug.Log("[MainMenu] Đang thoát game...");
        Application.Quit();

#if UNITY_EDITOR
        // Dành cho việc test trong Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ==========================================
    // HÀM BỔ TRỢ
    // ==========================================
    private void ChuyenVaoGame()
    {
        if (!string.IsNullOrEmpty(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            Debug.LogError("[MainMenuManager] gameplaySceneName đang để trống! Hãy điền tên Scene chơi game (ví dụ: MainGameplay).");
        }
    }

    /// <summary>
    /// Tự động sinh nút bấm từ Level 1 đến tongSoLevel nếu có gắn Container & Prefab
    /// </summary>
    private void TaoDanhSachNutLevel()
    {
        if (levelButtonsContainer == null || levelButtonPrefab == null) return;

        // Xóa các nút con tạm trong container
        foreach (Transform child in levelButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        // Level cao nhất đã mở khóa (mặc định mở ít nhất level 1)
        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);

        for (int i = 1; i <= tongSoLevel; i++)
        {
            int levelNum = i;
            GameObject btnObj = Instantiate(levelButtonPrefab, levelButtonsContainer);

            // Gán chữ hiển thị trên nút
            TextMeshProUGUI tmpText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = levelNum.ToString();
            }
            else
            {
                Text legacyText = btnObj.GetComponentInChildren<Text>();
                if (legacyText != null) legacyText.text = levelNum.ToString();
            }

            // Gán sự kiện click
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SelectLevel(levelNum));
            }
        }
    }
}

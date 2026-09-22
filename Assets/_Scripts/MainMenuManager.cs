using System.Collections.Generic;
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
    [Tooltip("Panel chọn màn chơi. Khi bấm nút Level sẽ bật panel này")]
    public GameObject panelLevelSelect;

    [Header("Quản lý Khóa / Mở Khóa 10 Nút Level")]
    [Tooltip("Kéo thả 10 nút Level (Level 1 đến Level 10) trong Scene vào đây. Nếu để trống code sẽ tự tìm")]
    public Button[] danhSachNutLevel;

    [Tooltip("Sprite hình ổ khóa (Assets/_Ui/_Icon/LockLV.png)")]
    public Sprite spriteKhoaLevel;

    [Tooltip("Kích thước hiển thị của icon ổ khóa trên nút")]
    public Vector2 kichThuocIconKhoa = new Vector2(85f, 85f);

    [Tooltip("Màu làm tối/mờ nút khi bị khóa")]
    public Color mauNutKhiKhoa = new Color(0.65f, 0.65f, 0.65f, 1f);
    public Color mauNutBinhThuong = Color.white;

    [Header("Tùy chọn: Tự Động Sinh Nút Level (Nếu dùng Grid)")]
    [Tooltip("Khung chứa danh sách các nút level (nếu dùng Grid Layout sinh tự động)")]
    public Transform levelButtonsContainer;
    public GameObject levelButtonPrefab;
    public int tongSoLevel = 10;

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

        // Cập nhật trạng thái khóa/mở khóa cho 10 nút Level
        CapNhatTrangThaiKhoaLevel();

        // Nếu có kéo thả Container và Prefab thì tự sinh nút động (tùy chọn)
        TaoDanhSachNutLevel();
    }

    // ==========================================
    // 1. NÚT PLAY (CHƠI TIẾP HOẶC BẮT ĐẦU MỚI)
    // ==========================================
    public void PlayGame()
    {
        Debug.Log("[MainMenu] Bắt đầu chơi game!");
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 0);

        // Đảm bảo không mở level vượt quá level đã mở khóa
        if (currentLevel >= maxUnlocked)
        {
            currentLevel = maxUnlocked - 1;
        }

        LevelManager.levelIndex = Mathf.Max(0, currentLevel);
        ChuyenVaoGame();
    }

    // ==========================================
    // 2. NÚT LEVEL (BẬT / TẮT BẢNG CHỌN LEVEL)
    // ==========================================
    public void OpenLevelSelect()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        if (panelLevelSelect != null)
        {
            panelLevelSelect.SetActive(true);
            // Cập nhật lại trạng thái khóa/mở khóa mỗi khi mở bảng
            CapNhatTrangThaiKhoaLevel();
        }
        else
        {
            Debug.LogWarning("[MainMenuManager] Bạn chưa kéo Panel Level Select vào ô panelLevelSelect trong Inspector!");
        }
    }

    public void CloseLevelSelect()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

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
    public void SelectLevel(int levelNumber)
    {
        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);

        // Nếu level này đang bị khóa -> Không cho vào chơi và báo lỗi
        if (levelNumber > maxUnlocked)
        {
            Debug.LogWarning($"[MainMenu] Level {levelNumber} đang bị khóa! Bạn cần hoàn thành Level {maxUnlocked} trước.");
            if (AudioManager.Instance != null) AudioManager.Instance.PlayKhachBiChan(); // Tiếng báo lỗi
            return;
        }

        int index = Mathf.Max(0, levelNumber - 1);
        Debug.Log($"[MainMenu] Chọn Level {levelNumber} (index {index})");

        LevelManager.levelIndex = index;
        PlayerPrefs.SetInt("CurrentLevel", index);
        PlayerPrefs.Save();

        ChuyenVaoGame();
    }

    public void SelectLevelIndex(int index)
    {
        SelectLevel(index + 1);
    }

    // ==========================================
    // 4. LOGIC KHÓA & MỞ KHÓA LEVEL
    // ==========================================
    public void CapNhatTrangThaiKhoaLevel()
    {
        // Level cao nhất hiện tại người chơi đã mở khóa (mặc định là Level 1)
        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);

        // Tự động tìm 10 nút level nếu trong Inspector chưa kéo thả
        if (danhSachNutLevel == null || danhSachNutLevel.Length == 0)
        {
            TuDongTimDanhSachNutLevel();
        }

        if (danhSachNutLevel == null || danhSachNutLevel.Length == 0) return;

        for (int i = 0; i < danhSachNutLevel.Length; i++)
        {
            Button btn = danhSachNutLevel[i];
            if (btn == null) continue;

            int levelNum = i + 1;
            bool isUnlocked = (levelNum <= maxUnlocked);

            // 1. Cài đặt có tương tác được nút không
            btn.interactable = isUnlocked;

            // 2. Đổi màu nút (nút bị khóa thì tối mờ đi nhẹ)
            Image btnImage = btn.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = isUnlocked ? mauNutBinhThuong : mauNutKhiKhoa;
            }

            // 3. Ẩn chữ "Level X" khi bị khóa để nhường chỗ cho icon ổ khóa
            TextMeshProUGUI tmpText = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmpText != null)
            {
                tmpText.gameObject.SetActive(isUnlocked);
            }

            // 4. Tìm hoặc tự động tạo GameObject icon ổ khóa bên trong nút
            Transform lockChild = btn.transform.Find("LockIcon");
            if (lockChild == null)
            {
                lockChild = btn.transform.Find("Lock");
            }

            // Nếu nút chưa có đối tượng ổ khóa, tự động tạo mới
            if (lockChild == null)
            {
                GameObject lockObj = new GameObject("LockIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                lockObj.transform.SetParent(btn.transform, false);

                RectTransform rect = lockObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = kichThuocIconKhoa;

                Image lockImg = lockObj.GetComponent<Image>();
                lockImg.raycastTarget = false;
                if (spriteKhoaLevel != null)
                {
                    lockImg.sprite = spriteKhoaLevel;
                }
                lockChild = lockObj.transform;
            }

            // Cập nhật trạng thái hiển thị của ổ khóa
            if (lockChild != null)
            {
                // Đang bị khóa -> Bật ổ khóa; Đã mở -> Tắt ổ khóa
                lockChild.gameObject.SetActive(!isUnlocked);

                Image img = lockChild.GetComponent<Image>();
                if (img != null && spriteKhoaLevel != null)
                {
                    img.sprite = spriteKhoaLevel;
                }
            }

            // 5. Gán sự kiện OnClick cho nút
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectLevel(levelNum));
        }
    }

    private void TuDongTimDanhSachNutLevel()
    {
        if (panelLevelSelect == null) return;

        List<Button> list = new List<Button>();
        Button[] allBtns = panelLevelSelect.GetComponentsInChildren<Button>(true);
        foreach (Button b in allBtns)
        {
            if (b.gameObject.name.ToLower().Contains("back")) continue;

            TextMeshProUGUI tmp = b.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null && tmp.text.ToLower().Contains("level"))
            {
                list.Add(b);
            }
            else if (b.gameObject.name.StartsWith("Button"))
            {
                list.Add(b);
            }
        }

        if (list.Count > 0)
        {
            // Sắp xếp các nút chính xác từ Level 1 đến Level 10 dựa trên chữ số trong nhãn
            list.Sort((a, b) =>
            {
                int numA = LaySoLevelTuButton(a);
                int numB = LaySoLevelTuButton(b);
                return numA.CompareTo(numB);
            });

            danhSachNutLevel = list.ToArray();
            Debug.Log($"[MainMenu] Đã tự động tìm và sắp xếp {danhSachNutLevel.Length} nút Level!");
        }
    }

    private int LaySoLevelTuButton(Button btn)
    {
        if (btn == null) return 0;
        TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null)
        {
            string digitsOnly = System.Text.RegularExpressions.Regex.Replace(tmp.text, @"\D", "");
            if (int.TryParse(digitsOnly, out int num)) return num;
        }
        return 0;
    }

    // ==========================================
    // 5. CÁC HÀM TIỆN ÍCH DÀNH CHO EDITOR TEST
    // ==========================================
#if UNITY_EDITOR
    [ContextMenu("Reset Tiến Trình (Về Level 1)")]
    public void ResetTienTrinhLevel()
    {
        PlayerPrefs.SetInt("MaxUnlockedLevel", 1);
        PlayerPrefs.SetInt("CurrentLevel", 0);
        PlayerPrefs.Save();
        CapNhatTrangThaiKhoaLevel();
        Debug.Log("[MainMenu] Đã reset: Chỉ mở Level 1, các level 2-10 bị khóa!");
    }

    [ContextMenu("Mở Khóa Toàn Bộ 10 Level")]
    public void MoKhoaToanBoLevel()
    {
        PlayerPrefs.SetInt("MaxUnlockedLevel", 10);
        PlayerPrefs.Save();
        CapNhatTrangThaiKhoaLevel();
        Debug.Log("[MainMenu] Đã mở khóa toàn bộ 10 Level để test!");
    }
#endif

    // ==========================================
    // 6. NÚT QUIT (THOÁT GAME)
    // ==========================================
    public void QuitGame()
    {
        Debug.Log("[MainMenu] Đang thoát game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

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

    private void TaoDanhSachNutLevel()
    {
        if (levelButtonsContainer == null || levelButtonPrefab == null) return;

        foreach (Transform child in levelButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);

        for (int i = 1; i <= tongSoLevel; i++)
        {
            int levelNum = i;
            GameObject btnObj = Instantiate(levelButtonPrefab, levelButtonsContainer);

            TextMeshProUGUI tmpText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null) tmpText.text = levelNum.ToString();

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = (levelNum <= maxUnlocked);
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SelectLevel(levelNum));
            }
        }
    }
}

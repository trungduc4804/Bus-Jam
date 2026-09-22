using UnityEngine;
using UnityEngine.SceneManagement; // Thư viện bắt buộc để load lại màn chơi

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Giao diện UI")]
    public GameObject panelVictory;
    public GameObject panelGameOver;
    public GameObject panelSetting;

    // Biến khóa chặn việc gọi Thắng/Thua nhiều lần
    private bool isGameOver = false; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void WinGame()
    {
        if (isGameOver) return; // Nếu game đã kết thúc rồi thì bỏ qua
        
        isGameOver = true;
        Debug.Log("Victory! Hoàn thành xuất sắc!");
        
        // Phát âm thanh chiến thắng
        if (AudioManager.Instance != null) AudioManager.Instance.PlayChienThang();

        if (panelVictory != null) 
            panelVictory.SetActive(true); // Bật bảng Win
        else
            Debug.LogWarning("[GameManager] Bạn chưa kéo Panel Victory vào ô Panel Victory trong Inspector của GameManager!");
    }

    public void LoseGame()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        Debug.Log("Game Over! Kẹt xe rồi!");
        
        // Phát âm thanh thất bại
        if (AudioManager.Instance != null) AudioManager.Instance.PlayThatBai();

        if (panelGameOver != null) 
            panelGameOver.SetActive(true); // Bật bảng Lose
        else
            Debug.LogWarning("[GameManager] Bạn chưa kéo Panel Game Over vào ô Panel Game Over trong Inspector của GameManager!");
    }

    // Hàm này sẽ được gán vào nút "Replay" trên UI
    public void ReplayGame()
    {
        // Ngắt ngay lập tức âm thanh thua game (hoặc SFX còn dở) trước khi load lại
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopSFX();
            AudioManager.Instance.PlayButtonClick();
        }

        // Load lại chính Scene đang mở hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopSFX();
            AudioManager.Instance.PlayButtonClick();
        }

        LevelManager.levelIndex++;
        PlayerPrefs.SetInt("CurrentLevel", LevelManager.levelIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Quay trở về màn hình Menu chính
    public void BackToMenu()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopSFX();
            AudioManager.Instance.PlayButtonClick();
        }

        SceneManager.LoadScene("MenuGame");
    }

    public void Setting()
    {
        if (panelSetting != null) 
            panelSetting.SetActive(true); // Bật bảng Setting
        else
            Debug.LogWarning("[GameManager] Bạn chưa kéo Panel Setting vào ô Panel Setting trong Inspector của GameManager!");
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
    }

    public void CloseSetting()
    {
        if (panelSetting != null) 
            panelSetting.SetActive(false); // Tắt bảng Setting
        else
            Debug.LogWarning("[GameManager] Bạn chưa kéo Panel Setting vào ô Panel Setting trong Inspector của GameManager!");
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
    }
}
using UnityEngine;
using UnityEngine.SceneManagement; // Thư viện bắt buộc để load lại màn chơi

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Giao diện UI")]
    public GameObject panelVictory;
    public GameObject panelGameOver;

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
        
        if (panelVictory != null) 
            panelVictory.SetActive(true); // Bật bảng Win
    }

    public void LoseGame()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        Debug.Log("Game Over! Kẹt xe rồi!");
        
        if (panelGameOver != null) 
            panelGameOver.SetActive(true); // Bật bảng Lose
    }

    // Hàm này sẽ được gán vào nút "Replay" trên UI
    public void ReplayGame()
    {
        // Load lại chính Scene đang mở hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "BusJam/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Cài đặt Độ khó")]
    public int soSlotHangCho = 5; 
    
    [Header("Danh sách Xe Bus sẽ xuất hiện")]
    public LoaiMau[] danhSachXeBus; 

    [Header("Bản đồ Nhân vật (R=Đỏ, B=Xanh, Y=Vàng, 0=Trống)")]
    [Tooltip("Mỗi phần tử là 1 hàng (Row). Ví dụ: 'R B 0'")]
    public string[] banDoLuoii; 
}
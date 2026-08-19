using UnityEngine;

public class TouchManager : MonoBehaviour
{
    [Header("Kéo các Slot_0, Slot_1, Slot_2 vào đây")]
    public Transform[] danhSachSlot; // Mảng chứa các vị trí đứng
    
    // Biến đếm xem đã có bao nhiêu người đi vào hàng chờ
    private int soNguoiTrongHang = 0;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                MoveToTarget nvBiCham = hit.collider.GetComponent<MoveToTarget>(); 
                
                if (nvBiCham != null)
                {
                    // 1. Kiểm tra xem hàng chờ còn chỗ trống không?
                    if (soNguoiTrongHang < danhSachSlot.Length)
                    {
                        // 2. Lấy tọa độ của vị trí trống tiếp theo
                        Vector3 diemDenTiepTheo = danhSachSlot[soNguoiTrongHang].position;
                        
                        // 3. Ra lệnh cho nhân vật chạy tới đó
                        nvBiCham.DiChuyenToi(diemDenTiepTheo);
                        
                        // 4. Tắt Collider của nhân vật này để không thể bị click lần thứ 2
                        nvBiCham.GetComponent<Collider>().enabled = false;

                        // 5. Tăng số đếm người trong hàng lên 1 cho lần click sau
                        soNguoiTrongHang++;
                    }
                    else
                    {
                        // Nếu hàng chờ đã đầy (soNguoiTrongHang >= số lượng Slot)
                        Debug.Log("Game Over! Hàng chờ đã kín chỗ!");
                    }
                }
            }
        }
    }
}
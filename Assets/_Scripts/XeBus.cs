using UnityEngine;

public class XeBus : MonoBehaviour
{
    [Header("Thông tin chuyến xe")]
    public LoaiMau mauCuaXe; 
    public int soGhe = 3;    
    
    private int soGheTrong; // Biến đếm số chỗ còn lại
    private bool dangKhoiHanh = false; // Trạng thái chạy của xe
    public float tocDoChay = 10f;

    private void Start()
    {
        // Khi game bắt đầu, số ghế trống bằng đúng sức chứa của xe
        soGheTrong = soGhe;
    }

    void Update()
    {
        // Nếu xe đã đầy khách, cho xe chạy thẳng về phía trước
        if (dangKhoiHanh)
        {
            // Vector3.forward tương đương với trục Z (mũi tên màu xanh dương trên Unity)
            transform.Translate(Vector3.forward * tocDoChay * Time.deltaTime);
        }
    }

    // Hàm này được gọi khi có 1 khách bước lên xe
    public bool ThemKhach()
    {
        soGheTrong--; // Trừ đi 1 ghế trống
        Debug.Log($"Xe {mauCuaXe} vừa đón 1 khách. Còn lại {soGheTrong} chỗ.");

        // Kiểm tra xem xe đã đầy chưa
        if (soGheTrong <= 0)
        {
            Debug.Log($"Xe {mauCuaXe} ĐÃ ĐẦY! Khởi hành thôi!");
            dangKhoiHanh = true; // Cho phép nổ máy chạy
            
            // Hủy object xe sau 3 giây để giải phóng bộ nhớ (tránh xe chạy mãi ra ngoài vũ trụ)
            Destroy(gameObject, 3f); 
            
            return true; // Trả về true báo hiệu xe đã đầy
        }
        
        return false; // Trả về false báo hiệu xe vẫn còn chỗ đón thêm
    }
}
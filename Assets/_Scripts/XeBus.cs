using UnityEngine;

public class XeBus : MonoBehaviour
{
    [Header("Thông tin chuyến xe")]
    public LoaiMau mauCuaXe; 
    public int soGhe = 3;    
    
    private int soGheTrong; 
    private bool dangKhoiHanh = false; 
    
    // Quản lý việc chạy vào bến
    private bool dangVaoBen = false;
    private Vector3 diemDungTrongBen;
    public float tocDoChay = 10f;
    public float tocDoXoay = 12f; // Tốc độ xoay mượt của xe

    [Header("Góc xoay xe (Y-Rotation)")]
    [Tooltip("Góc xoay khi chạy từ Trái sang Phải vào bến (-90 hoặc 270 nếu Model Prefab bị ngược)")]
    public float gocXoayKhiVaoBen = -90f;   

    [Tooltip("Góc xoay khi đỗ xong và chạy Thẳng Lên Trên (180 nếu Model Prefab bị ngược đầu)")]
    public float gocXoayKhiDoXong = 180f;    

    public bool DangDungTrongBen => !dangVaoBen && !dangKhoiHanh; // Xe đã đỗ xong trong bến và chưa khởi hành

    private void Awake()
    {
        soGheTrong = soGhe;
    }

    void Update()
    {
        // 1. Trạng thái chạy từ TRÁI sang PHẢI vào bến
        if (dangVaoBen)
        {
            // Di chuyển tới điểm đỗ
            transform.position = Vector3.MoveTowards(transform.position, diemDungTrongBen, tocDoChay * Time.deltaTime);

            // Quay mặt xe theo gocXoayKhiVaoBen
            Quaternion targetRotation = Quaternion.Euler(0, gocXoayKhiVaoBen, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tocDoXoay * Time.deltaTime);

            // Khi tới vị trí đỗ
            if (Vector3.Distance(transform.position, diemDungTrongBen) < 0.01f)
            {
                transform.position = diemDungTrongBen;
                // Xoay mặt xe hướng thẳng LÊN TRÊN
                transform.rotation = Quaternion.Euler(0, gocXoayKhiDoXong, 0);
                
                dangVaoBen = false; // Đã đỗ đúng vị trí
                if (BenXe.Instance != null)
                {
                    BenXe.Instance.QuetKhachDangCho();
                }
            }
        }
        // 2. Trạng thái khởi hành THẲNG HƯỚNG LÊN TRÊN
        else if (dangKhoiHanh)
        {
            // Giữ xoay đầu xe hướng lên trên
            Quaternion targetRotation = Quaternion.Euler(0, gocXoayKhiDoXong, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tocDoXoay * Time.deltaTime);
            
            // Chạy thẳng lên phía trên màn hình (+Z thế giới)
            transform.position += Vector3.forward * tocDoChay * Time.deltaTime;
        }
    }

    // Hàm nhận lệnh từ BenXe để chạy vào điểm đỗ
    public void TienVaoBen(Vector3 diemDung)
    {
        diemDungTrongBen = diemDung;
        dangVaoBen = true;
    }

    public bool ThemKhach()
    {
        soGheTrong--; 
        if (soGheTrong <= 0)
        {
            dangKhoiHanh = true; 
            Destroy(gameObject, 3f); 
            return true; 
        }
        return false; 
    }
}
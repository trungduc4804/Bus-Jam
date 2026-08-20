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

    public bool DangDungTrongBen => !dangVaoBen && !dangKhoiHanh; // Xe đã đỗ xong trong bến và chưa khởi hành

    private void Awake()
    {
        soGheTrong = soGhe;
    }

    void Update()
    {
        // 1. Trạng thái chạy vào bến
        if (dangVaoBen)
        {
            transform.position = Vector3.MoveTowards(transform.position, diemDungTrongBen, tocDoChay * Time.deltaTime);
            if (Vector3.Distance(transform.position, diemDungTrongBen) < 0.01f)
            {
                transform.position = diemDungTrongBen;
                dangVaoBen = false; // Đã đỗ đúng vị trí, dừng lại để đón khách
                if (BenXe.Instance != null)
                {
                    BenXe.Instance.QuetKhachDangCho();
                }
            }
        }
        // 2. Trạng thái khởi hành rời đi
        else if (dangKhoiHanh)
        {
            transform.Translate(Vector3.forward * tocDoChay * Time.deltaTime);
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
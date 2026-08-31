using UnityEngine;
using DG.Tweening;

public class XeBus : MonoBehaviour
{
    [Header("Thông tin chuyến xe")]
    public LoaiMau mauCuaXe; 
    public int soGhe = 3;    
    
    private int soGheTrong; 
    private int soKhachDangDiDen = 0; // Số lượng khách đang đi bộ từ slot tới xe
    private bool dangKhoiHanh = false; 
    
    // Quản lý việc chạy vào bến
    private bool dangVaoBen = false;
    private Vector3 diemDungTrongBen;
    public float tocDoChay = 12f;
    public float tocDoXoay = 12f; // Tốc độ xoay mượt của xe

    [Header("Góc xoay xe (Y-Rotation)")]
    [Tooltip("Góc xoay để đầu xe hướng sang BÊN PHẢI (-90 nếu Model bị ngược, hoặc 90 nếu Model chuẩn)")]
    public float gocXoayXe = -90f;   

    [Header("Game Feel / Juice Settings")]
    public bool coHieuUngNhunPhanh = true; // Bật/tắt hiệu ứng nhún phanh
    private float brakeBounceTimer = 0f;
    private Vector3 gocScaleBanDau;

    public bool DangDungTrongBen => !dangVaoBen && !dangKhoiHanh; // Xe đã đỗ xong trong bến và chưa khởi hành

    private void Awake()
    {
        soGheTrong = soGhe;
        gocScaleBanDau = transform.localScale;
    }

    // Kiểm tra xe còn chỗ trống cho khách mới bắt đầu đi bộ tới không
    public bool CoChoTrongChoKhach()
    {
        return (soGheTrong - soKhachDangDiDen) > 0;
    }

    // Đăng ký 1 chỗ cho khách đang bắt đầu đi bộ tới xe
    public void DangKyKhachDiDen()
    {
        soKhachDangDiDen++;
    }

    void Update()
    {
        // Luôn xoay mặt xe hướng sang bên PHẢI (Y = gocXoayXe)
        Quaternion targetRotation = Quaternion.Euler(0, gocXoayXe, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tocDoXoay * Time.deltaTime);

        // 1. Trạng thái chạy từ TRÁI vào bến đỗ
        if (dangVaoBen)
        {
            transform.position = Vector3.MoveTowards(transform.position, diemDungTrongBen, tocDoChay * Time.deltaTime);

            // Khi tới vị trí đỗ
            if (Vector3.Distance(transform.position, diemDungTrongBen) < 0.01f)
            {
                transform.position = diemDungTrongBen;
                dangVaoBen = false; // Đã đỗ đúng vị trí, dừng lại đón khách
                brakeBounceTimer = 0.2f; // Kích hoạt hiệu ứng nhún phanh dừng xe 0.2s
                
                if (BenXe.Instance != null)
                {
                    BenXe.Instance.QuetKhachDangCho();
                }
            }
        }
        // Hiệu ứng Nhún Phanh Giật Xe khi dừng hoặc có khách nhảy lên
        else if (brakeBounceTimer > 0)
        {
            brakeBounceTimer -= Time.deltaTime;
            if (coHieuUngNhunPhanh)
            {
                float bounce = Mathf.Sin(brakeBounceTimer * Mathf.PI * 10f) * 0.08f;
                transform.localScale = gocScaleBanDau + new Vector3(-bounce, bounce, -bounce);
            }
            if (brakeBounceTimer <= 0)
            {
                transform.localScale = gocScaleBanDau;
            }
        }
        // 2. Trạng thái khởi hành CHẠY TIẾP TỤC SANG BÊN PHẢI (phóng đi)
        else if (dangKhoiHanh)
        {
            // Hiệu ứng rồ ga nảy xe khi tăng tốc phóng đi
            if (coHieuUngNhunPhanh)
            {
                float stretch = Mathf.Sin(Time.time * 25f) * 0.04f;
                transform.localScale = gocScaleBanDau + new Vector3(stretch, -stretch, stretch);
            }

            // Chạy tiếp tục sang phải
            transform.position += Vector3.right * tocDoChay * Time.deltaTime;
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
        if (soKhachDangDiDen > 0) soKhachDangDiDen--;
        soGheTrong--; 
        
        // Nhún nhẹ xe 0.15s tạo cảm giác thỏa mãn mỗi khi có 1 khách nhảy lên xe
        brakeBounceTimer = 0.15f;

        if (soGheTrong <= 0)
        {
            dangKhoiHanh = true; 
            Destroy(gameObject, 3f); 
            return true; 
        }
        return false; 
    }
}
using System.Collections.Generic;
using UnityEngine;

public class BenXe : MonoBehaviour
{
    public static BenXe Instance { get; private set; }

    [Header("Quản lý bến xe")]
    public Transform viTriDoXe;
    public List<XeBus> danhSachXeChuanBi = new List<XeBus>();
    
    // Danh sách lưu những khách ra hàng đợi nhưng sai màu xe
    public List<NhanVat> danhSachKhachDangCho = new List<NhanVat>();

    public XeBus xeBusHienTai; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Không gọi GoiXeTiepTheo trong Start() nữa vì LevelManager sẽ tự gọi sau khi load map xong
    public void GoiXeTiepTheo(NhanVat khachDangXoa = null)
    {
        if (LevelManager.Instance != null && LevelManager.Instance.ConXeBusTrongQueue())
        {
            xeBusHienTai = LevelManager.Instance.SinhXeBusTiepTheo();
            if (xeBusHienTai != null && viTriDoXe != null)
            {
                xeBusHienTai.TienVaoBen(viTriDoXe.position); 
            }
        }
        else
        {
            xeBusHienTai = null;
            KiemTraKetThucGame(khachDangXoa);
        }
    }

    // Tách riêng logic cho lên xe ra một hàm để tái sử dụng
    private void ChoPhepLenXe(NhanVat khachHang)
    {
        // Xóa khách khỏi danh sách chờ nếu họ đang ở trong đó
        if (danhSachKhachDangCho.Contains(khachHang))
        {
            danhSachKhachDangCho.Remove(khachHang);
        }

        // GIẢI PHÓNG SLOT TRONG TOUCHMANAGER KHI KHÁCH LÊN XE
        if (TouchManager.Instance != null && khachHang.slotIndexHienTai != -1)
        {
            TouchManager.Instance.GiaiPhongSlot(khachHang.slotIndexHienTai);
        }

        Destroy(khachHang.gameObject); 

        if (xeBusHienTai != null)
        {
            bool xeDaDay = xeBusHienTai.ThemKhach();
            if (xeDaDay)
            {
                xeBusHienTai = null; 
                GoiXeTiepTheo(khachHang); 
            }
            else
            {
                KiemTraKetThucGame(khachHang);
            }
        }
        else
        {
            KiemTraKetThucGame(khachHang);
        }
    }

    public void KiemTraKetThucGame(NhanVat khachDangXoa = null)
    {
        NhanVat[] tatCaKhach = Object.FindObjectsOfType<NhanVat>();
        int soKhachConLai = 0;

        foreach (NhanVat nv in tatCaKhach)
        {
            if (nv != null && nv != khachDangXoa)
            {
                soKhachConLai++;
            }
        }

        if (soKhachConLai == 0)
        {
            // Tất cả khách đã lên xe -> Thắng màn chơi!
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinGame();
            }
        }
        else if (xeBusHienTai == null && (LevelManager.Instance == null || !LevelManager.Instance.ConXeBusTrongQueue()))
        {
            // Hết xe bus nhưng vẫn còn khách trên sân/hàng chờ -> Thua Game!
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoseGame();
            }
        }
    }

    public bool KtraVaLenXe(NhanVat khachHang)
    {
        // Chỉ cho phép lên xe nếu xe hiện tại ĐÃ ĐỖ XONG TRONG BẾN (DangDungTrongBen)
        if (xeBusHienTai != null && xeBusHienTai.DangDungTrongBen && khachHang.mauNV == xeBusHienTai.mauCuaXe)
        {
            ChoPhepLenXe(khachHang);
            return true;
        }
        else
        {
            // Sai màu hoặc xe chưa tới đỗ xong -> Thêm vào danh sách chờ
            if (!danhSachKhachDangCho.Contains(khachHang))
            {
                danhSachKhachDangCho.Add(khachHang);
            }
            return false;
        }
    }

    // Hàm này được gọi từ XeBus.cs khi xe mới vừa đỗ xong
    public void QuetKhachDangCho()
    {
        // Phải lặp ngược danh sách (từ cuối lên đầu) khi có thao tác Xóa (Remove) phần tử
        for (int i = danhSachKhachDangCho.Count - 1; i >= 0; i--)
        {
            // Nếu xe hiện tại đã hết hoặc đang di chuyển thì dừng quét
            if (xeBusHienTai == null || !xeBusHienTai.DangDungTrongBen) break;

            NhanVat khach = danhSachKhachDangCho[i];
            
            // Nếu phát hiện khách hợp màu với xe mới đến
            if (khach != null && khach.mauNV == xeBusHienTai.mauCuaXe)
            {
                ChoPhepLenXe(khach);
            }
        }
    }
}
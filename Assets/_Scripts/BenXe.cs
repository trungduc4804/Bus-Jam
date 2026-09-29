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
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }
    }

    // Danh sách theo dõi toàn bộ nhân vật đang còn sống trong game
    [HideInInspector]
    public List<NhanVat> danhSachTatCaKhach = new List<NhanVat>();

    public void DangKyNhanVat(NhanVat nv)
    {
        if (nv != null && !danhSachTatCaKhach.Contains(nv))
        {
            danhSachTatCaKhach.Add(nv);
        }
    }

    public void HuyDangKyNhanVat(NhanVat nv)
    {
        if (nv != null && danhSachTatCaKhach.Contains(nv))
        {
            danhSachTatCaKhach.Remove(nv);
        }
    }

    // Không gọi GoiXeTiepTheo trong Start() nữa vì LevelManager sẽ tự gọi sau khi load map xong
    public void GoiXeTiepTheo()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.ConXeBusTrongQueue())
        {
            xeBusHienTai = LevelManager.Instance.SinhXeBusTiepTheo();
            if (xeBusHienTai != null)
            {
                Vector3 diemDung = (viTriDoXe != null) ? viTriDoXe.position : new Vector3(0, 0, 5);
                xeBusHienTai.TienVaoBen(diemDung); 
            }
        }
        else
        {
            xeBusHienTai = null;
            KiemTraKetThucGame();
        }
    }

    // Cho phép khách chui vào xe sau khi đã đi bộ tới sát vị trí xe bus
    public void ChoPhepLenXe(NhanVat khachHang)
    {
        if (danhSachKhachDangCho.Contains(khachHang))
        {
            danhSachKhachDangCho.Remove(khachHang);
        }

        HuyDangKyNhanVat(khachHang);

        if (TouchManager.Instance != null && khachHang.slotIndexHienTai != -1)
        {
            TouchManager.Instance.GiaiPhongSlot(khachHang.slotIndexHienTai);
        }

        // Hiệu ứng thu nhỏ mượt khi chui vào xe rồi mới Destroy
        StartCoroutine(HieuUngThuNhoVaXoa(khachHang.gameObject));

        // Phát âm thanh Ting khi khách lên xe thành công
        if (AudioManager.Instance != null) AudioManager.Instance.PlayKhachLenXe();

        if (xeBusHienTai != null)
        {
            bool xeDaDay = xeBusHienTai.ThemKhach();
            if (xeDaDay)
            {
                xeBusHienTai = null; 
                GoiXeTiepTheo(); 
            }
            else
            {
                KiemTraKetThucGame();
            }
        }
        else
        {
            KiemTraKetThucGame();
        }
    }

    public void KiemTraKetThucGame()
    {
        int soKhachConLai = danhSachTatCaKhach.Count;

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

    private System.Collections.IEnumerator HieuUngThuNhoVaXoa(GameObject obj)
    {
        if (obj == null) yield break;
        Vector3 startScale = obj.transform.localScale;
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            if (obj != null)
            {
                obj.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t / 0.15f);
            }
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    public bool KtraVaChoLenXe(NhanVat khachHang)
    {
        if (khachHang == null) return false;

        bool hopMau = (xeBusHienTai != null && (xeBusHienTai.mauCuaXe == LoaiMau.CauVong || khachHang.mauNV == xeBusHienTai.mauCuaXe));

        if (xeBusHienTai != null && xeBusHienTai.DangDungTrongBen 
            && hopMau 
            && xeBusHienTai.CoChoTrongChoKhach())
        {
            // Xóa ngay lập tức khỏi danh sách chờ để không bao giờ bị quét trùng lặp
            if (danhSachKhachDangCho.Contains(khachHang))
            {
                danhSachKhachDangCho.Remove(khachHang);
            }

            // Vô hiệu hóa lệnh Undo cho khách này vì đã lên xe thành công
            if (BusJam.Commands.UndoManager.Instance != null)
            {
                BusJam.Commands.UndoManager.Instance.InvalidatePassenger(khachHang);
            }

            // Đăng ký 1 chỗ trên xe cho khách đang di chuyển tới
            xeBusHienTai.DangKyKhachDiDen();

            // Giải phóng slot hàng chờ để người khác có thể đi vào
            if (TouchManager.Instance != null && khachHang.slotIndexHienTai != -1)
            {
                TouchManager.Instance.GiaiPhongSlot(khachHang.slotIndexHienTai);
                khachHang.slotIndexHienTai = -1;
            }

            // Cho khách đi bộ tới xe bus
            Vector3 viTriXe = (viTriDoXe != null) ? viTriDoXe.position : xeBusHienTai.transform.position;
            khachHang.DiChuyenToiXe(viTriXe);
            return true;
        }
        else
        {
            if (!danhSachKhachDangCho.Contains(khachHang))
            {
                danhSachKhachDangCho.Add(khachHang);
            }
            return false;
        }
    }

    // Alias để giữ tương thích ngược với code cũ
    public bool KtraVaLenXe(NhanVat khachHang)
    {
        return KtraVaChoLenXe(khachHang);
    }

    // Hàm này được gọi từ XeBus.cs khi xe mới vừa đỗ xong
    public void QuetKhachDangCho()
    {
        // Duyệt theo thứ tự FIFO (người vào slot trước sẽ được lên xe trước)
        for (int i = 0; i < danhSachKhachDangCho.Count; )
        {
            // Nếu xe hiện tại đã hết hoặc đang di chuyển thì dừng quét
            if (xeBusHienTai == null || !xeBusHienTai.DangDungTrongBen || !xeBusHienTai.CoChoTrongChoKhach()) break;

            NhanVat khach = danhSachKhachDangCho[i];
            
            // Nếu phát hiện khách hợp màu với xe mới đến (hoặc là Xe Cầu Vồng)
            bool hopMau = (xeBusHienTai.mauCuaXe == LoaiMau.CauVong || (khach != null && khach.mauNV == xeBusHienTai.mauCuaXe));
            if (khach != null && hopMau)
            {
                // KtraVaChoLenXe tự động xóa khach khỏi danh sách, nên không tăng i
                KtraVaChoLenXe(khach);
            }
            else
            {
                i++;
            }
        }

        // Sau khi quét xong toàn bộ khách trong hàng chờ:
        // Nếu hàng chờ kín chỗ và chiếc xe này không đón được ai -> Kích hoạt kiểm tra Game Over!
        if (TouchManager.Instance != null && TouchManager.Instance.TimSlotTrongDauTien() == -1)
        {
            TouchManager.Instance.KiemTraGameOverSauKhiCho();
        }
    }
}
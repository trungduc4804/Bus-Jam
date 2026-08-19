using System.Collections.Generic; // Thư viện để dùng List
using UnityEngine;

public class BenXe : MonoBehaviour
{
    public static BenXe Instance { get; private set; }

    [Header("Quản lý bến xe")]
    public Transform viTriDoXe; // Chỗ đỗ xe cố định trong bến
    public List<XeBus> danhSachXeChuanBi = new List<XeBus>(); // Danh sách xe xếp hàng chờ
    
    // Không cần gán tay trên Inspector nữa, code sẽ tự lấy từ List
    private XeBus xeBusHienTai; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GoiXeTiepTheo(); // Vừa vào game là gọi ngay chiếc xe đầu tiên vào bến
    }

    // Hàm gọi chiếc xe đầu hàng tiến vào
    public void GoiXeTiepTheo()
    {
        if (danhSachXeChuanBi.Count > 0)
        {
            xeBusHienTai = danhSachXeChuanBi[0]; // Lấy chiếc xe đầu tiên (Index 0)
            danhSachXeChuanBi.RemoveAt(0); // Xóa xe đó khỏi danh sách chờ
            xeBusHienTai.TienVaoBen(viTriDoXe.position); // Gọi nó tiến vào vị trí đỗ
        }
        else
        {
            Debug.Log("Tuyệt vời! Đã hết xe, tất cả khách đều đã lên đường!");
        }
    }

    public bool KtraVaLenXe(NhanVat khachHang)
    {
        if (xeBusHienTai != null && khachHang.mauNV == xeBusHienTai.mauCuaXe)
        {
            Destroy(khachHang.gameObject); 
            bool xeDaDay = xeBusHienTai.ThemKhach();

            if (xeDaDay)
            {
                xeBusHienTai = null; 
                GoiXeTiepTheo(); // Xe trước vừa đi thì gọi ngay xe sau vào
            }
            return true;
        }
        return false;
    }
}
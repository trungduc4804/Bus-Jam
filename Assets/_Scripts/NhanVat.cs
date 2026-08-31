using UnityEngine;

public enum LoaiMau {
    Do,
    Xanh,
    Vang, 
    Tim
}

public class NhanVat : MonoBehaviour
{
    public LoaiMau mauNV;
    public float tocDo = 5f; // Tốc độ chạy của nhân vật

    [Header("Cấu hình kiểm tra đường bị chặn")]
    public float khoangCachKiemTra = 1.5f; // Khoảng cách quét nhân vật phía trước
    public float banKinhKiemTra = 0.4f;   // Bán kính quét (SphereCast) để tránh lọt khe

    [HideInInspector]
    public int slotIndexHienTai = -1;
    public Animator animator;
    private Vector3 diemDen;
    private bool dangDiChuyen = false;

    public bool DangDiChuyen => dangDiChuyen;
    private void Start()
    {
        if (BenXe.Instance != null)
        {
            BenXe.Instance.DangKyNhanVat(this);
        }
    }

    private void OnDestroy()
    {
        if (BenXe.Instance != null)
        {
            BenXe.Instance.HuyDangKyNhanVat(this);
        }
    }

    private Vector3 diemDenXe;
    private bool dangDiChuyenToiXe = false;

    public void DiChuyenToiXe(Vector3 viTriXe)
    {
        diemDenXe = viTriXe;
        dangDiChuyenToiXe = true;
    }

    private void Update()
    {
        // 1. Di chuyển từ vị trí đố tới Slot hàng chờ
        if (dangDiChuyen)
        {
            if (animator != null) animator.SetBool("isWalking", true);

            // Quay mặt nhân vật về hướng đang di chuyển
            Vector3 huongDi = (diemDen - transform.position).normalized;
            if (huongDi != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(huongDi);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);
            }

            transform.position = Vector3.MoveTowards(transform.position, diemDen, tocDo * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, diemDen) < 0.01f)
            {
                transform.position = diemDen;
                dangDiChuyen = false;
                if (animator != null) animator.SetBool("isWalking", false);
                // Thử lên xe bus khi vừa tới slot
                ThuLenXeBus();
            }
        }
        // 2. Di chuyển từ Slot tới sát vị trí Xe Bus để chui vào xe
        else if (dangDiChuyenToiXe)
        {
            if (animator != null) animator.SetBool("isWalking", true);

            Vector3 huongDi = (diemDenXe - transform.position).normalized;
            if (huongDi != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(huongDi);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);
            }

            transform.position = Vector3.MoveTowards(transform.position, diemDenXe, tocDo * Time.deltaTime);
            
            // Khi đi tới khoảng cách sát xe (< 0.4f)
            if (Vector3.Distance(transform.position, diemDenXe) < 0.4f)
            {
                dangDiChuyenToiXe = false;
                if (animator != null) animator.SetBool("isWalking", false);
                
                // Chính thức chui vào xe bus
                if (BenXe.Instance != null)
                {
                    BenXe.Instance.ChoPhepLenXe(this);
                }
            }
        }
    }

    public void ThuLenXeBus()
    {
        if (BenXe.Instance != null)
        {
            bool batDauLenXe = BenXe.Instance.KtraVaChoLenXe(this);
            if (batDauLenXe)
            {
                // Sau khi 1 người rời slot đi tới xe, thông báo cho người khác trong hàng chờ biết
                if (TouchManager.Instance != null)
                {
                    TouchManager.Instance.KiemTraNguoiTrongHangChoLenXe();
                }
            }
        }
    }

    // Hàm này dùng để nhận lệnh di chuyển từ bên ngoài (từ TouchManager)
    public void DiChuyenToi(Vector3 viTriMoi)
    {
        diemDen = viTriMoi; // Ghi nhớ tọa độ đích
        dangDiChuyen = true; // Bắt đầu cho phép di chuyển trong hàm Update
    }

    // Hàm kiểm tra xem đường đi có bị chặn bởi nhân vật khác không
    public bool KiemTraDuongThoat()
    {
        // Nhấc vị trí gốc bắn tia lên cao 0.5f để không bị đụng mặt đất
        Vector3 viTriBan = transform.position + (Vector3.up * 0.5f); 
        Vector3 huongBan = transform.forward; // Hướng mặt trước của nhân vật

        // Sử dụng SphereCastAll để lấy TẤT CẢ các vật thể nằm trên luồng quét (tránh bị cản bởi mặt đất hay object khác)
        RaycastHit[] hits = Physics.SphereCastAll(viTriBan, banKinhKiemTra, huongBan, khoangCachKiemTra);

        foreach (RaycastHit hit in hits)
        {
            // Bỏ qua chính bản thân nhân vật này
            if (hit.collider.gameObject == gameObject) continue;

            // Nếu phát hiện 1 nhân vật khác nằm trên hướng đi
            NhanVat nvKhac = hit.collider.GetComponent<NhanVat>();
            if (nvKhac != null)
            {
                Debug.Log($"Nhân vật {gameObject.name} ({mauNV}) bị chặn bởi {nvKhac.gameObject.name}!");
                return false; // Bị chặn, không được đi
            }
        }
        
        return true; // Đường thoáng, được phép đi
    }

    // Vẽ tia kiểm tra trực quan trong Unity Editor Scene view khi chọn nhân vật
    private void OnDrawGizmosSelected()
    {
        Vector3 viTriBan = transform.position + (Vector3.up * 0.5f);
        Gizmos.color = KiemTraDuongThoat() ? Color.green : Color.red;
        Gizmos.DrawRay(viTriBan, transform.forward * khoangCachKiemTra);
        Gizmos.DrawWireSphere(viTriBan + transform.forward * khoangCachKiemTra, banKinhKiemTra);
    }
}

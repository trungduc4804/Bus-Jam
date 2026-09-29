using UnityEngine;
using DG.Tweening;

public enum LoaiMau {
    Do,
    Xanh,
    Vang, 
    Tim,
    CauVong // Xe Cầu Vồng (VIP / Rainbow Bus) - chở bất kỳ màu nào
}

public class NhanVat : MonoBehaviour
{
    public LoaiMau mauNV;
    public float tocDo = 5f; // Tốc độ chạy của nhân vật

    [Header("Cơ chế Khách Ẩn (Mystery Passenger)")]
    public bool laKhachAn = false;
    public LoaiMau mauThatSu;
    private GameObject iconChamHoi;
    private Color mauGocRenderer;
    private Renderer[] allRenderers;

    [Header("Cấu hình kiểm tra đường bị chặn")]
    public float khoangCachKiemTra = 1.5f; // Khoảng cách quét nhân vật phía trước
    public float banKinhKiemTra = 0.4f;   // Bán kính quét (SphereCast) để tránh lọt khe

    [HideInInspector]
    public int slotIndexHienTai = -1;
    public Animator animator;
    private Vector3 diemDen;
    private bool dangDiChuyen = false;

    public bool DangDiChuyen => dangDiChuyen;
    public bool DangDiChuyenToiXe => dangDiChuyenToiXe;

    private void Start()
    {
        allRenderers = GetComponentsInChildren<Renderer>();
        if (allRenderers != null && allRenderers.Length > 0 && allRenderers[0] != null && allRenderers[0].material != null)
        {
            mauGocRenderer = allRenderers[0].material.color;
        }

        if (laKhachAn)
        {
            ApDungGiaoDienKhachAn();
        }

        if (BenXe.Instance != null)
        {
            BenXe.Instance.DangKyNhanVat(this);
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if (iconChamHoi != null) iconChamHoi.transform.DOKill();

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

    /// <summary>
    /// Nhảy quay về vị trí ban đầu trên sân khi người chơi bấm nút Undo
    /// </summary>
    public void QuayVeViTriCu(Vector3 viTriCu, Quaternion xoayCu, System.Action onComplete = null)
    {
        transform.DOKill();
        dangDiChuyen = false;
        dangDiChuyenToiXe = false;
        slotIndexHienTai = -1;

        if (animator != null) animator.SetBool("isWalking", true);

        // Hiệu ứng nhảy vồng ngược lại vị trí cũ
        transform.DOJump(viTriCu, 0.8f, 1, 0.4f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            transform.position = viTriCu;
            transform.rotation = xoayCu;
            if (animator != null) animator.SetBool("isWalking", false);

            // Bật lại Collider để người chơi có thể click chọn lại
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = true;

            onComplete?.Invoke();
        });

        // Xoay mặt trở lại hướng ban đầu
        transform.DORotateQuaternion(xoayCu, 0.35f);
    }

    /// <summary>
    /// Cấu hình nhân vật này là Khách Ẩn với màu thật sự sẽ hé lộ khi đường đi được giải phóng
    /// </summary>
    public void CaiDatKhachAn(LoaiMau mauThat)
    {
        laKhachAn = true;
        mauThatSu = mauThat;
        ApDungGiaoDienKhachAn();
    }

    private void ApDungGiaoDienKhachAn()
    {
        if (allRenderers == null) allRenderers = GetComponentsInChildren<Renderer>();
        if (allRenderers != null)
        {
            foreach (var r in allRenderers)
            {
                if (r != null && r.material != null)
                {
                    r.material.color = new Color(0.18f, 0.18f, 0.22f);
                }
            }
        }

        // Tạo icon "?" lơ lửng trên đầu
        if (iconChamHoi == null)
        {
            iconChamHoi = new GameObject("MysteryQuestionMark");
            iconChamHoi.transform.SetParent(transform, false);
            iconChamHoi.transform.localPosition = new Vector3(0, 1.8f, 0);

            TextMesh tm = iconChamHoi.AddComponent<TextMesh>();
            tm.text = "?";
            tm.fontSize = 50;
            tm.characterSize = 0.12f;
            tm.color = new Color(1f, 0.85f, 0.2f);
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;

            // Hiệu ứng nhấp nhô lơ lửng
            iconChamHoi.transform.DOLocalMoveY(2.05f, 0.65f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
    }

    /// <summary>
    /// Hé lộ màu sắc thật sự của khách ẩn khi không còn ai chặn đường phía trước
    /// </summary>
    public void HeLoMauThat()
    {
        if (!laKhachAn) return;
        laKhachAn = false;
        mauNV = mauThatSu;

        // Xóa icon dấu hỏi
        if (iconChamHoi != null)
        {
            iconChamHoi.transform.DOKill();
            Destroy(iconChamHoi);
        }

        // Khôi phục màu sắc gốc của nhân vật
        if (allRenderers != null)
        {
            foreach (var r in allRenderers)
            {
                if (r != null && r.material != null)
                {
                    r.material.color = mauGocRenderer;
                }
            }
        }

        // Hiệu ứng Pop nảy người bất ngờ (Juice)
        transform.DOKill();
        transform.DOPunchScale(Vector3.one * 0.35f, 0.35f, 10, 1f);

        // Phát âm thanh Pop hé lộ
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTapKhach();
        }

        Debug.Log($"[Khách Ẩn] Đã hé lộ màu thật: {mauNV} ({gameObject.name})");
    }

    private void Update()
    {
        // 0. Nếu là Khách Ẩn và đường thoát phía trước đã thông thoáng -> Tự động hé lộ màu thật!
        if (laKhachAn && !dangDiChuyen && !dangDiChuyenToiXe)
        {
            if (KiemTraDuongThoat())
            {
                HeLoMauThat();
            }
        }

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

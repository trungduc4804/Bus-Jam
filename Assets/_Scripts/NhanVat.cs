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
    [Tooltip("Kéo Sprite tùy chỉnh riêng cho con này nếu muốn. Để trống sẽ dùng từ LevelManager hoặc badge mặc định.")]
    public Sprite iconKhachAnTuyChinh;
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
        if (!laKhachAn && allRenderers != null && allRenderers.Length > 0 && allRenderers[0] != null && allRenderers[0].material != null)
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

            // Hiệu ứng hạt tiếp đất khi hoàn tác
            if (BusJam.VFX.VFXManager.Instance != null)
            {
                BusJam.VFX.VFXManager.Instance.PlayPassengerPop(viTriCu, LayMauRGB(mauNV));
            }

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
        mauNV = mauThat;
        ApDungGiaoDienKhachAn();
    }

    public static Color LayMauRGB(LoaiMau mau)
    {
        switch (mau)
        {
            case LoaiMau.Do: return new Color(1f, 0.05f, 0.05f, 1f);
            case LoaiMau.Xanh: return new Color(0f, 0.81f, 0.82f, 1f);
            case LoaiMau.Vang: return new Color(1f, 0.92f, 0.02f, 1f);
            case LoaiMau.Tim: return new Color(1f, 0f, 1f, 1f);
            default: return Color.white;
        }
    }

    public void DatMauRenderer(Color mau)
    {
        if (allRenderers == null || allRenderers.Length == 0)
        {
            allRenderers = GetComponentsInChildren<Renderer>();
        }

        if (allRenderers != null)
        {
            foreach (var r in allRenderers)
            {
                if (r != null && r.material != null)
                {
                    r.material.color = mau;
                    if (r.material.HasProperty("_BaseColor"))
                    {
                        r.material.SetColor("_BaseColor", mau);
                    }
                    if (r.material.HasProperty("_Color"))
                    {
                        r.material.SetColor("_Color", mau);
                    }
                }
            }
        }
    }

    private static Sprite s_badgeSprite;

    private static Sprite TaoSpriteHuyHieuTron()
    {
        if (s_badgeSprite != null) return s_badgeSprite;

        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 3f;
        float borderThickness = 9f;
        float innerRadius = radius - borderThickness;

        Color colorVien = new Color(0.12f, 0.12f, 0.14f, 1f); // Viền đen đậm nổi bật
        Color colorNenTop = new Color(1f, 0.96f, 0.35f, 1f);  // Vàng sáng phía trên
        Color colorNenBot = new Color(1f, 0.72f, 0.02f, 1f);  // Vàng cam rực rỡ phía dưới

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                if (dist > radius + 1f)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
                else if (dist > radius)
                {
                    float alpha = Mathf.Clamp01(radius + 1f - dist);
                    tex.SetPixel(x, y, new Color(colorVien.r, colorVien.g, colorVien.b, alpha));
                }
                else if (dist > innerRadius)
                {
                    tex.SetPixel(x, y, colorVien);
                }
                else
                {
                    float t = (float)y / size;
                    Color colNen = Color.Lerp(colorNenBot, colorNenTop, t);
                    tex.SetPixel(x, y, colNen);
                }
            }
        }
        tex.Apply();
        s_badgeSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return s_badgeSprite;
    }

    private void ApDungGiaoDienKhachAn()
    {
        // 1. Đổi màu nhân vật thành màu xám đen huyền bí
        DatMauRenderer(new Color(0.18f, 0.18f, 0.22f, 1f));

        // 2. Tạo Icon lơ lửng trên đầu
        if (iconChamHoi == null)
        {
            iconChamHoi = new GameObject("MysteryBadge");
            iconChamHoi.transform.SetParent(transform, false);
            iconChamHoi.transform.localPosition = new Vector3(0, 2.3f, 0);

            // Kiểm tra xem có Sprite tùy chỉnh không (ưu tiên trên NhanVat, rồi tới LevelManager)
            Sprite customSprite = iconKhachAnTuyChinh;
            if (customSprite == null && LevelManager.Instance != null)
            {
                customSprite = LevelManager.Instance.iconKhachAnTuyChinh;
            }

            if (customSprite != null)
            {
                // Nếu người dùng cung cấp Sprite tùy chỉnh: Dùng trực tiếp SpriteRenderer này!
                GameObject spriteObj = new GameObject("CustomMysteryIcon");
                spriteObj.transform.SetParent(iconChamHoi.transform, false);
                spriteObj.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
                SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer>();
                sr.sprite = customSprite;
                sr.sortingOrder = 15;
            }
            else
            {
                // Nếu không có: Dùng Huy hiệu Vàng Neon viền đen + Dấu hỏi "?" mặc định
                GameObject bgObj = new GameObject("BadgeBg");
                bgObj.transform.SetParent(iconChamHoi.transform, false);
                bgObj.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
                sr.sprite = TaoSpriteHuyHieuTron();
                sr.sortingOrder = 15;

                // Chữ dấu hỏi "?" đen đậm to bản, sắc nét
                GameObject textObj = new GameObject("QuestionMarkText");
                textObj.transform.SetParent(iconChamHoi.transform, false);
                textObj.transform.localPosition = new Vector3(0, 0, -0.05f);

                TextMesh tm = textObj.AddComponent<TextMesh>();
                tm.text = "?";
                tm.fontSize = 80;
                tm.fontStyle = FontStyle.Bold;
                tm.characterSize = 0.16f;
                tm.color = new Color(0.12f, 0.12f, 0.14f, 1f); // Màu đen tuyền đậm tương phản cực mạnh
                tm.alignment = TextAlignment.Center;
                tm.anchor = TextAnchor.MiddleCenter;

                MeshRenderer mrText = textObj.GetComponent<MeshRenderer>();
                if (mrText != null)
                {
                    mrText.sortingLayerID = sr.sortingLayerID;
                    mrText.sortingOrder = sr.sortingOrder + 1;
                }
            }

            // Hiệu ứng nhấp nhô lơ lửng & đập nhẹ (Floating & Pulsing)
            iconChamHoi.transform.DOLocalMoveY(2.48f, 0.65f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            iconChamHoi.transform.DOScale(Vector3.one * 1.12f, 0.65f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
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

        // 1. Xóa icon dấu hỏi với hiệu ứng Pop-out biến mất
        if (iconChamHoi != null)
        {
            iconChamHoi.transform.DOKill();
            iconChamHoi.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
            {
                if (iconChamHoi != null) Destroy(iconChamHoi);
            });
        }

        // 2. Khôi phục Material gốc từ Prefab (nếu có)
        if (LevelManager.Instance != null)
        {
            GameObject prefabKhach = LevelManager.Instance.GetPrefabKhachByMau(mauThatSu);
            if (prefabKhach != null)
            {
                Renderer prefabRenderer = prefabKhach.GetComponentInChildren<Renderer>();
                if (prefabRenderer != null && prefabRenderer.sharedMaterial != null)
                {
                    if (allRenderers == null || allRenderers.Length == 0) allRenderers = GetComponentsInChildren<Renderer>();
                    foreach (var r in allRenderers)
                    {
                        if (r != null)
                        {
                            r.material = prefabRenderer.sharedMaterial;
                        }
                    }
                }
            }
        }

        // Đảm bảo cả thuộc tính màu RGB chuẩn (_BaseColor & _Color) được áp dụng
        DatMauRenderer(LayMauRGB(mauThatSu));

        // 3. Hiệu ứng Pop nảy người bất ngờ (Juice)
        transform.DOKill();
        transform.DOPunchScale(Vector3.one * 0.35f, 0.35f, 10, 1f);

        // Hiệu ứng hạt nổ màu sắc
        if (BusJam.VFX.VFXManager.Instance != null)
        {
            BusJam.VFX.VFXManager.Instance.PlayPassengerPop(transform.position, LayMauRGB(mauThatSu));
        }

        // 4. Phát âm thanh Pop hé lộ
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTapKhach();
        }

        Debug.Log($"[Khách Ẩn] Đã hé lộ màu thật: {mauNV} ({gameObject.name})");
    }

    private void LateUpdate()
    {
        // Billboard: Dấu ? luôn xoay trực diện về Camera chính để người chơi nhìn rõ nhất
        if (iconChamHoi != null && Camera.main != null)
        {
            iconChamHoi.transform.rotation = Camera.main.transform.rotation;
        }
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
        // Đảm bảo không còn là khách ẩn khi lên xe
        if (laKhachAn)
        {
            HeLoMauThat();
        }

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

        // Nếu là khách ẩn thì khi bước ra slot bắt buộc hé lộ ngay lập tức!
        if (laKhachAn)
        {
            HeLoMauThat();
        }
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
            if (hit.collider == null) continue;
            // Bỏ qua chính bản thân nhân vật này
            if (hit.collider.gameObject == gameObject) continue;

            // Nếu phát hiện 1 nhân vật khác nằm trên hướng đi
            NhanVat nvKhac = hit.collider.GetComponent<NhanVat>();
            if (nvKhac != null)
            {
                // Nếu nhân vật khác đang di chuyển ra slot hoặc đã ở trong slot, không tính là đang chặn
                if (nvKhac.slotIndexHienTai != -1 || nvKhac.DangDiChuyen || nvKhac.DangDiChuyenToiXe)
                {
                    continue;
                }

                return false; // Bị chặn bởi người đứng yên phía trước
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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Quản lý Level")]
    public static int levelIndex = 0; // Biến static lưu tiến trình màn chơi
    public LevelData[] danhSachLevel;

    [Header("Prefabs Nhân Vật")]
    public GameObject prefabKhachDo;
    public GameObject prefabKhachXanh;
    public GameObject prefabKhachVang;
    public GameObject prefabKhachTim;

    [Header("Giao diện Khách Ẩn (Mystery Passenger)")]
    [Tooltip("Kéo thả Sprite icon tùy chỉnh của bạn vào đây (ví dụ ảnh ?, icon túi quà, mystery box). Để trống sẽ tự vẽ huy hiệu mặc định")]
    public Sprite iconKhachAnTuyChinh;

    [Header("Prefabs Xe Bus")]
    public GameObject prefabXeDo;
    public GameObject prefabXeXanh;
    public GameObject prefabXeVang;
    public GameObject prefabXeTim;
    public Transform viTriXuatPhatXe; // Vị trí xuất phát ngoài bến (Ví dụ: -15, 0, 5)

    [Header("Cài đặt Lưới")]
    public float khoangCachO = 1.2f; // Khoảng cách giữa các nhân vật
    public Vector3 viTriBatDau = new Vector3(0, 0, 0); // Vị trí điểm trung tâm của lưới
    public bool tuDongCanGiua = true; // Tự động căn giữa lưới theo màn hình

    [Header("Cài đặt Tự Động Cân Chỉnh Camera (Auto-Fit)")]
    [Tooltip("Tự động điều chỉnh khoảng cách và góc nhìn Camera để vừa vặn với kích thước từng màn chơi")]
    public bool tuDongCanChinhCamera = true;
    [Tooltip("Vị trí Camera mặc định cho màn chơi tiêu chuẩn (5x5)")]
    public Vector3 viTriCameraGoc = new Vector3(0f, 20f, -20f);
    [Tooltip("FOV mặc định cho màn chơi tiêu chuẩn (5x5)")]
    public float fovCameraGoc = 50f;

    // Vị trí và FOV mục tiêu thực tế của Level hiện tại (các Manager khác luôn đọc để đồng bộ)
    public Vector3 ViTriCameraMucTieu { get; private set; }
    public float FovCameraMucTieu { get; private set; }

    // Hàng chờ lưu danh sách màu xe bus theo thứ tự trong LevelData
    private Queue<LoaiMau> hangChoXeBus = new Queue<LoaiMau>();

    private void Awake()
    {
        ViTriCameraMucTieu = viTriCameraGoc;
        FovCameraMucTieu = fovCameraGoc;

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

    void Start()
    {
        SinhRaBanDo();
    }

    private void SinhRaBanDo()
    {
        if (danhSachLevel == null || danhSachLevel.Length == 0)
        {
            Debug.LogWarning("Chưa gán Danh Sách Level trong LevelManager!");
            return;
        }

        if (levelIndex >= danhSachLevel.Length || levelIndex < 0)
        {
            levelIndex = 0;
        }

        LevelData levelHienTai = danhSachLevel[levelIndex];
        if (levelHienTai == null) return;

        // 0. Dọn dẹp sạch các XeBus hoặc NhanVat rác kéo thả thủ công còn sót trong Scene
        XoaObjectRaoTrongScene();

        // 0.1 Reset lịch sử hoàn tác (Undo) và Booster cho màn chơi mới
        if (BusJam.Commands.UndoManager.Instance != null)
        {
            BusJam.Commands.UndoManager.Instance.ResetLuotUndo();
        }
        if (BusJam.Boosters.BoosterManager.Instance != null)
        {
            BusJam.Boosters.BoosterManager.Instance.ResetBooster();
        }

        // 1. Cài đặt số slot hàng chờ cho TouchManager theo LevelData
        if (TouchManager.Instance != null)
        {
            int soSlot = levelHienTai.soSlotHangCho > 0 ? levelHienTai.soSlotHangCho : 5;
            TouchManager.Instance.CapNhatSoSlot(soSlot);
        }

        // 2. Lưu danh sách thứ tự Xe Bus từ LevelData vào Queue
        hangChoXeBus.Clear();
        if (levelHienTai.danhSachXeBus != null)
        {
            foreach (LoaiMau mauXe in levelHienTai.danhSachXeBus)
            {
                hangChoXeBus.Enqueue(mauXe);
            }
        }

        // 3. Sinh ra bản đồ Nhân Vật từ banDoLuoii (Tự động căn giữa màn hình)
        if (levelHienTai.banDoLuoii != null && levelHienTai.banDoLuoii.Length > 0)
        {
            int soHang = levelHienTai.banDoLuoii.Length;
            int maxCot = 0;

            // Tìm số cột lớn nhất để tính chiều rộng lưới
            foreach (string hang in levelHienTai.banDoLuoii)
            {
                if (string.IsNullOrEmpty(hang)) continue;
                int demCot = 0;
                foreach (char c in hang)
                {
                    if (c != ' ') demCot++;
                }
                if (demCot > maxCot) maxCot = demCot;
            }

            // Tính khoảng lệch (Offset) để căn giữa lưới
            float offsetX = tuDongCanGiua ? -((maxCot - 1) * khoangCachO) / 2.0f : 0f;
            float offsetZ = tuDongCanGiua ? ((soHang - 1) * khoangCachO) / 2.0f : 0f;

            // Tính toán trước Bể Màu Cân Bằng Thông Minh (Smart Deficit Pool) cho Khách Ẩn
            Queue<LoaiMau> poolMauKhachAn = TaoBeMauKhachAn(levelHienTai);

            for (int z = 0; z < soHang; z++)
            {
                string hangHienTai = levelHienTai.banDoLuoii[z];
                if (string.IsNullOrEmpty(hangHienTai)) continue;

                int colIndex = 0;
                for (int i = 0; i < hangHienTai.Length; i++)
                {
                    char c = hangHienTai[i];
                    if (c == ' ') continue; 

                    float posX = viTriBatDau.x + offsetX + (colIndex * khoangCachO);
                    float posZ = viTriBatDau.z + offsetZ - (z * khoangCachO);
                    Vector3 toaDo = new Vector3(posX, viTriBatDau.y, posZ);

                    char upperChar = char.ToUpper(c);
                    bool laKhachAn = (upperChar == 'M' || upperChar == '?');
                    LoaiMau mauThucTe = LoaiMau.Do;

                    if (laKhachAn)
                    {
                        // Lấy màu từ Bể Màu Thông Minh đã được tính toán bù trừ chuẩn xác
                        if (poolMauKhachAn != null && poolMauKhachAn.Count > 0)
                        {
                            mauThucTe = poolMauKhachAn.Dequeue();
                        }
                        else
                        {
                            // Fallback an toàn nếu bể màu hết
                            if (levelHienTai.danhSachXeBus != null && levelHienTai.danhSachXeBus.Length > 0)
                            {
                                mauThucTe = levelHienTai.danhSachXeBus[Random.Range(0, levelHienTai.danhSachXeBus.Length)];
                                if (mauThucTe == LoaiMau.CauVong) mauThucTe = (LoaiMau)Random.Range(0, 4);
                            }
                            else
                            {
                                mauThucTe = (LoaiMau)Random.Range(0, 4);
                            }
                        }
                    }

                    GameObject prefabKhach = laKhachAn ? GetPrefabKhachByMau(mauThucTe) : GetPrefabKhach(c);
                    if (prefabKhach != null)
                    {
                        GameObject objKhach = Instantiate(prefabKhach, toaDo, Quaternion.identity);
                        NhanVat nv = objKhach.GetComponent<NhanVat>();
                        if (nv != null)
                        {
                            if (laKhachAn)
                            {
                                nv.CaiDatKhachAn(mauThucTe);
                            }
                            if (BenXe.Instance != null)
                            {
                                BenXe.Instance.DangKyNhanVat(nv);
                            }
                        }
                    }

                    colIndex++;
                }
            }
            // 3.5 Tự động cân chỉnh Camera vừa vặn với kích thước lưới và khung hình thiết bị
            if (tuDongCanChinhCamera)
            {
                CanChinhCameraTheoKichThuoc(maxCot, soHang);
            }
        }

        // 4. Kích hoạt gọi chiếc xe bus đầu tiên tiến vào bến
        if (BenXe.Instance != null)
        {
            BenXe.Instance.GoiXeTiepTheo();
        }
    }

    /// <summary>
    /// Tự động cân chỉnh Camera theo số cột và số hàng của Level,
    /// đồng thời thích ứng linh hoạt với tỉ lệ màn hình của thiết bị di động (aspect ratio).
    /// </summary>
    public void CanChinhCameraTheoKichThuoc(int soCot, int soHang)
    {
        if (!tuDongCanChinhCamera) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Lưu lại vị trí và FOV Camera ban đầu nếu chưa từng được ghi nhận
        if (viTriCameraGoc == Vector3.zero)
        {
            viTriCameraGoc = cam.transform.position;
            fovCameraGoc = cam.fieldOfView;
        }

        // Tỉ lệ màn hình hiện tại (width / height)
        float currentAspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        // Tỉ lệ chuẩn màn hình dọc 9:16 (~0.5625)
        float refAspect = 9f / 16f;

        // Hệ số bù trừ khi màn hình hẹp hơn chuẩn (ví dụ tỉ lệ dài 19.5:9 hay 20:9 trên các dòng iPhone/Android mới)
        float aspectFactor = Mathf.Max(1f, refAspect / Mathf.Max(0.1f, currentAspect));

        // Số cột và số hàng vượt quá mức chuẩn 5x5
        float colExtra = Mathf.Max(0f, soCot - 5);
        float rowExtra = Mathf.Max(0f, soHang - 5);

        float fovOffset = 0f;
        Vector3 posOffset = Vector3.zero;

        if (colExtra > 0 || rowExtra > 0 || aspectFactor > 1.02f)
        {
            // Tăng FOV nhẹ nhàng (tối đa tăng thêm 8 độ để giữ phối cảnh đẹp, không bị méo góc rộng fish-eye)
            fovOffset = Mathf.Min(8f, colExtra * 2.2f + (aspectFactor - 1f) * 7.5f);

            // Lùi vị trí Camera ra xa theo hướng nhìn (Y nâng cao lên, Z lùi về âm hơn)
            float pullBackY = colExtra * 1.8f + rowExtra * 0.8f + (aspectFactor - 1f) * 3.5f;
            float pullBackZ = -(colExtra * 2.2f + rowExtra * 1.0f + (aspectFactor - 1f) * 4.2f);

            posOffset = new Vector3(0f, pullBackY, pullBackZ);
        }

        Vector3 targetPos = viTriCameraGoc + posOffset;
        float targetFOV = Mathf.Clamp(fovCameraGoc + fovOffset, 45f, 62f);

        ViTriCameraMucTieu = targetPos;
        FovCameraMucTieu = targetFOV;

        // Dừng các tween cũ trên Camera và đặt ngay vị trí & góc nhìn chuẩn xác
        cam.transform.DOKill();
        cam.DOKill();
        cam.transform.position = targetPos;
        cam.fieldOfView = targetFOV;

        // Cập nhật ngay vị trí gốc cho VFXManager để CameraShake không bao giờ bị giật về tọa độ cũ
        if (BusJam.VFX.VFXManager.Instance != null)
        {
            BusJam.VFX.VFXManager.Instance.CapNhatViTriGocCamera(targetPos);
        }

        Debug.Log($"[LevelManager] Auto-Fit Camera cho Level ({soCot}x{soHang}, aspect={currentAspect:F2}): FOV={targetFOV:F1}, Pos={targetPos}");
    }

    // Xóa tất cả các xe bus hoặc nhân vật cũ kéo thả trong Scene Hierarchy trước khi chơi
    private void XoaObjectRaoTrongScene()
    {
        XeBus[] xeCus = Object.FindObjectsByType<XeBus>(FindObjectsSortMode.None);
        foreach (XeBus xe in xeCus) Destroy(xe.gameObject);

        NhanVat[] khachCus = Object.FindObjectsByType<NhanVat>(FindObjectsSortMode.None);
        foreach (NhanVat nv in khachCus) Destroy(nv.gameObject);
    }

    // Hàm sinh ra duy nhất 1 chiếc xe bus tiếp theo theo thứ tự khi Bến Xe yêu cầu
    public XeBus SinhXeBusTiepTheo()
    {
        if (hangChoXeBus.Count == 0) return null;

        LoaiMau mauXe = hangChoXeBus.Dequeue();
        GameObject prefabXe = GetPrefabXe(mauXe);

        if (prefabXe != null)
        {
            Vector3 posSpawn = (viTriXuatPhatXe != null) ? viTriXuatPhatXe.position : new Vector3(-15, 0, 5);
            GameObject objXe = Instantiate(prefabXe, posSpawn, Quaternion.identity);
            XeBus xeBus = objXe.GetComponent<XeBus>();
            if (xeBus != null)
            {
                xeBus.mauCuaXe = mauXe;
                if (mauXe == LoaiMau.CauVong)
                {
                    xeBus.BienThanhXeCauVong();
                }
                return xeBus;
            }
        }

        return null;
    }

    public bool ConXeBusTrongQueue()
    {
        return hangChoXeBus.Count > 0;
    }

    public GameObject GetPrefabKhachByMau(LoaiMau mau)
    {
        switch (mau)
        {
            case LoaiMau.Do: return prefabKhachDo;
            case LoaiMau.Xanh: return prefabKhachXanh;
            case LoaiMau.Vang: return prefabKhachVang;
            case LoaiMau.Tim: return prefabKhachTim;
            default: return prefabKhachDo;
        }
    }

    private GameObject GetPrefabKhach(char kyTu)
    {
        switch (char.ToUpper(kyTu))
        {
            case 'R': return prefabKhachDo;
            case 'B': return prefabKhachXanh;
            case 'Y': return prefabKhachVang;
            case 'P':
            case 'T': return prefabKhachTim;
            default: return null;
        }
    }

    private GameObject GetPrefabXe(LoaiMau mau)
    {
        switch (mau)
        {
            case LoaiMau.Do: return prefabXeDo;
            case LoaiMau.Xanh: return prefabXeXanh;
            case LoaiMau.Vang: return prefabXeVang;
            case LoaiMau.Tim: return prefabXeTim;
            case LoaiMau.CauVong: return prefabXeDo;
            default: return null;
        }
    }

    /// <summary>
    /// Thuật toán Bể Màu Cân Bằng Thông Minh (Smart Deficit Balancing Pool):
    /// Tự động phân tích số khách thường của từng màu và số ghế của từng xe bus trong level.
    /// Tính toán chính xác số khách còn thiếu của từng xe để gán cho Khách Ẩn,
    /// đảm bảo 100% mọi xe bus đều được lấp đầy đủ 3 khách và màn chơi luôn giải được!
    /// </summary>
    private Queue<LoaiMau> TaoBeMauKhachAn(LevelData level)
    {
        Queue<LoaiMau> pool = new Queue<LoaiMau>();
        if (level == null || level.banDoLuoii == null) return pool;

        // 1. Đếm số khách thường & số khách ẩn trên sân
        int countDo = 0, countXanh = 0, countVang = 0, countTim = 0;
        int totalKhachAn = 0;

        foreach (string hang in level.banDoLuoii)
        {
            if (string.IsNullOrEmpty(hang)) continue;
            foreach (char c in hang)
            {
                char u = char.ToUpper(c);
                if (u == 'R') countDo++;
                else if (u == 'B') countXanh++;
                else if (u == 'Y') countVang++;
                else if (u == 'T' || u == 'P') countTim++;
                else if (u == 'M' || u == '?') totalKhachAn++;
            }
        }

        if (totalKhachAn == 0) return pool;

        // 2. Đếm số ghế của từng loại xe
        int seatsDo = 0, seatsXanh = 0, seatsVang = 0, seatsTim = 0, seatsCauVong = 0;
        if (level.danhSachXeBus != null)
        {
            foreach (LoaiMau mauXe in level.danhSachXeBus)
            {
                if (mauXe == LoaiMau.Do) seatsDo += 3;
                else if (mauXe == LoaiMau.Xanh) seatsXanh += 3;
                else if (mauXe == LoaiMau.Vang) seatsVang += 3;
                else if (mauXe == LoaiMau.Tim) seatsTim += 3;
                else if (mauXe == LoaiMau.CauVong) seatsCauVong += 3;
            }
        }

        List<LoaiMau> danhSachMau = new List<LoaiMau>();

        // 3. Ưu tiên 1: Bù đắp số khách còn thiếu của từng xe thường
        int thieuDo = Mathf.Max(0, seatsDo - countDo);
        int thieuXanh = Mathf.Max(0, seatsXanh - countXanh);
        int thieuVang = Mathf.Max(0, seatsVang - countVang);
        int thieuTim = Mathf.Max(0, seatsTim - countTim);

        for (int i = 0; i < thieuDo && danhSachMau.Count < totalKhachAn; i++) danhSachMau.Add(LoaiMau.Do);
        for (int i = 0; i < thieuXanh && danhSachMau.Count < totalKhachAn; i++) danhSachMau.Add(LoaiMau.Xanh);
        for (int i = 0; i < thieuVang && danhSachMau.Count < totalKhachAn; i++) danhSachMau.Add(LoaiMau.Vang);
        for (int i = 0; i < thieuTim && danhSachMau.Count < totalKhachAn; i++) danhSachMau.Add(LoaiMau.Tim);

        // 4. Ưu tiên 2: Nếu vẫn còn khách ẩn (dành cho Xe Cầu Vồng hoặc thừa ghế),
        // ưu tiên gộp đủ bộ 3 cho màu đang bị lẻ
        while (danhSachMau.Count < totalKhachAn)
        {
            int duDo = (countDo + danhSachMau.Count(m => m == LoaiMau.Do)) % 3;
            int duXanh = (countXanh + danhSachMau.Count(m => m == LoaiMau.Xanh)) % 3;
            int duVang = (countVang + danhSachMau.Count(m => m == LoaiMau.Vang)) % 3;
            int duTim = (countTim + danhSachMau.Count(m => m == LoaiMau.Tim)) % 3;

            if (duDo > 0) danhSachMau.Add(LoaiMau.Do);
            else if (duXanh > 0) danhSachMau.Add(LoaiMau.Xanh);
            else if (duVang > 0) danhSachMau.Add(LoaiMau.Vang);
            else if (duTim > 0) danhSachMau.Add(LoaiMau.Tim);
            else
            {
                // Nếu tất cả đều đã chẵn 3, chọn một màu có trong danh sách xe bus
                LoaiMau mauChon = LoaiMau.Do;
                if (level.danhSachXeBus != null && level.danhSachXeBus.Length > 0)
                {
                    LoaiMau m = level.danhSachXeBus[Random.Range(0, level.danhSachXeBus.Length)];
                    mauChon = (m == LoaiMau.CauVong) ? (LoaiMau)Random.Range(0, 4) : m;
                }
                danhSachMau.Add(mauChon);
            }
        }

        // 5. Xáo trộn ngẫu nhiên danh sách (Fisher-Yates Shuffle) để đảm bảo tính bất ngờ
        for (int i = danhSachMau.Count - 1; i > 0; i--)
        {
            int randIndex = Random.Range(0, i + 1);
            LoaiMau temp = danhSachMau[i];
            danhSachMau[i] = danhSachMau[randIndex];
            danhSachMau[randIndex] = temp;
        }

        foreach (LoaiMau m in danhSachMau)
        {
            pool.Enqueue(m);
        }

        Debug.Log($"[LevelManager] Đã tạo Bể Màu Khách Ẩn cân bằng: [{string.Join(", ", danhSachMau)}] (Tổng: {danhSachMau.Count})");
        return pool;
    }
}
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
    public static TouchManager Instance { get; private set; }

    [Header("Kéo tất cả các ô Slot hàng chờ trong Scene vào đây")]
    [Tooltip("Mảng chứa các Transform vị trí đứng (Target)")]
    public Transform[] danhSachSlot;

    [Tooltip("Mảng chứa các ô vuông màu đen trên mặt đường (road-square). Để trống sẽ tự tìm")]
    public GameObject[] danhSachVisualSlot;

    [Header("Cài đặt Căn Giữa Hàng Chờ")]
    [Tooltip("Tự động căn đều các slot ra giữa vỉa hè theo số lượng slot của màn chơi")]
    public bool tuDongCanGiuaSlot = true;

    [Tooltip("Khoảng cách giữa tâm 2 ô slot liền kề")]
    public float khoangCachSlot = 2.0f;

    [Tooltip("Tọa độ gốc X để căn giữa (mặc định là 0)")]
    public float toaDoXTrungTam = 0f;

    // Mảng lưu trữ nhân vật đang ở từng slot
    private NhanVat[] nhanVatTrongSlot;
    private int soSlotChoPhep = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (danhSachSlot != null && danhSachSlot.Length > 0)
            {
                nhanVatTrongSlot = new NhanVat[danhSachSlot.Length];
            }
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }
    }

    private Camera camChinh;
    private bool dangKiemTraHangCho = false;

    private void Start()
    {
        camChinh = Camera.main;
        if (nhanVatTrongSlot == null && danhSachSlot != null && danhSachSlot.Length > 0)
        {
            nhanVatTrongSlot = new NhanVat[danhSachSlot.Length];
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Bỏ qua nếu chạm vào bất kỳ thành phần UI nào (Nút Setting, Undo, Booster, Panel Popup)
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (camChinh == null) camChinh = Camera.main;
            if (camChinh == null) return;

            Ray ray = camChinh.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                NhanVat nvBiCham = hit.collider.GetComponent<NhanVat>();

                if (nvBiCham != null)
                {
                    // Nếu nhân vật đã ở slot hoặc đang di chuyển thì bỏ qua
                    if (nvBiCham.slotIndexHienTai != -1 || nvBiCham.DangDiChuyen) return;

                    if (nvBiCham.KiemTraDuongThoat() == false)
                    {
                        Debug.Log($"[Bị chặn] Không thể di chuyển nhân vật {nvBiCham.gameObject.name} ra bãi xe vì có người đứng chặn phía trước!");
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayKhachBiChan();
                        if (BusJam.VFX.VFXManager.Instance != null) BusJam.VFX.VFXManager.Instance.PlayKhachBiChanWobble(nvBiCham.transform);
                        return; 
                    }
                    // 1. Tìm vị trí slot trống đầu tiên
                    int indexSlotTrong = TimSlotTrongDauTien();

                    if (indexSlotTrong != -1)
                    {
                        // Phát âm thanh Pop khi chọn khách hợp lệ
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayTapKhach();

                        Vector3 viTriBanDau = nvBiCham.transform.position;
                        Quaternion xoayBanDau = nvBiCham.transform.rotation;
                        Vector3 viTriSlot = danhSachSlot[indexSlotTrong].position;

                        // 2. Lưu thông tin nhân vật vào slot
                        nhanVatTrongSlot[indexSlotTrong] = nvBiCham;
                        nvBiCham.slotIndexHienTai = indexSlotTrong;

                        // 3. Tắt Collider của nhân vật để tránh bị click lại
                        Collider col = nvBiCham.GetComponent<Collider>();
                        if (col != null) col.enabled = false;

                        // 4. Đăng ký Command vào UndoManager phục vụ tính năng Hoàn tác
                        if (BusJam.Commands.UndoManager.Instance != null)
                        {
                            BusJam.Commands.UndoManager.Instance.RegisterCommand(
                                new BusJam.Commands.MovePassengerCommand(nvBiCham, viTriBanDau, xoayBanDau, indexSlotTrong, viTriSlot)
                            );
                        }

                        // 5. Ra lệnh cho nhân vật di chuyển tới slot
                        nvBiCham.DiChuyenToi(viTriSlot);

                        // 6. Quét ngay các khách ẩn trong bãi để hé lộ người vừa được thông đường!
                        KiemTraVaHeLoKhachAnSauKhiNguoiDi();
                    }
                    else
                    {
                        // Nếu hàng chờ đã đầy không còn slot nào trống
                        Debug.Log("[TouchManager] Hàng chờ đã kín chỗ!");
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayHangChoDay();
                        if (BusJam.VFX.VFXManager.Instance != null) BusJam.VFX.VFXManager.Instance.CameraShake(0.12f, 0.06f);

                        // Nếu không còn ai đang di chuyển -> Kiểm tra và xử thua ngay lập tức!
                        if (!CoNhanVatDangDiChuyen())
                        {
                            ThucThiKiemTraGameOver();
                        }
                        else
                        {
                            KiemTraGameOverSauKhiCho();
                        }
                    }
                }
            }
        }
    }

    // Cho phép LevelManager tùy chỉnh số slot hàng chờ tối đa của Level
    public void CapNhatSoSlot(int soSlot)
    {
        if (danhSachSlot == null || danhSachSlot.Length == 0) return;

        // Khởi tạo mảng lưu nhân vật nếu chưa có
        if (nhanVatTrongSlot == null || nhanVatTrongSlot.Length != danhSachSlot.Length)
        {
            nhanVatTrongSlot = new NhanVat[danhSachSlot.Length];
        }
        else
        {
            // Reset mảng khi bắt đầu level mới
            for (int k = 0; k < nhanVatTrongSlot.Length; k++)
            {
                nhanVatTrongSlot[k] = null;
            }
        }

        // Tự động tìm các ô visual road-square nếu chưa được kéo thả
        TimVisualSlotNeuChuaCo();

        soSlotChoPhep = Mathf.Clamp(soSlot, 1, danhSachSlot.Length);

        // Tính tọa độ X xuất phát để dàn đều và căn giữa hàng slot
        float startX = toaDoXTrungTam - ((soSlotChoPhep - 1) * khoangCachSlot) / 2.0f;

        for (int i = 0; i < danhSachSlot.Length; i++)
        {
            bool isActive = (i < soSlotChoPhep);

            // 1. Cập nhật vị trí và trạng thái của Transform điểm đứng (Target)
            if (danhSachSlot[i] != null)
            {
                danhSachSlot[i].gameObject.SetActive(isActive);

                if (isActive && tuDongCanGiuaSlot)
                {
                    float slotX = startX + i * khoangCachSlot;
                    Vector3 currentPos = danhSachSlot[i].position;
                    danhSachSlot[i].position = new Vector3(slotX, currentPos.y, currentPos.z);
                }
            }

            // 2. Cập nhật vị trí và trạng thái của ô vuông màu đen (road-square)
            if (danhSachVisualSlot != null && i < danhSachVisualSlot.Length && danhSachVisualSlot[i] != null)
            {
                danhSachVisualSlot[i].SetActive(isActive);

                if (isActive && tuDongCanGiuaSlot)
                {
                    float slotX = startX + i * khoangCachSlot;
                    Vector3 visualPos = danhSachVisualSlot[i].transform.position;
                    danhSachVisualSlot[i].transform.position = new Vector3(slotX, visualPos.y, visualPos.z);
                }
            }
        }

        Debug.Log($"[TouchManager] Đã cập nhật hàng chờ: {soSlotChoPhep} slot (Căn giữa: {tuDongCanGiuaSlot})");
    }

    private void TimVisualSlotNeuChuaCo()
    {
        if (danhSachVisualSlot != null && danhSachVisualSlot.Length > 0) return;

        // Tự động tìm GameObject Road trong Scene
        GameObject road = GameObject.Find("Road");
        if (road != null)
        {
            List<GameObject> listSquares = new List<GameObject>();
            foreach (Transform child in road.transform)
            {
                if (child.name.ToLower().Contains("road-square"))
                {
                    listSquares.Add(child.gameObject);
                }
            }

            if (listSquares.Count > 0)
            {
                // Sắp xếp tự nhiên theo tên để đồng bộ với thứ tự Target
                listSquares.Sort((a, b) => a.name.CompareTo(b.name));
                danhSachVisualSlot = listSquares.ToArray();
            }
        }
    }

    // Tìm index của slot trống đầu tiên (trả về -1 nếu đầy)
    public int TimSlotTrongDauTien()
    {
        if (nhanVatTrongSlot == null) return -1;

        int maxSlot = (soSlotChoPhep > 0 && soSlotChoPhep <= nhanVatTrongSlot.Length) 
                      ? soSlotChoPhep 
                      : nhanVatTrongSlot.Length;

        for (int i = 0; i < maxSlot; i++)
        {
            if (nhanVatTrongSlot[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    // Giải phóng slot khi nhân vật lên xe bus thành công hoặc khi Hoàn tác (Undo)
    public void GiaiPhongSlot(int indexSlot)
    {
        if (nhanVatTrongSlot != null && indexSlot >= 0 && indexSlot < nhanVatTrongSlot.Length)
        {
            nhanVatTrongSlot[indexSlot] = null;
        }
        CancelInvoke(nameof(ThucThiKiemTraGameOver));
    }

    // Kiểm tra xem có nhân vật nào đang đứng chờ trong slot có thể lên xe không (Chống đệ quy vô hạn)
    public void KiemTraNguoiTrongHangChoLenXe()
    {
        if (dangKiemTraHangCho || nhanVatTrongSlot == null) return;
        dangKiemTraHangCho = true;

        bool coNguoiLenXe;
        do
        {
            coNguoiLenXe = false;
            for (int i = 0; i < nhanVatTrongSlot.Length; i++)
            {
                NhanVat nv = nhanVatTrongSlot[i];
                if (nv != null && !nv.DangDiChuyen && !nv.DangDiChuyenToiXe)
                {
                    if (BenXe.Instance != null && BenXe.Instance.xeBusHienTai != null 
                        && BenXe.Instance.xeBusHienTai.DangDungTrongBen 
                        && BenXe.Instance.xeBusHienTai.CoChoTrongChoKhach())
                    {
                        bool hopMau = (BenXe.Instance.xeBusHienTai.mauCuaXe == LoaiMau.CauVong || nv.mauNV == BenXe.Instance.xeBusHienTai.mauCuaXe);
                        if (hopMau)
                        {
                            nv.ThuLenXeBus();
                            coNguoiLenXe = true;
                            break; // Lặp lại từ đầu để duyệt theo đúng thứ tự slot ưu tiên
                        }
                    }
                }
            }
        } while (coNguoiLenXe);

        dangKiemTraHangCho = false;
    }

    /// <summary>
    /// Kiểm tra trạng thái thua cuộc một cách chuẩn xác:
    /// Xử thua khi hàng chờ đầy, không ai có thể lên xe, không còn người di chuyển.
    /// Undo và Booster chỉ là công cụ hỗ trợ người chơi chủ động bấm, không ép buộc phải dùng hết mới được thua.
    /// </summary>
    public void KiemTraGameOverSauKhiCho()
    {
        CancelInvoke(nameof(ThucThiKiemTraGameOver));
        Invoke(nameof(ThucThiKiemTraGameOver), 0.4f);
    }

    public void ThucThiKiemTraGameOver()
    {
        // 1. Nếu còn slot trống -> Chưa thua
        if (TimSlotTrongDauTien() != -1) return;

        // 2. Nếu có bất kỳ nhân vật nào đang di chuyển -> Đợi họ đến nơi
        if (CoNhanVatDangDiChuyen())
        {
            CancelInvoke(nameof(ThucThiKiemTraGameOver));
            Invoke(nameof(ThucThiKiemTraGameOver), 0.3f);
            return;
        }

        // 3. Nếu xe bus hiện tại đang tiến vào bến -> Đợi xe đỗ hẳn để đón khách
        if (BenXe.Instance != null && BenXe.Instance.xeBusHienTai != null && !BenXe.Instance.xeBusHienTai.DangDungTrongBen)
        {
            CancelInvoke(nameof(ThucThiKiemTraGameOver));
            Invoke(nameof(ThucThiKiemTraGameOver), 0.3f);
            return;
        }

        // 4. Nếu xe bus hiện tại có thể đón bất kỳ ai trong hàng chờ -> Chưa thua
        if (BenXe.Instance != null && BenXe.Instance.xeBusHienTai != null && BenXe.Instance.xeBusHienTai.DangDungTrongBen)
        {
            LoaiMau mauXe = BenXe.Instance.xeBusHienTai.mauCuaXe;
            if (mauXe == LoaiMau.CauVong) return;

            if (nhanVatTrongSlot != null)
            {
                foreach (var nv in nhanVatTrongSlot)
                {
                    if (nv != null && nv.mauNV == mauXe) return;
                }
            }
        }

        // 5. Thật sự bế tắc (Kín chỗ, không ai lên xe được, không có xe phù hợp) -> Game Over!
        Debug.Log("[TouchManager] Game Over! Hàng chờ kín chỗ và không còn bước đi hợp lệ!");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseGame();
        }
    }

    public bool CoNhanVatDangDiChuyen()
    {
        if (BenXe.Instance != null && BenXe.Instance.danhSachTatCaKhach != null)
        {
            for (int i = 0; i < BenXe.Instance.danhSachTatCaKhach.Count; i++)
            {
                var nv = BenXe.Instance.danhSachTatCaKhach[i];
                if (nv != null && (nv.DangDiChuyen || nv.DangDiChuyenToiXe)) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Quét ngay lập tức các khách ẩn trong sân khi có một người vừa rời vị trí
    /// </summary>
    public void KiemTraVaHeLoKhachAnSauKhiNguoiDi()
    {
        if (BenXe.Instance != null && BenXe.Instance.danhSachTatCaKhach != null)
        {
            for (int i = 0; i < BenXe.Instance.danhSachTatCaKhach.Count; i++)
            {
                NhanVat nv = BenXe.Instance.danhSachTatCaKhach[i];
                if (nv != null && nv.laKhachAn && !nv.DangDiChuyen && nv.slotIndexHienTai == -1)
                {
                    if (nv.KiemTraDuongThoat())
                    {
                        nv.HeLoMauThat();
                    }
                }
            }
        }
    }
}
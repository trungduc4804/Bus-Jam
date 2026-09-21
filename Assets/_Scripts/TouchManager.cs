using UnityEngine;

public class TouchManager : MonoBehaviour
{
    public static TouchManager Instance { get; private set; }

    [Header("Kéo tất cả các ô Slot hàng chờ trong Scene vào đây")]
    public Transform[] danhSachSlot; // Mảng chứa các vị trí đứng

    // Mảng lưu trữ nhân vật đang ở từng slot
    private NhanVat[] nhanVatTrongSlot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (danhSachSlot != null && danhSachSlot.Length > 0)
        {
            nhanVatTrongSlot = new NhanVat[danhSachSlot.Length];
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
                        return; 
                    }
                    // 1. Tìm vị trí slot trống đầu tiên
                    int indexSlotTrong = TimSlotTrongDauTien();

                    if (indexSlotTrong != -1)
                    {
                        // Phát âm thanh Pop khi chọn khách hợp lệ
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayTapKhach();

                        // 2. Lưu thông tin nhân vật vào slot
                        nhanVatTrongSlot[indexSlotTrong] = nvBiCham;
                        nvBiCham.slotIndexHienTai = indexSlotTrong;

                        // 3. Tắt Collider của nhân vật để tránh bị click lại
                        Collider col = nvBiCham.GetComponent<Collider>();
                        if (col != null) col.enabled = false;

                        // 4. Ra lệnh cho nhân vật di chuyển tới slot
                        nvBiCham.DiChuyenToi(danhSachSlot[indexSlotTrong].position);
                    }
                    else
                    {
                        // Nếu hàng chờ đã đầy không còn slot nào trống
                        Debug.Log("Game Over! Hàng chờ đã kín chỗ!");
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayHangChoDay();
                        GameManager.Instance.LoseGame();
                    }
                }
            }
        }
    }

    private int soSlotChoPhep = -1;

    // Cho phép LevelManager tùy chỉnh số slot hàng chờ tối đa của Level
    public void CapNhatSoSlot(int soSlot)
    {
        soSlotChoPhep = soSlot;
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

    // Giải phóng slot khi nhân vật lên xe bus thành công
    public void GiaiPhongSlot(int indexSlot)
    {
        if (nhanVatTrongSlot != null && indexSlot >= 0 && indexSlot < nhanVatTrongSlot.Length)
        {
            nhanVatTrongSlot[indexSlot] = null;
        }
    }

    // Kiểm tra xem có nhân vật nào đang đứng chờ trong slot có thể lên xe không
    public void KiemTraNguoiTrongHangChoLenXe()
    {
        if (nhanVatTrongSlot == null) return;

        for (int i = 0; i < nhanVatTrongSlot.Length; i++)
        {
            NhanVat nv = nhanVatTrongSlot[i];
            if (nv != null && !nv.DangDiChuyen)
            {
                nv.ThuLenXeBus();
            }
        }
    }
}
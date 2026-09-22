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
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (nhanVatTrongSlot == null && danhSachSlot != null && danhSachSlot.Length > 0)
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
using System.Collections.Generic;
using UnityEngine;

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

    // Hàng chờ lưu danh sách màu xe bus theo thứ tự trong LevelData
    private Queue<LoaiMau> hangChoXeBus = new Queue<LoaiMau>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

                    GameObject prefabKhach = GetPrefabKhach(c);
                    if (prefabKhach != null)
                    {
                        Instantiate(prefabKhach, toaDo, Quaternion.identity);
                    }

                    colIndex++;
                }
            }
        }

        // 4. Kích hoạt gọi chiếc xe bus đầu tiên tiến vào bến
        if (BenXe.Instance != null)
        {
            BenXe.Instance.GoiXeTiepTheo();
        }
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
                return xeBus;
            }
        }

        return null;
    }

    public bool ConXeBusTrongQueue()
    {
        return hangChoXeBus.Count > 0;
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
            default: return null;
        }
    }
}
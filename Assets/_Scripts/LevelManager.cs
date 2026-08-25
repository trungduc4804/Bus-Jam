using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Quản lý Level")]
    public static int levelIndex = 0; // Biến static để lưu tiến trình khi đổi Scene
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
    public Transform viTriXuatPhatXe; // Vị trí xuất phát của xe bus trước khi chạy vào bến

    [Header("Cài đặt Lưới")]
    public float khoangCachO = 1.2f; // Khoảng cách giữa các nhân vật trong lưới
    public Vector3 viTriBatDau = new Vector3(0, 0, 0); // Vị trí ô đầu tiên (0,0)

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

        // Quay lại màn 0 nếu levelIndex vượt quá số lượng level khả dụng
        if (levelIndex >= danhSachLevel.Length || levelIndex < 0)
        {
            levelIndex = 0;
        }

        LevelData levelHienTai = danhSachLevel[levelIndex];
        if (levelHienTai == null) return;

        // 1. Cài đặt số slot hàng chờ cho TouchManager theo LevelData
        if (TouchManager.Instance != null && levelHienTai.soSlotHangCho > 0)
        {
            TouchManager.Instance.CapNhatSoSlot(levelHienTai.soSlotHangCho);
        }

        // 2. Sinh ra danh sách Xe Bus từ LevelData
        if (BenXe.Instance != null && levelHienTai.danhSachXeBus != null)
        {
            BenXe.Instance.danhSachXeChuanBi.Clear();
            Vector3 posSpawnBus = (viTriXuatPhatXe != null) ? viTriXuatPhatXe.position : new Vector3(-15, 0, 5);

            foreach (LoaiMau mauXe in levelHienTai.danhSachXeBus)
            {
                GameObject prefabXe = GetPrefabXe(mauXe);
                if (prefabXe != null)
                {
                    GameObject objXe = Instantiate(prefabXe, posSpawnBus, Quaternion.identity);
                    XeBus xeBus = objXe.GetComponent<XeBus>();
                    if (xeBus != null)
                    {
                        xeBus.mauCuaXe = mauXe;
                        BenXe.Instance.danhSachXeChuanBi.Add(xeBus);
                    }
                }
            }

            // Gọi xe bus đầu tiên tiến vào bến
            BenXe.Instance.GoiXeTiepTheo();
        }

        // 3. Sinh ra bản đồ Nhân Vật từ banDoLuoii
        if (levelHienTai.banDoLuoii != null)
        {
            for (int z = 0; z < levelHienTai.banDoLuoii.Length; z++)
            {
                string hangHienTai = levelHienTai.banDoLuoii[z];
                if (string.IsNullOrEmpty(hangHienTai)) continue;

                int colIndex = 0;
                for (int i = 0; i < hangHienTai.Length; i++)
                {
                    char c = hangHienTai[i];
                    if (c == ' ') continue; // Bỏ qua khoảng trắng trang trí

                    Vector3 toaDo = viTriBatDau + new Vector3(colIndex * khoangCachO, 0, -z * khoangCachO);
                    GameObject prefabKhach = GetPrefabKhach(c);

                    if (prefabKhach != null)
                    {
                        Instantiate(prefabKhach, toaDo, Quaternion.identity);
                    }

                    colIndex++;
                }
            }
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
            default: return null; // Ký tự '0' hoặc ô trống
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
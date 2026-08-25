using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("File Data Màn Chơi")]
    public LevelData[] danhSachLevel;
    private int levelIndex = 0;

    [Header("Kéo các Prefab từ thư mục vào đây")]
    public GameObject prefabKhachDo;
    public GameObject prefabKhachXanh;
    public GameObject prefabKhachVang;

    [Header("Cài đặt Lưới")]
    public float khoangCachO = 1.2f; // Chỉnh khoảng cách giữa người với người
    public Vector3 viTriBatDau = new Vector3(0, 0, 0); // Vị trí người đầu tiên đứng

    void Start()
    {
        SinhRaBanDo();
    }

    private void SinhRaBanDo()
    {
        if(levelIndex >= danhSachLevel.Length){
            levelIndex = 0;
        }
        LevelData levelHienTai = danhSachLevel[levelIndex];
        // Duyệt qua từng dòng chữ trong mảng Ban Do Luoii
        for (int z = 0; z < levelHienTai.banDoLuoii.Length; z++)
        {
            string hangHienTai = levelHienTai.banDoLuoii[z];
            
            // Xóa khoảng trắng để dễ tính toán (VD: "R B Y" -> "RBY")
            hangHienTai = hangHienTai.Replace(" ", "");

            // Duyệt qua từng chữ cái trong dòng
            for (int x = 0; x < hangHienTai.Length; x++)
            {
                char loaiKhach = hangHienTai[x];
                
                // Tính toán tọa độ (trục Z lùi dần để xếp thành các hàng sau)
                Vector3 toaDo = viTriBatDau + new Vector3(x * khoangCachO, 0, -z * khoangCachO);

                // Sinh ra Prefab tương ứng với chữ cái
                if (loaiKhach == 'R') Instantiate(prefabKhachDo, toaDo, Quaternion.identity);
                else if (loaiKhach == 'B') Instantiate(prefabKhachXanh, toaDo, Quaternion.identity);
                else if (loaiKhach == 'Y') Instantiate(prefabKhachVang, toaDo, Quaternion.identity);
            }
        }
    }
}
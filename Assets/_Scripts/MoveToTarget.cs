using UnityEngine;


public enum TypeCar {
    Do,
    Xanh,
    Vang, 
    Tim
}

public class MoveToTarget : MonoBehaviour
{
    public TypeCar typeCar;
    private Vector3 diemDen;
    private bool dangDiChuyen = false;
    public float tocDo = 5f; // Tốc độ chạy của nhân vật

    void Update()
    {
        // Nếu biến dangDiChuyen là true, nhân vật sẽ liên tục nhích về phía đích
        if (dangDiChuyen)
        {
            // Vector3.MoveTowards giúp tính toán và di chuyển mượt mà giữa 2 điểm
            transform.position = Vector3.MoveTowards(transform.position, diemDen, tocDo * Time.deltaTime);
            
            // Nếu khoảng cách đến đích rất nhỏ (gần như đã tới nơi), thì dừng lại
            if (Vector3.Distance(transform.position, diemDen) < 0.01f)
            {
                dangDiChuyen = false;
            }
        }
    }

    // Hàm này dùng để nhận lệnh di chuyển từ bên ngoài (từ TouchManager)
    public void DiChuyenToi(Vector3 viTriMoi)
    {
        diemDen = viTriMoi; // Ghi nhớ tọa độ đích
        dangDiChuyen = true; // Bắt đầu cho phép di chuyển trong hàm Update
    }
}



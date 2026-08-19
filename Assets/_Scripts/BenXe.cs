using UnityEngine;

public class BenXe : MonoBehaviour
{
    public static BenXe Instance { get; private set; }
    public XeBus xeBusHienTai;

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

    // Kiểm tra nhân vật có cùng màu với xe bus và còn ghế không
    public bool KtraVaLenXe(NhanVat khachhang)
    {
        if (xeBusHienTai != null && khachhang.mauNV == xeBusHienTai.mauCuaXe)
        {
            Destroy(khachhang.gameObject);
            bool xeDaDay = xeBusHienTai.ThemKhach();
            if(xeDaDay){
                xeBusHienTai = null;
            }
            return true;
        }
        return false;
    }
}

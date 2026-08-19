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
    public bool KtraVaLenXe(NhanVat nv)
    {
        if (xeBusHienTai != null && xeBusHienTai.ConGhe() && nv.mauNV == xeBusHienTai.maucuaxe)
        {
            xeBusHienTai.GiamSoGhe();
            Destroy(nv.gameObject);
            return true;
        }
        return false;
    }
}

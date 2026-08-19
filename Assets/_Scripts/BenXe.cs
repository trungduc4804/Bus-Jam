using UnityEngine;

public class BenXe : MonoBehaviour
{
    public static BenXe Instence {get; private set;}
    public XeBus xeBusHienTai;
    private void Awake()
    {
        if(Instence = null){
            Instence = this;
        }else{
            Destroy (gameObject);
        }
    }

    public bool 
}

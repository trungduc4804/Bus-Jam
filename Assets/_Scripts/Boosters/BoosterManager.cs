using System;
using UnityEngine;

namespace BusJam.Boosters
{
    /// <summary>
    /// Quản lý hệ thống Booster (Vật phẩm hỗ trợ) trong màn chơi:
    /// - Xe Cầu Vồng (Rainbow Bus): Biến chiếc xe bus hiện tại thành xe đa năng chở bất kỳ màu nào
    /// </summary>
    public class BoosterManager : MonoBehaviour
    {
        private static BoosterManager instance;
        public static BoosterManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = UnityEngine.Object.FindAnyObjectByType<BoosterManager>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject("BoosterManager");
                        instance = obj.AddComponent<BoosterManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Cài đặt Booster Xe Cầu Vồng")]
        [Tooltip("Số lượt sử dụng Booster Xe Cầu Vồng trong một ván")]
        public int soLuotXeCauVongToiDa = 1;
        public bool voHanBooster = false;

        private int soLuotXeCauVongConLai;

        /// <summary>
        /// Event thông báo khi số lượng booster thay đổi: (int soLuotConLai, bool coTheDung)
        /// </summary>
        public event Action<int, bool> OnBoosterStateChanged;

        public int SoLuotXeCauVongConLai => voHanBooster ? -1 : soLuotXeCauVongConLai;

        public bool CoTheKichHoatXeCauVong
        {
            get
            {
                if (!voHanBooster && soLuotXeCauVongConLai <= 0) return false;
                if (BenXe.Instance == null || BenXe.Instance.xeBusHienTai == null) return false;
                // Chỉ kích hoạt khi xe đang đỗ trong bến và chưa phải là xe cầu vồng
                return BenXe.Instance.xeBusHienTai.DangDungTrongBen 
                    && BenXe.Instance.xeBusHienTai.mauCuaXe != LoaiMau.CauVong;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this);
                return;
            }

            ResetBooster();
        }

        public void ResetBooster()
        {
            soLuotXeCauVongConLai = soLuotXeCauVongToiDa;
            NotifyStateChanged();
        }

        /// <summary>
        /// Kích hoạt Booster Xe Cầu Vồng cho chiếc xe bus đang đỗ trong bến
        /// </summary>
        public bool KichHoatBoosterXeCauVong()
        {
            if (!CoTheKichHoatXeCauVong)
            {
                Debug.Log("[BoosterManager] Không thể kích hoạt Xe Cầu Vồng lúc này (Không có xe trong bến hoặc đã hết lượt)!");
                return false;
            }

            BenXe.Instance.xeBusHienTai.BienThanhXeCauVong();

            if (!voHanBooster)
            {
                soLuotXeCauVongConLai--;
            }

            NotifyStateChanged();
            return true;
        }

        public void NotifyStateChanged()
        {
            OnBoosterStateChanged?.Invoke(SoLuotXeCauVongConLai, CoTheKichHoatXeCauVong);
        }
    }
}

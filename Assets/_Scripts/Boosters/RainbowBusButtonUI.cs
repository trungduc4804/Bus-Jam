using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace BusJam.Boosters
{
    /// <summary>
    /// Điều khiển hiển thị và tương tác của nút Booster Xe Cầu Vồng trên Canvas
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class RainbowBusButtonUI : MonoBehaviour
    {
        [Header("Giao diện UI")]
        public TextMeshProUGUI textSoLuotConLai;
        public Image iconImage;

        [Header("Màu sắc trạng thái")]
        public Color mauBinhThuong = Color.white;
        public Color mauVoHieuHoa = new Color(1f, 1f, 1f, 0.45f);

        private Button button;
        private Vector3 gocScaleBanDau;

        private void Awake()
        {
            button = GetComponent<Button>();
            gocScaleBanDau = transform.localScale;

            if (iconImage == null)
            {
                iconImage = GetComponent<Image>();
            }

            if (textSoLuotConLai == null)
            {
                textSoLuotConLai = GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if (textSoLuotConLai == null)
            {
                TaoBadgeSoDem();
            }

            // Đảm bảo BoosterManager được khởi tạo
            _ = BoosterManager.Instance;
        }

        private void TaoBadgeSoDem()
        {
            GameObject badgeObj = new GameObject("BoosterCountBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            badgeObj.transform.SetParent(transform, false);

            RectTransform rect = badgeObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.6f, 0f);
            rect.anchorMax = new Vector2(1f, 0.4f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(70, 70);

            textSoLuotConLai = badgeObj.GetComponent<TextMeshProUGUI>();
            textSoLuotConLai.alignment = TextAlignmentOptions.Center;
            textSoLuotConLai.fontSize = 46;
            textSoLuotConLai.fontStyle = FontStyles.Bold;
            textSoLuotConLai.color = new Color(1f, 0.9f, 0.2f);
            textSoLuotConLai.raycastTarget = false;
        }

        private void Start()
        {
            button.onClick.RemoveListener(OnClickRainbowBus);
            button.onClick.AddListener(OnClickRainbowBus);

            if (BoosterManager.Instance != null)
            {
                BoosterManager.Instance.OnBoosterStateChanged += CapNhatTrangThaiNut;
                CapNhatTrangThaiNut(BoosterManager.Instance.SoLuotXeCauVongConLai, BoosterManager.Instance.CoTheKichHoatXeCauVong);
            }
        }

        private void OnDestroy()
        {
            transform.DOKill();
            if (BoosterManager.HasInstance && BoosterManager.Instance != null)
            {
                BoosterManager.Instance.OnBoosterStateChanged -= CapNhatTrangThaiNut;
            }
        }

        private void Update()
        {
            // Hiệu ứng ánh sáng cầu vồng nhẹ nhấp nháy cho nút Booster để thu hút người chơi
            if (button != null && button.interactable && iconImage != null)
            {
                float t = Mathf.PingPong(Time.time * 0.8f, 1f);
                iconImage.color = Color.Lerp(Color.white, new Color(1f, 0.85f, 0.3f), t);
            }
        }

        private void OnClickRainbowBus()
        {
            if (BoosterManager.Instance == null) return;

            transform.DOKill();
            transform.localScale = gocScaleBanDau;
            transform.DOPunchScale(new Vector3(-0.18f, -0.18f, 0), 0.25f, 10, 1f);

            bool thanhCong = BoosterManager.Instance.KichHoatBoosterXeCauVong();
            if (!thanhCong)
            {
                transform.DOShakePosition(0.25f, new Vector3(8f, 0, 0), 15);
            }
        }

        public void CapNhatTrangThaiNut(int remainingCount, bool canUse)
        {
            if (button != null)
            {
                button.interactable = (remainingCount != 0);
            }

            if (iconImage != null)
            {
                iconImage.color = (remainingCount != 0) ? mauBinhThuong : mauVoHieuHoa;
            }

            if (textSoLuotConLai != null)
            {
                if (remainingCount < 0)
                {
                    textSoLuotConLai.text = "∞";
                }
                else
                {
                    textSoLuotConLai.text = remainingCount.ToString();
                    textSoLuotConLai.color = remainingCount > 0 ? new Color(1f, 0.9f, 0.2f) : Color.red;
                }
            }
        }

        /// <summary>
        /// Tự động sinh ra nút Rainbow Bus bên cạnh UndoButton khi vào màn chơi nếu scene chưa có
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void TuDongSinhNutRainbowBusNeuChuaCo()
        {
            if (Object.FindAnyObjectByType<TouchManager>() == null) return;
            if (Object.FindAnyObjectByType<RainbowBusButtonUI>() != null) return;

            GameObject settingBtn = GameObject.Find("SettingButton");
            if (settingBtn != null)
            {
                GameObject rainbowBtn = Instantiate(settingBtn, settingBtn.transform.parent);
                rainbowBtn.name = "RainbowBusButton";

                RectTransform rt = rainbowBtn.GetComponent<RectTransform>();
                if (rt != null)
                {
                    // Đặt cạnh nút Undo (-360) với khoảng cách đều là -580
                    rt.anchoredPosition = new Vector2(-580f, -151f);
                    rt.sizeDelta = new Vector2(200f, 200f);
                }

                Button btn = rainbowBtn.GetComponent<Button>();
                if (btn != null) btn.onClick.RemoveAllListeners();

                // Đổi icon sang icon xe hoặc ngôi sao / logo cầu vồng
                Image img = rainbowBtn.GetComponent<Image>();
                Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                foreach (var s in allSprites)
                {
                    if (s.name.ToLower().Contains("icon") || s.name.ToLower().Contains("choselv"))
                    {
                        if (img != null) img.sprite = s;
                        break;
                    }
                }

                rainbowBtn.AddComponent<RainbowBusButtonUI>();
                Debug.Log("[RainbowBusButtonUI] Đã tự động tạo nút Rainbow Bus trên Canvas!");
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace BusJam.Commands
{
    /// <summary>
    /// Điều khiển hiển thị và tương tác của nút Hoàn tác (Undo) trên UI Canvas
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UndoButtonUI : MonoBehaviour
    {
        [Header("Giao diện UI")]
        [Tooltip("Text hiển thị số lượt hoàn tác còn lại (tuỳ chọn)")]
        public TextMeshProUGUI textSoLuotConLai;

        [Tooltip("Ảnh hiển thị icon nút để đổi độ mờ khi disable")]
        public Image iconImage;

        [Header("Màu sắc trạng thái")]
        public Color mauBinhThuong = Color.white;
        public Color mauKhiVoHieuHoa = new Color(1f, 1f, 1f, 0.45f);

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

            // Tự động tìm hoặc tạo Text hiển thị số lượt nếu chưa có
            if (textSoLuotConLai == null)
            {
                textSoLuotConLai = GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if (textSoLuotConLai == null)
            {
                TaoBadgeSoDem();
            }

            // Đảm bảo UndoManager tồn tại trong scene
            if (UndoManager.Instance == null)
            {
                GameObject obj = new GameObject("UndoManager");
                obj.AddComponent<UndoManager>();
            }
        }

        private void TaoBadgeSoDem()
        {
            GameObject badgeObj = new GameObject("CountBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
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
            textSoLuotConLai.color = Color.white;
            textSoLuotConLai.raycastTarget = false;
        }

        /// <summary>
        /// Tự động sinh ra nút Undo bên cạnh SettingButton khi vào Gameplay nếu scene chưa gắn sẵn
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void TuDongSinhNutUndoNeuChuaCo()
        {
            if (Object.FindAnyObjectByType<TouchManager>() == null) return;
            if (Object.FindAnyObjectByType<UndoButtonUI>() != null) return;

            GameObject settingBtn = GameObject.Find("SettingButton");
            if (settingBtn != null)
            {
                GameObject undoBtn = Instantiate(settingBtn, settingBtn.transform.parent);
                undoBtn.name = "UndoButton";

                RectTransform rt = undoBtn.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(-360f, -151f);
                    rt.sizeDelta = new Vector2(200f, 200f);
                }

                Button btn = undoBtn.GetComponent<Button>();
                if (btn != null) btn.onClick.RemoveAllListeners();

                Image img = undoBtn.GetComponent<Image>();
                Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                foreach (var s in allSprites)
                {
                    if (s.name.ToLower().Contains("replay"))
                    {
                        if (img != null) img.sprite = s;
                        break;
                    }
                }

                undoBtn.AddComponent<UndoButtonUI>();
                Debug.Log("[UndoButtonUI] Đã tự động tạo nút Undo trên Canvas bên cạnh SettingButton!");
            }
        }

        private void Start()
        {
            button.onClick.RemoveListener(OnClickUndo);
            button.onClick.AddListener(OnClickUndo);

            if (UndoManager.Instance != null)
            {
                UndoManager.Instance.OnUndoStateChanged += CapNhatTrangThaiNut;
                CapNhatTrangThaiNut(UndoManager.Instance.CanUndo, UndoManager.Instance.SoLuotUndoConLai);
            }
        }

        private void OnDestroy()
        {
            transform.DOKill();
            if (UndoManager.Instance != null)
            {
                UndoManager.Instance.OnUndoStateChanged -= CapNhatTrangThaiNut;
            }
        }

        private void OnClickUndo()
        {
            if (UndoManager.Instance == null) return;

            // Hiệu ứng nảy nút (Juice)
            transform.DOKill();
            transform.localScale = gocScaleBanDau;
            transform.DOPunchScale(new Vector3(-0.15f, -0.15f, 0), 0.2f, 10, 1f);

            bool thanhCong = UndoManager.Instance.Undo();
            if (!thanhCong)
            {
                // Rung nhẹ báo lỗi nếu bấm khi không thể hoàn tác
                transform.DOShakePosition(0.25f, new Vector3(8f, 0, 0), 15);
            }
        }

        public void CapNhatTrangThaiNut(bool canUndo, int remainingCount)
        {
            if (button != null)
            {
                button.interactable = canUndo;
            }

            if (iconImage != null)
            {
                iconImage.color = canUndo ? mauBinhThuong : mauKhiVoHieuHoa;
            }

            if (textSoLuotConLai != null)
            {
                if (remainingCount < 0)
                {
                    // Vô hạn
                    textSoLuotConLai.text = "∞";
                }
                else
                {
                    textSoLuotConLai.text = remainingCount.ToString();
                    textSoLuotConLai.color = remainingCount > 0 ? Color.white : Color.red;
                }
            }
        }
    }
}

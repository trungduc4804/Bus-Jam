#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using BusJam.Commands;

namespace BusJam.Editor
{
    public static class GameSetupMenu
    {
        [MenuItem("Tools/Bus Jam/Tạo Nút Undo Trên Canvas", false, 10)]
        public static void TaoNutUndoTrenCanvas()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Canvas trong Scene!", "OK");
                return;
            }

            UndoButtonUI existing = Object.FindAnyObjectByType<UndoButtonUI>();
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorUtility.DisplayDialog("Thông báo", "Nút UndoButton đã có sẵn trong Scene!", "OK");
                return;
            }

            GameObject settingBtn = GameObject.Find("SettingButton");
            GameObject undoBtn;

            if (settingBtn != null)
            {
                undoBtn = Object.Instantiate(settingBtn, settingBtn.transform.parent);
                undoBtn.name = "UndoButton";
            }
            else
            {
                undoBtn = new GameObject("UndoButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                undoBtn.transform.SetParent(canvas.transform, false);
            }

            RectTransform rt = undoBtn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-360f, -151f);
            rt.sizeDelta = new Vector2(200f, 200f);

            Button btn = undoBtn.GetComponent<Button>();
            if (btn != null) btn.onClick.RemoveAllListeners();

            Image img = undoBtn.GetComponent<Image>();
            string[] guids = AssetDatabase.FindAssets("Replay t:Sprite");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (spr != null && img != null) img.sprite = spr;
            }

            UndoButtonUI ui = undoBtn.GetComponent<UndoButtonUI>();
            if (ui == null) ui = undoBtn.AddComponent<UndoButtonUI>();

            Selection.activeGameObject = undoBtn;
            UnityEditor.Undo.RegisterCreatedObjectUndo(undoBtn, "Tạo UndoButton");
            EditorUtility.SetDirty(undoBtn);

            Debug.Log("[GameSetupMenu] Đã tạo nút UndoButton trên Canvas!");
        }

        [MenuItem("Tools/Bus Jam/Tạo Nút Booster Xe Cầu Vồng Trên Canvas", false, 11)]
        public static void TaoNutBoosterXeCauVongTrenCanvas()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Canvas trong Scene!", "OK");
                return;
            }

            BusJam.Boosters.RainbowBusButtonUI existing = Object.FindAnyObjectByType<BusJam.Boosters.RainbowBusButtonUI>();
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorUtility.DisplayDialog("Thông báo", "Nút Booster Xe Cầu Vồng đã có sẵn trong Scene!", "OK");
                return;
            }

            // Ưu tiên nhân bản từ UndoButton hoặc SettingButton để giữ trọn vẹn phong cách giao diện
            GameObject mauBtn = GameObject.Find("UndoButton");
            if (mauBtn == null) mauBtn = GameObject.Find("SettingButton");

            GameObject rainbowBtn;
            if (mauBtn != null)
            {
                rainbowBtn = Object.Instantiate(mauBtn, mauBtn.transform.parent);
                rainbowBtn.name = "RainbowBusButton";

                // Xóa script UndoButtonUI nếu nhân bản từ UndoButton
                UndoButtonUI oldUndoUI = rainbowBtn.GetComponent<UndoButtonUI>();
                if (oldUndoUI != null) Object.DestroyImmediate(oldUndoUI);
            }
            else
            {
                rainbowBtn = new GameObject("RainbowBusButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                rainbowBtn.transform.SetParent(canvas.transform, false);
            }

            RectTransform rt = rainbowBtn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            // Đặt nằm bên trái nút Undo (Setting: -127, Undo: -360, Rainbow: -593)
            rt.anchoredPosition = new Vector2(-593f, -151f);
            rt.sizeDelta = new Vector2(200f, 200f);

            Button btn = rainbowBtn.GetComponent<Button>();
            if (btn != null) btn.onClick.RemoveAllListeners();

            Image img = rainbowBtn.GetComponent<Image>();
            if (img != null)
            {
                string[] guids = AssetDatabase.FindAssets("Icon t:Sprite");
                if (guids.Length == 0) guids = AssetDatabase.FindAssets("ChoseLV t:Sprite");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (spr != null) img.sprite = spr;
                }
                img.color = new Color(1f, 0.88f, 0.25f, 1f); // Màu vàng ánh kim nổi bật
            }

            // Gắn component RainbowBusButtonUI
            BusJam.Boosters.RainbowBusButtonUI ui = rainbowBtn.GetComponent<BusJam.Boosters.RainbowBusButtonUI>();
            if (ui == null) ui = rainbowBtn.AddComponent<BusJam.Boosters.RainbowBusButtonUI>();

            // Đảm bảo có BoosterManager trong Scene
            BusJam.Boosters.BoosterManager bm = Object.FindAnyObjectByType<BusJam.Boosters.BoosterManager>();
            if (bm == null)
            {
                GameObject bmObj = new GameObject("BoosterManager");
                bmObj.AddComponent<BusJam.Boosters.BoosterManager>();
                UnityEditor.Undo.RegisterCreatedObjectUndo(bmObj, "Tạo BoosterManager");
            }

            Selection.activeGameObject = rainbowBtn;
            UnityEditor.Undo.RegisterCreatedObjectUndo(rainbowBtn, "Tạo RainbowBusButton");
            EditorUtility.SetDirty(rainbowBtn);

            Debug.Log("[GameSetupMenu] Đã tạo nút Booster Xe Cầu Vồng (RainbowBusButton) thành công trên Canvas!");
        }

        [MenuItem("Tools/Bus Jam/Thiết Lập Toàn Bộ UI Boosters (Undo + Xe Cầu Vồng)", false, 12)]
        public static void ThietLapToanBoBoosterUI()
        {
            TaoNutUndoTrenCanvas();
            TaoNutBoosterXeCauVongTrenCanvas();
            EditorUtility.DisplayDialog("Thành công", "Đã thiết lập đầy đủ Nút Undo và Nút Booster Xe Cầu Vồng trên Canvas!", "OK");
        }
    }
}
#endif

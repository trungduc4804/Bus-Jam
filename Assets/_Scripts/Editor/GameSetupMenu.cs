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
    }
}
#endif

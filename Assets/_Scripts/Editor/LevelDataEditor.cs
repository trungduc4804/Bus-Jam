#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BusJam.Editor
{
    /// <summary>
    /// Custom Inspector cho LevelData ScriptableObject
    /// Cung cấp nút mở nhanh cửa sổ Level Editor và bản xem trước trực quan (Visual Preview)
    /// </summary>
    [CustomEditor(typeof(LevelData))]
    public class LevelDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            LevelData data = (LevelData)target;

            EditorGUILayout.Space(6);

            // Nút bấm nổi bật mở cửa sổ Level Editor
            GUI.backgroundColor = new Color(0.25f, 0.85f, 0.45f);
            if (GUILayout.Button("🎨 MỞ TRONG LEVEL EDITOR WINDOW", GUILayout.Height(38)))
            {
                BusJamLevelEditorWindow.OpenWithLevel(data);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(8);

            // Thống kê nhanh
            DrawQuickSummary(data);

            EditorGUILayout.Space(8);

            // Bản xem trước trực quan (Mini Grid Preview)
            DrawMiniGridPreview(data);

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Dữ Liệu Gốc (Raw Data)", EditorStyles.boldLabel);

            // Vẽ Inspector mặc định bên dưới
            DrawDefaultInspector();
        }

        private void DrawQuickSummary(LevelData data)
        {
            int demDo = 0, demXanh = 0, demVang = 0, demTim = 0;
            if (data.banDoLuoii != null)
            {
                foreach (string row in data.banDoLuoii)
                {
                    if (string.IsNullOrEmpty(row)) continue;
                    foreach (char c in row)
                    {
                        char upper = char.ToUpper(c);
                        if (upper == 'R') demDo++;
                        else if (upper == 'B') demXanh++;
                        else if (upper == 'Y') demVang++;
                        else if (upper == 'T' || upper == 'P') demTim++;
                    }
                }
            }

            int gheDo = 0, gheXanh = 0, gheVang = 0, gheTim = 0;
            if (data.danhSachXeBus != null)
            {
                foreach (var xe in data.danhSachXeBus)
                {
                    switch (xe)
                    {
                        case LoaiMau.Do: gheDo += 3; break;
                        case LoaiMau.Xanh: gheXanh += 3; break;
                        case LoaiMau.Vang: gheVang += 3; break;
                        case LoaiMau.Tim: gheTim += 3; break;
                    }
                }
            }

            int tongKhach = demDo + demXanh + demVang + demTim;
            int tongXe = data.danhSachXeBus != null ? data.danhSachXeBus.Length : 0;
            int tongGhe = gheDo + gheXanh + gheVang + gheTim;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Thống Kê Màn Chơi", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"• Số ô chờ (Slots): {data.soSlotHangCho}");
            EditorGUILayout.LabelField($"• Tổng hành khách: {tongKhach} (Đỏ: {demDo}, Xanh: {demXanh}, Vàng: {demVang}, Tím: {demTim})");
            EditorGUILayout.LabelField($"• Tổng xe bus: {tongXe} xe ({tongGhe} chỗ ngồi)");

            if (tongKhach == tongGhe && demDo == gheDo && demXanh == gheXanh && demVang == gheVang && demTim == gheTim)
            {
                EditorGUILayout.HelpBox("✓ Màn chơi cân bằng: Ghế xe khớp 100% với số khách!", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("! Màn chơi chưa cân bằng số ghế và số khách!", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawMiniGridPreview(LevelData data)
        {
            if (data.banDoLuoii == null || data.banDoLuoii.Length == 0) return;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Bản Xem Trước Lưới (Preview)", EditorStyles.boldLabel);

            for (int r = 0; r < data.banDoLuoii.Length; r++)
            {
                string row = data.banDoLuoii[r];
                if (string.IsNullOrEmpty(row)) continue;

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (int i = 0; i < row.Length; i++)
                {
                    char c = row[i];
                    if (c == ' ') continue;

                    Color bg = GetColorForChar(c);
                    GUI.backgroundColor = bg;
                    GUIStyle style = new GUIStyle(GUI.skin.button)
                    {
                        fontSize = 11,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    };
                    style.normal.textColor = (c == '0') ? Color.gray : Color.white;

                    GUILayout.Box(c == '0' ? "·" : c.ToString(), style, GUILayout.Width(24), GUILayout.Height(24));
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
        }

        private Color GetColorForChar(char c)
        {
            switch (char.ToUpper(c))
            {
                case 'R': return new Color(0.95f, 0.3f, 0.3f);
                case 'B': return new Color(0.25f, 0.6f, 0.98f);
                case 'Y': return new Color(0.98f, 0.82f, 0.2f);
                case 'T':
                case 'P': return new Color(0.75f, 0.35f, 0.95f);
                default: return new Color(0.25f, 0.25f, 0.25f);
            }
        }
    }
}
#endif

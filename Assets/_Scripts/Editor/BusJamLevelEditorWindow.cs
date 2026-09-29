#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BusJam.Editor
{
    /// <summary>
    /// Công cụ thiết kế màn chơi trực quan (Custom Level Editor Window) cho Bus Jam
    /// Cho phép vẽ lưới nhân vật, sắp xếp thứ tự xe bus, kiểm tra tính hợp lệ và lưu LevelData
    /// </summary>
    public class BusJamLevelEditorWindow : EditorWindow
    {
        private LevelData currentLevelData;
        private Vector2 scrollPos;
        private Vector2 gridScrollPos;

        // Cài đặt lưới
        private int soHang = 4;
        private int soCot = 4;
        private char[,] banDoGrid = new char[4, 4];

        // Palette chọn màu để vẽ
        private enum BrushColor { Trong, Do, Xanh, Vang, Tim }
        private BrushColor currentBrush = BrushColor.Do;

        // Danh sách xe bus đang chỉnh sửa
        private List<LoaiMau> danhSachXe = new List<LoaiMau>();
        private int soSlotHangCho = 5;

        // Màu sắc giao diện cho từng loại khách
        private readonly Color colorRed = new Color(0.95f, 0.3f, 0.3f);
        private readonly Color colorBlue = new Color(0.25f, 0.6f, 0.98f);
        private readonly Color colorYellow = new Color(0.98f, 0.82f, 0.2f);
        private readonly Color colorPurple = new Color(0.75f, 0.35f, 0.95f);
        private readonly Color colorEmpty = new Color(0.25f, 0.25f, 0.25f);

        [MenuItem("Tools/Bus Jam/Level Editor Window", false, 1)]
        public static void OpenWindow()
        {
            BusJamLevelEditorWindow window = GetWindow<BusJamLevelEditorWindow>("Bus Jam Level Editor");
            window.minSize = new Vector2(750, 650);
            window.Show();
        }

        public static void OpenWithLevel(LevelData level)
        {
            BusJamLevelEditorWindow window = GetWindow<BusJamLevelEditorWindow>("Bus Jam Level Editor");
            window.minSize = new Vector2(750, 650);
            window.currentLevelData = level;
            window.LoadFromLevelData(level);
            window.Show();
        }

        private void OnEnable()
        {
            if (currentLevelData != null)
            {
                LoadFromLevelData(currentLevelData);
            }
            else
            {
                KhoiTaoGridMoi(4, 4);
            }
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawHeader();
            EditorGUILayout.Space(8);

            DrawLevelSelector();
            EditorGUILayout.Space(10);

            DrawSettingsSection();
            EditorGUILayout.Space(10);

            DrawPaletteSection();
            EditorGUILayout.Space(10);

            DrawGridSection();
            EditorGUILayout.Space(15);

            DrawBusQueueSection();
            EditorGUILayout.Space(15);

            DrawValidationSection();
            EditorGUILayout.Space(15);

            DrawActionButtons();

            EditorGUILayout.Space(20);
            EditorGUILayout.EndScrollView();
        }

        // ==========================================
        // 1. HEADER
        // ==========================================
        private void DrawHeader()
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            headerStyle.normal.textColor = new Color(0.3f, 0.85f, 1f);

            EditorGUILayout.LabelField("BUS JAM - LEVEL DESIGNER TOOL", headerStyle);
            EditorGUILayout.HelpBox("Công cụ trực quan hỗ trợ Game Designer vẽ bản đồ khách, thiết lập thứ tự xe bus và tự động kiểm tra tính giải được của màn chơi.", MessageType.Info);
        }

        // ==========================================
        // 2. CHỌN LEVEL ĐỂ CHỈNH SỬA
        // ==========================================
        private void DrawLevelSelector()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("1. Chọn / Tạo Mới Màn Chơi", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            currentLevelData = (LevelData)EditorGUILayout.ObjectField("Level Asset:", currentLevelData, typeof(LevelData), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (currentLevelData != null)
                {
                    LoadFromLevelData(currentLevelData);
                }
            }

            if (GUILayout.Button("Tạo Level Mới", GUILayout.Width(130), GUILayout.Height(20)))
            {
                TaoLevelMoi();
            }

            if (GUILayout.Button("Làm mới", GUILayout.Width(80), GUILayout.Height(20)))
            {
                if (currentLevelData != null) LoadFromLevelData(currentLevelData);
            }
            EditorGUILayout.EndHorizontal();

            // Hiển thị danh sách các Level hiện có trong thư mục Assets/_Data
            DrawQuickLevelSwitchButtons();

            EditorGUILayout.EndVertical();
        }

        private void DrawQuickLevelSwitchButtons()
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { "Assets/_Data" });
            if (guids.Length > 0)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Chuyển nhanh:", EditorStyles.miniBoldLabel);
                EditorGUILayout.BeginHorizontal();
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    LevelData lv = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                    if (lv != null)
                    {
                        bool isCurrent = (currentLevelData == lv);
                        GUI.backgroundColor = isCurrent ? new Color(0.3f, 1f, 0.4f) : Color.white;
                        if (GUILayout.Button(lv.name, GUILayout.Height(22)))
                        {
                            currentLevelData = lv;
                            LoadFromLevelData(lv);
                        }
                        GUI.backgroundColor = Color.white;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        // ==========================================
        // 3. THIẾT LẬP CƠ BẢN (SỐ SLOT & KÍCH THƯỚC LƯỚI)
        // ==========================================
        private void DrawSettingsSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("2. Cài Đặt Độ Khó & Kích Thước Lưới", EditorStyles.boldLabel);

            soSlotHangCho = EditorGUILayout.IntSlider("Số Slot Hàng Chờ (Waiting Slots):", soSlotHangCho, 3, 7);

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            int newHang = EditorGUILayout.IntSlider("Số Hàng (Rows):", soHang, 1, 12);
            int newCot = EditorGUILayout.IntSlider("Số Cột (Columns):", soCot, 1, 10);

            if (newHang != soHang || newCot != soCot)
            {
                ThayDoiKichThuocGrid(newHang, newCot);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        // ==========================================
        // 4. BẢNG MÀU CHỌN CỌ VẼ (PALETTE)
        // ==========================================
        private void DrawPaletteSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("3. Chọn Cọ Vẽ (Brush Palette)", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            DrawBrushButton(BrushColor.Trong, "Ô Trống [ 0 ]", colorEmpty);
            DrawBrushButton(BrushColor.Do, "Khách Đỏ [ R ]", colorRed);
            DrawBrushButton(BrushColor.Xanh, "Khách Xanh [ B ]", colorBlue);
            DrawBrushButton(BrushColor.Vang, "Khách Vàng [ Y ]", colorYellow);
            DrawBrushButton(BrushColor.Tim, "Khách Tím [ T ]", colorPurple);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Xóa Hết (Clear All)"))
            {
                if (EditorUtility.DisplayDialog("Xác nhận", "Bạn có chắc muốn xóa sạch các ô về Trống không?", "Có", "Hủy"))
                {
                    XoaSachGrid();
                }
            }
            if (GUILayout.Button("Điền đầy bằng cọ hiện tại"))
            {
                DienDayGrid(CharFromBrush(currentBrush));
            }
            if (GUILayout.Button("Xáo trộn ngẫu nhiên (Random)"))
            {
                XaoTronGrid();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawBrushButton(BrushColor brush, string label, Color color)
        {
            bool isSelected = (currentBrush == brush);
            GUI.backgroundColor = isSelected ? color : color * 0.7f;
            if (isSelected) GUI.contentColor = Color.white;

            if (GUILayout.Button((isSelected ? "► " : "") + label, GUILayout.Height(30)))
            {
                currentBrush = brush;
            }

            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
        }

        // ==========================================
        // 5. MA TRẬN BẢN ĐỒ LƯỚI (VISUAL GRID)
        // ==========================================
        private void DrawGridSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("4. Bản Đồ Nhân Vật (Click ô để vẽ / Chuột phải để xóa)", EditorStyles.boldLabel);

            gridScrollPos = EditorGUILayout.BeginScrollView(gridScrollPos, GUILayout.MinHeight(180), GUILayout.MaxHeight(350));

            float cellWidth = Mathf.Clamp((position.width - 80) / Mathf.Max(soCot, 1), 38f, 65f);
            float cellHeight = cellWidth * 0.85f;

            for (int r = 0; r < soHang; r++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                // Nhãn chỉ số hàng bên trái
                GUILayout.Label($"H{r + 1}", GUILayout.Width(28), GUILayout.Height(cellHeight));

                for (int c = 0; c < soCot; c++)
                {
                    char ch = banDoGrid[r, c];
                    Color cellColor = GetColorForChar(ch);
                    string label = GetDisplayLabelForChar(ch);

                    GUI.backgroundColor = cellColor;
                    GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
                    {
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    };
                    btnStyle.normal.textColor = (ch == '0' || ch == ' ') ? Color.gray : Color.white;

                    Rect btnRect = GUILayoutUtility.GetRect(new GUIContent(label), btnStyle, GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
                    
                    // Xử lý click chuột
                    Event e = Event.current;
                    if (btnRect.Contains(e.mousePosition))
                    {
                        if (e.type == EventType.MouseDown)
                        {
                            if (e.button == 0) // Chuột trái: Vẽ màu hiện tại
                            {
                                banDoGrid[r, c] = CharFromBrush(currentBrush);
                                GUI.changed = true;
                                e.Use();
                            }
                            else if (e.button == 1) // Chuột phải: Xóa về 0
                            {
                                banDoGrid[r, c] = '0';
                                GUI.changed = true;
                                e.Use();
                            }
                        }
                    }

                    GUI.Button(btnRect, label, btnStyle);
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        // ==========================================
        // 6. THỨ TỰ XE BUS (BUS SEQUENCE)
        // ==========================================
        private void DrawBusQueueSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"5. Danh Sách Xe Bus ({danhSachXe.Count} xe)", EditorStyles.boldLabel);

            if (GUILayout.Button("⚡ Tự động tính xe theo khách", GUILayout.Width(220)))
            {
                TuDongTaoXeTheoSoKhach();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            // Nút bấm thêm xe theo từng màu
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = colorRed;
            if (GUILayout.Button("+ Xe Đỏ", GUILayout.Height(26))) danhSachXe.Add(LoaiMau.Do);

            GUI.backgroundColor = colorBlue;
            if (GUILayout.Button("+ Xe Xanh", GUILayout.Height(26))) danhSachXe.Add(LoaiMau.Xanh);

            GUI.backgroundColor = colorYellow;
            if (GUILayout.Button("+ Xe Vàng", GUILayout.Height(26))) danhSachXe.Add(LoaiMau.Vang);

            GUI.backgroundColor = colorPurple;
            if (GUILayout.Button("+ Xe Tím", GUILayout.Height(26))) danhSachXe.Add(LoaiMau.Tim);

            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Xóa Tất Cả Xe", GUILayout.Height(26), GUILayout.Width(110))) danhSachXe.Clear();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            // Hiển thị danh sách các xe bus theo thứ tự
            for (int i = 0; i < danhSachXe.Count; i++)
            {
                LoaiMau mau = danhSachXe[i];
                Color c = GetColorForBus(mau);

                EditorGUILayout.BeginHorizontal("box");
                GUI.backgroundColor = c;
                GUILayout.Label($"Xe #{i + 1}: {mau} (3 chỗ)", EditorStyles.boldLabel, GUILayout.Width(150));
                GUI.backgroundColor = Color.white;

                // Nút đổi màu trực tiếp
                danhSachXe[i] = (LoaiMau)EditorGUILayout.EnumPopup(danhSachXe[i], GUILayout.Width(90));

                GUILayout.FlexibleSpace();

                // Nút di chuyển lên / xuống
                GUI.enabled = (i > 0);
                if (GUILayout.Button("▲", GUILayout.Width(28)))
                {
                    LoaiMau temp = danhSachXe[i];
                    danhSachXe[i] = danhSachXe[i - 1];
                    danhSachXe[i - 1] = temp;
                }

                GUI.enabled = (i < danhSachXe.Count - 1);
                if (GUILayout.Button("▼", GUILayout.Width(28)))
                {
                    LoaiMau temp = danhSachXe[i];
                    danhSachXe[i] = danhSachXe[i + 1];
                    danhSachXe[i + 1] = temp;
                }
                GUI.enabled = true;

                // Nút xóa xe
                GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("✕", GUILayout.Width(28)))
                {
                    danhSachXe.RemoveAt(i);
                    break;
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        // ==========================================
        // 7. KIỂM TRA TÍNH HỢP LỆ (LEVEL VALIDATOR)
        // ==========================================
        private void DrawValidationSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("6. Kiểm Tra Tính Cân Bằng & Hợp Lệ (Validator)", EditorStyles.boldLabel);

            // Thống kê số lượng khách từng màu
            int demDo = 0, demXanh = 0, demVang = 0, demTim = 0;
            for (int r = 0; r < soHang; r++)
            {
                for (int c = 0; c < soCot; c++)
                {
                    char ch = char.ToUpper(banDoGrid[r, c]);
                    if (ch == 'R') demDo++;
                    else if (ch == 'B') demXanh++;
                    else if (ch == 'Y') demVang++;
                    else if (ch == 'T' || ch == 'P') demTim++;
                }
            }

            // Thống kê số ghế chở của xe bus từng màu (mỗi xe 3 chỗ)
            int gheDo = 0, gheXanh = 0, gheVang = 0, gheTim = 0;
            foreach (var xe in danhSachXe)
            {
                switch (xe)
                {
                    case LoaiMau.Do: gheDo += 3; break;
                    case LoaiMau.Xanh: gheXanh += 3; break;
                    case LoaiMau.Vang: gheVang += 3; break;
                    case LoaiMau.Tim: gheTim += 3; break;
                }
            }

            EditorGUILayout.BeginHorizontal();
            DrawColorStat("Đỏ (R)", demDo, gheDo, colorRed);
            DrawColorStat("Xanh (B)", demXanh, gheXanh, colorBlue);
            DrawColorStat("Vàng (Y)", demVang, gheVang, colorYellow);
            DrawColorStat("Tím (T)", demTim, gheTim, colorPurple);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            // Đánh giá tổng quan
            int tongKhach = demDo + demXanh + demVang + demTim;
            int tongGhe = gheDo + gheXanh + gheVang + gheTim;

            if (tongKhach == 0)
            {
                EditorGUILayout.HelpBox("Bản đồ hiện đang trống chưa có khách nào!", MessageType.Warning);
            }
            else if (demDo == gheDo && demXanh == gheXanh && demVang == gheVang && demTim == gheTim)
            {
                EditorGUILayout.HelpBox($"Màn chơi hoàn hảo! Số ghế của tất cả các xe bus khớp chính xác 100% với số lượng hành khách ({tongKhach} khách / {tongGhe} ghế).", MessageType.Info);
            }
            else
            {
                string warning = "Cảnh báo không cân bằng:\n";
                if (demDo != gheDo) warning += $"- Khách Đỏ: {demDo} người, Xe Đỏ chở được {gheDo} ghế (chênh lệch: {gheDo - demDo})\n";
                if (demXanh != gheXanh) warning += $"- Khách Xanh: {demXanh} người, Xe Xanh chở được {gheXanh} ghế (chênh lệch: {gheXanh - demXanh})\n";
                if (demVang != gheVang) warning += $"- Khách Vàng: {demVang} người, Xe Vàng chở được {gheVang} ghế (chênh lệch: {gheVang - demVang})\n";
                if (demTim != gheTim) warning += $"- Khách Tím: {demTim} người, Xe Tím chở được {gheTim} ghế (chênh lệch: {gheTim - demTim})\n";
                warning += "Hành khách dư thừa sẽ không có xe đón (dẫn đến Thua), hoặc xe thừa sẽ bị kẹt lại bến!";
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawColorStat(string name, int passengerCount, int seatCount, Color color)
        {
            bool isMatch = (passengerCount == seatCount);
            Color boxCol = isMatch ? new Color(0.2f, 0.6f, 0.2f) : (passengerCount > seatCount ? new Color(0.7f, 0.2f, 0.2f) : new Color(0.6f, 0.4f, 0.1f));

            EditorGUILayout.BeginVertical("box");
            GUI.color = color;
            EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
            GUI.color = Color.white;

            EditorGUILayout.LabelField($"Khách: {passengerCount}");
            EditorGUILayout.LabelField($"Ghế xe: {seatCount}");

            GUI.color = boxCol;
            EditorGUILayout.LabelField(isMatch ? "✓ Khớp chuẩn" : (passengerCount > seatCount ? "✗ Thiếu ghế" : "! Thừa ghế"), EditorStyles.boldLabel);
            GUI.color = Color.white;

            EditorGUILayout.EndVertical();
        }

        // ==========================================
        // 8. CÁC NÚT HÀNH ĐỘNG (SAVE / TEST PLAY)
        // ==========================================
        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.25f, 0.85f, 0.35f);
            if (GUILayout.Button("💾 LƯU LEVEL (Save Level)", GUILayout.Height(40)))
            {
                LuuLevel();
            }

            GUI.backgroundColor = new Color(0.35f, 0.65f, 1f);
            if (GUILayout.Button("▶ LƯU & CHƠI THỬ NGAY (Play Test)", GUILayout.Height(40)))
            {
                LuuVaChoiThu();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        // ==========================================
        // CÁC HÀM XỬ LÝ LOGIC DỮ LIỆU
        // ==========================================
        private void KhoiTaoGridMoi(int hang, int cot)
        {
            soHang = Mathf.Clamp(hang, 1, 15);
            soCot = Mathf.Clamp(cot, 1, 15);
            banDoGrid = new char[soHang, soCot];

            for (int r = 0; r < soHang; r++)
            {
                for (int c = 0; c < soCot; c++)
                {
                    banDoGrid[r, c] = '0';
                }
            }
        }

        private void ThayDoiKichThuocGrid(int newHang, int newCot)
        {
            newHang = Mathf.Clamp(newHang, 1, 15);
            newCot = Mathf.Clamp(newCot, 1, 15);

            char[,] newGrid = new char[newHang, newCot];

            for (int r = 0; r < newHang; r++)
            {
                for (int c = 0; c < newCot; c++)
                {
                    if (r < soHang && c < soCot)
                    {
                        newGrid[r, c] = banDoGrid[r, c];
                    }
                    else
                    {
                        newGrid[r, c] = '0';
                    }
                }
            }

            soHang = newHang;
            soCot = newCot;
            banDoGrid = newGrid;
        }

        private void XoaSachGrid()
        {
            for (int r = 0; r < soHang; r++)
            {
                for (int c = 0; c < soCot; c++)
                {
                    banDoGrid[r, c] = '0';
                }
            }
        }

        private void DienDayGrid(char ch)
        {
            for (int r = 0; r < soHang; r++)
            {
                for (int c = 0; c < soCot; c++)
                {
                    banDoGrid[r, c] = ch;
                }
            }
        }

        private void XaoTronGrid()
        {
            char[] colors = { 'R', 'B', 'Y', 'T' };
            for (int r = 0; r < soHang; r++)
            {
                for (int c = 0; c < soCot; c++)
                {
                    // 80% có khách, 20% ô trống
                    if (Random.value < 0.2f)
                    {
                        banDoGrid[r, c] = '0';
                    }
                    else
                    {
                        banDoGrid[r, c] = colors[Random.Range(0, colors.Length)];
                    }
                }
            }
        }

        private void TuDongTaoXeTheoSoKhach()
        {
            int demDo = 0, demXanh = 0, demVang = 0, demTim = 0;
            for (int r = 0; r < soHang; r++)
            {
                for (int c = 0; c < soCot; c++)
                {
                    char ch = char.ToUpper(banDoGrid[r, c]);
                    if (ch == 'R') demDo++;
                    else if (ch == 'B') demXanh++;
                    else if (ch == 'Y') demVang++;
                    else if (ch == 'T' || ch == 'P') demTim++;
                }
            }

            danhSachXe.Clear();

            int soXeDo = Mathf.CeilToInt(demDo / 3f);
            int soXeXanh = Mathf.CeilToInt(demXanh / 3f);
            int soXeVang = Mathf.CeilToInt(demVang / 3f);
            int soXeTim = Mathf.CeilToInt(demTim / 3f);

            for (int i = 0; i < soXeDo; i++) danhSachXe.Add(LoaiMau.Do);
            for (int i = 0; i < soXeXanh; i++) danhSachXe.Add(LoaiMau.Xanh);
            for (int i = 0; i < soXeVang; i++) danhSachXe.Add(LoaiMau.Vang);
            for (int i = 0; i < soXeTim; i++) danhSachXe.Add(LoaiMau.Tim);

            // Xáo trộn nhẹ thứ tự xe để tạo tính giải đố
            for (int i = 0; i < danhSachXe.Count; i++)
            {
                int rnd = Random.Range(i, danhSachXe.Count);
                LoaiMau temp = danhSachXe[i];
                danhSachXe[i] = danhSachXe[rnd];
                danhSachXe[rnd] = temp;
            }

            Debug.Log($"[LevelEditor] Đã tự động tạo {danhSachXe.Count} xe bus khớp với tổng số hành khách!");
        }

        private void LoadFromLevelData(LevelData data)
        {
            if (data == null) return;

            soSlotHangCho = data.soSlotHangCho > 0 ? data.soSlotHangCho : 5;

            // Load danh sách xe
            danhSachXe.Clear();
            if (data.danhSachXeBus != null)
            {
                danhSachXe.AddRange(data.danhSachXeBus);
            }

            // Load bản đồ lưới
            if (data.banDoLuoii != null && data.banDoLuoii.Length > 0)
            {
                int hang = data.banDoLuoii.Length;
                int maxCot = 0;

                foreach (string row in data.banDoLuoii)
                {
                    if (string.IsNullOrEmpty(row)) continue;
                    int count = 0;
                    foreach (char c in row)
                    {
                        if (c != ' ') count++;
                    }
                    if (count > maxCot) maxCot = count;
                }

                KhoiTaoGridMoi(hang, Mathf.Max(maxCot, 1));

                for (int r = 0; r < hang; r++)
                {
                    string row = data.banDoLuoii[r];
                    if (string.IsNullOrEmpty(row)) continue;

                    int colIdx = 0;
                    for (int i = 0; i < row.Length; i++)
                    {
                        char c = row[i];
                        if (c == ' ') continue;

                        if (colIdx < soCot)
                        {
                            banDoGrid[r, colIdx] = c;
                            colIdx++;
                        }
                    }
                }
            }
            else
            {
                KhoiTaoGridMoi(4, 4);
            }

            Repaint();
        }

        private void LuuLevel()
        {
            if (currentLevelData == null)
            {
                TaoLevelMoi();
                if (currentLevelData == null) return;
            }

            Undo.RecordObject(currentLevelData, "Save Bus Jam Level");

            currentLevelData.soSlotHangCho = soSlotHangCho;
            currentLevelData.danhSachXeBus = danhSachXe.ToArray();

            // Chuyển mảng char[,] thành string[]
            List<string> rows = new List<string>();
            for (int r = 0; r < soHang; r++)
            {
                List<string> cols = new List<string>();
                for (int c = 0; c < soCot; c++)
                {
                    cols.Add(banDoGrid[r, c].ToString());
                }
                rows.Add(string.Join(" ", cols));
            }
            currentLevelData.banDoLuoii = rows.ToArray();

            EditorUtility.SetDirty(currentLevelData);
            AssetDatabase.SaveAssets();

            Debug.Log($"[LevelEditor] Đã lưu thành công dữ liệu cho {currentLevelData.name}!");
            EditorUtility.DisplayDialog("Thành công", $"Đã lưu thành công {currentLevelData.name}!", "OK");
        }

        private void TaoLevelMoi()
        {
            string folderPath = "Assets/_Data";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "_Data");
            }

            // Tìm số thứ tự Level tiếp theo
            int index = 1;
            while (File.Exists($"{folderPath}/Level_{index}.asset"))
            {
                index++;
            }

            string assetPath = $"{folderPath}/Level_{index}.asset";
            LevelData newLevel = CreateInstance<LevelData>();
            newLevel.soSlotHangCho = soSlotHangCho;

            AssetDatabase.CreateAsset(newLevel, assetPath);
            AssetDatabase.SaveAssets();

            currentLevelData = newLevel;
            LuuLevel();

            Selection.activeObject = newLevel;
            Debug.Log($"[LevelEditor] Đã tạo thành công Level mới: {assetPath}");
        }

        private void LuuVaChoiThu()
        {
            LuuLevel();

            if (currentLevelData == null) return;

            // Tìm index của level trong danh sách LevelManager
            LevelManager levelManager = Object.FindAnyObjectByType<LevelManager>();
            if (levelManager != null && levelManager.danhSachLevel != null)
            {
                int foundIndex = -1;
                for (int i = 0; i < levelManager.danhSachLevel.Length; i++)
                {
                    if (levelManager.danhSachLevel[i] == currentLevelData)
                    {
                        foundIndex = i;
                        break;
                    }
                }

                if (foundIndex != -1)
                {
                    LevelManager.levelIndex = foundIndex;
                    PlayerPrefs.SetInt("CurrentLevel", foundIndex);
                    PlayerPrefs.Save();
                }
            }

            // Nếu chưa ở trong Play Mode thì bật Play Mode
            if (!EditorApplication.isPlaying)
            {
                string activeScene = EditorSceneManager.GetActiveScene().name;
                if (!activeScene.ToLower().Contains("gameplay"))
                {
                    // Chuyển sang scene MainGameplay trước khi Play
                    string gameplayPath = "Assets/Scenes/MainGameplay.unity";
                    if (File.Exists(gameplayPath))
                    {
                        EditorSceneManager.OpenScene(gameplayPath);
                    }
                }

                EditorApplication.isPlaying = true;
            }
        }

        // ==========================================
        // CÁC HÀM TIỆN ÍCH MÀU SẮC & KÝ TỰ
        // ==========================================
        private char CharFromBrush(BrushColor brush)
        {
            switch (brush)
            {
                case BrushColor.Do: return 'R';
                case BrushColor.Xanh: return 'B';
                case BrushColor.Vang: return 'Y';
                case BrushColor.Tim: return 'T';
                default: return '0';
            }
        }

        private Color GetColorForChar(char c)
        {
            switch (char.ToUpper(c))
            {
                case 'R': return colorRed;
                case 'B': return colorBlue;
                case 'Y': return colorYellow;
                case 'T':
                case 'P': return colorPurple;
                default: return colorEmpty;
            }
        }

        private string GetDisplayLabelForChar(char c)
        {
            switch (char.ToUpper(c))
            {
                case 'R': return "R";
                case 'B': return "B";
                case 'Y': return "Y";
                case 'T':
                case 'P': return "T";
                default: return "·";
            }
        }

        private Color GetColorForBus(LoaiMau mau)
        {
            switch (mau)
            {
                case LoaiMau.Do: return colorRed;
                case LoaiMau.Xanh: return colorBlue;
                case LoaiMau.Vang: return colorYellow;
                case LoaiMau.Tim: return colorPurple;
                default: return Color.white;
            }
        }
    }
}
#endif

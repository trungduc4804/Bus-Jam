# 🚌 Bus Jam 3D - Mobile Sorting Puzzle Game

[![Unity](https://img.shields.io/badge/Unity-6000.5.5f1%20(Unity%206)-black?logo=unity&logoColor=white)](https://unity.com/)
[![Render Pipeline](https://img.shields.io/badge/Render%20Pipeline-Universal%20Render%20Pipeline%20(URP)-blue)](https://unity.com/srp/universal-render-pipeline)
[![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20iOS%20%7C%20WebGL-brightgreen)](#)
[![License](https://img.shields.io/badge/License-MIT-orange)](#)

> **Bus Jam 3D** là tựa game giải đố 3D Hyper-Casual/Puzzle được phát triển bằng **Unity 6 (URP)**. Người chơi điều hướng hành khách trên bàn cờ di chuyển ra các ô chờ và lên những chiếc xe bus cùng màu để giải cứu bến xe khỏi ùn tắc giao thông.

---

## 🎮 Demo Trải Nghiệm Trực Tiếp
* **Chơi trên Web (Itch.io):** [Link chơi thử WebGL](#) *(Cập nhật link của bạn tại đây)*
* **Video Gameplay Preview:** [Xem video trên YouTube/Drive](#) *(Cập nhật link video)*

---

## ✨ Điểm Nổi Bật Kỹ Thuật (Technical Highlights)

### 1. 🏗️ Kiến Trúc Command Design Pattern (Undo System)
* Áp dụng **Command Pattern** (`ICommand`, `MovePassengerCommand`, `UndoManager`) quản lý ngăn xếp lịch sử thao tác.
* Hỗ trợ hoàn tác vị trí 3D với hiệu ứng nhảy vồng mượt mà (`DOJump`), khôi phục trạng thái Collider và slot hàng chờ.
* **Tự động vô hiệu hóa lệnh cũ:** Khi hành khách đã bước lên xe bus thành công, command tương ứng sẽ tự động bị loại bỏ khỏi stack, ngăn ngừa hoàn tác sai logic.

### 2. 🧮 Thuật Toán Cân Bằng Màu Thông Minh (Smart Deficit Balancing Pool)
* Khắc phục triệt để bài toán ngẫu nhiên dẫn đến kẹt màn (Unsolvable Levels).
* Thuật toán phân tích tổng số ghế của từng màu xe bus và số lượng khách thường trên sân:
  $$\text{Deficit}(\text{Màu}) = \text{Tổng ghế xe}(\text{Màu}) - \text{Khách thường}(\text{Màu})$$
* Tự động bù trừ chính xác số lượng màu còn thiếu vào **Bể màu Khách Ẩn (Mystery Pool)**, đảm bảo 100% mọi màn chơi ngẫu nhiên luôn có lời giải hoàn hảo.

### 3. 🛠️ Công Cụ Level Editor Trực Quan (Custom Unity Editor Tooling)
* Phát triển riêng cửa sổ **`BusJamLevelEditorWindow`** trên Unity Editor cho Game Designer:
  * Vẽ ma trận nhân vật trực quan bằng thao tác click chuột.
  * Tùy chỉnh danh sách màu xe bus xuất bến theo thứ tự hàng đợi.
  * Tùy chỉnh số lượng ô slot hàng chờ (từ 3 đến 7 ô).
  * Xuất bản (Export) và Nhập (Import) trực tiếp với `LevelData` ScriptableObject chỉ trong 1 click.

### 4. 📐 Hệ Thống Auto-Fit Camera Thích Ứng Mọi Màn Hình
* Tự động tính toán khung nhìn dựa trên số hàng, số cột và tỉ lệ màn hình thực tế (`Screen.width / Screen.height`).
* Hỗ trợ hoàn hảo mọi dòng máy từ màn hình chuẩn 16:9 đến các dòng điện thoại tai thỏ dài (iPhone 19.5:9, Android 20:9).
* Tự động lùi vị trí Camera và mở rộng góc nhìn (`FOV`) một cách cân đối, không làm biến dạng góc rộng (Fish-eye) khi chơi màn lớn ($7 \times 7$).
* **Khử xung đột rung Camera:** Đồng bộ hóa tọa độ gốc mục tiêu giữa `LevelManager` và `VFXManager`, đảm bảo hiệu ứng `CameraShake` không bị giật về tọa độ mặc định.

### 5. 🛡️ Cơ Chế Phát Hiện Va Chạm Kép Chống Lách Rào (Double-Check Obstacle Detection)
* **Tầng 1 (Toạ độ hình học):** Quét trực tiếp danh sách toạ độ khách trên sân. Bất kỳ ai đứng phía trước ($Z > 0.35m$) trên cùng làn ($|X| < 0.85m$) đều lập tức bị chặn. Chống triệt để 100% bug spam click làm lệch góc xoay.
* **Tầng 2 (Vật lý quét xa):** `Physics.SphereCastNonAlloc` cố định theo trục `Vector3.forward` với tầm xa 15m, loại bỏ hoàn toàn rác bộ nhớ (Zero GC Allocations).

### 6. 🌈 Booster Xe Cầu Vồng (Rainbow Bus)
* Biến xe hiện tại thành xe đa năng chở bất kỳ màu khách nào.
* Ứng dụng **`MaterialPropertyBlock`** để đổi màu cầu vồng liên tục trong `Update()` mà không tạo instance Material mới, tránh rò rỉ bộ nhớ (Memory Leak).

---

## 🎨 Game Feel & Juice

* **Particle VFX System:**
  * 🎊 Pháo giấy Confetti bắn tung tóe khi vượt qua màn chơi.
  * 💨 Khói xả đuôi xe khi rồ ga tăng tốc khởi hành.
  * 🛑 Khói phanh dừng bánh xe khi tiến vào bến đỗ.
  * ⭐ Chùm sao vàng nổ trên nóc xe khi chở đủ khách.
  * 💥 Hạt nảy màu Pop khi hé lộ khách ẩn hoặc lên xe.
* **Camera Shake & Wobble:** Rung lắc nhẹ khi bấm vào người bị chặn, xe phanh hoặc khi hàng chờ bị đầy.
* **Âm thanh (Audio System):** BGM nền thư giãn, tiếng chạm Pop giòn tan, còi xe bíp bíp, tiếng phanh xì hơi và chuông báo chiến thắng.

---

## 📁 Cấu Trúc Thư Mục Dự Án (Project Structure)

```text
Assets/
├── _Data/                     # ScriptableObject dữ liệu màn chơi (Level_1 đến Level_10)
├── _Prefabs/
│   ├── NV/                    # Prefab nhân vật các màu (Đỏ, Xanh, Vàng, Tím)
│   └── Xe/                    # Prefab xe bus các màu
├── _Scripts/
│   ├── Boosters/              # Hệ thống Booster (Rainbow Bus, UI binding)
│   │   ├── BoosterManager.cs
│   │   └── RainbowBusButtonUI.cs
│   ├── Commands/              # Design Pattern: Command Pattern cho Undo
│   │   ├── ICommand.cs
│   │   ├── MovePassengerCommand.cs
│   │   ├── UndoManager.cs
│   │   └── UndoButtonUI.cs
│   ├── Editor/                # Công cụ Level Editor nội bộ
│   │   ├── BusJamLevelEditorWindow.cs
│   │   ├── LevelDataEditor.cs
│   │   └── GameSetupMenu.cs
│   ├── VFX/                   # Hệ thống Particle VFX, Game Feel & Camera Shake
│   │   └── VFXManager.cs
│   ├── AudioManager.cs        # Quản lý nhạc nền và hiệu ứng âm thanh
│   ├── BenXe.cs               # Logic điều phối bến xe bus FIFO
│   ├── GameManager.cs         # Quản lý vòng đời màn chơi (Win / Lose)
│   ├── LevelData.cs           # ScriptableObject schema cho Level
│   ├── LevelManager.cs        # Sinh bản đồ, Auto-Fit Camera, thuật toán Pool màu
│   ├── MainMenuManager.cs     # Quản lý màn hình Menu, chọn màn, lưu tiến trình
│   ├── NhanVat.cs             # FSM di chuyển, kiểm tra đường đi, khách ẩn
│   ├── TouchManager.cs        # Raycast tương tác chạm, quản lý ô hàng chờ
│   └── XeBus.cs               # Trạng thái di chuyển, đón khách, xe cầu vồng
└── Scenes/
    ├── MenuGame.unity         # Scene màn hình chính & chọn màn
    └── MainGameplay.unity     # Scene trải nghiệm chơi game
```

---

## 🚀 Hướng Dẫn Cài Đặt & Chạy Dự Án

### Yêu Cầu Môi Trường:
* **Unity Version:** Unity 6 (`6000.5.5f1`) hoặc các phiên bản Unity 6 LTS mới hơn.
* **Render Pipeline:** Universal Render Pipeline (URP).
* **Package phụ thuộc:** `com.unity.ugui`, `DOTween (Demigiant)`.

### Các Bước Thực Hiện:
1. **Clone repository về máy:**
   ```bash
   git clone https://github.com/your-username/bus-jam-3d.git
   ```
2. **Mở dự án:**
   * Mở **Unity Hub** $\rightarrow$ Bấm **Add** $\rightarrow$ Chọn thư mục dự án vừa tải về.
   * Chọn phiên bản Unity `6000.5.5f1`.
3. **Mở Scene trải nghiệm:**
   * Điều hướng đến thư mục `Assets/Scenes/`.
   * Mở scene **`MenuGame.unity`** và bấm nút **Play** để bắt đầu chơi từ giao diện chính.

---

## 🛠️ Hướng Dẫn Sử Dụng Level Editor

1. Trên thanh công cụ Unity, vào menu: **Tools $\rightarrow$ Bus Jam $\rightarrow$ Level Editor Window**.
2. Chọn màn chơi muốn chỉnh sửa hoặc bấm **Tạo Level Mới**.
3. Sử dụng bảng màu (Đỏ, Xanh, Vàng, Tím, Khách Ẩn, Xóa) để vẽ trực tiếp ma trận khách trên lưới bàn chơi.
4. Cài đặt số lượng ô slot hàng chờ và thứ tự các chuyến xe bus.
5. Bấm **Lưu Level** để lưu trực tiếp vào ScriptableObject.

---

## 👤 Tác Giả & Liên Hệ

* **Lập trình viên:** [Tên của bạn]
* **Email:** [Email của bạn]
* **LinkedIn:** [Link LinkedIn của bạn]
* **Portfolio:** [Link Portfolio hoặc Itch.io]

---
*Dự án được xây dựng với mục tiêu thể hiện năng lực lập trình Game Gameplay, Design Pattern, Custom Tooling và Tối ưu hóa hiệu năng trên nền tảng Unity.*

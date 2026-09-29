using UnityEngine;

namespace BusJam.Commands
{
    /// <summary>
    /// Command đại diện cho hành động di chuyển nhân vật từ sân đỗ vào ô chờ (Slot)
    /// </summary>
    public class MovePassengerCommand : ICommand
    {
        public NhanVat Passenger { get; private set; }
        public Vector3 OriginalPosition { get; private set; }
        public Quaternion OriginalRotation { get; private set; }
        public int SlotIndex { get; private set; }
        public Vector3 SlotPosition { get; private set; }

        private bool isInvalidated = false;

        public MovePassengerCommand(NhanVat passenger, Vector3 originalPos, Quaternion originalRot, int slotIndex, Vector3 slotPos)
        {
            Passenger = passenger;
            OriginalPosition = originalPos;
            OriginalRotation = originalRot;
            SlotIndex = slotIndex;
            SlotPosition = slotPos;
        }

        public bool IsValid
        {
            get
            {
                if (isInvalidated) return false;
                if (Passenger == null || Passenger.gameObject == null) return false;
                // Nếu khách đang di chuyển lên xe hoặc không còn ở slot này nữa thì không được undo
                if (Passenger.DangDiChuyenToiXe) return false;
                if (Passenger.slotIndexHienTai != SlotIndex) return false;
                return true;
            }
        }

        public void Invalidate()
        {
            isInvalidated = true;
        }

        public void Execute()
        {
            if (Passenger != null)
            {
                Passenger.DiChuyenToi(SlotPosition);
            }
        }

        public void Undo()
        {
            if (!IsValid) return;

            // 1. Giải phóng ô trong TouchManager
            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.GiaiPhongSlot(SlotIndex);
            }

            // 2. Xóa khỏi danh sách chờ trong BenXe nếu có
            if (BenXe.Instance != null && BenXe.Instance.danhSachKhachDangCho.Contains(Passenger))
            {
                BenXe.Instance.danhSachKhachDangCho.Remove(Passenger);
            }

            // 3. Ra lệnh cho nhân vật nhảy quay về vị trí ban đầu
            Passenger.QuayVeViTriCu(OriginalPosition, OriginalRotation);
        }
    }
}

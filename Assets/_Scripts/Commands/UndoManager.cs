using System;
using System.Collections.Generic;
using UnityEngine;

namespace BusJam.Commands
{
    /// <summary>
    /// Quản lý ngăn xếp (Stack) các Command để thực thi cơ chế Hoàn tác (Undo)
    /// </summary>
    public class UndoManager : MonoBehaviour
    {
        private static UndoManager instance;
        public static UndoManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = UnityEngine.Object.FindAnyObjectByType<UndoManager>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject("UndoManager");
                        instance = obj.AddComponent<UndoManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Cài đặt Hoàn tác (Undo)")]
        [Tooltip("Cho phép hoàn tác không giới hạn (true) hoặc giới hạn số lượt mỗi màn (false)")]
        public bool voHanLuotUndo = false;

        [Tooltip("Số lượt Undo tối đa trong một màn chơi nếu không bật vô hạn")]
        public int soLuotUndoToiDa = 3;

        private int soLuotUndoConLai;
        private Stack<ICommand> commandStack = new Stack<ICommand>();

        /// <summary>
        /// Event thông báo thay đổi trạng thái Undo:
        /// bool canUndo: có thể bấm nút Undo không
        /// int remainingCount: số lượt còn lại (-1 nếu vô hạn)
        /// </summary>
        public event Action<bool, int> OnUndoStateChanged;

        public int SoLuotUndoConLai => voHanLuotUndo ? -1 : soLuotUndoConLai;

        public bool CanUndo
        {
            get
            {
                if (!voHanLuotUndo && soLuotUndoConLai <= 0) return false;
                
                // Kiểm tra xem trên đỉnh stack có ít nhất 1 command hợp lệ không
                DondepCommandKhongHopLe();
                return commandStack.Count > 0;
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

            ResetLuotUndo();
        }

        public void ResetLuotUndo()
        {
            soLuotUndoConLai = soLuotUndoToiDa;
            commandStack.Clear();
            NotifyStateChanged();
        }

        public void ClearHistory()
        {
            commandStack.Clear();
            NotifyStateChanged();
        }

        public void RegisterCommand(ICommand cmd)
        {
            if (cmd == null) return;
            commandStack.Push(cmd);
            NotifyStateChanged();
        }

        public bool Undo()
        {
            DondepCommandKhongHopLe();

            if (!CanUndo)
            {
                Debug.Log("[UndoManager] Không thể hoàn tác (Hết lượt hoặc không có thao tác hợp lệ)!");
                return false;
            }

            ICommand cmd = commandStack.Pop();
            cmd.Undo();

            if (!voHanLuotUndo)
            {
                soLuotUndoConLai--;
            }

            // Phát âm thanh hoàn tác
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUndo();
            }

            NotifyStateChanged();
            return true;
        }

        /// <summary>
        /// Khi một khách bắt đầu lên xe bus thành công, vô hiệu hóa các lệnh liên quan
        /// </summary>
        public void InvalidatePassenger(NhanVat passenger)
        {
            if (passenger == null) return;
            
            foreach (var cmd in commandStack)
            {
                if (cmd is MovePassengerCommand moveCmd && moveCmd.Passenger == passenger)
                {
                    moveCmd.Invalidate();
                }
            }

            DondepCommandKhongHopLe();
            NotifyStateChanged();
        }

        private void DondepCommandKhongHopLe()
        {
            while (commandStack.Count > 0 && !commandStack.Peek().IsValid)
            {
                commandStack.Pop();
            }
        }

        public void NotifyStateChanged()
        {
            OnUndoStateChanged?.Invoke(CanUndo, SoLuotUndoConLai);
        }
    }
}

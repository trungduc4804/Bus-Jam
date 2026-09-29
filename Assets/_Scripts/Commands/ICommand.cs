namespace BusJam.Commands
{
    /// <summary>
    /// Giao diện chuẩn cho Command Pattern phục vụ cơ chế Undo / Redo
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Thực thi lệnh
        /// </summary>
        void Execute();

        /// <summary>
        /// Hoàn tác lệnh (quay về trạng thái trước đó)
        /// </summary>
        void Undo();

        /// <summary>
        /// Kiểm tra xem lệnh này còn hợp lệ để hoàn tác hay không
        /// (Ví dụ: khách đã chui vào xe bus thì lệnh không còn hợp lệ)
        /// </summary>
        bool IsValid { get; }
    }
}

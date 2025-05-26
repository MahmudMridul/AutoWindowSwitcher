namespace AutoWindowSwitcher.Models
{
    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string? Title { get; set; }
        public string? ProcessName { get; set; }
    }
}

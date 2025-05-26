using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MouseWindowController
{
    class Program
    {
        // Windows API imports for mouse control
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        // Windows API imports for window management
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // Constants
        const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        const uint MOUSEEVENTF_LEFTUP = 0x04;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        const uint MOUSEEVENTF_RIGHTUP = 0x10;
        const int SW_RESTORE = 9;
        const int SW_MAXIMIZE = 3;

        // Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        // Delegate for EnumWindows
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        static void Main(string[] args)
        {
            Console.WriteLine("=== Mouse and Window Controller ===");
            Console.WriteLine();

            while (true)
            {
                ShowMenu();
                string choice = Console.ReadLine();

                switch (choice?.ToLower())
                {
                    case "1":
                        MoveMouse();
                        break;
                    case "2":
                        ClickMouse();
                        break;
                    case "3":
                        ShowCurrentMousePosition();
                        break;
                    case "4":
                        ListRunningWindows();
                        break;
                    case "5":
                        SwitchToWindow();
                        break;
                    case "6":
                        DemoMouseMovement();
                        break;
                    case "7":
                        DemoWindowSwitching();
                        break;
                    case "8":
                        TimedWindowSwitching();
                        break;
                    case "q":
                    case "quit":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Move Mouse to Coordinates");
            Console.WriteLine("2. Click Mouse at Current Position");
            Console.WriteLine("3. Show Current Mouse Position");
            Console.WriteLine("4. List Running Windows");
            Console.WriteLine("5. Switch to Window by Name");
            Console.WriteLine("6. Demo: Automated Mouse Movement");
            Console.WriteLine("7. Demo: Cycle Through Windows");
            Console.WriteLine("8. Timed Window Switching (Custom Duration)");
            Console.WriteLine("Q. Quit");
            Console.Write("\nEnter your choice: ");
        }

        static void MoveMouse()
        {
            Console.Write("Enter X coordinate: ");
            if (int.TryParse(Console.ReadLine(), out int x))
            {
                Console.Write("Enter Y coordinate: ");
                if (int.TryParse(Console.ReadLine(), out int y))
                {
                    SetCursorPos(x, y);
                    Console.WriteLine($"Mouse moved to ({x}, {y})");
                }
                else
                {
                    Console.WriteLine("Invalid Y coordinate.");
                }
            }
            else
            {
                Console.WriteLine("Invalid X coordinate.");
            }
        }

        static void ClickMouse()
        {
            GetCursorPos(out POINT currentPos);
            Console.WriteLine($"Clicking at current position: ({currentPos.X}, {currentPos.Y})");

            // Simulate left mouse button down and up
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

            Console.WriteLine("Mouse clicked!");
        }

        static void ShowCurrentMousePosition()
        {
            GetCursorPos(out POINT currentPos);
            Console.WriteLine($"Current mouse position: ({currentPos.X}, {currentPos.Y})");
        }

        static void ListRunningWindows()
        {
            List<WindowInfo> windows = new List<WindowInfo>();

            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    int length = GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        StringBuilder sb = new StringBuilder(length + 1);
                        GetWindowText(hWnd, sb, sb.Capacity);
                        string title = sb.ToString();

                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            GetWindowThreadProcessId(hWnd, out uint processId);
                            try
                            {
                                Process process = Process.GetProcessById((int)processId);
                                windows.Add(new WindowInfo
                                {
                                    Handle = hWnd,
                                    Title = title,
                                    ProcessName = process.ProcessName
                                });
                            }
                            catch
                            {
                                // Process might have closed
                            }
                        }
                    }
                }
                return true;
            }, IntPtr.Zero);

            Console.WriteLine("Running Windows:");
            Console.WriteLine("================");
            for (int i = 0; i < windows.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {windows[i].Title} ({windows[i].ProcessName})");
            }
        }

        static void SwitchToWindow()
        {
            Console.Write("Enter part of the window title to switch to: ");
            string searchTitle = Console.ReadLine()?.ToLower();

            if (string.IsNullOrWhiteSpace(searchTitle))
            {
                Console.WriteLine("No search term provided.");
                return;
            }

            bool found = false;
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    int length = GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        StringBuilder sb = new StringBuilder(length + 1);
                        GetWindowText(hWnd, sb, sb.Capacity);
                        string title = sb.ToString();

                        if (title.ToLower().Contains(searchTitle))
                        {
                            ShowWindow(hWnd, SW_RESTORE);
                            SetForegroundWindow(hWnd);
                            Thread.Sleep(100); // Small delay to ensure window is restored
                            ShowWindow(hWnd, SW_MAXIMIZE);
                            Console.WriteLine($"Switched to window: {title} (Maximized)");
                            found = true;
                            return false; // Stop enumeration
                        }
                    }
                }
                return true;
            }, IntPtr.Zero);

            if (!found)
            {
                Console.WriteLine($"No window found containing '{searchTitle}'");
            }
        }

        static void DemoMouseMovement()
        {
            Console.WriteLine("Starting automated mouse movement demo...");
            Console.WriteLine("Press any key to stop the demo.");

            Random rand = new Random();
            int screenWidth = 1920; // Adjust based on your screen
            int screenHeight = 1080; // Adjust based on your screen

            DateTime startTime = DateTime.Now;
            while (!Console.KeyAvailable && (DateTime.Now - startTime).TotalSeconds < 10)
            {
                int x = rand.Next(100, screenWidth - 100);
                int y = rand.Next(100, screenHeight - 100);

                SetCursorPos(x, y);
                Console.WriteLine($"Moved to ({x}, {y})");
                Thread.Sleep(1000);
            }

            if (Console.KeyAvailable)
            {
                Console.ReadKey(true); // Clear the key press
            }

            Console.WriteLine("Demo completed.");
        }

        static void DemoWindowSwitching()
        {
            Console.WriteLine("Starting window switching demo...");

            List<WindowInfo> windows = new List<WindowInfo>();

            // Collect visible windows
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    int length = GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        StringBuilder sb = new StringBuilder(length + 1);
                        GetWindowText(hWnd, sb, sb.Capacity);
                        string title = sb.ToString();

                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            windows.Add(new WindowInfo { Handle = hWnd, Title = title });
                        }
                    }
                }
                return true;
            }, IntPtr.Zero);

            if (windows.Count < 2)
            {
                Console.WriteLine("Need at least 2 windows to demonstrate switching.");
                return;
            }

            Console.WriteLine($"Found {windows.Count} windows. Cycling through first 5...");

            for (int i = 0; i < Math.Min(5, windows.Count); i++)
            {
                Console.WriteLine($"Switching to: {windows[i].Title}");
                ShowWindow(windows[i].Handle, SW_RESTORE);
                SetForegroundWindow(windows[i].Handle);
                Thread.Sleep(100); // Small delay to ensure window is restored
                ShowWindow(windows[i].Handle, SW_MAXIMIZE);
                Thread.Sleep(2000);
            }

            Console.WriteLine("Window switching demo completed.");
        }

        static void TimedWindowSwitching()
        {
            Console.Write("Enter total duration in minutes: ");
            if (!int.TryParse(Console.ReadLine(), out int totalMinutes) || totalMinutes <= 0)
            {
                Console.WriteLine("Invalid duration. Please enter a positive number.");
                return;
            }

            Console.WriteLine("Collecting available windows...");

            List<WindowInfo> windows = new List<WindowInfo>();

            // Collect visible windows (excluding system windows)
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    int length = GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        StringBuilder sb = new StringBuilder(length + 1);
                        GetWindowText(hWnd, sb, sb.Capacity);
                        string title = sb.ToString();

                        // Filter out system windows and empty titles
                        if (!string.IsNullOrWhiteSpace(title) &&
                            !title.Equals("Program Manager") &&
                            !title.StartsWith("Windows Security") &&
                            title.Length > 2)
                        {
                            GetWindowThreadProcessId(hWnd, out uint processId);
                            try
                            {
                                Process process = Process.GetProcessById((int)processId);
                                windows.Add(new WindowInfo
                                {
                                    Handle = hWnd,
                                    Title = title,
                                    ProcessName = process.ProcessName
                                });
                            }
                            catch
                            {
                                // Process might have closed, skip it
                            }
                        }
                    }
                }
                return true;
            }, IntPtr.Zero);

            if (windows.Count == 0)
            {
                Console.WriteLine("No suitable windows found for switching.");
                return;
            }

            Console.WriteLine($"\nFound {windows.Count} windows:");
            for (int i = 0; i < windows.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {windows[i].Title}");
            }

            Console.WriteLine($"\nStarting timed window switching for {totalMinutes} minutes...");
            Console.WriteLine("Each window will be active for 5 minutes.");
            Console.WriteLine("Press 'Q' at any time to stop the process.\n");

            DateTime endTime = DateTime.Now.AddMinutes(totalMinutes);
            int currentWindowIndex = 0;
            DateTime nextSwitchTime = DateTime.Now.AddMinutes(5);

            while (DateTime.Now < endTime)
            {
                // Switch to current window
                WindowInfo currentWindow = windows[currentWindowIndex];
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Switching to: {currentWindow.Title}");

                try
                {
                    ShowWindow(currentWindow.Handle, SW_RESTORE);
                    SetForegroundWindow(currentWindow.Handle);
                    Thread.Sleep(200); // Small delay to ensure window is restored and focused
                    ShowWindow(currentWindow.Handle, SW_MAXIMIZE);
                    Console.WriteLine($"Window maximized: {currentWindow.Title}");
                }
                catch
                {
                    Console.WriteLine($"Failed to switch to window: {currentWindow.Title}");
                }

                // Wait for 5 minutes or until it's time to switch, checking for user input
                while (DateTime.Now < nextSwitchTime && DateTime.Now < endTime)
                {
                    // Check for user input to quit
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Q)
                        {
                            Console.WriteLine("\nWindow switching stopped by user.");
                            return;
                        }
                    }

                    // Show remaining time every 30 seconds
                    TimeSpan remainingInWindow = nextSwitchTime - DateTime.Now;
                    TimeSpan remainingTotal = endTime - DateTime.Now;

                    if (remainingInWindow.TotalSeconds > 0 && remainingTotal.TotalSeconds > 0)
                    {
                        Console.Write($"\r[{DateTime.Now:HH:mm:ss}] Window: {remainingInWindow.Minutes:D2}:{remainingInWindow.Seconds:D2} | Total: {remainingTotal.Minutes:D2}:{remainingTotal.Seconds:D2}");
                    }

                    Thread.Sleep(1000); // Check every second
                }

                Console.WriteLine(); // New line after the timer display

                // Move to next window
                currentWindowIndex = (currentWindowIndex + 1) % windows.Count;
                nextSwitchTime = DateTime.Now.AddMinutes(5);

                // Re-check if windows still exist and update the list if needed
                if (currentWindowIndex == 0) // Full cycle completed, refresh window list
                {
                    Console.WriteLine("Refreshing window list...");
                    List<WindowInfo> updatedWindows = new List<WindowInfo>();

                    foreach (var window in windows)
                    {
                        try
                        {
                            if (IsWindowVisible(window.Handle))
                            {
                                updatedWindows.Add(window);
                            }
                        }
                        catch
                        {
                            // Window no longer exists, remove it
                        }
                    }

                    windows = updatedWindows;
                    if (windows.Count == 0)
                    {
                        Console.WriteLine("No more windows available. Stopping.");
                        return;
                    }
                }
            }

            Console.WriteLine($"\nTimed window switching completed after {totalMinutes} minutes.");
        }
    }

    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; }
        public string ProcessName { get; set; }
    }
}
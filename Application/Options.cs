
using AutoWindowSwitcher.Models;
using AutoWindowSwitcher.WindowUtils;
using AutoWindowSwitcher.Utilities;
using System.Text;
using System.Diagnostics;

namespace AutoWindowSwitcher.Application
{
    public static class Options
    {
        public static void MoveMouse()
        {
            Console.Write("Enter X coordinate: ");
            if (int.TryParse(Console.ReadLine(), out int x))
            {
                Console.Write("Enter Y coordinate: ");
                if (int.TryParse(Console.ReadLine(), out int y))
                {
                    WindowApis.SetCursorPos(x, y);
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

        public static void ClickMouse()
        {
            WindowApis.GetCursorPos(out POINT currentPos);
            Console.WriteLine($"Clicking at current position: ({currentPos.X}, {currentPos.Y})");

            WindowApis.mouse_event(Constants.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(50);
            WindowApis.mouse_event(Constants.MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

            Console.WriteLine("Mouse clicked!");
        }

        public static void ShowCurrentMousePosition()
        {
            WindowApis.GetCursorPos(out POINT currentPos);
            Console.WriteLine($"Current mouse position: ({currentPos.X}, {currentPos.Y})");
        }

        public static void ListRunningWindows()
        {
            List<WindowInfo> windows = new List<WindowInfo>();

            WindowApis.EnumWindows((hWnd, lParam) =>
            {
                if (WindowApis.IsWindowVisible(hWnd))
                {
                    int length = WindowApis.GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        StringBuilder sb = new StringBuilder(length + 1);
                        WindowApis.GetWindowText(hWnd, sb, sb.Capacity);
                        string title = sb.ToString();

                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            WindowApis.GetWindowThreadProcessId(hWnd, out uint processId);
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

        public static void SwitchToWindow()
        {
            Console.Write("Enter part of the window title to switch to: ");
            string? searchTitle = Console.ReadLine()?.ToLower();

            if (string.IsNullOrWhiteSpace(searchTitle))
            {
                Console.WriteLine("No search term provided.");
                return;
            }

            bool found = false;
            WindowApis.EnumWindows((hWnd, lParam) =>
            {
                if (WindowApis.IsWindowVisible(hWnd))
                {
                    int length = WindowApis.GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        StringBuilder sb = new StringBuilder(length + 1);
                        WindowApis.GetWindowText(hWnd, sb, sb.Capacity);
                        string title = sb.ToString();

                        if (title.ToLower().Contains(searchTitle))
                        {
                            WindowApis.ShowWindow(hWnd, Constants.SW_RESTORE);
                            WindowApis.SetForegroundWindow(hWnd);
                            Thread.Sleep(100); // Small delay to ensure window is restored
                            WindowApis.ShowWindow(hWnd, Constants.SW_MAXIMIZE);
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

        public static void DemoMouseMovement()
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

                WindowApis.SetCursorPos(x, y);
                Console.WriteLine($"Moved to ({x}, {y})");
                Thread.Sleep(1000);
            }

            if (Console.KeyAvailable)
            {
                Console.ReadKey(true); // Clear the key press
            }

            Console.WriteLine("Demo completed.");
        }

        public static void DemoWindowSwitching()
        {
            Console.WriteLine("Starting window switching demo...");

            List<WindowInfo> windows = new List<WindowInfo>();

            // Collect visible windows
            WindowApis.EnumWindows((hWnd, lParam) =>
            {
                if (WindowApis.IsWindowVisible(hWnd))
                {
                    int length = WindowApis.GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        StringBuilder sb = new StringBuilder(length + 1);
                        WindowApis.GetWindowText(hWnd, sb, sb.Capacity);
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
                WindowApis.ShowWindow(windows[i].Handle, Constants.SW_RESTORE);
                WindowApis.SetForegroundWindow(windows[i].Handle);
                Thread.Sleep(100); // Small delay to ensure window is restored
                WindowApis.ShowWindow(windows[i].Handle, Constants.SW_MAXIMIZE);
                Thread.Sleep(2000);
            }

            Console.WriteLine("Window switching demo completed.");
        }

        public static void TimedWindowSwitching()
        {
            Console.Write("Enter total duration in minutes: ");
            if (!int.TryParse(Console.ReadLine(), out int totalMinutes) || totalMinutes <= 0)
            {
                Console.WriteLine("Invalid duration. Please enter a positive number.");
                return;
            }

            Console.WriteLine("Collecting available windows...");

            List<WindowInfo> windows = new List<WindowInfo>();

            WindowApis.EnumWindows((hWnd, lParam) =>
            {
                if (WindowApis.IsWindowVisible(hWnd))
                {
                    int length = WindowApis.GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        StringBuilder sb = new StringBuilder(length + 1);
                        WindowApis.GetWindowText(hWnd, sb, sb.Capacity);
                        string title = sb.ToString();

                        // Filter out system windows and empty titles
                        if (!string.IsNullOrWhiteSpace(title) &&
                            !title.Equals("Program Manager") &&
                            !title.StartsWith("Windows Security") &&
                            title.Length > 2)
                        {
                            WindowApis.GetWindowThreadProcessId(hWnd, out uint processId);
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
                    WindowApis.ShowWindow(currentWindow.Handle, Constants.SW_RESTORE);
                    WindowApis.SetForegroundWindow(currentWindow.Handle);
                    Thread.Sleep(200); // Small delay to ensure window is restored and focused
                    WindowApis.ShowWindow(currentWindow.Handle, Constants.SW_MAXIMIZE);
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
                            if (WindowApis.IsWindowVisible(window.Handle))
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
}

using AutoWindowSwitcher.Utilities;
using AutoWindowSwitcher.Application;


namespace AutoWindowSwitcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Utils.ShowTitle();

            while (true)
            {
                Utils.ShowMenu();
                string? choice = Console.ReadLine();

                switch (choice?.ToLower())
                {
                    case "1":
                        Options.MoveMouse();
                        break;
                    case "2":
                        Options.ClickMouse();
                        break;
                    case "3":
                        Options.ShowCurrentMousePosition();
                        break;
                    case "4":
                        Options.ListRunningWindows();
                        break;
                    case "5":
                        Options.SwitchToWindow();
                        break;
                    case "6":
                        Options.DemoMouseMovement();
                        break;
                    case "7":
                        Options.DemoWindowSwitching();
                        break;
                    case "8":
                        Options.TimedWindowSwitching();
                        break;
                    case "q":
                    case "quit":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                Utils.ShowPressToContinuePrompt();
            }
        }
    }
}
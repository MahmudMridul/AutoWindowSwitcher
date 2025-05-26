
namespace AutoWindowSwitcher.Utilities
{
    public class Utils
    {
        public static void ShowPressToContinuePrompt()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
        public static void ShowTitle()
        {
            Console.WriteLine("=== Mouse and Window Controller ===");
            Console.WriteLine();
        }
        public static void ShowMenu()
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
    }
}

using System.Text;
using System.Runtime.InteropServices;
using System.Transactions;
using System.Runtime.CompilerServices;

public class Program
{
    #region DLLS Windows
    // Importation de la fonction mouse_event depuis user32.dll
    [DllImport("user32.dll", SetLastError = true)]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);

    // Importation de la fonction GetAsyncKeyState depuis user32.dll
    [DllImport("user32.dll", SetLastError = true)]
    public static extern short GetAsyncKeyState(int vKey);

    // Définir les P/Invoke pour utiliser les API Windows
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);
    #endregion

    // Mouse events
    private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
    private const uint MOUSEEVENTF_LEFTUP = 0x04;

    // Virtual keyCodes  
    const int VK_ESCAPE = 0x43; // "C" key
    
    // Variables
    static bool run = true;
    const string TargetWindowTitle = "DeadByDaylight";
    const int DEFAULT_RATE = 500;

    static void Main(string[] args)
    {
        // Get clickrate
        int rate = args.Length <= 0 ? DEFAULT_RATE : GetClickRate(args[0]);
        string message = rate == DEFAULT_RATE ? $"Using the default clickrate of {DEFAULT_RATE}ms." : $"Using a clickrate of {rate}ms.";
        Console.WriteLine(message);

        // Create a thread only to check the Escape key
        Thread keyThread = new Thread(KeyThread);
        keyThread.Start();

        // Simulate clicks
        while(run)
        {
            // Check if dbd is focused
            if(IsDBDFocused())
            {
                Click();
                Thread.Sleep(rate);
            }
            else
            {
                Console.WriteLine("Fenêtre incorrecte. En attente...");
                Thread.Sleep(1000);
            }
        }

        keyThread.Join(); // Wait that the keyThread ends correctly before ending the program.
        Thread.Sleep(50);
    }

    /// <summary>
    /// Thread that handles a press check on the Escape key
    /// </summary>
    static void KeyThread()
    {
        while (true)
        {
            if ((GetAsyncKeyState(VK_ESCAPE) & 0x8000) != 0)
            {
                Console.WriteLine("Shutting down autoClicker...");
                run = false;
                break;
            }
            // Delay to avoid CPU surge
            Thread.Sleep(50);
        }
    }

    static bool IsDBDFocused()
    {
        // Get the foreground window 
        IntPtr hWnd = GetForegroundWindow();

        // Get its title
        StringBuilder windowTitle = new StringBuilder(256);
        GetWindowText(hWnd, windowTitle, windowTitle.Capacity);

        // Compare foreground window's name with the target Window
        return windowTitle.ToString().Contains(TargetWindowTitle);
    }

    /// <summary>
    /// Simulate a left mouse click
    /// </summary>
    static void Click()
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        Thread.Sleep(30);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }

    /// <summary>
    /// Return the clickrate value for the main thread.
    /// </summary>
    static int GetClickRate(string argValue)
    {
        try {
            int arg = (int)float.Parse(argValue) * 1000;
            return Math.Clamp(arg, 1, 10);
        }
        catch {
            return DEFAULT_RATE;
        }
    }
}
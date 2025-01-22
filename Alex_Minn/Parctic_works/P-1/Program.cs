using System.Runtime.InteropServices;

internal class Program
{
    [DllImport("user32.dll")]
    static extern int MessageBox(IntPtr hWnd, string text, string caption, int options);
    private static void Main(string[] args)
    {
        MessageBox(IntPtr.Zero, "Hello, World!", "Message", 0);
    }
}
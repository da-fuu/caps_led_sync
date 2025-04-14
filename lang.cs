using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.ComponentModel;


class LanguageChangeDetector
{
    [DllImport("user32.dll")]
    private static extern UInt32 GetKeyboardLayout(uint idThread);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    private static UInt32 lastLayout = 0;

    [STAThread]
    static void Main()
    {
        while (true)
        {
            IntPtr foregroundWindow = GetForegroundWindow();
			if (foregroundWindow == (IntPtr)(0))
			{
				Thread.Sleep(1000);
				continue;
			}
			uint temp;
            uint threadId = GetWindowThreadProcessId(foregroundWindow, out temp);
			if (threadId == 0)
			{
				Thread.Sleep(1000);
				continue;
			}
            UInt32 keyboardLayout = GetKeyboardLayout(threadId);

            if (keyboardLayout != lastLayout)
            {
                lastLayout = keyboardLayout;
				int cultureId = (int)(keyboardLayout & 0xFFFF);
				try
				{	
					string newLanguage = System.Globalization.CultureInfo.GetCultureInfo(cultureId).Name;
					if (newLanguage == "ru-RU")
					{
						CapsLockLight.SetCapsLock(true);
					}
					else
					{
						CapsLockLight.SetCapsLock(false);
					} 
					continue;
				}
				catch (Exception)
				{
					Thread.Sleep(1000);
					continue;
				}
			}
            Thread.Sleep(100);
        }
    }
}


class CapsLockLight
{
    [DllImport("kernel32.dll", SetLastError=true)]
    public static extern Boolean DefineDosDevice(UInt32 flags, String deviceName, String targetPath);

    [DllImport("kernel32.dll", SetLastError=true)]
    public static extern IntPtr CreateFile(String fileName,
                       UInt32 desiredAccess, UInt32 shareMode, IntPtr securityAttributes,
                       UInt32 creationDisposition, UInt32 flagsAndAttributes, IntPtr templateFile
                      );

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBOARD_INDICATOR_PARAMETERS
    {
        public UInt16 unitID;
        public UInt16 LEDflags;
    }      

    [DllImport("kernel32.dll", SetLastError=true)]
    public static extern Boolean DeviceIoControl(IntPtr device, UInt32 ioControlCode,
                          ref KEYBOARD_INDICATOR_PARAMETERS KIPin,  UInt32  inBufferSize,
                          ref KEYBOARD_INDICATOR_PARAMETERS KIPout, UInt32 outBufferSize,
                          ref UInt32 bytesReturned, IntPtr overlapped
                         );
    [DllImport("kernel32.dll", SetLastError=true)]
    public static extern Boolean DeviceIoControl(IntPtr device, UInt32 ioControlCode,
                          IntPtr KIPin,  UInt32  inBufferSize,
                          ref KEYBOARD_INDICATOR_PARAMETERS KIPout, UInt32 outBufferSize,
                          ref UInt32 bytesReturned, IntPtr overlapped
                         );
    [DllImport("kernel32.dll", SetLastError=true)]
    public static extern Boolean DeviceIoControl(IntPtr device, UInt32 ioControlCode,
                          ref KEYBOARD_INDICATOR_PARAMETERS KIPin,  UInt32  inBufferSize,
                          IntPtr KIPout, UInt32 outBufferSize,
                          ref UInt32 bytesReturned, IntPtr overlapped
                         );

    [DllImport("kernel32.dll", SetLastError=true)]
    public static extern Boolean CloseHandle(IntPtr handle);

    public static void SetCapsLock(Boolean enable)
    {
        UInt32 bytesReturned = 0;
        IntPtr device;
        KEYBOARD_INDICATOR_PARAMETERS KIPbuf = new KEYBOARD_INDICATOR_PARAMETERS { unitID = 0, LEDflags = 0 };

        if (!DefineDosDevice(1, "myKBD", "\\Device\\KeyboardClass0"))
        {
            Int32 err = Marshal.GetLastWin32Error();
            throw new Win32Exception(err);
        }
        // Console.WriteLine("Created device");

        device = CreateFile("\\\\.\\myKBD", 0x40000000, 0, IntPtr.Zero, 3, 0, IntPtr.Zero);
        if (device == (IntPtr)(-1))
        {
            Int32 err = Marshal.GetLastWin32Error();
            throw new Win32Exception(err);
        }
        // Console.WriteLine("Opened device");

        if (!DeviceIoControl(device, 0xB0040, IntPtr.Zero, 0, ref KIPbuf, (UInt32)Marshal.SizeOf(KIPbuf), ref bytesReturned, IntPtr.Zero))
        {
            Int32 err = Marshal.GetLastWin32Error();
            throw new Win32Exception(err);
        }
        // Console.WriteLine(String.Format("Read LED status: {0:x}", KIPbuf.LEDflags));

		if (enable)
        {
            KIPbuf.LEDflags = (UInt16)(KIPbuf.LEDflags | 4);
        }
        else
        {
            KIPbuf.LEDflags = (UInt16)(KIPbuf.LEDflags & ~4);
        }
        // Console.WriteLine(String.Format("Changed LED status to: {0:x}", KIPbuf.LEDflags));

        if (!DeviceIoControl(device, 0xB0008, ref KIPbuf, (UInt32)Marshal.SizeOf(KIPbuf), IntPtr.Zero, 0, ref bytesReturned, IntPtr.Zero))
        {
            Int32 err = Marshal.GetLastWin32Error();
            throw new Win32Exception(err);
        }
        // Console.WriteLine("Set new LED status");

        if (!CloseHandle(device))
        {
            Int32 err = Marshal.GetLastWin32Error();
            throw new Win32Exception(err);
        }
        // Console.WriteLine("Closed device handle");

        if (!DefineDosDevice(2, "myKBD", null))
        {
            Int32 err = Marshal.GetLastWin32Error();
            throw new Win32Exception(err);
        }
        // Console.WriteLine("Removed device definition");
    }
};

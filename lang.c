#include <windows.h>
#include <ntddkbd.h>


BOOL SetCapsLock(BOOL enable)
{
	KEYBOARD_INDICATOR_PARAMETERS KIPbuf = { 0 };

	if (!DefineDosDeviceA(1, "myKBD", "\\Device\\KeyboardClass0"))
	{
		return FALSE;
	}

	HANDLE device = CreateFileA("\\\\.\\myKBD", 0x40000000, 0, NULL, 3, 0, NULL);
	if (device == INVALID_HANDLE_VALUE)
	{
		return FALSE;
	}

	if (!DeviceIoControl(device, 0xB0040, NULL, 0, &KIPbuf, sizeof(KIPbuf), NULL, NULL))
	{
		return FALSE;
	}

	if (enable)
	{
		KIPbuf.LedFlags |= 4;
	}
	else
	{
		KIPbuf.LedFlags &= ~4;
	}

	if (!DeviceIoControl(device, 0xB0008, &KIPbuf, sizeof(KIPbuf), NULL, 0, NULL, NULL))
	{
		return FALSE;
	}

	if (!CloseHandle(device))
	{
		return FALSE;
	}

	if (!DefineDosDevice(2, "myKBD", NULL))
	{
		return FALSE;
	}
	
	return TRUE;
}

int main()
{
	HKL lastLayout = (HKL)0x0409;
	
	while (TRUE)
	{
		HWND foregroundWindow = GetForegroundWindow();
		if (foregroundWindow == NULL)
		{
			Sleep(1000);
			continue;
		}
		DWORD threadId = GetWindowThreadProcessId(foregroundWindow, NULL);
		if (threadId == 0)
		{
			Sleep(1000);
			continue;
		}
		HKL keyboardLayout = GetKeyboardLayout(threadId);

		if (keyboardLayout != lastLayout)
		{
			lastLayout = keyboardLayout;
			if (!SetCapsLock(((UINT_PTR)keyboardLayout & 0xFFFF) != 0x0409))
			{
				Sleep(1000);
				continue;
			}	
		}
		Sleep(125);
	}
	return 0;
}

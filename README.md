Simple C and C# programs which synchronizes CapsLock LED with current keyboard layout

Compilation:

C#
```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc /out:lang.exe /target:winexe .\lang.cs
```

C
```
cl /O2 lang.c /link user32.lib /SUBSYSTEM:WINDOWS /ENTRY:mainCRTStartup
```

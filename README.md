# ProcessMemory Library
Converted from VB to C#, Reference: https://github.com/FilipKufalov/VB.NET/blob/main/ReadWriteProcessMemory/Mem.vb
# Example Usage:
1. **Use Initialize from Mem, Example:** "Mem.Initialize("Your application/game proccess");"
2. **Use InitWindow from Mem as Second Alternative, Example:** "Mem.InitWindow("Your application/game class process") - Process information from Class" 
3. **Ability to Allocate and Free Memory Regions, Example:** "Mem.AllocMem(0x100);" or "Mem.FreeMem(0x02C40000, 0)"
4. **Ability to read memory from process, Example:** "Mem.ReadMemory<T>(Address));"
5. **Ability to write to memory, Example:** "Mem.WriteMemory<T>(Address, Value);"
6. **Ability to get any process width and height (mostly for World2Screen Function)**  "Mem.GetWindowSize(processName)"
# Change Log

- VirtualAllocEx & VirtualFreeEx (29.11.2023)
- Summary explanation for all functions (28.11.2023)
- GetWindowSize (27.05.2023)
- InitWindow (08.12.2022)
# Additional Info
- Change <T> with your type value
- Mem.BaseAddress is entry point/address of .exe/process

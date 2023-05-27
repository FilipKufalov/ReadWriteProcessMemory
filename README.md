# ReadWriteProcessMemory
Converted from VB to C#, Reference: https://github.com/FilipKufalov/VB.NET/blob/main/ReadWriteProcessMemory/Mem.vb
# Example Usage:
1. **Use Initialize from Mem, Example:** "Mem.Initialize("Your application/game proccess");"
2. **Use InitWindow from Mem as Second Alternative, Example:** "Mem.InitWindow("Your application/game class process") - Process information from Class" 
3. **Ability to read memory from process, Example:** "Mem.ReadMemory<T>(Address));"
4. **Ability to write to memory, Example:** "Mem.WriteMemory<T>(Address, Value);"
5. **Ability to get any process width and height** "Mem.GetWindowSize(processName)"
# Added
- InitWindow
- GetWindowSize
# Additional Info
- Change <T> with your type value
- Mem.BaseAddress is entry point/address of .exe/process
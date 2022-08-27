# ReadWriteProcessMemory
Converted from VB to C#, Reference: https://github.com/FilipKufalov/VB.NET/blob/main/ReadWriteProcessMemory/Mem.vb

# Example Usage:

1. **Use Initialize from Mem, Example:** "Mem.Initialize("Your application/game proccess");"
2. **Ability to read memory from process, Example:** "Mem.ReadMemory<T>(Address));"
3. **Ability to write to memory, Example:** "Mem.WriteMemory<T>(Address, Value);"

- Change <T> with your type value
- Mem.BaseAddress is entry point/address of .exe/process



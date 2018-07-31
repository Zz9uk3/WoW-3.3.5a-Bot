using Binarysharp.Assemblers.Fasm;
using Binarysharp.MemoryManagement.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenCore
{
    public class AmeisenHook
    {
        RemoteAllocation injectedCode;
        RemoteAllocation injectionAdress;
        RemoteAllocation returnInjection;

        public bool isHooked;

        public AmeisenHook()
        {
            IntPtr device = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<IntPtr>(AmeisenOffsets.WoWOffsets.devicePtr1 - 0x400000);
            IntPtr ayy = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<IntPtr>(IntPtr.Add(device, AmeisenOffsets.WoWOffsets.devicePtr2).ToInt32() - 0x400000);
            IntPtr lmao = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<IntPtr>(ayy.ToInt32() - 0x400000);
            IntPtr endScene = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<IntPtr>(IntPtr.Add(lmao, AmeisenOffsets.WoWOffsets.endScene).ToInt32() - 0x400000);

            if (AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<byte>(endScene.ToInt32() - 0x400000) != 0xE9)
            {
                isHooked = false;

                injectedCode = AmeisenManager.GetInstance().GetMemorySharp().Memory.Allocate(2048);
                injectionAdress = AmeisenManager.GetInstance().GetMemorySharp().Memory.Allocate(4);
                returnInjection = AmeisenManager.GetInstance().GetMemorySharp().Memory.Allocate(4);

                injectedCode.Write<int>(0);
                returnInjection.Write<int>(0);

                string[] asm = new string[]{
                    "pushad",
                    "pushfd",
                    "mov eax, " + injectionAdress,
                    "test eax, eax",
                    "je @out",
                    "mov eax, " + injectionAdress,
                    "call eax",
                    "mov [" + returnInjection + "], eax",
                    "mov edx, " + injectionAdress,
                    "mov ecx, 0",
                    "mov [edx], ecx",
                    "@out:",
                    "popfd",
                    "popad"
                };

                int asmLenght = FasmNet.Assemble(asm).Length;
                AmeisenManager.GetInstance().GetMemorySharp().Assembly.InjectAndExecute(asm, injectedCode.BaseAddress);

                asm = new string[]{
                    "mov edi, edi",
                    "push ebp",
                    "mov ebp, esp"
                };

                AmeisenManager.GetInstance().GetMemorySharp().Assembly.InjectAndExecute(asm, injectedCode.BaseAddress + asmLenght);

                asm = new string[]{
                    "jmp " + endScene + 0x5
                };

                AmeisenManager.GetInstance().GetMemorySharp().Assembly.InjectAndExecute(asm, injectedCode.BaseAddress + asmLenght + 0x5);

                asm = new string[]{
                    "jmp " + injectedCode
                };

                AmeisenManager.GetInstance().GetMemorySharp().Assembly.InjectAndExecute(asm, endScene);

                isHooked = true;
            }
        }

        public byte[] InjectAndExecute(string[] asm)
        {
            byte[] result;
            byte buffer = new Byte();
            List<byte> returnBytes = new List<byte>();

            int asmLenght = FasmNet.Assemble(asm).Length;
            RemoteAllocation injectionAsmCC = AmeisenManager.GetInstance().GetMemorySharp().Memory.Allocate(asmLenght);

            AmeisenManager.GetInstance().GetMemorySharp().Assembly.InjectAndExecute(asm, injectionAsmCC.BaseAddress);
            AmeisenManager.GetInstance().GetMemorySharp().Write<int>(injectionAdress.BaseAddress, injectionAsmCC.BaseAddress.ToInt32());

            IntPtr dwAddress = AmeisenManager.GetInstance().GetMemorySharp().Read<IntPtr>(returnInjection.BaseAddress);
            buffer = AmeisenManager.GetInstance().GetMemorySharp().Read<byte>(dwAddress);

            while (buffer != 0)
            {
                returnBytes.Add(buffer);
                dwAddress = dwAddress + 1;
                buffer = AmeisenManager.GetInstance().GetMemorySharp().Read<byte>(dwAddress);
            }
            result = returnBytes.ToArray();

            injectionAsmCC.Release();

            return result;
        }
    }
}

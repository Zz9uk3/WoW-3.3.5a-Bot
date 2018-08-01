using System;
using System.Collections.Generic;

namespace AmeisenCore
{
    /// <summary>
    /// Class that manages the hooking of WoW's EndScene
    /// 
    /// !!! W.I.P !!!
    /// </summary>
    public class AmeisenHook
    {
        uint injectedCodeAdress;
        uint injectionAdress;
        uint returnInjectionAdress;

        uint device;
        uint ayy;
        uint lmao;
        uint endScene;

        public bool isHooked;

        /// <summary>
        /// Hook WoW's EndScene
        /// </summary>
        public void Hook()
        {
            device = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(AmeisenOffsets.WoWOffsets.devicePtr1);
            ayy = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(device + AmeisenOffsets.WoWOffsets.devicePtr2);
            lmao = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(ayy);
            endScene = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(lmao + AmeisenOffsets.WoWOffsets.endScene);

            if (AmeisenManager.GetInstance().GetBlackMagic().ReadByte(endScene) != 0xE9)
            {
                isHooked = false;

                injectedCodeAdress = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(2048);
                injectionAdress = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(4);
                returnInjectionAdress = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(4);

                // Clear dem memories
                AmeisenManager.GetInstance().GetBlackMagic().WriteInt(injectedCodeAdress, 0);
                AmeisenManager.GetInstance().GetBlackMagic().WriteInt(returnInjectionAdress, 0);

                // EndScene -----------------------------------------------------------------------

                string[] asm = new string[]{
                    "pushad",
                    "pushfd",
                    "mov eax, " + injectionAdress,
                    "test eax, eax", // Check if there is code
                    "je @out", // If there is no code jump to @out
                    "mov eax, " + injectionAdress,
                    "call eax",
                    "mov [" + returnInjectionAdress + "], eax",
                    "mov edx, " + injectionAdress,
                    "mov ecx, 0",
                    "mov [edx], ecx",
                    "@out:",
                    "popfd", // Cleanup
                    "popad"
                };

                AddASM(asm);
                int asmLenght = AmeisenManager.GetInstance().GetBlackMagic().Asm.Assemble().Length;
                ClearAndInject(injectedCodeAdress);

                // Original EndScene --------------------------------------------------------------

                InsertOriginalEndScene();
                ClearAndInject(injectedCodeAdress + (uint)asmLenght);

                // Injected end -------------------------------------------------------------------

                AddASM(new string[]{
                    "jmp " + endScene + 0x5
                });
                ClearAndInject(injectedCodeAdress + (uint)asmLenght + 0x5);

                // EndScene detour ----------------------------------------------------------------

                AddASM(new string[]{
                    "jmp " + injectedCodeAdress
                });
                ClearAndInject(endScene);

                // Final clear --------------------------------------------------------------------

                AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();
                isHooked = true;
            }
        }

        /// <summary>
        /// Restore Original EndScene state
        /// </summary>
        public void UnHook()
        {
            if (!isHooked)
            {
                device = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(AmeisenOffsets.WoWOffsets.devicePtr1);
                ayy = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(device + AmeisenOffsets.WoWOffsets.devicePtr2);
                lmao = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(ayy);
                endScene = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(lmao + AmeisenOffsets.WoWOffsets.endScene);
            }

            if (AmeisenManager.GetInstance().GetBlackMagic().ReadByte(endScene) == 0xE9)
            {
                AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();
                InsertOriginalEndScene();
                AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject(endScene);
            }

            AmeisenManager.GetInstance().GetBlackMagic().FreeMemory(injectedCodeAdress);
            AmeisenManager.GetInstance().GetBlackMagic().FreeMemory(injectionAdress);
            AmeisenManager.GetInstance().GetBlackMagic().FreeMemory(returnInjectionAdress);
        }

        /// <summary>
        /// Inject ASM to WoW and run this shit after EndScene function.
        /// </summary>
        /// <param name="asm">ASM to inject</param>
        /// <returns>Bytes that were returned</returns>
        public byte[] InjectAndExecute(string[] asm)
        {
            byte[] result;
            byte buffer = new Byte();
            List<byte> returnBytes = new List<byte>();

            AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();

            foreach (string s in asm)
                AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine(s);

            int asmLenght = AmeisenManager.GetInstance().GetBlackMagic().Asm.Assemble().Length;
            uint injectionAsmCC = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(asmLenght);

            AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject(injectionAsmCC);
            AmeisenManager.GetInstance().GetBlackMagic().WriteUInt(injectionAdress, injectionAsmCC);

            uint dwAddress = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(returnInjectionAdress);
            buffer = AmeisenManager.GetInstance().GetBlackMagic().ReadByte(dwAddress);

            while (buffer != 0)
            {
                returnBytes.Add(buffer);
                dwAddress++;
                buffer = AmeisenManager.GetInstance().GetBlackMagic().ReadByte(dwAddress);
            }
            result = returnBytes.ToArray();

            AmeisenManager.GetInstance().GetBlackMagic().FreeMemory(injectionAsmCC);

            return result;
        }

        private void ClearAndInject(uint address)
        {
            AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject(address);
            AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();
        }

        private void AddASM(string[] asm)
        {
            foreach (string s in asm)
                AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine(s);
        }

        private void InsertOriginalEndScene()
        {
            AddASM(new string[]{
                    "mov edi, edi",
                    "push ebp",
                    "mov ebp, esp"
                });
        }
    }
}

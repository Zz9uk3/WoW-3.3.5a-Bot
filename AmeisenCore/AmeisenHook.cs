using System;
using System.Collections.Generic;
using System.Threading;

namespace AmeisenCore
{
    /// <summary>
    /// Class that manages the hooking of WoW's EndScene
    /// 
    /// !!! W.I.P !!!
    /// </summary>
    public class AmeisenHook
    {
        uint injectedCodeAddress = 0;
        uint injectionAddress = 0;
        uint returnInjectionASM = 0;
        byte[] originalEndSceneBytes;

        bool threadHooked = false;

        public AmeisenHook()
        {
            Hook();
        }

        public void Hook()
        {
            if (AmeisenManager.GetInstance().GetBlackMagic().IsProcessOpen)
            {
                uint pDevice = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(AmeisenOffsets.WoWOffsets.devicePtr1);
                uint pEnd = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pDevice + AmeisenOffsets.WoWOffsets.devicePtr2);
                uint pScene = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pEnd);
                uint pEndScene = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pScene + AmeisenOffsets.WoWOffsets.endScene);

                if (AmeisenManager.GetInstance().GetBlackMagic().ReadByte(pEndScene) == 0xE9 && (injectedCodeAddress == 0 || injectionAddress == 0))
                {
                    DisposeHooking();
                }

                if (AmeisenManager.GetInstance().GetBlackMagic().ReadByte(pEndScene) != 0xE9)
                {
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("pushad");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("pushfd");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("mov eax, [" + injectionAddress + "]");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("test eax, eax");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("je @out");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("mov eax, [" + injectionAddress + "]");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("call eax");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("mov [" + returnInjectionASM + "], eax");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("mov edx, " + returnInjectionASM);
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("mov ecx, 0");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("mov [edx], ecx");

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("@out:");

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("popfd");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("popad");

                    var sizeAsm = (uint)AmeisenManager.GetInstance().GetBlackMagic().Asm.Assemble().Length;
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject(injectedCodeAddress);

                    const int sizeJumpBack = 2;

                    originalEndSceneBytes = AmeisenManager.GetInstance().GetBlackMagic().ReadBytes(pEndScene - 5, 7);

                    AmeisenManager.GetInstance().GetBlackMagic().WriteBytes((uint)IntPtr.Add(new IntPtr(injectedCodeAddress), (int)sizeAsm), new[] { originalEndSceneBytes[5], originalEndSceneBytes[6] });
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("jmp " + (pEndScene + sizeJumpBack));
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject(injectedCodeAddress + sizeAsm + sizeJumpBack);

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("@top:");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("jmp " + injectedCodeAddress);
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("jmp @top");

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject(pEndScene - 5);
                }
                threadHooked = true;
            }

        }

        public void DisposeHooking()
        {
            uint pDevice = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(AmeisenOffsets.WoWOffsets.devicePtr1);
            uint pEnd = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pDevice + AmeisenOffsets.WoWOffsets.devicePtr2);
            uint pScene = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pEnd);
            uint pEndScene = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pScene + AmeisenOffsets.WoWOffsets.endScene);

            if (AmeisenManager.GetInstance().GetBlackMagic().ReadByte(pEndScene) == 0xEB) // check if wow is already hooked and dispose Hook
            {
                if (originalEndSceneBytes != null)
                {
                    // Restore original endscene:
                    AmeisenManager.GetInstance().GetBlackMagic().WriteBytes(pEndScene - 5, originalEndSceneBytes);
                }
            }
        }

        public byte[] InjectAndExecute(string[] asm, int returnLength = 0)
        {
            byte[] tempsByte = new byte[0];
            AmeisenManager.GetInstance().GetBlackMagic().WriteInt(returnInjectionASM, 0);

            if (!AmeisenManager.GetInstance().GetBlackMagic().IsProcessOpen || !threadHooked) return tempsByte;

            AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();
            foreach (var tempLineAsm in asm)
            {
                AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine(tempLineAsm);
            }

            uint injectionAsmCodecave = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(AmeisenManager.GetInstance().GetBlackMagic().Asm.Assemble().Length);

            try
            {
                AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject(injectionAsmCodecave);
                AmeisenManager.GetInstance().GetBlackMagic().WriteUInt(injectionAddress, injectionAsmCodecave);

                while (AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(injectionAddress) > 0)
                {
                    Thread.Sleep(5);
                }

                if (returnLength > 0)
                {
                    tempsByte = AmeisenManager.GetInstance().GetBlackMagic().ReadBytes(AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(returnInjectionASM), returnLength);
                }
                else
                {
                    List<byte> retnByte = new List<byte>();
                    uint dwAddress = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(returnInjectionASM);
                    if (dwAddress != 0)
                    {
                        byte buf = AmeisenManager.GetInstance().GetBlackMagic().ReadByte(dwAddress);
                        while (buf != 0)
                        {
                            retnByte.Add(buf);
                            dwAddress = dwAddress + 1;
                            buf = AmeisenManager.GetInstance().GetBlackMagic().ReadByte(dwAddress);
                        }
                    }
                    tempsByte = retnByte.ToArray();
                }
            }
            catch (Exception e) { }
            finally { new Timer(state => AmeisenManager.GetInstance().GetBlackMagic().FreeMemory((uint)state), injectionAsmCodecave, 100, 0); }
            return tempsByte;
        }
    }
}

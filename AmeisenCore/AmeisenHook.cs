using AmeisenLogging;
using AmeisenUtilities;
using System;
using System.Collections.Generic;
using System.Text;
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
        public bool isHooked = false;

        private uint codeCave;
        private uint codeCaveForInjection;
        private uint codeToExecute;
        uint endsceneReturnAddress;
        private byte[] originalEndscene;

        public AmeisenHook() { Hook(); }

        public void Hook()
        {
            if (AmeisenManager.GetInstance().GetBlackMagic().IsProcessOpen)
            {
                // Get D3D9 Endscene Pointer
                uint endscene = GetEndScene();

                // If WoW is already hooked, unhook it
                if (AmeisenManager.GetInstance().GetBlackMagic().ReadByte(endscene) == 0xE9)
                {
                    DisposeHooking();
                }

                // If WoW is now/was unhooked, hook it
                if (AmeisenManager.GetInstance().GetBlackMagic().ReadByte(endscene) != 0xE9)
                {
                    uint endsceneHookOffset = 0x2;
                    endscene += endsceneHookOffset;

                    endsceneReturnAddress = endscene + 0x5;

                    originalEndscene = AmeisenManager.GetInstance().GetBlackMagic().ReadBytes(endscene, 5);

                    codeToExecute = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(4);
                    AmeisenManager.GetInstance().GetBlackMagic().WriteInt(codeToExecute, 0);

                    codeCave = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(32);
                    codeCaveForInjection = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(256);

                    AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "EndScene at: " + endscene.ToString("X"), this);
                    AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "EndScene returning at: " + (endsceneReturnAddress).ToString("X"), this);
                    AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "CodeCave at:" + codeCave.ToString("X"), this);
                    AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "CodeCaveForInjection at:" + codeCaveForInjection.ToString("X"), this);
                    AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "CodeToExecute at:" + codeToExecute.ToString("X"), this);

                    AmeisenManager.GetInstance().GetBlackMagic().WriteBytes(codeCave, originalEndscene);

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("PUSHFD");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("PUSHAD");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("MOV EAX, [" + (codeToExecute) + "]");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("TEST EAX, 1");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("JE @out");

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("CALL " + (codeCaveForInjection));
                    //AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("MOV [" + (returnAdress) + "], EAX");

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("MOV EDX, 0");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("MOV [" + (codeToExecute) + "], EDX");

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("@out:");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("POPAD");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("POPFD");
                    int asmLenght = AmeisenManager.GetInstance().GetBlackMagic().Asm.Assemble().Length;
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject(codeCave + 5);

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("JMP " + (endsceneReturnAddress));
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject((codeCave + (uint)asmLenght) + 5);

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("JMP " + (codeCave));
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject(endscene);

                }
                isHooked = true;
            }

        }

        public void DisposeHooking()
        {
            // Get D3D9 Endscene Pointer
            uint endscene = GetEndScene();

            uint endsceneHookOffset = 0x2;
            endscene += endsceneHookOffset;

            // Check if WoW is hooked
            if (AmeisenManager.GetInstance().GetBlackMagic().ReadByte(endscene) == 0xE9)
            {
                AmeisenManager.GetInstance().GetBlackMagic().WriteBytes(endscene, originalEndscene);

                AmeisenManager.GetInstance().GetBlackMagic().FreeMemory(codeCave);
                AmeisenManager.GetInstance().GetBlackMagic().FreeMemory(codeToExecute);
                AmeisenManager.GetInstance().GetBlackMagic().FreeMemory(codeCaveForInjection);
            }

            isHooked = false;
        }

        public void InjectAndExecute(string[] asm)
        {
            AmeisenManager.GetInstance().GetBlackMagic().WriteInt(codeToExecute, 1);

            AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();

            if (asm != null)
                foreach (string s in asm)
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine(s);

            AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("POPAD");
            AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("POPFD");
            int asmLenght = AmeisenManager.GetInstance().GetBlackMagic().Asm.Assemble().Length;
            AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject(codeCaveForInjection);

            AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();
            AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("JMP " + (endsceneReturnAddress));
            AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject((codeCaveForInjection + (uint)asmLenght));

            // You need this, trust me
            Thread.Sleep(100);
        }

        public byte[] TryReadReturnValue(uint returnAdress)
        {
            try
            {
                AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "reading returnValue:" + returnAdress.ToString("X"), this);

                byte buffer = new Byte();
                List<byte> retnByte = new List<byte>();
                uint dwAddress = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(returnAdress);

                buffer = AmeisenManager.GetInstance().GetBlackMagic().ReadByte(dwAddress);
                while (buffer != 0)
                {
                    retnByte.Add(buffer);
                    dwAddress = dwAddress + 1;
                    buffer = AmeisenManager.GetInstance().GetBlackMagic().ReadByte(dwAddress);
                }

                return retnByte.ToArray();
            }
            catch (Exception e) { AmeisenLogger.GetInstance().Log(LogLevel.ERROR, "Error reading returnValue:" + e.ToString(), this); }
            return new byte[] { 0 };
        }

        private uint GetEndScene()
        {
            uint pDevice = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(WoWOffsets.devicePtr1);
            uint pEnd = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pDevice + WoWOffsets.devicePtr2);
            uint pScene = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pEnd);
            uint endscene = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pScene + WoWOffsets.endScene);
            return endscene;
        }
    }
}

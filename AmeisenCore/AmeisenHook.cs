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
        bool threadHooked = false;

        public AmeisenHook() { }

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
                    // TODO: Need to Hook this bad boid here
                }
                threadHooked = true;
            }

        }

        public void DisposeHooking()
        {
            // Get D3D9 Endscene Pointer
            uint endscene = GetEndScene();

            // Check if WoW is hooked
            if (AmeisenManager.GetInstance().GetBlackMagic().ReadByte(endscene) == 0xE9)
            {
                // TODO: Restore saved EndScene
            }
        }

        public byte[] InjectAndExecute(string[] asm, int returnLength = 0)
        {
            // TODO: Implement this shit
            return null;
        }

        private uint GetEndScene()
        {

            uint pDevice = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(AmeisenUtilities.WoWOffsets.devicePtr1);
            uint pEnd = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pDevice + AmeisenUtilities.WoWOffsets.devicePtr2);
            uint pScene = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pEnd);
            return AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pScene + AmeisenUtilities.WoWOffsets.endScene);
        }
    }
}

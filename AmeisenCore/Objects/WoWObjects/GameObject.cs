using AmeisenUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenCore.Objects
{
    public class GameObject : WoWObject
    {
        public GameObject(uint baseAddress) : base(baseAddress)
        {
            pos.x = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat(baseAddress + 0x3C);
            pos.y = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat(baseAddress + 0x40);
            pos.z = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat(baseAddress + 0x44);
            rotation = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat(baseAddress + 0x28);

            // too cpu heavy
            /*try
            {
                distance = Utils.GetDistance(pos, AmeisenManager.GetInstance().GetMe().pos);
            }
            catch { }*/
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("GAMEOBJECT");
            sb.Append(" >> Address: " + baseAddress.ToString("X"));
            sb.Append(" >> Name: " + name);
            sb.Append(" >> GUID: " + guid);
            sb.Append(" >> PosX: " + pos.x);
            sb.Append(" >> PosY: " + pos.y);
            sb.Append(" >> PosZ: " + pos.z);
            sb.Append(" >> Rotation: " + rotation);
            sb.Append(" >> Distance: " + distance);
            sb.Append(" >> MapID: " + mapID);
            sb.Append(" >> ZoneID: " + zoneID);

            return sb.ToString();
        }
    }
}

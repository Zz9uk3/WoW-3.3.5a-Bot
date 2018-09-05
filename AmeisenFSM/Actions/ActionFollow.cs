using AmeisenCoreUtils;
using AmeisenData;
using AmeisenFSM.Interfaces;
using AmeisenLogging;
using AmeisenManager;
using AmeisenUtilities;
using System;
using System.Collections.Generic;
using static AmeisenFSM.Objects.Delegates;

namespace AmeisenFSM.Actions
{
    public class ActionFollow : IAction
    {
        private List<Unit> ActiveUnits { get; set; }
        private Unit ActiveUnit { get; set; }

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        public Start StartAction { get { return Start; } }
        public DoThings StartDoThings { get { return DoThings; } }
        public Exit StartExit { get { return Stop; } }

        public void Start()
        {
            ActiveUnits = GetUnitsToFollow();
        }

        public void DoThings()
        {
            Me.Update();
            ActiveUnit.Update();

            if (Utils.GetDistance(Me.pos, ActiveUnit.pos)
                > AmeisenSettings.Instance.Settings.followDistance)
            {

            }
        }


        public void Stop()
        {
        }

        private List<Unit> GetUnitsToFollow()
        {
            List<Unit> tempList = new List<Unit>();

            if (AmeisenDataHolder.Instance.IsAllowedToFollowParty)
            {
                List<ulong> partymembers = Me.PartymemberGuids;
                foreach (ulong guid in partymembers)
                    tempList.Add(
                       (Unit)AmeisenObjectManager.Instance.GetWoWObjectFromGUID(guid));
            }

            return tempList;
        }

        private void RefreshActiveUnit()
        {
            foreach (Unit u in ActiveUnits)
            {
                if (u == null)
                    continue;
                u.Update();
                ActiveUnit = u;
            }
            ActiveUnit = null;
        }

        private void DoFollow()
        {
            Me.Update();
            Vector3 initialPosition = Me.pos;
            Vector3 posToGoTo = CalculatePosToGoTo(position, (int)distance);

            AmeisenLogger.Instance.Log(LogLevel.VERBOSE, "Allowed to move, move near", this);
            AmeisenCore.MovePlayerToXYZ(posToGoTo, InteractionType.MOVE);
            AmeisenDataHolder.Instance.IsMoving = true;

            //Me.Update();
            //Vector3 activePosition = Me.pos;
            // Stuck check, if we haven't moved since the last iteration, jump
            //CheckIfWeAreStuckIfYesJump(initialPosition, activePosition);
        }

        /// <summary> Modify our go-to-position by a small factor to provide "naturality" </summary>
        /// <param name="targetPos">pos you want to go to/param> 
        /// <param name="distanceToTarget">distance to keep to the target</param> 
        /// <returns>modified position</returns>
        private Vector3 CalculatePosToGoTo(Vector3 targetPos, int distanceToTarget)
        {
            Random rnd = new Random();
            float factorX = rnd.Next((distanceToTarget / 4) * -1, distanceToTarget / 2);
            float factorY = rnd.Next((distanceToTarget / 4) * -1, distanceToTarget / 2);
            return new Vector3(targetPos.X + factorX, targetPos.Y + factorY, targetPos.Z);
        }
    }
}

using AmeisenBotData;
using AmeisenBotDB;
using AmeisenBotUtilities;
using AmeisenMovement;
using AmeisenMovement.Structs;
using System;
using System.Collections.Generic;
using static AmeisenBotFSM.Objects.Delegates;

namespace AmeisenBotFSM.Actions
{
    public class ActionFollow : ActionMoving
    {
        public override Start StartAction { get { return Start; } }
        public override DoThings StartDoThings { get { return DoThings; } }
        public override Exit StartExit { get { return Stop; } }
        private Unit ActiveUnit { get; set; }
        private List<Unit> ActiveUnits { get; set; }
        private AmeisenDataHolder AmeisenDataHolder { get; set; }
        private AmeisenDBManager AmeisenDBManager { get; set; }
        private AmeisenMovementEngine AmeisenMovementEngine { get; set; }

        private double XOffset { get; set; }
        private double YOffset { get; set; }

        private Me Me
        {
            get { return AmeisenDataHolder.Me; }
            set { AmeisenDataHolder.Me = value; }
        }

        public ActionFollow(
            AmeisenDataHolder ameisenDataHolder,
            AmeisenDBManager ameisenDBManager,
            AmeisenMovementEngine ameisenMovementEngine) : base(ameisenDataHolder, ameisenDBManager)
        {
            AmeisenDataHolder = ameisenDataHolder;
            AmeisenDBManager = ameisenDBManager;
            AmeisenMovementEngine = ameisenMovementEngine;
        }

        public override void DoThings()
        {
            ActiveUnits = GetUnitsToFollow();
            RefreshActiveUnit();
            Me?.Update();
            ActiveUnit?.Update();

            if (Me == null || ActiveUnit == null)
            {
                return;
            }

            // When we are on different zones, stop following
            if (Me.ZoneID != ActiveUnit.ZoneID)
            {
                return;
            }

            /*Vector3 posToMoveTo = ActiveUnit.pos;
            posToMoveTo = CalculateMovementOffset(
                posToMoveTo,
                GetFollowAngle(
                    GetPartymemberCount(),
                    GetMyPartyPosition()),
                AmeisenDataHolder.Settings.followDistance);*/

            //AmeisenMovementEngine.MemberCount = GetPartymemberCount();
            Vector4 targetPos = AmeisenMovementEngine.GetPosition(
                                    new Vector4(ActiveUnit.pos.X, ActiveUnit.pos.Y, ActiveUnit.pos.Z, ActiveUnit.Rotation),
                                    AmeisenDataHolder.Settings.followDistance / 2,
                                    GetMyPartyPosition());

            Vector3 posToMoveTo = new Vector3(targetPos.X, targetPos.Y, targetPos.Z);

            // When we are far enough away, follow
            if (Utils.GetDistance(Me.pos, ActiveUnit.pos)
                > AmeisenDataHolder.Settings.followDistance)
            {
                // Dont add waypoints twice
                if (!WaypointQueue.Contains(posToMoveTo))
                {
                    WaypointQueue.Enqueue(posToMoveTo);
                }
            }

            // Do the movement stuff
            base.DoThings();
        }

        /// <summary>
        /// Return a Player by the given GUID
        /// </summary>
        /// <param name="guid">guid of the player you want to get</param>
        /// <returns>Player that you want to get</returns>
        public WowObject GetWoWObjectFromGUID(ulong guid)
        {
            foreach (WowObject p in AmeisenDataHolder.ActiveWoWObjects)
            {
                if (p.Guid == guid)
                {
                    return p;
                }
            }

            return null;
        }

        public override void Start()
        {
            base.Start();
            ActiveUnits = GetUnitsToFollow();

            Random rnd = new Random();
            XOffset = rnd.NextDouble() * AmeisenDataHolder.Settings.followDistance;
            YOffset = rnd.NextDouble() * AmeisenDataHolder.Settings.followDistance;
        }

        public override void Stop()
        {
            base.Stop();
        }

        private int GetMyPartyPosition()
        {
            int pos = 1;
            Random rnd = new Random();

            if (AmeisenDataHolder.ActiveNetworkBots != null)
            {
                foreach (NetworkBot bot in AmeisenDataHolder.ActiveNetworkBots)
                {
                    if (bot.GetMe().Guid == Me.Guid)
                    {
                        return pos;
                    }
                    else
                    {
                        pos++;
                    }
                }
            }
            else
            {
                return rnd.Next(1, 6);
            }

            return pos;
        }

        private int GetPartymemberCount()
        {
            int count = 0; // subtract ourself
            foreach (ulong guid in Me.PartymemberGuids)
            {
                if (guid != 0)
                {
                    count++;
                }
            }

            return count;
        }

        private double GetFollowAngle(int memberCount, int myPosition)
        {
            return 2 * Math.PI / (memberCount * myPosition);
        }

        private Vector3 CalculateMovementOffset(Vector3 posToMoveTo, double angle, double distance)
        {
            return new Vector3(
                posToMoveTo.X + (Math.Cos(angle) * (distance / 2) - XOffset),
                posToMoveTo.Y + (Math.Sin(angle) * (distance / 2) - YOffset),
                posToMoveTo.Z);
        }

        /// <summary>
        /// Returns a list with Units that we are allowed to follow: Party Units if:
        /// AmeisenDataHolder.Instance.IsAllowedToFollowParty is true. ...
        /// </summary>
        /// <returns>List containing all Units we are able to follow</returns>
        private List<Unit> GetUnitsToFollow()
        {
            List<Unit> tempList = new List<Unit>();

            try
            {
                if (AmeisenDataHolder.IsAllowedToFollowParty)
                {
                    List<ulong> partymembers = Me.PartymemberGuids;
                    foreach (ulong guid in partymembers)
                    {
                        if (guid != 0)
                        {
                            tempList.Add((Unit)GetWoWObjectFromGUID(guid));
                        }
                    }
                }
            }
            catch { }

            return tempList;
        }

        // Get the first Unit from our active Unit list and refresh it.
        private void RefreshActiveUnit()
        {
            foreach (Unit u in ActiveUnits)
            {
                if (u == null)
                {
                    continue;
                }

                u.Update();
                ActiveUnit = u;
                return;
            }
            ActiveUnit = null;
        }
    }
}
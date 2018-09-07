using AmeisenBotData;
using AmeisenBotUtilities;
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

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        public override void DoThings()
        {
            ActiveUnits = GetUnitsToFollow();
            RefreshActiveUnit();
            Me?.Update();
            ActiveUnit?.Update();

            // When we are far enough away, follow
            if (Me == null || ActiveUnit == null)
                return;

            if (Utils.GetDistance(Me.pos, ActiveUnit.pos)
                > AmeisenSettings.Instance.Settings.followDistance)
            {
                // Dont add waypoints twice
                if (!WaypointQueue.Contains(ActiveUnit.pos))
                {
                    WaypointQueue.Enqueue(ActiveUnit.pos);
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
            foreach (WowObject p in AmeisenDataHolder.Instance.ActiveWoWObjects)
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
        }

        public override void Stop()
        {
            base.Stop();
        }

        /// <summary>
        /// Returns a list with Units that we are allowed to follow: Party Units if:
        /// AmeisenDataHolder.Instance.IsAllowedToFollowParty is true. ...
        /// </summary>
        /// <returns>List containing all Units we are able to follow</returns>
        private List<Unit> GetUnitsToFollow()
        {
            List<Unit> tempList = new List<Unit>();

            if (AmeisenDataHolder.Instance.IsAllowedToFollowParty)
            {
                List<ulong> partymembers = Me.PartymemberGuids;
                foreach (ulong guid in partymembers)
                {
                    if (guid != 0)
                        tempList.Add((Unit)GetWoWObjectFromGUID(guid));
                }
            }

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
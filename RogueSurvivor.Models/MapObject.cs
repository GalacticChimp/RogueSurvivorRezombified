﻿using System;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public class MapObject
    {
        [Serializable]
        public enum Break : byte
        {
            UNBREAKABLE,
            BREAKABLE,
            BROKEN
        }

        [Serializable]
        public enum Fire : byte
        {
            UNINFLAMMABLE,
            BURNABLE,
            ONFIRE,
            ASHES
        }

        [Flags]
        enum Flags
        {
            NONE = 0,
            IS_AN = (1 << 0),
            IS_PLURAL = (1 << 1),
            IS_MATERIAL_TRANSPARENT = (1 << 2),
            IS_WALKABLE = (1 << 3),
            IS_CONTAINER = (1 << 4),
            IS_COUCH = (1 << 5),
            GIVES_WOOD = (1 << 6),
            IS_MOVABLE = (1 << 7),
            BREAKS_WHEN_FIRED_THROUGH = (1 << 8),
            STANDON_FOV_BONUS = (1<<9)
        }
        
        string m_ImageID;
        string m_HiddenImageID;
        string m_Name;

        Flags m_Flags;

        int m_JumpLevel;
        int m_Weight;

        Break m_BreakState = Break.UNBREAKABLE;
        int m_MaxHitPoints;
        int m_HitPoints;

        Fire m_FireState = Fire.UNINFLAMMABLE;

        Location m_Location;

        public string AName
        {
            get { return (IsAn ? "an " : IsPlural ? "some " : "a ") + m_Name; }
        }

        public string TheName
        {
            get { return "the " + m_Name; }
        }

        public bool IsAn
        {
            get { return GetFlag(Flags.IS_AN); }
            set { SetFlag(Flags.IS_AN, value); }
        }

        public bool IsPlural
        {
            get { return GetFlag(Flags.IS_PLURAL); }
            set { SetFlag(Flags.IS_PLURAL, value); }
        }

        public string ImageID
        {
            get { return m_ImageID; }
            set { m_ImageID = value; }
        }

        /// <summary>
        /// Gray level image shown when the object is known to the player (visited) but currently not in view.
        /// </summary>
        public string HiddenImageID
        {
            get { return m_HiddenImageID; }
        }

        public Location Location
        {
            get { return m_Location; }
            set { m_Location = value; }
        }

        public virtual bool IsTransparent
        {
            get
            {
                if (m_FireState == Fire.ONFIRE)
                    return false;
                if (m_BreakState == Break.BROKEN)
                    return true;
                if (m_FireState == Fire.ASHES)
                    return true;

                return GetFlag(Flags.IS_MATERIAL_TRANSPARENT);
            }
        }

        public bool IsMaterialTransparent
        {
            get { return GetFlag(Flags.IS_MATERIAL_TRANSPARENT); }
            set { SetFlag(Flags.IS_MATERIAL_TRANSPARENT, value); }
        }

        public bool IsWalkable
        {
            get { return GetFlag(Flags.IS_WALKABLE); }
            set { SetFlag(Flags.IS_WALKABLE, value); }
        }

        public int JumpLevel
        {
            get { return m_JumpLevel; }
            set { m_JumpLevel = value; }
        }

        /// <summary>
        /// Gets if object are walkable by actors with the CanJump ability.
        /// </summary>
        public bool IsJumpable
        {
            get { return m_JumpLevel > 0; }
        }

        /// <summary>
        /// Gets or sets if the object behaves as a container : an actor bumping into the object will pickup items stacked there.
        /// </summary>
        public bool IsContainer
        {
            get { return GetFlag(Flags.IS_CONTAINER); }
            set { SetFlag(Flags.IS_CONTAINER, value); }
        }

        public bool IsCouch
        {
            get { return GetFlag(Flags.IS_COUCH); }
            set { SetFlag(Flags.IS_COUCH, value); }
        }

        public bool IsBreakable
        {
            get { return m_BreakState == Break.BREAKABLE; }
        }

        public Break BreakState
        {
            get { return m_BreakState; }
            set { m_BreakState = value; }
        }

        public bool GivesWood
        {
            get { return GetFlag(Flags.GIVES_WOOD) && m_BreakState != Break.BROKEN; }
            set { SetFlag(Flags.GIVES_WOOD, value); }
        }

        public bool IsMovable
        {
            get { return GetFlag(Flags.IS_MOVABLE); }
            set { SetFlag(Flags.IS_MOVABLE, value); }
        }

        public bool BreaksWhenFiredThrough
        {
            get { return GetFlag(Flags.BREAKS_WHEN_FIRED_THROUGH); }
            set { SetFlag(Flags.BREAKS_WHEN_FIRED_THROUGH, value); }
        }

        public bool StandOnFovBonus
        {
            get { return GetFlag(Flags.STANDON_FOV_BONUS); }
            set { SetFlag(Flags.STANDON_FOV_BONUS, value); }
        }

        public int Weight
        {
            get { return m_Weight; }
            set { m_Weight = Math.Max(1, value); }
        }

        public bool IsFlammable
        {
            get { return m_FireState == Fire.ONFIRE || m_FireState == Fire.BURNABLE; }
        }

        public bool IsOnFire
        {
            get { return m_FireState == Fire.ONFIRE; }
        }

        public bool IsBurntToAshes
        {
            get { return m_FireState == Fire.ASHES; }
        }

        public Fire FireState
        {
            get { return m_FireState; }
            set { m_FireState = value; }
        }

        public int HitPoints
        {
            get { return m_HitPoints; }
            set { m_HitPoints = value; }
        }

        public int MaxHitPoints
        {
            get { return m_MaxHitPoints; }
        }

        public MapObject(string aName, string hiddenImageID)
            : this(aName, hiddenImageID, Break.UNBREAKABLE, Fire.UNINFLAMMABLE, 0)
        {
        }

        public MapObject(string aName, string hiddenImageID, Break breakable, Fire burnable, int hitPoints)
        {
            if (aName == null)
                throw new ArgumentNullException("aName");
            if (hiddenImageID == null)
                throw new ArgumentNullException("hiddenImageID");

            m_Name = aName;
            m_ImageID = m_HiddenImageID = hiddenImageID;
            m_BreakState = breakable;
            m_FireState = burnable;
            if (breakable != Break.UNBREAKABLE || burnable != Fire.UNINFLAMMABLE)
                m_HitPoints = m_MaxHitPoints = hitPoints;
        }

        public bool CanPushObjectTo(Point toPos, out string reason)
        {
            ///////////////////////////
            // Not pushable there:
            // 1. Out of bounds.
            // 2. Not walkable.
            // 3. Another object there.
            // 4. An actor there.
            ///////////////////////////

            // 1. Out of bounds.
            if (!Location.Map.IsInBounds(toPos))
            {
                reason = "out of map";
                return false;
            }

            // 2. Not walkable.
            if (!Location.Map.GetTileAt(toPos.X, toPos.Y).Model.IsWalkable)
            {
                reason = "blocked by an obstacle";
                return false;
            }

            // 3. Another object there.
            if (Location.Map.GetMapObjectAt(toPos) != null)
            {
                reason = "blocked by an object";
                return false;
            }

            // 4. An actor there.
            if (Location.Map.GetActorAt(toPos) != null)
            {
                reason = "blocked by someone";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        private bool GetFlag(Flags f) { return (m_Flags & f) != 0; }
        private void SetFlag(Flags f, bool value) { if (value) m_Flags |= f; else m_Flags &= ~f; }
        private void OneFlag(Flags f) { m_Flags |= f; }
        private void ZeroFlag(Flags f) { m_Flags &= ~f; }
    }
}
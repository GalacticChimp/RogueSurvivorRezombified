using System;
using System.Collections.Generic;
using System.Drawing;
using djack.RogueSurvivor.Data.Items;
using djack.RogueSurvivor.Data.Enums;
using djack.RogueSurvivor.Data.Helpers;
using djack.RogueSurvivor.Common;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public class TrustRecord
    {
        public Actor Actor { get; set; }
        public int Trust { get; set; }
    }

    [Serializable]
    public class Actor
    {
        public const int BARRICADING_MAX = 2 * DoorWindow.BASE_HITPOINTS;
        public static float SKILL_ZTRACKER_SMELL_BONUS = 0.10f;
        public const int FOOD_BASE_POINTS = WorldTime.TURNS_PER_HOUR * 48;
        public const int FOOD_HUNGRY_LEVEL = FOOD_BASE_POINTS / 2;

        public const int ROT_BASE_POINTS = WorldTime.TURNS_PER_DAY * 4;
        public const int ROT_HUNGRY_LEVEL = ROT_BASE_POINTS / 2;

        public const int SLEEP_BASE_POINTS = WorldTime.TURNS_PER_HOUR * 60;  // 60 = starting game at midnight => sleepy in late evening.
        public const int SLEEP_SLEEPY_LEVEL = SLEEP_BASE_POINTS / 2;
        public const int STAMINA_MIN_FOR_ACTIVITY = 10;

        public const int MURDERER_SPOTTING_BASE_CHANCE = 5;
        public const int MURDERER_SPOTTING_DISTANCE_PENALTY = 1;
        public const int MURDER_SPOTTING_MURDERCOUNTER_BONUS = 5;

        public const int SANITY_BASE_POINTS = WorldTime.TURNS_PER_DAY * 4;
        public const int SANITY_UNSTABLE_LEVEL = SANITY_BASE_POINTS / 2;

        public const int TRUST_TRUSTING_THRESHOLD = 12 * WorldTime.TURNS_PER_HOUR; // 12h of sticking together.

        public static int SKILL_LEADERSHIP_FOLLOWER_BONUS = 1;

        /// <summary>
        /// When sleeping on a couch, how many sleep points per turn are regenerated.
        /// </summary>
        const int SLEEP_COUCH_SLEEPING_REGEN = 1 + SLEEP_BASE_POINTS / (12 * WorldTime.TURNS_PER_HOUR);

        /// <summary>
        /// When sleeping out of a couch, how many sleep points per turn are regenerated.
        /// </summary>
        public const int SLEEP_NOCOUCH_SLEEPING_REGEN = (2 * SLEEP_COUCH_SLEEPING_REGEN) / 3;

        /// <summary>
        /// When sleeping on a couch, chance to heal per turn.
        /// </summary>
        public const int SLEEP_ON_COUCH_HEAL_CHANCE = 5;

        [Flags]
        enum Flags
        {
            NONE = 0,
            IS_UNIQUE = (1 << 0),
            IS_PROPER_NAME = (1 << 1),
            IS_PLURAL_NAME = (1 << 2),
            IS_DEAD = (1 << 3),
            IS_RUNNING = (1 << 4),
            IS_SLEEPING = (1 << 5)
        }

        Flags m_Flags;

        int m_ModelID;
        /*bool m_IsUnique;*/
        int m_FactionID;
        int m_GangID;
        string m_Name;
        /*bool m_IsProperName;
        bool m_IsPluralName;*/
        ActorController m_Controller;
        bool m_isBotPlayer;  // alpha10.1
        ActorSheet m_Sheet;
        int m_SpawnTime;

        Inventory m_Inventory = null;
        Doll m_Doll;
        int m_HitPoints;
        int m_previousHitPoints;
        int m_StaminaPoints;
        int m_previousStamina;
        int m_FoodPoints;
        int m_previousFoodPoints;
        int m_SleepPoints;
        int m_previousSleepPoints;
        int m_Sanity;
        int m_previousSanity;
        Location m_Location;
        int m_ActionPoints;
        int m_LastActionTurn;
        Activity m_Activity = Activity.IDLE;
        Actor m_TargetActor;
        int m_AudioRangeMod;
        Attack m_CurrentMeleeAttack;
        Attack m_CurrentRangedAttack;
        Defence m_CurrentDefence;
        Actor m_Leader;
        List<Actor> m_Followers = null;
        int m_TrustInLeader;
        List<TrustRecord> m_TrustList = null;
        int m_KillsCount;
        List<Actor> m_AggressorOf = null;
        List<Actor> m_SelfDefenceFrom = null;
        int m_MurdersCounter;
        int m_Infection;
        Corpse m_DraggedCorpse;
        // alpha10 moved out of Actor
        //List<Item> m_BoringItems = null;
        // alpha10
        bool m_IsInvincible;
        int m_OdorSuppressorCounter;

        /// <summary>
        /// Gets or sets model. Setting model reset inventory and all stats to the model default values.
        /// </summary>
        public ActorModel Model
        {
            get { return Models.Actors[m_ModelID]; }
            set
            {
                m_ModelID = value.ID;
                OnModelSet();
            }
        }

        public bool IsUnique
        {
            get { return GetFlag(Flags.IS_UNIQUE); }
            set { SetFlag(Flags.IS_UNIQUE, value); }
        }

        public Faction Faction
        {
            get { return Models.Factions[m_FactionID]; }
            set { m_FactionID = value.ID; }
        }

        /// <summary>
        /// Appends "(YOU) " if the actor is the player.
        /// </summary>
        public string Name
        {
            get { return IsPlayer ? "(YOU) " + m_Name : m_Name; }
            set
            {
                m_Name = value;
                if (value != null)
                    m_Name.Replace("(YOU) ", "");
            }
        }

        /// <summary>
        /// Raw name without "(YOU) " for the player.
        /// </summary>
        public string UnmodifiedName
        {
            get { return m_Name; }
        }

        public bool IsProperName
        {
            get { return GetFlag(Flags.IS_PROPER_NAME); }
            set { SetFlag(Flags.IS_PROPER_NAME, value); }
        }

        public bool IsPluralName
        {
            get { return GetFlag(Flags.IS_PLURAL_NAME); }
            set { SetFlag(Flags.IS_PLURAL_NAME, value); }
        }

        public string TheName
        {
            get { return IsProperName || IsPluralName ? Name : "the " + m_Name; }
        }

        public ActorController Controller
        {
            get { return m_Controller; }
            set
            {
                if (m_Controller != null)
                    m_Controller.LeaveControl();
                m_Controller = value;
                if (m_Controller != null)
                    m_Controller.TakeControl(this);
            }
        }

        /// <summary>
        /// Gets if this actor is controlled by the player or bot.
        /// </summary>
        public bool IsPlayer
        {
            get { return m_Controller != null && m_Controller is PlayerController; }
        }

        // alpha10.1 bot
        /// <summary>
        /// Gets or set if this actor is declared as controlled by a bot.
        /// The controller is STILL PlayerController and IsPlayer will still return true.
        /// We need this because in some rare parts of the code outside of RogueGame we need to know if the player is a bot or not.
        /// See RogueGame.
        /// </summary>
        public bool IsBotPlayer
        {
            get { return m_isBotPlayer; }
            set { m_isBotPlayer = value;  }
        }

        public int SpawnTime
        {
            get { return m_SpawnTime; }
        }

        public int GangID
        {
            get { return m_GangID; }
            set { m_GangID = value; }
        }

        public bool IsInAGang
        {
            get { return m_GangID != 0; }//TODO (int)GameGangs.IDs.NONE; }
        }

        public Doll Doll
        {
            get { return m_Doll; }
        }

        public bool IsDead
        {
            get { return GetFlag(Flags.IS_DEAD); }
            set { SetFlag(Flags.IS_DEAD, value); }
        }

        public bool IsSleeping
        {
            get { return GetFlag(Flags.IS_SLEEPING); }
            set { SetFlag(Flags.IS_SLEEPING, value); }
        }

        public bool IsRunning
        {
            get { return GetFlag(Flags.IS_RUNNING); }
            set { SetFlag(Flags.IS_RUNNING, value); }
        }

        public Inventory Inventory
        {
            get { return m_Inventory; }
            set { m_Inventory = value; }
        }

        public int HitPoints
        {
            get { return m_HitPoints; }
            set
            {
                if (m_IsInvincible && value < m_HitPoints) // alpha10
                    return;
                m_HitPoints = value;
            }
        }

        public int PreviousHitPoints
        {
            get { return m_previousHitPoints; }
            set { m_previousHitPoints = value; }
        }

        public int StaminaPoints
        {
            get { return m_StaminaPoints; }
            set
            {
                if (m_IsInvincible && value < m_StaminaPoints) // alpha10
                    return;
                m_StaminaPoints = value;
            }
        }

        public int PreviousStaminaPoints
        {
            get { return m_previousStamina; }
            set { m_previousStamina = value; }
        }

        public int FoodPoints
        {
            get { return m_FoodPoints; }
            set
            {
                if (m_IsInvincible && value < m_FoodPoints) // alpha10
                    return;
                m_FoodPoints = value;
            }
        }

        public int PreviousFoodPoints
        {
            get { return m_previousFoodPoints; }
            set { m_previousFoodPoints = value; }
        }

        public int SleepPoints
        {
            get { return m_SleepPoints; }
            set
            {
                if (m_IsInvincible && value < m_SleepPoints) // alpha10
                    return;
                m_SleepPoints = value;
            }
        }

        public int PreviousSleepPoints
        {
            get { return m_previousSleepPoints; }
            set { m_previousSleepPoints = value; }
        }

        public int Sanity
        {
            get { return m_Sanity; }
            set
            {
                if (m_IsInvincible && value < m_Sanity) // alpha10
                    return;
                m_Sanity = value;
            }
        }

        public int PreviousSanity
        {
            get { return m_previousSanity; }
            set { m_previousSanity = value; }
        }

        public ActorSheet Sheet
        {
            get { return m_Sheet; }
        }

        public int ActionPoints
        {
            get { return m_ActionPoints; }
            set { m_ActionPoints = value; }
        }

        public int LastActionTurn
        {
            get { return m_LastActionTurn; }
            set { m_LastActionTurn = value; }
        }

        public Location Location
        {
            get { return m_Location; }
            set { m_Location = value; }
        }

        public Activity Activity
        {
            get { return m_Activity; }
            set { m_Activity = value; }
        }

        public Actor TargetActor
        {
            get { return m_TargetActor; }
            set { m_TargetActor = value; }
        }

        public int AudioRange
        {
            get { return m_Sheet.BaseAudioRange + m_AudioRangeMod; }
        }

        public int AudioRangeMod
        {
            get { return m_AudioRangeMod; }
            set { m_AudioRangeMod = value; }
        }

        public Attack CurrentMeleeAttack
        {
            get { return m_CurrentMeleeAttack; }
            set { m_CurrentMeleeAttack = value; }
        }

        public Attack CurrentRangedAttack
        {
            get { return m_CurrentRangedAttack; }
            set { m_CurrentRangedAttack = value; }
        }

        public Defence CurrentDefence
        {
            get { return m_CurrentDefence; }
            set { m_CurrentDefence = value; }
        }

        public Actor Leader
        {
            get { return m_Leader; }
        }

        /// <summary>
        /// Gets if has a leader and he is alive.
        /// </summary>
        public bool HasLeader
        {
            get { return m_Leader != null && !m_Leader.IsDead; }
        }

        public int TrustInLeader
        {
            get { return m_TrustInLeader; }
            set { m_TrustInLeader = value; }
        }

        public IEnumerable<Actor> Followers
        {
            get { return m_Followers; }
        }

        public int CountFollowers
        {
            get
            {
                if (m_Followers == null)
                    return 0;
                return m_Followers.Count;
            }
        }

        public int KillsCount
        {
            get { return m_KillsCount; }
            set { m_KillsCount = value; }
        }

        public IEnumerable<Actor> AggressorOf
        {
            get { return m_AggressorOf; }
        }

        public int CountAggressorOf
        {
            get
            {
                if (m_AggressorOf == null)
                    return 0;
                return m_AggressorOf.Count;
            }
        }

        public IEnumerable<Actor> SelfDefenceFrom
        {
            get { return m_SelfDefenceFrom; }
        }

        public int CountSelfDefenceFrom
        {
            get
            {
                if (m_SelfDefenceFrom == null)
                    return 0;
                return m_SelfDefenceFrom.Count;
            }
        }

        public int MurdersCounter
        {
            get { return m_MurdersCounter; }
            set { m_MurdersCounter = value; }
        }

        public int Infection
        {
            get { return m_Infection; }
            set
            {
                if (m_IsInvincible && value > m_Infection) // alpha10
                    return;
                m_Infection = value;
            }
        }

        public Corpse DraggedCorpse
        {
            get { return m_DraggedCorpse; }
            set { m_DraggedCorpse = value; }
        }

        // alpha 10
        public bool IsInvincible
        {
            get { return m_IsInvincible; }
            set { m_IsInvincible = value; }
        }
        
        public int OdorSuppressorCounter
        {
            get { return m_OdorSuppressorCounter; }
            set { m_OdorSuppressorCounter = value; }
        }

        public Actor(ActorModel model, Faction faction, string name, bool isProperName, bool isPluralName, int spawnTime)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (faction == null)
                throw new ArgumentNullException("faction");
            if (name == null)
                throw new ArgumentNullException("name");

            m_ModelID = model.ID;
            m_FactionID = faction.ID;
            // TODO
            m_GangID = 0;//(int)GameGangs.IDs.NONE;
            m_Name = name;
            this.IsProperName = isProperName;
            this.IsPluralName = isPluralName;
            m_Location = new Location();
            m_SpawnTime = spawnTime;
            this.IsUnique = false;
            this.IsDead = false;

            OnModelSet();
        }

        public Actor(ActorModel model, Faction faction, int spawnTime)
            : this(model, faction, model.Name, false, false, spawnTime)
        {
        }

        void OnModelSet()
        {
            ActorModel model = this.Model;

            m_Doll = new Doll(model.DollBody);
            m_Sheet = new ActorSheet(model.StartingSheet);

            // starting points maxed.
            m_ActionPoints = m_Doll.Body.Speed;
            m_HitPoints = m_previousHitPoints = m_Sheet.BaseHitPoints;
            m_StaminaPoints = m_previousStamina = m_Sheet.BaseStaminaPoints;
            m_FoodPoints = m_previousFoodPoints = m_Sheet.BaseFoodPoints;
            m_SleepPoints = m_previousSleepPoints = m_Sheet.BaseSleepPoints;
            m_Sanity = m_previousSanity = m_Sheet.BaseSanity;

            // create inventory.
            if (model.Abilities.HasInventory)
                m_Inventory = new Inventory(model.StartingSheet.BaseInventoryCapacity);

            // starting attacks.
            m_CurrentMeleeAttack = model.StartingSheet.UnarmedAttack;
            m_CurrentDefence = model.StartingSheet.BaseDefence;
            m_CurrentRangedAttack = Attack.BLANK;
        }

        public void AddFollower(Actor other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            if (m_Followers != null && m_Followers.Contains(other))
                throw new ArgumentException("other is already a follower");

            if (m_Followers == null)
                m_Followers = new List<Actor>(1);
            m_Followers.Add(other);

            if (other.Leader != null)
                other.Leader.RemoveFollower(other);
            other.m_Leader = this;
        }

        public void RemoveFollower(Actor other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            if (m_Followers == null)
                throw new InvalidOperationException("no followers");

            m_Followers.Remove(other);
            if (m_Followers.Count == 0)
                m_Followers = null;

            other.m_Leader = null;

            // reset directives & order.
            AIController ai = other.Controller as AIController;
            if (ai != null)
            {
                ai.Directives.Reset();
                ai.SetOrder(null);
            }
        }

        public void RemoveAllFollowers()
        {
            while (m_Followers != null && m_Followers.Count > 0)
            {
                RemoveFollower(m_Followers[0]);
            }
        }

        public void SetTrustIn(Actor other, int trust)
        {
            if (m_TrustList == null)
            {
                m_TrustList = new List<TrustRecord>(1) { new TrustRecord() { Actor = other, Trust = trust } };
                return;
            }

            foreach (TrustRecord r in m_TrustList)
            {
                if (r.Actor == other)
                {
                    r.Trust = trust;
                    return;
                }
            }

            m_TrustList.Add(new TrustRecord() { Actor = other, Trust = trust });
        }

        public int GetTrustIn(Actor other)
        {
            if (m_TrustList == null) return 0;
            foreach (TrustRecord r in m_TrustList)
            {
                if (r.Actor == other)
                    return r.Trust;
            }
            return 0;
        }
        
        // alpha10
        /// <summary>
        /// Is this other actor our leader, a follower or a mate.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsInGroupWith(Actor other)
        {
            // my leader?
            if (HasLeader && m_Leader == other)
                return true;

            // a mate?
            if (other.HasLeader && other.Leader == m_Leader)
                return true;

            // a follower?
            if (m_Followers != null)
                if (m_Followers.Contains(other))
                    return true;

            // nope
            return false;
        }

        public void MarkAsAgressorOf(Actor other)
        {
            if (other == null || other.IsDead)
                return;

            if (m_AggressorOf == null)
                m_AggressorOf = new List<Actor>(1);
            else if (m_AggressorOf.Contains(other))
                return;
            m_AggressorOf.Add(other);
        }

        public void MarkAsSelfDefenceFrom(Actor other)
        {
            if (other == null || other.IsDead)
                return;

            if (m_SelfDefenceFrom == null)
                m_SelfDefenceFrom = new List<Actor>(1);
            else if (m_SelfDefenceFrom.Contains(other))
                return;
            m_SelfDefenceFrom.Add(other);
        }

        public bool IsAggressorOf(Actor other)
        {
            if (m_AggressorOf == null)
                return false;
            return m_AggressorOf.Contains(other);
        }

        public bool IsSelfDefenceFrom(Actor other)
        {
            if (m_SelfDefenceFrom == null)
                return false;
            return m_SelfDefenceFrom.Contains(other);
        }

        public void RemoveAggressorOf(Actor other)
        {
            if (m_AggressorOf == null)
                return;
            m_AggressorOf.Remove(other);
            if (m_AggressorOf.Count == 0)
                m_AggressorOf = null;
        }

        public void RemoveSelfDefenceFrom(Actor other)
        {
            if (m_SelfDefenceFrom == null)
                return;
            m_SelfDefenceFrom.Remove(other);
            if (m_SelfDefenceFrom.Count == 0)
                m_SelfDefenceFrom = null;
        }

        public void RemoveAllAgressorSelfDefenceRelations()
        {
            while (m_AggressorOf != null)
            {
                Actor other = m_AggressorOf[0];
                RemoveAggressorOf(other);
                other.RemoveSelfDefenceFrom(this);
            }
            while (m_SelfDefenceFrom != null)
            {
                Actor other = m_SelfDefenceFrom[0];
                RemoveSelfDefenceFrom(other);
                other.RemoveAggressorOf(this);
            }
        }

        // alpha10 obsolete, moved to Rules
#if false

        /// <summary>
        /// Check for agressor/self defence relation.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool AreDirectEnemies(Actor other)
        {
            if (other == null || other.IsDead)
                return false;

            if (m_AggressorOf != null)
            {
                if (m_AggressorOf.Contains(other))
                    return true;
            }
            if (m_SelfDefenceFrom != null)
            {
                if (m_SelfDefenceFrom.Contains(other))
                    return true;
            }

            if (other.IsAggressorOf(this))
                return true;
            if (other.IsSelfDefenceFrom(this))
                return true;

            // nope.
            return false;
        }

        /// <summary>
        /// Check for direct enemies through leader/followers.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool AreIndirectEnemies(Actor other)
        {
            if (other == null || other.IsDead)
                return false;

            // check my leader and my mates, if any.
            if (this.HasLeader)
            {
                // my leader.
                if (m_Leader.AreDirectEnemies(other))
                    return true;
                // my mates = my leader followers.
                foreach (Actor mate in m_Leader.Followers)
                    if (mate != this && mate.AreDirectEnemies(other))
                        return true;
            }

            // check my followers, if any.
            if (this.CountFollowers > 0)
            {
                foreach (Actor fo in m_Followers)
                    if (fo.AreDirectEnemies(other))
                        return true;
            }

            // check their leader and mates.
            if (other.HasLeader)
            {
                // his leader.
                if (other.Leader.AreDirectEnemies(this))
                    return true;
                // his mates = his leader followers.
                foreach (Actor mate in other.Leader.Followers)
                    if (mate != other && mate.AreDirectEnemies(this))
                        return true;
            }

            // nope.
            return false;
        }
#endif

#if false
        /// <summary>
        /// Make sure another actor is added to the list of personal enemies.
        /// </summary>
        /// <param name="e"></param>
        public void MarkAsPersonalEnemy(Actor e)
        {
            if (e == null || e.IsDead)
                return;

            if (m_PersonalEnemies == null)
                m_PersonalEnemies = new List<Actor>(1);
            else if (m_PersonalEnemies.Contains(e))
                return;

            m_PersonalEnemies.Add(e);
        }

        public void RemoveAsPersonalEnemy(Actor e)
        {
            if (m_PersonalEnemies == null)
                return;

            m_PersonalEnemies.Remove(e);

            // minimize data size.
            if (m_PersonalEnemies.Count == 0)
                m_PersonalEnemies = null; 
        }

        public void RemoveAllPersonalEnemies()
        {
            if (m_PersonalEnemies == null)
                return;

            while (m_PersonalEnemies.Count > 0)
            {
                Actor e = m_PersonalEnemies[0];
                e.RemoveAsPersonalEnemy(this);
                m_PersonalEnemies.Remove(e);
            }
        }

        /// <summary>
        /// Checks for own personal enemy as well as our band (leader & followers).
        /// So in a band we share all personal enemies.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool HasActorAsPersonalEnemy(Actor other)
        {
            if (other == null || other.IsDead)
                return false;

            // first check personal list.
            if (m_PersonalEnemies != null)
            {
                if (m_PersonalEnemies.Contains(other))
                    return true;
            }

            // check my leader and my mates, if any.
            if (this.HasLeader)
            {
                // my leader.
                if (m_Leader.HasDirectPersonalEnemy(other))
                    return true;
                // my mates = my leader followers.
                foreach (Actor mate in m_Leader.Followers)
                    if (mate != this && mate.HasDirectPersonalEnemy(other))
                        return true;
            }

            // check my followers, if any.
            if (this.CountFollowers > 0)
            {
                foreach (Actor fo in m_Followers)
                    if (fo.HasDirectPersonalEnemy(other))
                        return true;
            }

            // nope.
            return false;
        }

        /// <summary>
        /// Checks only personal enemy list, do not check our band.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool HasDirectPersonalEnemy(Actor other)
        {
            if (m_PersonalEnemies == null)
                return false;
            return m_PersonalEnemies.Contains(other);
        }

        public void MarkAsSelfDefence(Actor e)
        {
            if (e == null || e.IsDead)
                return;

            if (m_SelfDefence == null)
                m_SelfDefence = new List<Actor>(1);
            else if (m_SelfDefence.Contains(e))
                return;

            m_SelfDefence.Add(e);
        }

        public void RemoveAsSelfDefence(Actor e)
        {
            if (m_SelfDefence == null)
                return;

            m_SelfDefence.Remove(e);

            // minimize data size.
            if (m_SelfDefence.Count == 0)
                m_SelfDefence = null;
        }

        public void RemoveAllSelfDefence()
        {
            if (m_SelfDefence == null)
                return;

            while (m_SelfDefence.Count > 0)
            {
                Actor e = m_SelfDefence[0];
                e.RemoveAsSelfDefence(this);
                m_SelfDefence.Remove(e);
            }
        }

        public bool HasDirectSelfDefence(Actor other)
        {
            if (m_SelfDefence == null)
                return false;
            return m_SelfDefence.Contains(other);
        }
#endif

        // alpha10 made item centric: moved out of Actor to ItemEntertainment
#if false
        #region Boring items
        public void AddBoringItem(Item it)
        {
            if (m_BoringItems == null) m_BoringItems = new List<Item>(1);
            if (m_BoringItems.Contains(it)) return;
            m_BoringItems.Add(it);
        }

        public bool IsBoredOf(Item it)
        {
            if (m_BoringItems == null) return false;
            return m_BoringItems.Contains(it);
        }
        #endregion
#endif

        public Item GetEquippedItem(DollPart part)
        {
            if (m_Inventory == null || part == DollPart.NONE)
                return null;

            foreach (Item it in m_Inventory.Items)
                if (it.EquippedPart == part)
                    return it;

            return null;
        }

        /// <summary>
        /// Assumed to be equiped at Right hand.
        /// </summary>
        /// <returns></returns>
        public Item GetEquippedWeapon()
        {
            return GetEquippedItem(DollPart.RIGHT_HAND);
        }

        // alpha10
        /// <summary>
        /// Assumed to be equiped at Right hand.
        /// </summary>
        /// <returns></returns>
        public ItemMeleeWeapon GetEquippedMeleeWeapon()
        {
            return GetEquippedItem(DollPart.RIGHT_HAND) as ItemMeleeWeapon;
        }

        // alpha10
        /// <summary>
        /// Assumed to be equiped at Right hand.
        /// </summary>
        /// <returns></returns>
        public ItemRangedWeapon GetEquippedRangedWeapon()
        {
            return GetEquippedItem(DollPart.RIGHT_HAND) as ItemRangedWeapon;
        }

        private bool GetFlag(Flags f) { return (m_Flags & f) != 0; }
        private void SetFlag(Flags f, bool value) { if (value) m_Flags |= f; else m_Flags &= ~f; }
        private void OneFlag(Flags f) { m_Flags |= f; }
        private void ZeroFlag(Flags f) { m_Flags &= ~f; }

        public void OptimizeBeforeSaving()
        {
            // remove dead target.
            if (m_TargetActor != null && m_TargetActor.IsDead) m_TargetActor = null;

            // alpha10 moved out of Actor
            //// trim.
            //if (m_BoringItems != null) m_BoringItems.TrimExcess();

            // alpha10
            // remove trust entries with dead actors.
            // side effect: this means revived actor will forget their trust after a save game!
            if (m_TrustList != null)
            {
                for (int i = 0; i < m_TrustList.Count;)
                {
                    if (m_TrustList[i].Actor.IsDead)
                        m_TrustList.RemoveAt(i);
                    else
                        i++;
                }
                if (m_TrustList.Count == 0)
                    m_TrustList = null;
            }
            // remove & trim agressor/self-defence entries with dead actors.
            // side effect: this means revived actor will forget their agressor/self-defence after a save game!
            if (m_AggressorOf != null)
            {
                for (int i = 0; i < m_AggressorOf.Count; )
                {
                    if (m_AggressorOf[i].IsDead)
                        m_AggressorOf.RemoveAt(i);
                    else
                        i++;
                }
                if (m_AggressorOf.Count == 0)
                    m_AggressorOf = null;
                else
                    m_AggressorOf.TrimExcess();
            }
            if (m_SelfDefenceFrom != null)
            {
                for (int i = 0; i < m_SelfDefenceFrom.Count;)
                {
                    if (m_SelfDefenceFrom[i].IsDead)
                        m_SelfDefenceFrom.RemoveAt(i);
                    else
                        i++;
                }
                if (m_SelfDefenceFrom.Count == 0)
                    m_SelfDefenceFrom = null;
                else
                    m_SelfDefenceFrom.TrimExcess();
            }

            // alpha10 inventory
            if (m_Inventory != null)
                m_Inventory.OptimizeBeforeSaving();
        }

        // Actions
        public void DoBarricadeDoor(DoorWindow door)
        {
            // TODO
            //// get barricading item.
            //ItemBarricadeMaterial it = this.Inventory.GetSmallestStackByType(typeof(ItemBarricadeMaterial)) as ItemBarricadeMaterial; // alpha10
            //ItemBarricadeMaterialModel m = it.Model as ItemBarricadeMaterialModel;

            //// do it.
            //this.Inventory.Consume(it);
            //door.BarricadePoints = Math.Min(door.BarricadePoints + m_Rules.ActorBarricadingPoints(this, m.BarricadingValue), Rules.BARRICADING_MAX);

            //// message.
            //bool isVisible = IsVisibleToPlayer(this) || IsVisibleToPlayer(door);
            //if (isVisible)
            //{
            //    AddMessage(MakeMessage(this, Conjugate(actor, VERB_BARRICADE), door));
            //}

            //// spend AP.
            //int barricadingCost = Rules.BASE_ACTION_COST;
            //SpendActorActionPoints(this, barricadingCost);
        }

        public bool CanActorBarricadeDoor(DoorWindow door, out string reason)
        {
            if (door == null)
                throw new ArgumentNullException("door");

            ////////////////////////////////////
            // Not close if any is true:
            // 1. Actor cannot barricade doors.
            // 2. Door is not closed or broken.
            // 3. Barricading limit reached.
            // 4. An actor is there.
            // 5. No barricading material.
            ////////////////////////////////////

            // 1. Actor cannot barricade doors.
            if (!Model.Abilities.CanBarricade)
            {
                reason = "no ability to barricade";
                return false;
            }

            // 2. Door is not closed or broken.
            if (door.State != DoorWindow.STATE_CLOSED && door.State != DoorWindow.STATE_BROKEN)
            {
                reason = "not closed or broken"; // alpha10 typo fix
                return false;
            }

            // 3. Barricading limit reached.
            if (door.BarricadePoints >= BARRICADING_MAX)
            {
                reason = "barricade limit reached";
                return false;
            }

            // 4. An actor is there.
            if (door.Location.Map.GetActorAt(door.Location.Position) != null)
            {
                reason = "someone is there";
                return false;
            }

            // 5. No barricading material.
            if (Inventory == null || Inventory.IsEmpty)
            {
                reason = "no items";
                return false;
            }
            if (!Inventory.HasItemOfType(typeof(ItemBarricadeMaterial)))
            {
                reason = "no barricading material";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool IsBashableFor(DoorWindow door, out string reason)
        {
            if (door == null)
                throw new ArgumentNullException("door");

            ///////////////////////////////
            // Not bashable:
            // 1. Actor cannot bash doors.
            // 2. Actor is tired.
            // 3. Door is not breakable.
            ///////////////////////////////

            // 1. Actor cannot bash doors.
            if (!Model.Abilities.CanBashDoors)
            {
                reason = "can't bash doors";
                return false;
            }

            // 2. Actor is tired. TODO
            //if (IsActorTired(this){
            //    reason = "tired";
            //    return false;
            //}

            // 2. Door is not breakable.
            if (door.BreakState != MapObject.Break.BREAKABLE && !door.IsBarricaded)
            {
                reason = "can't break this object";
                return false;
            }

            // all clear
            reason = "";
            return true;
        }

        public bool IsBreakableFor(MapObject mapObj, out string reason)
        {
            if (mapObj == null)
                throw new ArgumentNullException("mapObj");

            //////////////////////////////////
            // Not breakable:
            // 1. Actor cannot break.
            // 2. Actor is tired.
            // 3. Map object is not breakable.
            // 4. Another actor there.
            //////////////////////////////////

            // 1. Actor cannot break.
            if (!Model.Abilities.CanBreakObjects)
            {
                reason = "cannot break objects";
                return false;
            }

            //// 2. Actor is tired. TODO
            //if (IsActorTired(this))
            //{
            //    reason = "tired";
            //    return false;
            //}

            // 3. Map object is not breakable.
            DoorWindow door = mapObj as DoorWindow;
            bool isBarricadedDoor = (door != null && door.IsBarricaded);
            if (mapObj.BreakState != MapObject.Break.BREAKABLE && !isBarricadedDoor)
            {
                reason = "can't break this object";
                return false;
            }

            // 4. Another actor there.
            if (mapObj.Location.Map.GetActorAt(mapObj.Location.Position) != null)
            {
                reason = "someone is there";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public void DoBreak(MapObject mapObj)
        {
            /*
            Attack bashAttack = m_Rules.ActorMeleeAttack(this, this.CurrentMeleeAttack, null, mapObj);

            //Attacking a barricaded door.
            DoorWindow door = mapObj as DoorWindow;
            if (door != null && door.IsBarricaded)
            {
                // Spend APs & STA.
                int bashCost = Rules.BASE_ACTION_COST;
                SpendActorActionPoints(actor, bashCost);
                SpendActorStaminaPoints(actor, Rules.STAMINA_COST_MELEE_ATTACK);

                // Bash.
                door.BarricadePoints -= bashAttack.DamageValue;

                // loud noise.
                OnLoudNoise(door.Location.Map, door.Location.Position, "A loud *BASH*");

                // message.
                if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(door))
                {
                    if (IsVisibleToPlayer(door))
                    {
                        // alpha10 tell & show damage
                        Point screenPos = MapToScreen(mapObj.Location.Position);
                        AddOverlay(new OverlayImage(screenPos, GameImages.ICON_MELEE_DAMAGE));
                        AddOverlay(new OverlayText(screenPos.Add(DAMAGE_DX, DAMAGE_DY), Color.White, bashAttack.DamageValue.ToString(), Color.Black)); // alpha10
                        AddMessage(MakeMessage(actor, string.Format("{0} the barricade for {1} damage.", Conjugate(actor, VERB_BASH), bashAttack.DamageValue))); // alpha10
                        RedrawPlayScreen();
                        AnimDelay(actor.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
                        ClearOverlays();
                    }
                    else
                    {
                        AddMessage(MakeMessage(actor, string.Format("{0} the barricade.", Conjugate(actor, VERB_BASH)))); // alpha10
                    }
                }
                else
                {
                    if (m_Rules.RollChance(PLAYER_HEAR_BASH_CHANCE))
                        AddMessageIfAudibleForPlayer(door.Location, MakePlayerCentricMessage("You hear someone bashing barricades", door.Location.Position));

                }


                // done.
                return;
            }
            //Attacking a un-barricaded door or a normal object
            else
            {
                // Always hit.
                mapObj.HitPoints -= bashAttack.DamageValue;

                // Spend APs & STA.
                int bashCost = Rules.BASE_ACTION_COST;
                SpendActorActionPoints(actor, bashCost);
                SpendActorStaminaPoints(actor, Rules.STAMINA_COST_MELEE_ATTACK);

                // Broken?
                bool isBroken = false;
                if (mapObj.HitPoints <= 0)
                {
                    // breaks.
                    DoDestroyObject(mapObj);
                    isBroken = true;
                }

                // loud noise.
                OnLoudNoise(mapObj.Location.Map, mapObj.Location.Position, "A loud *CRASH*");

                // Message.
                bool isActorVisible = IsVisibleToPlayer(actor);
                bool isDoorVisible = IsVisibleToPlayer(mapObj);
                bool isPlayer = actor.IsPlayer;

                if (isActorVisible || isDoorVisible)
                {
                    if (isActorVisible)
                        AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(actor.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                    if (isDoorVisible)
                        AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(mapObj.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));

                    if (isBroken)
                    {
                        AddMessage(MakeMessage(actor, Conjugate(actor, VERB_BREAK), mapObj));
                        if (isActorVisible)
                            AddOverlay(new OverlayImage(MapToScreen(actor.Location.Position), GameImages.ICON_MELEE_ATTACK));
                        if (isDoorVisible)
                            AddOverlay(new OverlayImage(MapToScreen(mapObj.Location.Position), GameImages.ICON_KILLED));
                        RedrawPlayScreen();
                        AnimDelay(DELAY_LONG);
                    }
                    else
                    {
                        if (isDoorVisible)
                        {
                            AddMessage(MakeMessage(actor, string.Format("{0} {1} for {2} damage.", Conjugate(actor, VERB_BASH), mapObj.TheName, bashAttack.DamageValue))); // alpha10
                            AddOverlay(new OverlayImage(MapToScreen(mapObj.Location.Position), GameImages.ICON_MELEE_DAMAGE));
                            AddOverlay(new OverlayText(MapToScreen(mapObj.Location.Position).Add(DAMAGE_DX, DAMAGE_DY), Color.White, bashAttack.DamageValue.ToString(), Color.Black)); // alpha10
                        }
                        else if (isActorVisible)
                        {
                            AddMessage(MakeMessage(actor, string.Format("{0} {1}.", Conjugate(actor, VERB_BASH), mapObj.TheName))); // alpha10
                        }

                        if (isActorVisible)
                            AddOverlay(new OverlayImage(MapToScreen(actor.Location.Position), GameImages.ICON_MELEE_ATTACK));

                        RedrawPlayScreen();
                        AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
                    }

                    // alpha10 bug fix; clear overlays only if action is visible
                    ClearOverlays(); // was in the wrong place!
                }  // any is visible
                else
                {
                    if (isBroken)
                    {
                        if (m_Rules.RollChance(PLAYER_HEAR_BREAK_CHANCE))
                            AddMessageIfAudibleForPlayer(mapObj.Location, MakePlayerCentricMessage("You hear someone breaking furniture", mapObj.Location.Position));
                    }
                    else
                    {
                        if (m_Rules.RollChance(PLAYER_HEAR_BASH_CHANCE))
                            AddMessageIfAudibleForPlayer(mapObj.Location, MakePlayerCentricMessage("You hear someone bashing furniture", mapObj.Location.Position));
                    }
                }
            }
            */
        }

        public bool CanActorBuildFortification(Point pos, bool isLarge, out string reason)
        {
            ///////////////////////////
            // Can't if any is true:
            // 1. No carpentry skill.
            // 2. Not walkable.
            // 3. Not engouh material.
            // 4. Tile occupied.
            ///////////////////////////

            // 1. No carpentry skill. TODO
            //if (Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CARPENTRY) == 0)
            //{
            //    reason = "no skill in carpentry";
            //    return false;
            //}

            // 2. Not walkable.
            Map map = Location.Map;
            if (!map.GetTileAt(pos).Model.IsWalkable)
            {
                reason = "cannot build on walls";
                return false;
            }

            // 3. Not enough material. TODO
            //int need = ActorBarricadingMaterialNeedForFortification(actor, isLarge);
            //if (CountBarricadingMaterial(actor) < need)
            //{
            //    reason = String.Format("not enough barricading material, need {0}.", need);
            //    return false;
            //}

            // 4. Tile occupied.
            if (map.GetMapObjectAt(pos) != null || map.GetActorAt(pos) != null)
            {
                reason = "blocked";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public void DoBuildFortification(Point buildPos, bool isLarge)
        {
            // spend AP.
            //SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            //// consume material.
            //int need = m_Rules.ActorBarricadingMaterialNeedForFortification(actor, isLarge);
            //for (int i = 0; i < need; i++)
            //{
            //    Item it = actor.Inventory.GetSmallestStackByType(typeof(ItemBarricadeMaterial)); // alpha10
            //                                                                                     //actor.Inventory.GetFirstByType(typeof(ItemBarricadeMaterial));
            //    actor.Inventory.Consume(it);
            //}

            //// add object.
            //Fortification fortObj = isLarge ? m_TownGenerator.MakeObjLargeFortification(GameImages.OBJ_LARGE_WOODEN_FORTIFICATION) : m_TownGenerator.MakeObjSmallFortification(GameImages.OBJ_SMALL_WOODEN_FORTIFICATION);
            //actor.Location.Map.PlaceMapObjectAt(fortObj, buildPos);

            //// message.
            //if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(new Location(actor.Location.Map, buildPos)))
            //{
            //    AddMessage(MakeMessage(actor, String.Format("{0} a {1} fortification.", Conjugate(actor, VERB_BUILD), isLarge ? "large" : "small")));
            //}

            //// check traps.
            //CheckMapObjectTriggersTraps(actor.Location.Map, buildPos);
        }

        public ActorAction IsBumpableFor(Map map, int x, int y, out string reason)
        {
            reason = string.Empty;
            return null;
            /* TODO
            if (map == null)
                throw new ArgumentNullException("map");
            reason = "";

            ///////////////////////////////
            // Out of map : leave district?
            ///////////////////////////////
            if (!map.IsInBounds(x, y))
            {
                if (CanActorLeaveMap(this, out reason))
                {
                    reason = "";
                    return new ActionLeaveMap(this, game, new Point(x, y));
                }
                else
                {
                    return null;
                }
            }

            //////////////////////////////////////////
            // 1. Movement?
            // 2. Actor interact: fight or chat?
            // 3. Object interact?
            // 4. No action possible.
            //////////////////////////////////////////
            Point to = new Point(x, y);

            // 1. Move?
            ActionMoveStep moveAction = new ActionMoveStep(this, game, to);
            if (moveAction.IsLegal())
            {
                reason = "";
                return moveAction;
            }
            else
                reason = moveAction.FailReason;

            // 2. Actor interact: fight/chat/(AI:switch place)
            #region
            Actor targetActor = map.GetActorAt(to);
            if (targetActor != null)
            {
                // attacking?
                if (AreEnemies(this, targetActor))
                {
                    if (CanActorMeleeAttack(this, targetActor, out reason))
                        return new ActionMeleeAttack(this, game, targetActor);
                    else
                        return null;
                }

                // AI: switching place  // alpha10.1 handle bot like it was an AI
                if ((!IsPlayer || IsBotPlayer) &&
                    !targetActor.IsPlayer
                    && CanActorSwitchPlaceWith(this, targetActor, out reason))
                    return new ActionSwitchPlace(this, game, targetActor);

                // chatting?
                if (CanActorChatWith(this, targetActor, out reason))
                    return new ActionChat(this, game, targetActor);

                // nope, blocked.
                return null;
            }
            #endregion

            // 3. Object interact?
            #region
            MapObject mapObj = map.GetMapObjectAt(to);
            if (mapObj != null)
            {
                // 3.1 Door?
                #region
                DoorWindow door = mapObj as DoorWindow;
                if (door != null)
                {
                    if (door.IsClosed)  // closed: open/bash/barricade
                    {
                        if (IsOpenableFor(this, door, out reason))
                            return new ActionOpenDoor(this, game, door);
                        else if (IsBashableFor(this, door, out reason))
                            return new ActionBashDoor(this, game, door);
                        else
                            return null;
                    }
                    if (door.BarricadePoints > 0) // barricaded: bash
                    {
                        if (IsBashableFor(this, door, out reason))
                            return new ActionBashDoor(this, game, door);
                        else
                        {
                            reason = "cannot bash the barricade";
                            return null;
                        }
                    }
                }
                #endregion

                // 3.2 Container?
                if (CanActorGetItemFromContainer(this, to, out reason))
                    return new ActionGetFromContainer(this, game, to);

                // alpha10.1 removed break restriction, made civs ai get stuck in some rare cases but we punish break actions in baseai wander.
                //// 3.3 Break?
                if (IsBreakableFor(this, mapObj, out reason))
                    return new ActionBreak(this, game, mapObj);
                //// 3.3 Break? Only allow bashing actors to do that (don't want Civilians to bash stuff on their way)
                //if (actor.Model.Abilities.CanBashDoors && IsBreakableFor(actor, mapObj, out reason))
                //    return new ActionBreak(actor, game, mapObj);

                // 3.4 Power Generator?
                PowerGenerator powGen = mapObj as PowerGenerator;
                if (powGen != null)
                {
                    // Recharge battery powered item?
                    if (powGen.IsOn)
                    {
                        Item leftItem = actor.GetEquippedItem(DollPart.LEFT_HAND);
                        if (leftItem != null && CanActorRechargeItemBattery(actor, leftItem, out reason))
                            return new ActionRechargeItemBattery(actor, game, leftItem);
                        Item rightItem = actor.GetEquippedItem(DollPart.RIGHT_HAND);
                        if (rightItem != null && CanActorRechargeItemBattery(actor, rightItem, out reason))
                            return new ActionRechargeItemBattery(actor, game, rightItem);
                    }

                    // Switch?
                    if (IsSwitchableFor(actor, powGen, out reason))
                        // switch it.
                        return new ActionSwitchPowerGenerator(actor, game, powGen);

                    // Can do nothing by bumping.
                    return null;
                }

            }
            #endregion

            // 4. No action possible?
            //reason = "blocked";
            return null;

            */
        }

        public ActorAction IsBumpableFor(Location location, out string reason)
        {
            return this.IsBumpableFor(location.Map, location.Position.X, location.Position.Y, out reason);
        }

        public void DoChat(Actor target)
        {
            /*
            // spend APs.
            SpendActorActionPoints(this, Rules.BASE_ACTION_COST);

            // message
            bool isSpeakerVisible = IsVisibleToPlayer(this);
            bool isTargetVisible = IsVisibleToPlayer(target);
            if (isSpeakerVisible || isTargetVisible)
                AddMessage(MakeMessage(this, Conjugate(this, VERB_CHAT_WITH), target));

            // trade?
            if (m_Rules.CanActorInitiateTradeWith(this, target))
            {
                DoTrade(this, target);
            }

            // alpha10 recover san after "normal" chat or fast trade
            if (Model.Abilities.HasSanity)
            {
                RegenActorSanity(this, Rules.SANITY_RECOVER_CHAT_OR_TRADE);
                if (IsVisibleToPlayer(this))
                    AddMessage(MakeMessage(this, string.Format("{0} better after chatting with", Conjugate(this, VERB_FEEL)), target));
            }

            if (target.Model.Abilities.HasSanity)
            {
                RegenActorSanity(target, Rules.SANITY_RECOVER_CHAT_OR_TRADE);
                if (IsVisibleToPlayer(target))
                    AddMessage(MakeMessage(target, string.Format("{0} better after chatting with", Conjugate(this, VERB_FEEL)), this));
            }
            */
        }

        public bool IsClosableFor(DoorWindow door, out string reason)
        {
            if (door == null)
                throw new ArgumentNullException("door");

            ///////////////////////////////////////
            // Not close if any is true:
            // 1. Actor cannot use map objects.
            // 2. Door is not open.
            // 3. Another actor here.
            ///////////////////////////////////////

            // 1. Actor cannot use map objects.
            if (!Model.Abilities.CanUseMapObjects)
            {
                reason = "can't use objects";
                return false;
            }

            // 2. Door is not open.
            if (!door.IsOpen)
            {
                reason = "not open";
                return false;
            }

            // 3. Another actor here.
            if (door.Location.Map.GetActorAt(door.Location.Position) != null)
            {
                reason = "someone is there";
                return false;
            }

            // all clear
            reason = "";
            return true;
        }

        public void DoCloseDoor(DoorWindow door)
        {
            /*
            // Do it.
            door.SetState(DoorWindow.STATE_CLOSED);

            // Message.
            if (IsVisibleToPlayer(this) || IsVisibleToPlayer(door))
            {
                AddMessage(MakeMessage(this, Conjugate(this, VERB_CLOSE), door));
                RedrawPlayScreen();
            }

            // Spend APs.
            int closeCost = Rules.BASE_ACTION_COST;
            SpendActorActionPoints(this, closeCost);
            */
        }

        public bool CanActorDropItem(Item it, out string reason)
        {
            if (it == null)
                throw new ArgumentNullException("item");

            /////////////////////////
            // Can't if any is true:
            // 1. Equipped.
            // 2. Not in inventory.
            ////////////////////////

            // 1. Equipped.
            if (it.IsEquipped)
            {
                reason = "unequip first";
                return false;
            }

            // 2. Not in inventory.
            Inventory inv = Inventory;
            if (inv == null || !inv.Contains(it))
            {
                reason = "not in inventory";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public void DoDropItem(Item it)
        {
            /*
            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // which item to drop (original or a clone)
            Item dropIt = it;
            // discard?
            bool discardMe = false;

            // special case for traps and discared items.
            if (it is ItemTrap)
            {
                ItemTrap trap = it as ItemTrap;

                // drop one at a time.
                ItemTrap clone = trap.Clone();
                //alpha10 clone.IsActivated = trap.IsActivated;
                if (trap.IsActivated) // alpha10
                    clone.Activate(actor);
                dropIt = clone;

                // trap activates when dropped?
                if (clone.TrapModel.ActivatesWhenDropped)
                    clone.Activate(actor); // alpha10 //clone.IsActivated = true;

                // make sure source stack is desactivated (activate only activate the stack top item).
                trap.Desactivate();  // alpha10  //trap.IsActivated = false;
            }
            else
            {
                // drop or discard.
                if (it is ItemTracker)
                {
                    discardMe = (it as ItemTracker).Batteries <= 0;
                }
                else if (it is ItemLight)
                {
                    discardMe = (it as ItemLight).Batteries <= 0;
                }
                else if (it is ItemSprayPaint)
                {
                    discardMe = (it as ItemSprayPaint).PaintQuantity <= 0;
                }
                else if (it is ItemSprayScent)
                {
                    discardMe = (it as ItemSprayScent).SprayQuantity <= 0;
                }
            }

            if (discardMe)
            {
                DiscardItem(actor, it);
                // message
                if (IsVisibleToPlayer(actor))
                    AddMessage(MakeMessage(actor, Conjugate(actor, VERB_DISCARD), it));
            }
            else
            {
                if (dropIt == it)
                    DropItem(actor, it);
                else
                    DropCloneItem(actor, it, dropIt);
                // message
                if (IsVisibleToPlayer(actor))
                    AddMessage(MakeMessage(actor, Conjugate(actor, VERB_DROP), dropIt));
            }
            */
        }

        public bool CanActorEatCorpse(Corpse corpse, out string reason)
        {
            if (corpse == null)
                throw new ArgumentNullException("corpse");

            ///////////////////////////////////////////////
            // Can't if any is true:
            // 1. Actor is not undead or a starving/insane living.
            ///////////////////////////////////////////////

            // 1. Actor is not undead or a starving living.
            if (!Model.Abilities.IsUndead)
            {
                // TODO
                //if (!IsActorStarving(actor) && !IsActorInsane(actor))
                //{
                //    reason = "not starving or insane";
                //    return false;
                //}
            }

            // ok
            reason = "";
            return true;
        }

        public void DoEatCorpse(Corpse c)
        {
            /*
            bool isVisible = IsVisibleToPlayer(a);

            // spend ap.
            SpendActorActionPoints(a, Rules.BASE_ACTION_COST);

            // damage.
            int dmg = m_Rules.ActorDamageVsCorpses(a);

            // msg.
            if (isVisible)
            {
                AddMessage(MakeMessage(a, String.Format("{0} {1} corpse.", Conjugate(a, VERB_FEAST_ON), c.DeadGuy.Name, dmg)));
                // alpha10 replace with sfx
                m_MusicManager.Stop();
                m_MusicManager.Play(GameSounds.UNDEAD_EAT, MusicPriority.PRIORITY_EVENT);
            }

            // dmh corpse.
            InflictDamageToCorpse(c, dmg);

            // destroy?
            if (c.HitPoints <= 0)
            {
                DestroyCorpse(c, a.Location.Map);
                if (isVisible)
                    AddMessage(new Message(String.Format("{0} corpse is no more.", c.DeadGuy.Name), a.Location.Map.LocalTime.TurnCounter, Color.Purple));
            }

            // heal if undead / food.
            if (a.Model.Abilities.IsUndead)
            {
                RegenActorHitPoints(a, Rules.ActorBiteHpRegen(a, dmg));
                a.FoodPoints = Math.Min(a.FoodPoints + m_Rules.ActorBiteNutritionValue(a, dmg), m_Rules.ActorMaxRot(a));
            }
            else
            {
                // recover food points.
                a.FoodPoints = Math.Min(a.FoodPoints + m_Rules.ActorBiteNutritionValue(a, dmg), m_Rules.ActorMaxFood(a));
                // infection!
                InfectActor(a, m_Rules.CorpseEeatingInfectionTransmission(c.DeadGuy.Infection));
            }

            // cause insanity.
            SeeingCauseInsanity(a, a.Location, a.Model.Abilities.IsUndead ? Rules.SANITY_HIT_UNDEAD_EATING_CORPSE : Rules.SANITY_HIT_LIVING_EATING_CORPSE,
                String.Format("{0} eating {1}", a.Name, c.DeadGuy.Name));
            */
        }

        public bool CanActorEatFoodOnGround(Item it, out string reason)
        {
            /*
            if (it == null)
                throw new ArgumentNullException("item");

            ///////////////////////////////
            // Can't if any is true:
            // 1. Item not food.
            // 2. Item not in actor tile.
            //////////////////////////////

            // 1. Item not food.
            if (!(it is ItemFood))
            {
                reason = "not food";
                return false;
            }

            // 2. Item not in actor tile.
            Inventory stackHere = actor.Location.Map.GetItemsAt(actor.Location.Position);
            if (stackHere == null || !stackHere.Contains(it))
            {
                reason = "item not here";
                return false;
            }

            // ok
            */
            reason = "";
            return true;
        }

        public void DoEatFoodFromGround(Item it)
        {
            /*
            ItemFood food = it as ItemFood;

            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // recover food points.
            int baseNutrition = m_Rules.FoodItemNutrition(food, actor.Location.Map.LocalTime.TurnCounter);
            actor.FoodPoints = Math.Min(actor.FoodPoints + m_Rules.ActorItemNutritionValue(actor, baseNutrition), m_Rules.ActorMaxFood(actor));

            // consume it.
            Inventory inv = actor.Location.Map.GetItemsAt(actor.Location.Position);
            inv.Consume(food);

            // message.
            bool isVisible = IsVisibleToPlayer(actor);
            if (isVisible)
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_EAT), food));

            // vomit?
            if (m_Rules.IsFoodSpoiled(food, actor.Location.Map.LocalTime.TurnCounter))
            {
                if (m_Rules.RollChance(Rules.FOOD_EXPIRED_VOMIT_CHANCE))
                {
                    DoVomit(actor);

                    // message.
                    if (isVisible)
                    {
                        AddMessage(MakeMessage(actor, String.Format("{0} from eating spoiled food!", Conjugate(actor, VERB_VOMIT))));
                    }
                }
            }
            */
        }

        public bool CanActorEquipItem(Item it, out string reason)
        {
            /*
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (it == null)
                throw new ArgumentNullException("item");

            ////////////////////////////
            // Can't if any is true:
            // 1. Actor cant use items.
            // 2. Item not equipable.
            // (DISABLED 3. No free slot on doll.)
            ////////////////////////////

            // 1. Actor cant use items.
            if (!actor.Model.Abilities.CanUseItems)
            {
                reason = "no ability to use items";
                return false;
            }

            // 2. Item not equipable.
            if (!it.Model.IsEquipable)
            {
                reason = "this item cannot be equipped";
                return false;
            }

            // (DISABLED 3. No free slot on doll.)
            // reason: equipping automatically unequip other first.
#if false
            if (actor.GetEquippedItem(it.Model.EquipmentPart) != null)
            {
                reason = "equipment slot not free";
                return false;
            }
#endif

            // all clear.
            */
            reason = "";
            return true;
            
        }

        /// <summary>
        /// AP free
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="it"></param>
        public void DoEquipItem(Item it)
        {
            /*
            // unequip previous item first.
            Item previousItem = actor.GetEquippedItem(it.Model.EquipmentPart);
            if (previousItem != null)
            {
                DoUnequipItem(actor, previousItem);
            }

            // equip part.
            it.EquippedPart = it.Model.EquipmentPart;

            // update revelant datas.
            OnEquipItem(actor, it);

            // message
            if (IsVisibleToPlayer(actor))
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_EQUIP), it));
            */
        }

        public bool CanActorGetItemFromContainer(Point position, out string reason)
        {
            /*
            if (actor == null)
                throw new ArgumentNullException("actor");

            ////////////////////////////////////
            // Cant if any is true:
            // 1. Map object is not a container.
            // 2. There is no items there.
            // 3. Actor cannot take the item.
            ////////////////////////////////////

            // 1. Map object is not a container.
            MapObject mapObj = actor.Location.Map.GetMapObjectAt(position);
            if (mapObj == null || !mapObj.IsContainer)
            {
                reason = "object is not a container";
                return false;
            }

            // 2. There is no items there.
            Inventory invThere = actor.Location.Map.GetItemsAt(position);
            if (invThere == null || invThere.IsEmpty)
            {
                reason = "nothing to take there";
                return false;
            }

            // 3. Actor cannot take the item.
            if (!actor.Model.Abilities.HasInventory ||
                !actor.Model.Abilities.CanUseMapObjects ||
                actor.Inventory == null ||
                !CanActorGetItem(actor, invThere.TopItem))
            {
                reason = "cannot take an item";
                return false;
            }

            // all clear.
            
            */
            reason = "";
            return true;

        }

        public void DoTakeFromContainer(Point position)
        {
            Map map = Location.Map;

            // get topmost item.
            Item it = map.GetItemsAt(position).TopItem;

            // take it.
            //TODO DoTakeItem(actor, position, it);
        }

        public bool CanActorLeaveMap(out string reason)
        {
            ////////////////////////////
            // Only if :
            // 1. Player and not bot // alpha10.1
            /////////////////////////////////
            if (!IsPlayer || IsBotPlayer)
            {
                reason = "can't leave maps";
                return false;
            }

            reason = "";
            return true;
        }

        public bool DoLeaveMap(Point exitPoint, bool askForConfirmation)
        {
            /*
            bool isPlayer = actor.IsPlayer;

            Map fromMap = actor.Location.Map;
            Point fromPos = actor.Location.Position;

            // get exit.
            Exit exit = fromMap.GetExitAt(exitPoint);
            if (exit == null)
            {
                if (isPlayer)
                {
                    AddMessage(MakeErrorMessage("There is nowhere to go there."));
                }
                return true;
            }

            // if player, ask for a confirmation.
            if (isPlayer && askForConfirmation)
            {
                ClearMessages();
                AddMessage(MakeYesNoMessage(String.Format("REALLY LEAVE {0}", fromMap.Name)));
                RedrawPlayScreen();
                bool confirm = WaitYesOrNo();
                if (!confirm)
                {
                    AddMessage(new Message("Let's stay here a bit longer...", m_Session.WorldTime.TurnCounter, Color.Yellow));
                    RedrawPlayScreen();
                    return false;
                }
            }

            // alpha10.1 check autosave before player leaving map
            if (isPlayer)
                CheckAutoSaveTime();

            // Try to leave tile.
            if (!TryActorLeaveTile(actor))
            {
                // waste ap.
                SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
                return false;
            }

            // spend AP **IF AI**
            if (!actor.IsPlayer)
                SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // if player is leaving and changing district, prepare district.
            // alpha10.1 disallow bots from leaving districts
            bool playerChangedDistrict = false;  // alpha10
            if (isPlayer && !actor.IsBotPlayer && exit.ToMap.District != fromMap.District)
            {
                playerChangedDistrict = true;  // alpha10
                BeforePlayerEnterDistrict(exit.ToMap.District);
            }

            /////////////////////////////////////
            // 1. If spot not available, cancel.
            // 2. Remove from previous map (+ corpse)
            // 3. Enter map (+corpse).
            // 4. Handle followers.
            /////////////////////////////////////

            // 1. If spot not available, cancel.
            Actor other = exit.ToMap.GetActorAt(exit.ToPosition);
            if (other != null)
            {
                if (isPlayer)
                {
                    AddMessage(MakeErrorMessage(String.Format("{0} is blocking your way.", other.Name)));
                }
                return true;
            }
            MapObject blockingObj = exit.ToMap.GetMapObjectAt(exit.ToPosition);
            if (blockingObj != null)
            {
                bool canJump = blockingObj.IsJumpable && m_Rules.HasActorJumpAbility(actor);
                bool ignoreIt = blockingObj.IsCouch;
                if (!canJump && !ignoreIt)
                {
                    if (isPlayer)
                    {
                        AddMessage(MakeErrorMessage(String.Format("{0} is blocking your way.", blockingObj.AName)));
                    }
                    return true;
                }
            }

            // 2. Remove from previous map (+corpse)
            if (IsVisibleToPlayer(actor))
            {
                AddMessage(MakeMessage(actor, String.Format("{0} {1}.", Conjugate(actor, VERB_LEAVE), fromMap.Name)));
            }
            fromMap.RemoveActor(actor);
            if (actor.DraggedCorpse != null)
                fromMap.RemoveCorpse(actor.DraggedCorpse);
            if (isPlayer && exit.ToMap.District != fromMap.District)
            {
                OnPlayerLeaveDistrict();
            }

            // 3. Enter map (+corpse)
            exit.ToMap.PlaceActorAt(actor, exit.ToPosition);
            exit.ToMap.MoveActorToFirstPosition(actor);
            if (actor.DraggedCorpse != null)
            {
                exit.ToMap.AddCorpseAt(actor.DraggedCorpse, exit.ToPosition);
            }
            if (IsVisibleToPlayer(actor) || isPlayer)
            {
                AddMessage(MakeMessage(actor, String.Format("{0} {1}.", Conjugate(actor, VERB_ENTER), exit.ToMap.Name)));
            }
            if (isPlayer)
            {
                // scoring event.
                if (fromMap.District != exit.ToMap.District)
                {
                    m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, String.Format("Entered district {0}.", exit.ToMap.District.Name));
                }

                // change map.
                SetCurrentMap(exit.ToMap);
            }
            // Trigger stuff.
            OnActorEnterTile(actor);
            // 4. Handle followers.
            if (actor.CountFollowers > 0)
            {
                DoFollowersEnterMap(actor, fromMap, fromPos, exit.ToMap, exit.ToPosition);
            }

            // alpha10
            // handle player changing district
            if (playerChangedDistrict)
                AfterPlayerEnterDistrict();

            // done.
            */
            return true;
        }

        public void DoMeleeAttack(Actor defender)
        {
            /*
            // set activiy & target.
            attacker.Activity = Activity.FIGHTING;
            attacker.TargetActor = defender;

            // if not already enemies, attacker is aggressor.
            if (!m_Rules.AreEnemies(attacker, defender))
                DoMakeAggression(attacker, defender);

            // get attack & defence.
            Attack attack = m_Rules.ActorMeleeAttack(attacker, attacker.CurrentMeleeAttack, defender);
            Defence defence = m_Rules.ActorDefence(defender, defender.CurrentDefence);

            // spend APs & STA.
            SpendActorActionPoints(attacker, Rules.BASE_ACTION_COST);
            SpendActorStaminaPoints(attacker, Rules.STAMINA_COST_MELEE_ATTACK + attack.StaminaPenalty);

            // resolve attack.
            int hitRoll = m_Rules.RollSkill(attack.HitValue);
            int defRoll = m_Rules.RollSkill(defence.Value);

            // loud noise.
            OnLoudNoise(attacker.Location.Map, attacker.Location.Position, "Nearby fighting");

            // if defender is long waiting player, force stop.
            if (m_IsPlayerLongWait && defender.IsPlayer)
            {
                m_IsPlayerLongWaitForcedStop = true;
            }

            // show/hear.
            bool isDefVisible = IsVisibleToPlayer(defender);
            bool isAttVisible = IsVisibleToPlayer(attacker);
            bool isPlayer = attacker.IsPlayer || defender.IsPlayer;
            bool isBot = attacker.IsBotPlayer || defender.IsBotPlayer;  // alpha10.1 handle bot

            if (!isDefVisible && !isAttVisible && !isPlayer &&
                m_Rules.RollChance(PLAYER_HEAR_FIGHT_CHANCE))
            {
                AddMessageIfAudibleForPlayer(attacker.Location, MakePlayerCentricMessage("You hear fighting", attacker.Location.Position));
            }

            if (isAttVisible)
            {
                AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(attacker.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(defender.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                AddOverlay(new OverlayImage(MapToScreen(attacker.Location.Position), GameImages.ICON_MELEE_ATTACK));
            }

            // Hit vs Missed
            if (hitRoll > defRoll)
            {
                // alpha10
                // roll for attacker disarming defender
                if (attacker.Model.Abilities.CanDisarm && m_Rules.RollChance(attack.DisarmChance))
                {
                    Item disarmIt = Disarm(defender);
                    if (disarmIt != null)
                    {
                        // show
                        if (isDefVisible)
                        {
                            if (isPlayer)
                                ClearMessages();
                            AddMessage(MakeMessage(attacker, Conjugate(attacker, VERB_DISARM), defender));
                            AddMessage(new Message(string.Format("{0} is sent flying!", disarmIt.TheName), attacker.Location.Map.LocalTime.TurnCounter));
                            if (isPlayer && !isBot)
                            {
                                AddMessagePressEnter();
                            }
                            else
                            {
                                RedrawPlayScreen();
                                AnimDelay(DELAY_SHORT);
                            }
                        }
                    }
                }

                // roll damage - double potential if def is sleeping.
                int dmgRoll = m_Rules.RollDamage(defender.IsSleeping ? attack.DamageValue * 2 : attack.DamageValue) - defence.Protection_Hit;
                // damage?
                if (dmgRoll > 0)
                {
                    // inflict dmg.
                    InflictDamage(defender, dmgRoll);

                    // regen HP/Rot and infection?
                    if (attacker.Model.Abilities.CanZombifyKilled && !defender.Model.Abilities.IsUndead)
                    {
                        RegenActorHitPoints(attacker, Rules.ActorBiteHpRegen(attacker, dmgRoll));
                        attacker.FoodPoints = Math.Min(attacker.FoodPoints + m_Rules.ActorBiteNutritionValue(attacker, dmgRoll), m_Rules.ActorMaxRot(attacker));
                        if (isAttVisible)
                        {
                            AddMessage(MakeMessage(attacker, Conjugate(attacker, VERB_FEAST_ON), defender, " flesh !"));
                        }
                        InfectActor(defender, Rules.InfectionForDamage(attacker, dmgRoll));
                    }

                    // Killed?
                    if (defender.HitPoints <= 0) // def killed!
                    {
                        // show.
                        if (isAttVisible || isDefVisible)
                        {
                            AddMessage(MakeMessage(attacker, Conjugate(attacker, defender.Model.Abilities.IsUndead ? VERB_DESTROY : m_Rules.IsMurder(attacker, defender) ? VERB_MURDER : VERB_KILL), defender, " !"));
                            AddOverlay(new OverlayImage(MapToScreen(defender.Location.Position), GameImages.ICON_KILLED));
                            RedrawPlayScreen();
                            AnimDelay(DELAY_LONG);
                        }

                        // kill.
                        KillActor(attacker, defender, "hit");

                        // cause insanity?
                        if (attacker.Model.Abilities.IsUndead && !defender.Model.Abilities.IsUndead)
                            SeeingCauseInsanity(attacker, attacker.Location, Rules.SANITY_HIT_EATEN_ALIVE, String.Format("{0} eaten alive", defender.Name));

                        // turn victim into zombie; always turn player into zombie NOW if killed by zombifier or if was infected.
                        if (Rules.HasImmediateZombification(m_Session.GameMode) || defender == m_Player)
                        {
                            if (attacker.Model.Abilities.CanZombifyKilled && !defender.Model.Abilities.IsUndead && m_Rules.RollChance(s_Options.ZombificationChance))
                            {
                                if (defender.IsPlayer)
                                {
                                    // remove player corpse.
                                    defender.Location.Map.TryRemoveCorpseOf(defender);
                                }
                                // add new zombie.
                                Zombify(attacker, defender, false);

                                // show
                                if (isDefVisible)
                                {
                                    AddMessage(MakeMessage(attacker, Conjugate(attacker, "turn"), defender, " into a Zombie!"));
                                    RedrawPlayScreen();
                                    AnimDelay(DELAY_LONG);
                                }
                            }
                            else if (defender == m_Player && !defender.Model.Abilities.IsUndead && defender.Infection > 0)
                            {
                                // remove player corpse.
                                defender.Location.Map.TryRemoveCorpseOf(defender);
                                // zombify player!
                                Zombify(null, defender, false);

                                // show
                                AddMessage(MakeMessage(defender, Conjugate(defender, "turn") + " into a Zombie!"));
                                RedrawPlayScreen();
                                AnimDelay(DELAY_LONG);
                            }
                        }
                    }
                    else
                    {
                        // show
                        if (isAttVisible || isDefVisible)
                        {
                            AddMessage(MakeMessage(attacker, Conjugate(attacker, attack.Verb), defender, String.Format(" for {0} damage.", dmgRoll)));
                            AddOverlay(new OverlayImage(MapToScreen(defender.Location.Position), GameImages.ICON_MELEE_DAMAGE));
                            AddOverlay(new OverlayText(MapToScreen(defender.Location.Position).Add(DAMAGE_DX, DAMAGE_DY), Color.White, dmgRoll.ToString(), Color.Black));
                            RedrawPlayScreen();
                            AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
                        }
                    }
                }
                else
                {
                    if (isAttVisible || isDefVisible)
                    {
                        AddMessage(MakeMessage(attacker, Conjugate(attacker, attack.Verb), defender, " for no effect."));
                        AddOverlay(new OverlayImage(MapToScreen(defender.Location.Position), GameImages.ICON_MELEE_MISS));
                        RedrawPlayScreen();
                        AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
                    }
                }

            }   // end of hit
            else // miss
            {
                // show
                if (isAttVisible || isDefVisible)
                {
                    AddMessage(MakeMessage(attacker, Conjugate(attacker, VERB_MISS), defender));
                    AddOverlay(new OverlayImage(MapToScreen(defender.Location.Position), GameImages.ICON_MELEE_MISS));
                    RedrawPlayScreen();
                    AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
                }
            }

            // weapon break?
            ItemMeleeWeapon meleeWeapon = attacker.GetEquippedWeapon() as ItemMeleeWeapon;
            if (meleeWeapon != null && !(meleeWeapon.Model as ItemMeleeWeaponModel).IsUnbreakable)
            {
                if (m_Rules.RollChance(meleeWeapon.IsFragile ? Rules.MELEE_WEAPON_FRAGILE_BREAK_CHANCE : Rules.MELEE_WEAPON_BREAK_CHANCE))
                {
                    // do it.
                    // stackable weapons : only break ONE.
                    OnUnequipItem(attacker, meleeWeapon);
                    if (meleeWeapon.Quantity > 1)
                        --meleeWeapon.Quantity;
                    else
                        attacker.Inventory.RemoveAllQuantity(meleeWeapon);

                    // message.
                    if (isAttVisible)
                    {
                        AddMessage(MakeMessage(attacker, String.Format(": {0} breaks and is now useless!", meleeWeapon.TheName)));
                        RedrawPlayScreen();
                        AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
                    }
                }
            }

            // alpha10 bug fix; clear overlays only if action is visible
            if (isAttVisible || isDefVisible)
                ClearOverlays();

            */
        }

        public bool IsWalkableFor(Map map, int x, int y, out string reason)
        {
            /*
            if (map == null)
                throw new ArgumentNullException("map");
            if (actor == null)
                throw new ArgumentNullException("actor");

            ////////////////////////////////
            // Not walkable if any is true:
            // 1. Out of map.
            // 2. Tile not walkable.
            // 3. Map object not passable.
            // 4. An actor already there.
            // 5. Dragging a corpse when tired.
            ////////////////////////////////
            // 1. Out of map.
            if (!map.IsInBounds(x, y))
            {
                reason = "out of map";
                return false;
            }

            // 2. Tile not walkable.
            if (!map.GetTileAt(x, y).Model.IsWalkable)
            {
                reason = "blocked";
                return false;
            }

            // 3. Map object not passable.
            MapObject mapObj = map.GetMapObjectAt(x, y);
            if (mapObj != null)
            {
                if (!mapObj.IsWalkable)
                {
                    // jump?
                    if (mapObj.IsJumpable)
                    {
                        if (!HasActorJumpAbility(actor))
                        {
                            reason = "cannot jump";
                            return false;
                        }
                        if (actor.StaminaPoints < STAMINA_COST_JUMP)
                        {
                            reason = "not enough stamina to jump";
                            return false;
                        }
                    }
                    // small actor blocked only by closed doors.
                    else if (actor.Model.Abilities.IsSmall)
                    {
                        DoorWindow door = mapObj as DoorWindow;
                        if (door != null && door.IsClosed)
                        {
                            reason = "cannot slip through closed door";
                            return false;
                        }
                    }
                    else
                    {
                        // nope, blocking object.
                        reason = "blocked by object";
                        return false;
                    }
                }
            }

            // 4. An actor already there.
            if (map.GetActorAt(x, y) != null)
            {
                reason = "someone is there";
                return false;
            }

            // 5. Dragging a corpse when tired.
            if (actor.DraggedCorpse != null && IsActorTired(actor))
            {
                reason = "dragging a corpse when tired";
                return false;
            }
             */
            // all checks clear.
            reason = "";
            return true;
           
        }

        public bool IsWalkableFor(Location location, out string reason)
        {
            return IsWalkableFor(location.Map, location.Position.X, location.Position.Y, out reason);
        }

        public void DoMoveActor(Location newLocation)
        {
            /*
            Location oldLocation = actor.Location;

            // Try to leave tile.
            if (!TryActorLeaveTile(actor))
            {
                // waste ap.
                SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
                return;
            }

            // Do the move.
            if (oldLocation.Map == newLocation.Map)
                newLocation.Map.PlaceActorAt(actor, newLocation.Position);
            else
                throw new NotImplementedException("DoMoveActor : illegal to change map.");

            // If dragging corpse, move it along.
            Corpse draggedCorpse = actor.DraggedCorpse;
            if (draggedCorpse != null)
            {
                oldLocation.Map.MoveCorpseTo(draggedCorpse, newLocation.Position);
                if (IsVisibleToPlayer(newLocation) || IsVisibleToPlayer(oldLocation))
                    AddMessage(MakeMessage(actor, String.Format("{0} {1} corpse.", Conjugate(actor, VERB_DRAG), draggedCorpse.DeadGuy.TheName)));
            }

            // Spend AP & STA, check for running, jumping and dragging corpse.
            #region
            int moveCost = Rules.BASE_ACTION_COST;

            // running?
            if (actor.IsRunning)
            {
                // x2 faster.
                moveCost /= 2;
                // cost STA.
                SpendActorStaminaPoints(actor, Rules.STAMINA_COST_RUNNING);
            }

            bool isJump = false;
            MapObject mapObj = newLocation.Map.GetMapObjectAt(newLocation.Position.X, newLocation.Position.Y);
            if (mapObj != null && !mapObj.IsWalkable && mapObj.IsJumpable)
                isJump = true;

            // jumping?
            if (isJump)
            {
                // cost STA.
                SpendActorStaminaPoints(actor, Rules.STAMINA_COST_JUMP);

                // show.
                if (IsVisibleToPlayer(actor))
                    AddMessage(MakeMessage(actor, Conjugate(actor, VERB_JUMP_ON), mapObj));

                // if CanJumpStumble ability, has a chance to stumble.
                if (actor.Model.Abilities.CanJumpStumble && m_Rules.RollChance(Rules.JUMP_STUMBLE_CHANCE))
                {
                    // stumble!
                    moveCost += Rules.JUMP_STUMBLE_ACTION_COST;

                    // show.
                    if (IsVisibleToPlayer(actor))
                        AddMessage(MakeMessage(actor, String.Format("{0}!", Conjugate(actor, VERB_STUMBLE))));
                }
            }

            // dragging?
            if (draggedCorpse != null)
            {
                // cost STA.
                SpendActorStaminaPoints(actor, Rules.STAMINA_COST_MOVE_DRAGGED_CORPSE);
            }

            // spend move AP.
            SpendActorActionPoints(actor, moveCost);
            #endregion

            // If actor can move again, make sure he drops his scent here.
            // If we don't do this, since scents are dropped only in new turns,
            // there will be "holes" in the scent paths, and this is not fair
            // for zombies who will loose track of running livings easily.
            if (actor.ActionPoints > 0) // alpha10 fix; was Rules.BASE_ACTION_COST
                DropActorScents(actor);

            // Screams of terror?
            #region
            if (!actor.IsPlayer &&
                (actor.Activity == Activity.FLEEING || actor.Activity == Activity.FLEEING_FROM_EXPLOSIVE) &&
                !actor.Model.Abilities.IsUndead &&
                actor.Model.Abilities.CanTalk)
            {
                // loud noise.
                OnLoudNoise(newLocation.Map, newLocation.Position, "A loud SCREAM");

                // player hears?
                if (m_Rules.RollChance(PLAYER_HEAR_SCREAMS_CHANCE) && !IsVisibleToPlayer(actor))
                {
                    AddMessageIfAudibleForPlayer(actor.Location, MakePlayerCentricMessage("You hear screams of terror", actor.Location.Position));
                }
            }
            #endregion

            // Trigger stuff.
            OnActorEnterTile(actor);
            */
        }

        public void DoMoveActor(Direction direction)
        {
            DoMoveActor(Location + direction);
        }

        public bool IsOpenableFor(DoorWindow door, out string reason)
        {
            if (door == null)
                throw new ArgumentNullException("door");

            ////////////////////////////////////////
            // Not openable if any is true:
            // 1. Actor cannot use map objects.
            // 2. Door is not closed nor barricaded.
            ////////////////////////////////////////

            // 1. Actor cannot use map objects.
            if (Model.Abilities.CanUseMapObjects)
            {
                reason = "no ability to open";
                return false;
            }

            // 2. Door is not closed nor barricaded.
            if (!door.IsClosed || door.BarricadePoints > 0)
            {
                reason = "not closed nor barricaded";
                return false;
            }

            // all clear
            reason = "";
            return true;
        }

        public void DoOpenDoor(DoorWindow door)
        {
            /*
            // Do it.
            door.SetState(DoorWindow.STATE_OPEN);

            // Message.
            if (IsVisibleToPlayer(this) || IsVisibleToPlayer(door))
            {
                AddMessage(MakeMessage(this, Conjugate(this, VERB_OPEN), door));
                RedrawPlayScreen();
            }

            // Spend APs.
            int openCost = Rules.BASE_ACTION_COST;
            SpendActorActionPoints(this, openCost);
            */
        }

        // alpha10
        public bool CanPullObject(MapObject mapObj, Point moveToPos, out string reason)
        {
            /*
            /////////////////////////////////////////////
            // Basically check if can push and can walk.
            // 1. Actor cannot push.
            // 2. Another object already there.
            // 3. Actor cannot walk to pos.
            /////////////////////////////////////////////

            // 1. Actor cannot push this object.
            if (!CanActorPush(actor, mapObj, out reason))
                return false;

            // 2. Another object already there. eg: actor standing on a bed.
            MapObject otherMobj = actor.Location.Map.GetMapObjectAt(actor.Location.Position);
            if (otherMobj != null)
            {
                reason = string.Format("{0} is blocking", otherMobj.TheName);
                return false;
            }

            // 3. Actor cannot walk to pos.
            if (!IsWalkableFor(actor, new Location(actor.Location.Map, moveToPos), out reason))
                return false;

            // all clear
            */
            reason = "";
            return true;
        }

        // alpha10
        public void DoPull(MapObject mapObj, Point moveActorToPos)
        {
            /*
            bool isVisible = IsVisibleToPlayer(actor) || IsVisibleToPlayer(mapObj);
            int staCost = mapObj.Weight;

            // try leaving tile
            if (!TryActorLeaveTile(actor))
            {
                // waste ap.
                SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
                return;
            }

            // followers help?
            if (actor.CountFollowers > 0)
                DoPushPullFollowersHelp(actor, mapObj, true, ref staCost);

            // spend AP & STA.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
            SpendActorStaminaPoints(actor, staCost);

            // do it : move actor then move object
            Map map = mapObj.Location.Map;
            // actor...
            Point pullObjectTo = actor.Location.Position;
            map.RemoveActor(actor);
            map.PlaceActorAt(actor, moveActorToPos);  // assumed to be walkable, checked by rules
            // ...object
            map.RemoveMapObjectAt(mapObj.Location.Position.X, mapObj.Location.Position.Y);
            map.PlaceMapObjectAt(mapObj, pullObjectTo);

            // noise/message.
            if (isVisible)
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_PULL), mapObj));
                RedrawPlayScreen();
            }
            else
            {
                // loud noise.
                OnLoudNoise(map, mapObj.Location.Position, "Something being pushed");

                // player hears?
                if (m_Rules.RollChance(PLAYER_HEAR_PUSHPULL_CHANCE))
                {
                    AddMessageIfAudibleForPlayer(mapObj.Location, MakePlayerCentricMessage("You hear something being pushed", mapObj.Location.Position));
                }
            }

            // check triggers
            OnActorEnterTile(actor);
            CheckMapObjectTriggersTraps(map, mapObj.Location.Position);
            */
        }

        public bool CanActorPush(MapObject mapObj, out string reason)
        {
            /*
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (mapObj == null)
                throw new ArgumentNullException("mapObj");

            //////////////////////////////////
            // Not movable:
            // 1. Actor cannot push/pull.
            // 2. Actor is tired.
            // 3. Map obj is not movable.
            // 4. Another actor there.
            // 5. Map obj is on fire.
            // 6. Actor is dragging a corpse.  // alpha10
            /////////////////////////////////

            // 1. Actor cannot push/pull.
            if (!HasActorPushAbility(actor))
            {
                reason = "cannot push objects";
                return false;
            }

            // 2. Actor is tired.
            if (IsActorTired(actor))
            {
                reason = "tired";
                return false;
            }

            // 3. Map obj is not movable.
            if (!mapObj.IsMovable)
            {
                reason = "cannot be moved";
                return false;
            }

            // 4. Another actor there.
            if (mapObj.Location.Map.GetActorAt(mapObj.Location.Position) != null)
            {
                reason = "someone is there";
                return false;
            }

            // 5. Map obj is on fire.
            if (mapObj.IsOnFire)
            {
                reason = "on fire";
                return false;
            }

            // 6. Actor is dragging a corpse.  // alpha10
            if (actor.DraggedCorpse != null)
            {
                reason = "dragging a corpse";
                return false;
            }

            */
            // all clear
            reason = "";
            return true;
        }

        public void DoPush(MapObject mapObj, Point toPos)
        {
            /*
            bool isVisible = IsVisibleToPlayer(actor) || IsVisibleToPlayer(mapObj);
            int staCost = mapObj.Weight;

            // followers help?
            if (actor.CountFollowers > 0)
                DoPushPullFollowersHelp(actor, mapObj, false, ref staCost); // alpha10

            // spend AP & STA.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
            SpendActorStaminaPoints(actor, staCost);

            // do it : move object, then move actor if he is pushing it away and can enter the tile.
            Map map = mapObj.Location.Map;
            Point prevObjPos = mapObj.Location.Position;
            map.RemoveMapObjectAt(mapObj.Location.Position.X, mapObj.Location.Position.Y);
            map.PlaceMapObjectAt(mapObj, toPos);
            if (!m_Rules.IsAdjacent(toPos, actor.Location.Position) && m_Rules.IsWalkableFor(actor, map, prevObjPos.X, prevObjPos.Y))
            {
                // pushing away, need to follow.
                if (TryActorLeaveTile(actor))  // alpha10
                {
                    map.RemoveActor(actor);
                    map.PlaceActorAt(actor, prevObjPos);
                    OnActorEnterTile(actor);  // alpha10
                }
            }

            // noise/message.
            if (isVisible)
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_PUSH), mapObj));
                RedrawPlayScreen();
            }
            else
            {
                // loud noise.
                OnLoudNoise(map, toPos, "Something being pushed");

                // player hears?
                if (m_Rules.RollChance(PLAYER_HEAR_PUSHPULL_CHANCE))
                {
                    AddMessageIfAudibleForPlayer(mapObj.Location, MakePlayerCentricMessage("You hear something being pushed", toPos));
                }
            }

            // check traps.
            CheckMapObjectTriggersTraps(map, toPos);
            */
        }

        public bool CanActorFireAt(Actor target, out string reason)
        {
            return CanActorFireAt(target, null, out reason);
        }

        public bool CanActorFireAt(Actor target, List<Point> LoF)
        {
            string reason;
            return CanActorFireAt(target, LoF, out reason);
        }

        public bool CanActorFireAt(Actor target, List<Point> LoF, out string reason)
        {
            /*
            if (target == null)
                throw new ArgumentNullException("target");

            ///////////////////////////////////////
            // Cannot fire if:
            // 1. No ranged weapon or out of range.
            // 2. No ammo.
            // 3. No LoF.
            // 4. Target is dead (doh!).
            ///////////////////////////////////////
            if (LoF != null)
                LoF.Clear();

            // 1. No ranged weapon or out of range.
            ItemRangedWeapon rangedWeapon = GetEquippedWeapon() as ItemRangedWeapon;
            if (rangedWeapon == null)
            {
                reason = "no ranged weapon equipped";
                return false;
            }
            if (CurrentRangedAttack.Range < GridDistance(actor.Location.Position, target.Location.Position))
            {
                reason = "out of range";
                return false;
            }

            // 2. No ammo.
            if (rangedWeapon.Ammo <= 0)
            {
                reason = "no ammo left";
                return false;
            }

            // 3. No LoF.
            if (!LOS.CanTraceFireLine(actor.Location, target.Location.Position, actor.CurrentRangedAttack.Range, LoF))
            {
                reason = "no line of fire";
                return false;
            }

            // 4. Target is dead (doh!).
            // oddly this can happen for the AI when simulating... not clear why...
            // so this is a lame fix to prevent KillingActor from throwing an exception :-)
            if (target.IsDead)
            {
                reason = "already dead!";
                return false;
            }
            */
            // all clear.
            reason = "";
            return true;
        }

        public void DoRangedAttack(Actor defender, List<Point> LoF, FireMode mode)
        {
            /*
            // if not enemies, aggression.
            if (!m_Rules.AreEnemies(attacker, defender))
                DoMakeAggression(attacker, defender);

            // resolve, depending on mode.
            switch (mode)
            {
                case FireMode.DEFAULT:
                    // spend AP.
                    SpendActorActionPoints(attacker, Rules.BASE_ACTION_COST);

                    // do attack.
                    DoSingleRangedAttack(attacker, defender, LoF, 0);
                    break;

                case FireMode.RAPID:
                    // spend AP.
                    SpendActorActionPoints(attacker, Rules.BASE_ACTION_COST);

                    // 1st attack
                    DoSingleRangedAttack(attacker, defender, LoF, 1);

                    // 2nd attack.
                    // special cases:
                    // - target was killed by 1st attack.
                    // - no more ammo.
                    ItemRangedWeapon w = attacker.GetEquippedWeapon() as ItemRangedWeapon;
                    if (defender.IsDead)
                    {
                        // spend 2nd shot ammo.
                        --w.Ammo;

                        // shoot at nothing.
                        Attack attack = attacker.CurrentRangedAttack;
                        AddMessage(MakeMessage(attacker, String.Format("{0} at nothing.", Conjugate(attacker, attack.Verb))));
                    }
                    else if (w.Ammo <= 0)
                    {
                        // fail silently.
                        return;
                    }
                    else
                    {
                        // perform attack normally.
                        DoSingleRangedAttack(attacker, defender, LoF, 2);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unhandled mode");
            }
            */
        }

        public bool CanActorRechargeItemBattery(Item it, out string reason)
        {
            /*
            if (it == null)
                throw new ArgumentNullException("item");

            //////////////////////////////////////////////////
            // Can't if any is true:
            // 1. Actor cant use items.
            // 2. Item not equipped by actor.
            // 3. Not a battery powered item.
            //////////////////////////////////////////////////

            // 1. Actor cant use items.
            if (!Model.Abilities.CanUseItems)
            {
                reason = "no ability to use items";
                return false;
            }

            // 2. Item not equipped.            
            if (!it.IsEquipped || !Inventory.Contains(it))
            {
                reason = "item not equipped";
                return false;
            }

            // 3. Not a battery powered item.
            if (!IsItemBatteryPowered(it))
            {
                reason = "not a battery powered item";
                return false;
            }
            */
            // all clear.
            reason = "";
            return true;
        }

        public void DoRechargeItemBattery(Item it)
        {
            /*
            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // recharge.
            if (it is ItemLight)
            {
                ItemLight light = it as ItemLight;
                light.Batteries += WorldTime.TURNS_PER_HOUR;
            }
            else if (it is ItemTracker)
            {
                ItemTracker track = it as ItemTracker;
                track.Batteries += WorldTime.TURNS_PER_HOUR;
            }

            // message.
            if (IsVisibleToPlayer(actor))
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_RECHARGE), it, " batteries."));
            }
            */
        }

        public bool CanActorRepairFortification(Fortification fort, out string reason)
        {
            /*
            /////////////////////////////
            // Can't if any is true:
            // 1. Cannot use map objects.
            // 2. No material.
            /////////////////////////////

            // 1. Cannot use map objects.
            if (!Model.Abilities.CanUseMapObjects)
            {
                reason = "cannot use map objects";
                return false;
            }

            // 2. No material.
            int material = CountBarricadingMaterial(actor);
            if (material <= 0)
            {
                reason = "no barricading material";
                return false;
            }
            */
            // all clear.
            reason = "";
            return true;
        }

        public void DoRepairFortification(Fortification fort)
        {
            /*
            // spend AP.
            SpendActorActionPoints(this, Rules.BASE_ACTION_COST);

            // spend material.
            ItemBarricadeMaterial material = actor.Inventory.GetSmallestStackByType(typeof(ItemBarricadeMaterial)) as ItemBarricadeMaterial; // alpha10
                                                                                                                                             //actor.Inventory.GetFirstByType(typeof(ItemBarricadeMaterial)) as ItemBarricadeMaterial;
            if (material == null)
                throw new InvalidOperationException("no material");
            actor.Inventory.Consume(material);

            // repair HP.
            fort.HitPoints = Math.Min(fort.MaxHitPoints,
                fort.HitPoints + m_Rules.ActorBarricadingPoints(actor, (material.Model as ItemBarricadeMaterialModel).BarricadingValue));

            // message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(fort))
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_REPAIR), fort));
            }
            */
        }

        public bool CanActorReviveCorpse(Corpse corpse, out string reason)
        {
            /*
            if (corpse == null)
                throw new ArgumentNullException("corpse");

            ///////////////////////////
            // Can't if 
            // 1. No medic skill.
            // 2. Not on same tile.
            // 3. Corpse not fresh.
            // 4. No medikit.
            //////////////////////////

            // 1. No medic skill.
            if (actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.MEDIC) == 0)
            {
                reason = "lack medic skill";
                return false;
            }

            // 2. Not on same tile.
            if (corpse.Position != actor.Location.Position)
            {
                reason = "not there";
                return false;
            }

            // 3. Corpse not fresh.
            if (CorpseRotLevel(corpse) > 0)
            {
                reason = "corpse not fresh";
                return false;
            }

            // 4. No medikit.
            if (!actor.Inventory.HasItemMatching((it) => it.Model.ID == (int)GameItems.IDs.MEDICINE_MEDIKIT))
            {
                reason = "no medikit";
                return false;
            }
            */
            // ok
            reason = "";
            return true;
        }

        public void DoReviveCorpse(Corpse corpse)
        {
            /*
            bool visible = IsVisibleToPlayer(actor);

            // spend ap.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // make sure there is a walkable spot for revival.
            Map map = actor.Location.Map;
            List<Point> revivePoints = actor.Location.Map.FilterAdjacentInMap(actor.Location.Position,
                (pt) =>
                {
                    if (map.GetActorAt(pt) != null) return false;
                    if (map.GetMapObjectAt(pt) != null) return false;
                    return true;
                });
            if (revivePoints == null)
            {
                if (visible)
                    AddMessage(MakeMessage(actor, String.Format("{0} not enough room for reviving {1}.", Conjugate(actor, VERB_HAVE), corpse.DeadGuy.Name)));
                return;
            }
            Point revivePt = revivePoints[m_Rules.Roll(0, revivePoints.Count)];

            // spend medikit.
            Item medikit = actor.Inventory.GetSmallestStackByModel(GameItems.MEDIKIT);  // alpha10
                                                                                        //actor.Inventory.GetFirstMatching((it) => it.Model == GameItems.MEDIKIT);
            actor.Inventory.Consume(medikit);

            // try.
            int chance = m_Rules.CorpseReviveChance(actor, corpse);
            if (m_Rules.RollChance(chance))
            {
                // do it.
                corpse.DeadGuy.IsDead = false;
                corpse.DeadGuy.HitPoints = m_Rules.CorpseReviveHPs(actor, corpse);
                corpse.DeadGuy.Doll.RemoveDecoration(GameImages.BLOODIED);
                corpse.DeadGuy.Activity = Activity.IDLE;
                corpse.DeadGuy.TargetActor = null;
                map.RemoveCorpse(corpse);
                map.PlaceActorAt(corpse.DeadGuy, revivePt);
                // msg.
                if (visible)
                    AddMessage(MakeMessage(actor, Conjugate(actor, VERB_REVIVE), corpse.DeadGuy));
                // thank you... or not?
                if (!m_Rules.AreEnemies(actor, corpse.DeadGuy))
                    DoSay(corpse.DeadGuy, actor, "Thank you, you saved my life!", Sayflags.NONE);
            }
            else
            {
                // msg.
                if (visible)
                    AddMessage(MakeMessage(actor, String.Format("{0} to revive", Conjugate(actor, VERB_FAIL)), corpse.DeadGuy));
            }
            */
        }

        public void DoSay(Actor target, string text, Sayflags flags)
        {
            /*
            Color sayColor = ((flags & Sayflags.IS_DANGER) != 0) ? SAYOREMOTE_DANGER_COLOR : SAYOREMOTE_NORMAL_COLOR;

            // spend APS?
            if ((flags & Sayflags.IS_FREE_ACTION) == 0)
                SpendActorActionPoints(speaker, Rules.BASE_ACTION_COST);

            // message.
            if (IsVisibleToPlayer(speaker) || (IsVisibleToPlayer(target) && !(m_Player.IsSleeping && target == m_Player)))
            {
                bool isPlayer = target.IsPlayer;
                bool isBot = target.IsBotPlayer; // alpha10.1 handle bot
                bool isImportant = (flags & Sayflags.IS_IMPORTANT) != 0;
                if (isPlayer && isImportant)
                    ClearMessages();
                AddMessage(MakeMessage(speaker, String.Format("to {0} : ", target.TheName), sayColor));
                AddMessage(MakeMessage(speaker, String.Format("\"{0}\"", text), sayColor));
                if (isPlayer && isImportant && !isBot)
                {
                    AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(speaker.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                    AddMessagePressEnter();
                    ClearOverlays();
                    RemoveLastMessage();
                    RedrawPlayScreen();
                }
            }
            */
        }

        public bool CanActorShout(out string reason)
        {
            //////////////////////////
            // Can't shout if:
            // 1. Actor is sleeping.
            // 2. Actor can't talk.
            //////////////////////////

            // 1. Actor is sleeping.
            if (IsSleeping)
            {
                reason = "sleeping";
                return false;
            }

            // 2. Actor can't talk
            if (!Model.Abilities.CanTalk)
            {
                reason = "can't talk";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public void DoShout(string text)
        {
            /*
            // spend APs.
            SpendActorActionPoints(speaker, Rules.BASE_ACTION_COST);

            // loud noise.
            OnLoudNoise(speaker.Location.Map, speaker.Location.Position, "A SHOUT");

            // message.
            if (IsVisibleToPlayer(speaker) || AreLinkedByPhone(speaker, m_Player))
            {
                // if player follower, alert!
                if (speaker.Leader == m_Player && !m_Player.IsBotPlayer)  // alpha10.1 handle bot
                {
                    ClearMessages();
                    AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(speaker.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                    AddMessage(MakeMessage(speaker, String.Format("{0}!!", Conjugate(speaker, VERB_RAISE_ALARM))));
                    if (text != null)
                        DoEmote(speaker, text, true);
                    AddMessagePressEnter();
                    ClearOverlays();
                    RemoveLastMessage();
                }
                else
                {
                    if (text == null)
                        AddMessage(MakeMessage(speaker, String.Format("{0}!", Conjugate(speaker, VERB_SHOUT))));
                    else
                        DoEmote(speaker, String.Format("{0} \"{1}\"", Conjugate(speaker, VERB_SHOUT), text), true);
                }
            }
            */
        }

        public bool CanActorSleep(out string reason)
        {
            /*
            /////////////////////////
            // Can't sleep if:
            // 1. Already sleeping.
            // 2. Has not to sleep.
            // 3. Is hungry or worse.
            // 4. No need to sleep.
            /////////////////////////

            // 1. Already sleeping.
            if (IsSleeping)
            {
                reason = "already sleeping";
                return false;
            }

            // 2. Has not to sleep.
            if (!Model.Abilities.HasToSleep)
            {
                reason = "no ability to sleep";
                return false;
            }

            // 3. Is hungry or worse.
            if (IsActorHungry(actor) || IsActorStarving(actor))
            {
                reason = "hungry";
                return false;
            }

            // 4. No need to sleep.
            if (SleepPoints >= ActorMaxSleep(actor) - WorldTime.TURNS_PER_HOUR)
            {
                reason = "not sleepy at all";
                return false;
            }
            */
            // all clear.
            reason = "";
            return true;
        }

        public void DoStartSleeping()
        {
            /*
            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // force actor to stop dragging corpses.
            DoStopDraggingCorpses(actor);

            // set activity & state.
            actor.Activity = Activity.SLEEPING;
            actor.IsSleeping = true;
            */
        }

        public bool CanActorSprayOdorSuppressor(ItemSprayScent suppressor, Actor sprayOn, out string reason)
        {
            /*
            if (suppressor == null)
                throw new ArgumentNullException("suppressor");
            if (sprayOn == null)
                throw new ArgumentNullException("target");

            ////////////////////////////////////////////////////////
            // Cant if any is true:
            // 1. Actor cannot use items
            // 2. Not an odor suppressor
            // 3. Spray is not equiped by actor or has no spray left.
            // 4. SprayOn is not self or adjacent.
            ////////////////////////////////////////////////////////

            // 1. Actor cannot use items
            if (!Model.Abilities.CanUseItems)
            {
                reason = "cannot use items";
                return false;
            }

            // 2. Not an odor suppressor
            if (suppressor.Odor != Odor.SUPPRESSOR)
            {
                reason = "not an odor suppressor";
                return false;
            }

            // 2. Spray is not equiped by actor or has no spray left.
            if (suppressor.SprayQuantity <= 0)
            {
                reason = "no spray left";
                return false;
            }
            if (!(suppressor.IsEquipped && (Inventory != null && Inventory.Contains(suppressor))))
            {
                reason = "spray not equipped";
                return false;
            }

            // 3. SprayOn is not self or adjacent.
            if (sprayOn != this)
            {
                if (!(Location.Map == sprayOn.Location.Map && IsAdjacent(Location.Position, sprayOn.Location.Position)))
                {
                    reason = "not adjacent";
                    return false;
                }
            }
            */
            // all clear.
            reason = "";
            return true;
        }

        public void DoSprayOdorSuppressor(ItemSprayScent suppressor, Actor sprayOn)
        {
            /*
            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // spend spray.
            --suppressor.SprayQuantity;

            // add odor suppressor on spray target
            sprayOn.OdorSuppressorCounter += suppressor.Strength;

            // message.
            if (IsVisibleToPlayer(actor))
            {
                AddMessage(MakeMessage(actor, string.Format("{0} {1}.", Conjugate(actor, VERB_SPRAY),
                    (sprayOn == actor ? HimselfOrHerself(actor) : sprayOn.Name))));
            }
            */
        }

        public bool CanActorStartDragCorpse(Corpse corpse, out string reason)
        {
            /*
            if (corpse == null)
                throw new ArgumentNullException("corpse");


            ////////////////////////////////////////
            // Can't if any is true:
            // 1. Corpse already dragged.
            // 2. Actor tired.
            // 3. Corpse not in same tile as actor.
            // 4. Actor already dragging a corpse.
            ////////////////////////////////////////

            // 1. Corpse already dragged.
            if (corpse.IsDragged)
            {
                reason = "corpse is already being dragged";
                return false;
            }
            // 2. Actor tired.
            if (IsActorTired(actor))
            {
                reason = "tired";
                return false;
            }
            // 3. Corpse not in same tile as actor.
            if (corpse.Position != actor.Location.Position || !actor.Location.Map.HasCorpse(corpse))
            {
                reason = "not in same location";
                return false;
            }
            // 4. Actor already dragging a corpse.
            if (actor.DraggedCorpse != null)
            {
                reason = "already dragging a corpse";
                return false;
            }
            */
            // ok
            reason = "";
            return true;
        }

        public void DoStartDragCorpse(Corpse corpse)
        {
            corpse.DraggedBy = this;
            DraggedCorpse = corpse;
            // TODO
            //if (IsVisibleToPlayer(this))
            //    AddMessage(MakeMessage(a, String.Format("{0} dragging {1} corpse.", Conjugate(a, VERB_START), corpse.DeadGuy.Name)));
        }

        public bool CanActorStopDragCorpse(Corpse corpse, out string reason)
        {
            if (corpse == null)
                throw new ArgumentNullException("corpse");

            ////////////////////////////////////////
            // Can't if Corpse not being dragged by actor.
            ////////////////////////////////////////

            // Can't if Corpse not being dragged by actor.
            if (corpse.DraggedBy != this)
            {
                reason = "not dragging this corpse";
                return false;
            }

            // ok
            reason = "";
            return true;
        }

        public void DoStopDragCorpse(Corpse corpse)
        {
            corpse.DraggedBy = null;
            DraggedCorpse = null;
            //if (IsVisibleToPlayer(a)) // TODO
            //    AddMessage(MakeMessage(a, String.Format("{0} dragging {1} corpse.", Conjugate(a, VERB_STOP), c.DeadGuy.Name)));
        }

        public void DoStopDraggingCorpses()
        {
            if (DraggedCorpse != null)
            {
                DoStopDragCorpse(DraggedCorpse);
            }
        }

        public bool CanActorSwitchPlaceWith(Actor target, out string reason)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            //////////////////////////////////
            // Can't if any is true:
            // 1. Target not a actor follower.
            // 2. Target is sleeping.
            //////////////////////////////////

            // 1. Target not a actor follower.
            if (target.Leader != this)
            {
                reason = "not your follower";
                return false;
            }

            // 2. Target is sleeping.
            if (target.IsSleeping)
            {
                reason = "sleeping";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public void DoSwitchPlace(Actor other)
        {
            /*
            // spend a bunch of ap.
            SpendActorActionPoints(actor, 2 * Rules.BASE_ACTION_COST);

            // swap positions.
            Map map = other.Location.Map;
            Point actorPos = actor.Location.Position;
            map.RemoveActor(other);
            map.PlaceActorAt(actor, other.Location.Position);
            map.PlaceActorAt(other, actorPos);

            // message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(other))
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_SWITCH_PLACE_WITH), other));
            }
            */
        }

        public bool IsSwitchableFor(PowerGenerator powGen, out string reason)
        {
            if (powGen == null)
                throw new ArgumentNullException("powGen");

            ////////////////////////
            // Switchable only if:
            // 1. Actor has ability.
            // 2. Is not sleeping.
            ////////////////////////

            // 1. Actor has ability.
            if (!Model.Abilities.CanUseMapObjects)
            {
                reason = "cannot use map objects";
                return false;
            }

            // 2. Is not sleeping.
            if (IsSleeping)
            {
                reason = "is sleeping";
                return false;
            }


            // all clear.
            reason = "";
            return true;
        }

        public void DoSwitchPowerGenerator(PowerGenerator powGen)
        {
            /*
            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // switch it.
            powGen.TogglePower();

            // message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(powGen))
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_SWITCH), powGen, powGen.IsOn ? " on." : " off."));
            }

            // check for special effects.
            OnMapPowerGeneratorSwitch(actor.Location, powGen);

            // done.
            */
        }

        public bool CanActorGetItem(Item it, out string reason)
        {
            if (it == null)
                throw new ArgumentNullException("item");

            //////////////////////////////////////////////
            // Cant if any is true:
            // 1. Actor cannot take items.
            // 2. Inventory is full and cannot stack item
            // 3. Triggered trap.
            //////////////////////////////////////////////

            // 1. Actor cannot take items
            if (!Model.Abilities.HasInventory ||
                !Model.Abilities.CanUseMapObjects ||
                Inventory == null)
            {
                reason = "no inventory";
                return false;
            }
            if (Inventory.IsFull && !Inventory.CanAddAtLeastOne(it))
            {
                reason = "inventory is full";
                return false;
            }
            if (it is ItemTrap && (it as ItemTrap).IsTriggered)
            {
                reason = "triggered trap";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public void DoTakeItem(Point position, Item it)
        {
            /*
            Map map = actor.Location.Map;

            // spend APs.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // special case for traps
            if (it is ItemTrap)
            {
                ItemTrap trap = it as ItemTrap;
                // taking a trap desactivates it.
                trap.Desactivate(); // alpha10 // trap.IsActivated = false;
            }

            // add to inventory.
            int quantityAdded;
            int quantityBefore = it.Quantity;
            actor.Inventory.AddAsMuchAsPossible(it, out quantityAdded);
            // if added all, remove from map.
            if (quantityAdded == quantityBefore)
            {
                Inventory itemsThere = map.GetItemsAt(position);
                if (itemsThere != null && itemsThere.Contains(it))
                    map.RemoveItemAt(it, position);
            }

            // message
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(new Location(map, position)))
            {
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_TAKE), it));
            }

            // automatically equip item if flags set & possible, and not already equipped something.
            if (!it.Model.DontAutoEquip && m_Rules.CanActorEquipItem(actor, it) && actor.GetEquippedItem(it.Model.EquipmentPart) == null)
                DoEquipItem(actor, it);

            */
        }

        public bool CanActorTakeLead(Actor target, out string reason)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            ///////////////////////////////////////
            // Can't if any is true:
            // 1. Target is undead or an enemy.
            // 2. Target is sleeping.
            // 3. Target has already a leader and can't steal the follower.  alpha10.1
            // 4. Target is a leader.
            // 5. Actor has reached max followers.
            // 6. Target is player!
            // 7. Faction restrictions.
            ///////////////////////////////////////
            /*
            // 1. Target is undead or an enemy.
            if (target.Model.Abilities.IsUndead)
            {
                reason = "undead";
                return false;
            }
            if (AreEnemies(actor, target))
            {
                reason = "enemy";
                return false;
            }

            // 2. Target is sleeping.
            if (target.IsSleeping)
            {
                reason = "sleeping";
                return false;
            }

            // 3. Target has already a leader and can't steal the follower.  alpha10.1
            if (target.HasLeader)
            {
                // alpha10.1
                // can "steal" followers only if actor has better charismatic skill than current leader
                int actorCharism = actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CHARISMATIC);
                int leaderCharism = target.Leader.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CHARISMATIC);
                if (actorCharism <= leaderCharism)
                {
                    reason = "has already a leader at least as charismatic as you";
                    return false;
                }
            }

            // 4. Target is a leader.
            if (target.CountFollowers > 0)
            {
                reason = "is a leader";
                return false;
            }

            // 5. Actor has reached max followers.
            int maxFollowers = ActorMaxFollowers(actor);
            if (maxFollowers == 0)
            {
                reason = "can't lead";
                return false;
            }
            if (actor.CountFollowers >= maxFollowers)
            {
                reason = "too many followers";
                return false;
            }

            // 6. Target is player!
            if (target.IsPlayer)
            {
                reason = "is player";
                return false;
            }

            // 7. Faction restrictions.
            if (actor.Faction != target.Faction && target.Faction.LeadOnlyBySameFaction)
            {
                reason = String.Format("{0} can't lead {1}", actor.Faction.Name, target.Faction.Name);
                return false;
            }

            */
            // all clear.
            reason = "";
            return true;
        }

        public void DoStealLead(Actor other)
        {
            /*
            Actor prevLeader = other.Leader;

            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // remove from previous leader
            prevLeader.RemoveFollower(other);

            // take lead.
            actor.AddFollower(other);

            // reset trust in leader.
            int prevTrust = other.GetTrustIn(actor);
            other.TrustInLeader = prevTrust;

            // message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(other))
            {
                if (actor == m_Player)
                    ClearMessages();
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_PERSUADE), other, String.Format(" to leave {0} and join.", prevLeader.Name)));
                if (prevTrust != 0)
                    DoSay(other, actor, "Ah yes I remember you.", Sayflags.IS_FREE_ACTION);
            }
            */
        }

        public void DoTakeLead(Actor other)
        {
            /*
            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // take lead.
            actor.AddFollower(other);

            // reset trust in leader.
            int prevTrust = other.GetTrustIn(actor);
            other.TrustInLeader = prevTrust;

            // message.
            if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(other))
            {
                if (actor == m_Player)
                    ClearMessages();
                AddMessage(MakeMessage(actor, Conjugate(actor, VERB_PERSUADE), other, " to join."));
                if (prevTrust != 0)
                    DoSay(other, actor, "Ah yes I remember you.", Sayflags.IS_FREE_ACTION);
            }
            */
        }

        public bool CanActorThrowTo(Point pos, List<Point> LoF, out string reason)
        {
            /*
            ///////////////////////////////////////
            // Cannot fire if:
            // 1. No throwable item or out of range.
            // 2. No LoT.
            ///////////////////////////////////////
            if (LoF != null)
                LoF.Clear();

            // 1. No throwable item or out of range.
            ItemGrenade unprimedGrenade = actor.GetEquippedWeapon() as ItemGrenade;
            ItemGrenadePrimed primedGrenade = actor.GetEquippedWeapon() as ItemGrenadePrimed;
            if (unprimedGrenade == null && primedGrenade == null)
            {
                reason = "no grenade equiped";
                return false;
            }
            ItemGrenadeModel model;
            if (unprimedGrenade != null)
                model = unprimedGrenade.Model as ItemGrenadeModel;
            else
                model = (primedGrenade.Model as ItemGrenadePrimedModel).GrenadeModel;
            int maxThrowDist = ActorMaxThrowRange(actor, model.MaxThrowDistance);
            if (GridDistance(actor.Location.Position, pos) > maxThrowDist)
            {
                reason = "out of throwing range";
                return false;
            }

            // 2. No LoT.
            if (!LOS.CanTraceThrowLine(actor.Location, pos, maxThrowDist, LoF))
            {
                reason = "no line of throwing";
                return false;
            }
            */
            // all clear.
            reason = "";
            return true;

        }

        public void DoThrowGrenadeUnprimed(Point targetPos)
        {
            /*
            // get grenade.
            ItemGrenade grenade = actor.GetEquippedWeapon() as ItemGrenade;
            if (grenade == null)
                throw new InvalidOperationException("throwing grenade but no grenade equiped ");

            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // consume grenade.
            actor.Inventory.Consume(grenade);

            // drop primed grenade at target position.
            Map map = actor.Location.Map;
            ItemGrenadePrimed primedGrenade = new ItemGrenadePrimed(m_GameItems[grenade.PrimedModelID]);
            map.DropItemAt(primedGrenade, targetPos);

            // message about throwing.
            bool isVisible = IsVisibleToPlayer(actor) || IsVisibleToPlayer(actor.Location.Map, targetPos);
            if (isVisible)
            {
                AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(actor.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(targetPos), new Size(TILE_SIZE, TILE_SIZE))));
                AddMessage(MakeMessage(actor, String.Format("{0} a {1}!", Conjugate(actor, VERB_THROW), grenade.Model.SingleName)));
                RedrawPlayScreen();
                AnimDelay(DELAY_LONG);
                ClearOverlays();
                RedrawPlayScreen();
            }
            */
        }

        public void DoThrowGrenadePrimed(Point targetPos)
        {
            /*
            // get grenade.
            ItemGrenadePrimed primedGrenade = actor.GetEquippedWeapon() as ItemGrenadePrimed;
            if (primedGrenade == null)
                throw new InvalidOperationException("throwing primed grenade but no primed grenade equiped ");

            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // remove grenade from inventory.
            actor.Inventory.RemoveAllQuantity(primedGrenade);

            // drop primed grenade at target position.
            actor.Location.Map.DropItemAt(primedGrenade, targetPos);

            // message about throwing.
            bool isVisible = IsVisibleToPlayer(actor) || IsVisibleToPlayer(actor.Location.Map, targetPos);
            if (isVisible)
            {
                AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(actor.Location.Position), new Size(TILE_SIZE, TILE_SIZE))));
                AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(targetPos), new Size(TILE_SIZE, TILE_SIZE))));
                AddMessage(MakeMessage(actor, String.Format("{0} back a {1}!", Conjugate(actor, VERB_THROW), primedGrenade.Model.SingleName)));
                RedrawPlayScreen();
                AnimDelay(DELAY_LONG);
                ClearOverlays();
                RedrawPlayScreen();
            }
            */
        }

        public bool CanActorInitiateTradeWith(Actor target, out string reason)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            /////////////////////////////
            // Can't if any is true:
            // 1. Target is player.
            // 2. Actors are not traders and not leader->follower relationship.
            // 3. Target is sleeping.
            // 4. No item to trade.
            /////////////////////////////

            // 1. Target is player.
            if (target.IsPlayer)
            {
                reason = "target is player";
                return false;
            }

            // 2. Actors are not traders and not leader->follower relationship.
            if (!Model.Abilities.CanTrade && target.Leader != this)
            {
                reason = "can't trade";
                return false;
            }
            if (!target.Model.Abilities.CanTrade && target.Leader != this)
            {
                reason = "target can't trade";
                return false;
            }

            // TODO
            //if (AreEnemies(this, target))
            //{
            //    reason = "is an enemy";
            //    return false;
            //}

            // 3. Target is sleeping.
            if (target.IsSleeping)
            {
                reason = "is sleeping";
                return false;
            }

            // 4. No item to trade.
            if (Inventory == null || Inventory.IsEmpty)
            {
                reason = "nothing to offer";
                return false;
            }
            if (target.Inventory == null || target.Inventory.IsEmpty)
            {
                reason = "has nothing to trade";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }


        // alpha10 "fast" trade uses new trade mechanic of rating items and trades.
        // npcs will mostly only make mutually beneficial deals.
        // speaker and target are also somehow reversed from how they were in rs9(!?)
        // for the player should try to mimick most of trade results obtained by player negociating trade but not mandatory.
        public void DoTrade(Actor target)
        {
            /*
            // clean up activities
            speaker.Activity = Activity.IDLE;
            target.Activity = Activity.IDLE;

            bool isVisible = IsVisibleToPlayer(speaker) || IsVisibleToPlayer(target);
            if (isVisible) AddMessage(MakeMessage(speaker, string.Format("wants to make a quick trade with {0}.", target.Name)));

            // the basic idea is to pick an item the speaker wants from target, 
            // and offer an item the speaker is willing to get rid of.
            BaseAI speakerAI = speaker.Controller as BaseAI;
            BaseAI targetAI = target.Controller as BaseAI;
            Item offered, asked;
            offered = asked = null;

            // target not willing to trade if is ordered not to
            if ((!targetAI.Directives.CanTrade) && (speaker != target.Leader))
            {
                if (isVisible) AddMessage(MakeMessage(target, "is not willing to trade."));
                return;
            }

            // if speaker is the player, make the npc the speaker so the npc is the one offering an item.
            // alpha10.1 but not for bot
            if (speaker.IsPlayer)
            {
                // swap speaker and target so npc is always speaker in fast trade
                Actor swap = target;
                target = speaker;
                speaker = swap;
                targetAI = null;  // now player
                speakerAI = speaker.Controller as BaseAI;
            }

            // local lambdas just because -_-

            // get an item the speaker would like from target inventory.
            Item pickAskedItem(out ItemRating rating)
            {
                // pick an item in target inventory the speaker wants, or any item if target has only junk.
                List<Item> wants = target.Inventory.Filter((it) =>
                {
                    ItemRating r = speakerAI.RateItem(this, it, false);
                    // wants anything but junk. 
                    // don't limit to things speaker needs because the target ai is more likely to value the same item
                    // as being needed for himself! also makes for more varied deals.
                    return r != ItemRating.JUNK;
                });
                if (wants.Count == 0)
                {
                    // no non-junk items, extend to all items...
                    wants.AddRange(target.Inventory.Items);
                }

                // pick one from the wanted list.
                Item wantIt = wants[m_Rules.Roll(0, wants.Count)];
                rating = speakerAI.RateItem(this, wantIt, false);
                return wantIt;
            };

            // can return null 
            // get an item the speaker is willing to exhange for the target item it wants.
            Item pickOfferedItem(Item askedItem, ItemRating askedItemRating)
            {
                List<Item> offerables;

                // if target is npc: 
                //   - offer any item that could pass a trade deal with this npc (read their ai mind)
                // if target is player: 
                //   - cannot use rate trade offer on the npc itself...
                //   - so offer only items we rate less than the one we want (player should negociate deal instead)
                //   - accepting equal item ratings lead to bad deals for the npc, offering a need for a need (eg: a rifle for bullets!)
                // in all offers, never offer the same item model as the one asked eg: a pistol for a pistol!
                if (target.IsPlayer)
                {
                    offerables = speaker.Inventory.Filter((it) =>
                    {
                        return it.Model != askedItem.Model && speakerAI.RateItem(this, it, true) < askedItemRating;
                    });
                }
                else
                {
                    offerables = speaker.Inventory.Filter((it) =>
                    {
                        if (it.Model == askedItem.Model)
                            return false;
                        // read target ai mind...
                        TradeRating tr = targetAI.RateTradeOffer(this, speaker, it, askedItem);
                        // accept "Maybe" items to be a bit more realistic in not always making perfect deals
                        // ("hey! the ai always accept ai trades! they are cheating!")
                        // and let charisma influence the final result.
                        return tr != TradeRating.REFUSE;
                    });
                }

                if (offerables.Count == 0)
                {
                    // all our items are more valuable than the one we want or only silly deals. no deal.
                    return null;
                }

                Item offerIt = offerables[m_Rules.Roll(0, offerables.Count)];
                return offerIt;
            }

            ItemRating askedRating;
            asked = pickAskedItem(out askedRating);
            offered = pickOfferedItem(asked, askedRating);

            // if no item pairs found, failed trade.
            // either the target has no interesting items for speaker,
            // or the speaker has items too valuable for a trade.
            if ((asked == null) || (offered == null))
            {
                if (asked == null)
                {
                    // speaker finds nothing interesting in target inventory
                    if (isVisible)
                        AddMessage(MakeMessage(speaker, "is not interested in any item of your items."));
                }
                else
                {
                    // speaker has no item to give away (should not happen if target is player)
                    if (isVisible)
                        AddMessage(MakeMessage(speaker, string.Format("would prefer to keep {0} items.", HisOrHer(speaker))));
                }
                if (target.IsPlayer)
                    // help confused players...
                    AddMessage(new Message("(maybe try negociating a deal instead)", m_Session.WorldTime.TurnCounter, Color.Yellow));
                return;
            }

            // propose.
            // if player, ask.
            // if target is ai, check for it.
            // alpha10.1 handle bot player

            bool acceptTrade;
            if (isVisible) AddMessage(MakeMessage(speaker, string.Format("{0} {1} for {2}.", Conjugate(speaker, VERB_OFFER), offered.AName, asked.AName)));
            if (target.IsPlayer && !target.IsBotPlayer)  // speaker always ai unless bot
            {
                // ask player.
                AddOverlay(new OverlayPopup(TRADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, Point.Empty));
                RedrawPlayScreen();
                acceptTrade = WaitYesOrNo();
                ClearOverlays();
                RedrawPlayScreen();
            }
            else
            {
                // ask target ai/bot
                BaseAI ai;
#if DEBUG
                ai = target.IsPlayer && target.IsBotPlayer ? m_botControl : targetAI;
#else
                ai = targetAI;
#endif

                TradeRating r = ai.RateTradeOffer(this, speaker, offered, asked);
                if (r == TradeRating.ACCEPT)
                    acceptTrade = true;
                else if (r == TradeRating.REFUSE)
                    acceptTrade = false;
                else
                {
                    // use charisma on "maybe" trades, similar to what we do for the player in the negociating command we the ai won't
                    // exploit the game by asking several times so its ok not to store the charisma roll -_-
                    // note that a duo of charismatic npcs could in theory trade back and forth ha!
                    if (m_Rules.RollChance(m_Rules.ActorCharismaticTradeChance(speaker)))
                    {
                        if (isVisible) DoEmote(target, "Okay you convinced me.");
                        acceptTrade = true;
                    }
                    else
                        acceptTrade = false;
                }
            }

            // so, deal or not?
            if (acceptTrade)
            {
                if (isVisible) AddMessage(MakeMessage(target, string.Format("{0}.", Conjugate(target, VERB_ACCEPT_THE_DEAL))));
                if (target.IsPlayer || speaker.IsPlayer)
                    RedrawPlayScreen();

                // do it
                SwapActorItems(speaker, offered, target, asked);
            }
            else
            {
                if (isVisible) AddMessage(MakeMessage(target, string.Format("{0}.", Conjugate(target, VERB_REFUSE_THE_DEAL))));
                if (target.IsPlayer || speaker.IsPlayer)
                    RedrawPlayScreen();
            }

            */
        }

        public bool CanActorUnequipItem(Item it, out string reason)
        {
            if (it == null)
                throw new ArgumentNullException("item");

            /////////////////////////
            // Can't if any is true:
            // 1. Not equipped.
            // 2. Not in inventory.
            ////////////////////////

            // 1. Not equipped.
            if (!it.IsEquipped)
            {
                reason = "not equipped";
                return false;
            }

            // 2. Not in inventory.
            Inventory inv = Inventory;
            if (inv == null || !inv.Contains(it))
            {
                reason = "not in inventory";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        /// <summary>
        /// AP free
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="it"></param>
        public void DoUnequipItem(Item it, bool canMessage = true)
        {
            // unequip part.
            it.EquippedPart = DollPart.NONE;

            // update revelant datas.
            //OnUnequipItem(this, it);

            // TODO
            // message.
            //if (canMessage && IsVisibleToPlayer(actor))
            //    AddMessage(MakeMessage(actor, Conjugate(actor, VERB_UNEQUIP), it));
        }

        public bool CanActorUseExit(Point exitPoint, out string reason)
        {
            /////////////////////////////
            // Can't if any is true:
            // 1. No exit there.
            // 2. AI: can't use AI exits.
            // 3. Is sleeping.
            /////////////////////////////

            // 1. No exit there.
            if (Location.Map.GetExitAt(exitPoint) == null)
            {
                reason = "no exit there";
                return false;
            }

            // 2. AI: can't use AI exits.
            // alpha10.1 handle bots
            if ((!IsPlayer || IsBotPlayer) && !Model.Abilities.AI_CanUseAIExits)
            {
                reason = "this AI can't use exits";
                return false;
            }

            // 3. Is sleeping.
            if (IsSleeping)
            {
                reason = "is sleeping";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool DoUseExit(Point exitPoint)
        {
            // leave map.
            return DoLeaveMap(exitPoint, false);
        }

        public bool CanActorUseItem(Item it, out string reason)
        {
            if (it == null)
                throw new ArgumentNullException("item");

            /*
            //////////////////////////////////////////////////
            // Can't if any is true:
            // 1. Actor cant use items.
            // 2. Actor cant use this specific kind of items.
            // (unused 3. Actor cant use item in his present state.)
            // 4. Not in inventory.
            //////////////////////////////////////////////////

            // 1. Actor cant use items.
            if (!Model.Abilities.CanUseItems)
            {
                reason = "no ability to use items";
                return false;
            }

            // 2. Actor cant use this specific kind of items.
            if (it is ItemWeapon)
            {
                reason = "to use a weapon, equip it";
                return false;
            }
            else if (it is ItemFood && !actor.Model.Abilities.HasToEat)
            {
                reason = "no ability to eat";
                return false;
            }
            else if (it is ItemMedicine && actor.Model.Abilities.IsUndead)
            {
                reason = "undeads cannot use medecine";
                return false;
            }
            else if (it is ItemBarricadeMaterial)
            {
                reason = "to use material, build a barricade";
                return false;
            }
            else if (it is ItemAmmo)
            {
                ItemAmmo ammoIt = it as ItemAmmo;
                /////////////////////////////////////
                // Can't use ammo if
                // 1. No compatible weapon equipped.
                // 2. Weapon fully loaded.
                /////////////////////////////////////
                // 1. No compatible weapon equipped.
                ItemRangedWeapon eqRanged = actor.GetEquippedWeapon() as ItemRangedWeapon;
                if (eqRanged == null || eqRanged.AmmoType != ammoIt.AmmoType)
                {
                    reason = "no compatible ranged weapon equipped";
                    return false;
                }
                // 2. Weapon fully loaded.
                if (eqRanged.Ammo >= (eqRanged.Model as ItemRangedWeaponModel).MaxAmmo)
                {
                    reason = "weapon already fully loaded";
                    return false;
                }
                // fine!
            }
            // alpha10 new way to use spray scent
            //else if (it is ItemSprayScent)
            //{
            //    ItemSprayScent spray = it as ItemSprayScent;
            //    // can't if empty.
            //    if (spray.SprayQuantity <= 0)
            //    {
            //        reason = "no spray left.";
            //        return false;
            //    }
            //    // fine!
            //}
            else if (it is ItemTrap)
            {
                ItemTrap trap = it as ItemTrap;
                if (!trap.TrapModel.UseToActivate)
                {
                    reason = "does not activate manually";
                    return false;
                }
            }
            else if (it is ItemEntertainment)
            {
                if (!actor.Model.Abilities.IsIntelligent)
                {
                    reason = "not intelligent";
                    return false;
                }
                if ((it as ItemEntertainment).IsBoringFor(actor)) // alpha10 boring items item centric
                {
                    reason = "bored by this";
                    return false;
                }
            }

            // (3. Actor cant use item in his present state.)
            // todo if needed.

            // 4. Not in inventory.
            Inventory inv = actor.Inventory;
            if (inv == null || !inv.Contains(it))
            {
                reason = "not in inventory";
                return false;
            }
            */
            // all clear.
            reason = "";
            return true;
        }



        public void DoUseItem(Item it)
        {
            /*
            // alpha10 defrag ai inventories
            bool defragInventory = !actor.IsPlayer && it.Model.IsStackable;

            // concrete use.
            if (it is ItemFood)
                DoUseFoodItem(actor, it as ItemFood);
            else if (it is ItemMedicine)
                DoUseMedicineItem(actor, it as ItemMedicine);
            else if (it is ItemAmmo)
                DoUseAmmoItem(actor, it as ItemAmmo);
            //else if (it is ItemSprayScent)  // alpha10 new way to use spray scent
            //    DoUseSprayScentItem(actor, it as ItemSprayScent);
            else if (it is ItemTrap)
                DoUseTrapItem(actor, it as ItemTrap);
            else if (it is ItemEntertainment)
                DoUseEntertainmentItem(actor, it as ItemEntertainment);

            // alpha10 defrag ai inventories
            if (defragInventory)
                actor.Inventory.Defrag();

            */
        }

        public void DoWait()
        {
            /*
            // spend AP.
            SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);

            // message.
            if (IsVisibleToPlayer(actor))
            {
                if (actor.StaminaPoints < m_Rules.ActorMaxSTA(actor))
                    AddMessage(MakeMessage(actor, String.Format("{0} {1} breath.", Conjugate(actor, VERB_CATCH), HisOrHer(actor))));
                else
                    AddMessage(MakeMessage(actor, String.Format("{0}.", Conjugate(actor, VERB_WAIT))));
            }

            // regen STA.
            RegenActorStaminaPoints(actor, Rules.STAMINA_REGEN_WAIT);
            */
        }


        //
        // End Actions

        // Smell

        public float ActorSmell()
        {
            return (1.0f + SKILL_ZTRACKER_SMELL_BONUS * Model.StartingSheet.BaseSmellRating); //TODO * Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_TRACKER)) ;
        }

        public int ActorSmellThreshold()
        {
            // sleeping actors can't smell.
            if (IsSleeping)
                return -1;

            // actor model base value.
            float smellRating = ActorSmell();
            int minSmell = 1 + OdorScent.MAX_STRENGTH - (int)(smellRating * OdorScent.MAX_STRENGTH);
            return minSmell;
        }


        public int ActorFOV(WorldTime time, Weather weather)
        {
            /*
            // Sleeping actors have no FOV.
            if (actor.IsSleeping)
                return 0;

            // base value.
            int FOV = actor.Sheet.BaseViewRange;

            // lighting/weather
            Lighting light = actor.Location.Map.Lighting;
            switch (light)
            {
                case Lighting.DARKNESS:
                    // FoV in darkness depends on actor.
                    FOV = DarknessFov(actor);
                    break;
                case Lighting.LIT:
                    // nothing to do, unmodified base FOV.
                    break;
                case Lighting.OUTSIDE:
                    // night & weather penalty
                    FOV -= NightFovPenalty(actor, time);
                    FOV -= WeatherFovPenalty(actor, weather);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unhandled lighting");
            }

            // sleep penalty.
            if (IsActorExhausted(actor))
                FOV -= 2;
            else if (IsActorSleepy(actor))
                FOV -= 1;

            // light equipped or standing next to someone with light.
            // works only in darkness or during the night.
            if (light == Lighting.DARKNESS || (light == Lighting.OUTSIDE && time.IsNight))
            {
                int lightBonus = 0;

                lightBonus = GetLightBonusEquipped(actor);
                if (lightBonus == 0)
                {
                    Map map = actor.Location.Map;
                    if (map.HasAnyAdjacentInMap(actor.Location.Position,
                        (pt) =>
                        {
                            Actor other = map.GetActorAt(pt);
                            if (other == null)
                                return false;
                            return HasLightOnEquipped(other);
                        }))
                        lightBonus = 1;
                }
                FOV += lightBonus;
            }

            // standing on some map objects.
            MapObject mobj = actor.Location.Map.GetMapObjectAt(actor.Location.Position);
            if (mobj != null && mobj.StandOnFovBonus)
                ++FOV;

            // done.
            FOV = Math.Max(MINIMAL_FOV, FOV);
            return FOV;
            */
            return 1;
        }

        //
        // End Smell

        // Actor relations
        // alpha10 refactored and rewrote
        /// <summary>
        /// Check if both actors are enemies.
        /// - enemy factions
        /// - enemy gangs
        /// - personal enemies
        /// - group enemies (if checkGroups)
        /// Symetrical, don't need to call for actorB,actorA.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="other"></param>
        /// <param name="checkGroups"></param>
        /// <returns></returns>
        public bool IsEnemiesWith(Actor other, bool checkGroups = true)
        {
            if (other == null)
                return false;
            if (this == other)  // alpha10 silly fix
                return false;

            // Enemy factions? (symetrical)
            if (Faction.IsEnemyOf(other.Faction))
                return true;

            // Enemy gangs? (symetrical)
            if (Faction == other.Faction && IsInAGang && other.IsInAGang && GangID != other.GangID)
                return true;

            // alpha10 
            // Personal enemies? (symetrical)
            if (IsPersonalEnemyWith(other))
                return true;

            // alpha10
            // Enemy of groups (symetrical)
            if (checkGroups && IsGroupEnemiesWith(other))
                return true;

            // Not enemies.
            return false;
        }

        // alpha10
        /// <summary>
        /// Check if they are in an agressor-selfdefence reliation.
        /// Symetrical, don't need to call for actorB,actorA.
        /// </summary>
        /// <param name="actorA"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsPersonalEnemyWith(Actor other)
        {
            if (other == null)
                return false;
            if (this == other)
                return false;

            if (IsAggressorOf(other))
                return true;

            if (IsSelfDefenceFrom(other))
                return true;

            // doesnt need to check for target as the relation is symetrical (aggressor of <-> self defence from)
            return false;
        }

        // alpha10
        /// <summary>
        /// Check if they are enmemies through group relations : 
        /// - my leader enemies are my enemies
        /// - my mates enemies are my enemies.
        /// - my follower enemies are my enemies.
        /// Symetrical, don't need to call for actorB,actorA.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsGroupEnemiesWith(Actor other)
        {
            if (other == null)
                return false;
            if (this == other)
                return false;

            // my leader enemies are my enemies.
            // my mates enemies are my enemies.
            bool IsEnemyOfMyLeaderOrMates(Actor groupActor, Actor target)
            {
                if (groupActor.Leader.IsEnemiesWith(target, false))
                    return true;
                foreach (Actor mate in groupActor.Leader.Followers)
                    if (mate != groupActor && mate.IsEnemiesWith(target, false))
                        return true;
                return false;
            }

            // my followers enemies are my enemies
            bool IsEnemyOfMyFollowers(Actor groupActor, Actor target)
            {
                foreach (Actor follower in groupActor.Followers)
                    if (follower.IsEnemiesWith(target, false))
                        return true;
                return false;
            }

            // check A group
            if (HasLeader)
                if (IsEnemyOfMyLeaderOrMates(this, other))
                    return true;
            if (CountFollowers > 0)
                if (IsEnemyOfMyFollowers(this, other))
                    return true;

            // check B group
            if (other.HasLeader)
                if (IsEnemyOfMyLeaderOrMates(other, this))
                    return true;
            if (other.CountFollowers > 0)
                if (IsEnemyOfMyFollowers(other, this))
                    return true;

            // nope
            return false;
        }

        // End Actor Relations

        // Actor food

        public bool IsActorHungry()
        {
            return Model.Abilities.HasToEat && FoodPoints <= FOOD_HUNGRY_LEVEL;
        }

        public bool IsActorStarving()
        {
            return Model.Abilities.HasToEat && FoodPoints <= 0;
        }

        public bool IsRottingActorHungry()
        {
            return Model.Abilities.IsRotting && FoodPoints <= ROT_HUNGRY_LEVEL;
        }

        public bool IsRottingActorStarving()
        {
            return Model.Abilities.IsRotting && FoodPoints <= 0;
        }

        // End Actor food

        public bool IsAlmostSleepy()
        {
            if (!Model.Abilities.HasToSleep)
                return false;
            return SleepToHoursUntilSleepy(SleepPoints, Location.Map.LocalTime.IsNight) <= 3;
        }

        public int SleepToHoursUntilSleepy(int sleep, bool isNight)
        {
            int left = sleep - SLEEP_SLEEPY_LEVEL;
            if (isNight)
                left /= 2;
            if (left <= 0)
                return 0;
            return left / WorldTime.TURNS_PER_HOUR;
        }

        public bool IsActorTired()
        {
            return Model.Abilities.CanTire && StaminaPoints < STAMINA_MIN_FOR_ACTIVITY;
        }

        public bool CanActorMeleeAttack(Actor target, out string reason)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            //////////////////////////////
            // Cant run if any is true:
            // 1. Target not adjacent in map or not sharing an exit.
            // 2. Stamina below min level.
            // 3. Target is dead (doh!).
            //////////////////////////////

            // 1. Target not adjacent in map or not sharing an exit.
            if (Location.Map == target.Location.Map)
            {
                if (!Location.Position.IsAdjacent(target.Location.Position))
                {
                    reason = "not adjacent";
                    return false;
                }
            }
            else
            {
                // check there is an exit at both positions.
                Exit fromExit = Location.Map.GetExitAt(Location.Position);
                if (fromExit == null)
                {
                    reason = "not reachable";
                    return false;
                }
                Exit toExit = target.Location.Map.GetExitAt(target.Location.Position);
                if (toExit == null)
                {
                    reason = "not reachable";
                    return false;
                }
                // check the target stands on the exit.
                if (fromExit.ToMap != target.Location.Map || fromExit.ToPosition != target.Location.Position)
                {
                    reason = "not reachable";
                    return false;
                }
            }

            // 2. Stamina below min level.
            if (StaminaPoints < STAMINA_MIN_FOR_ACTIVITY)
            {
                reason = "not enough stamina to attack";
                return false;
            }

            // 3. Target is dead (doh!).
            // oddly this can happen for the AI when simulating... not clear why...
            // so this is a lame fix to prevent KillingActor from throwing an exception :-)
            if (target.IsDead)
            {
                reason = "already dead!";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool HasActorPushAbility()
        {
            return Model.Abilities.CanPush;
            //||
            //    actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.STRONG) > 0 ||
            //    actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_STRONG) > 0;
        }

        public Attack ActorRangedAttack(Attack baseAttack, int distance, Actor target)
        {
            int hitMod = 0;
            int dmgBonus = 0;

            // skill bonuses.
            //switch (baseAttack.Kind)
            //{
            //    case AttackKind.BOW:
            //        hitMod = SKILL_BOWS_ATK_BONUS * Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.BOWS);
            //        dmgBonus = SKILL_BOWS_DMG_BONUS * Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.BOWS);
            //        break;
            //    case AttackKind.FIREARM:
            //        hitMod = SKILL_FIREARMS_ATK_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.FIREARMS);
            //        dmgBonus = SKILL_FIREARMS_DMG_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.FIREARMS);
            //        break;
            //}

            //if (target != null && target.Model.Abilities.IsUndead)
            //    dmgBonus += ActorDamageBonusVsUndeads();

            // distance vs range penalties/bonus.
            int efficientRange = baseAttack.EfficientRange;
            // alpha10 distance as % modifier instead of flat bonus
            float distanceMod = 1;
            if (distance != efficientRange)
            {
                float distanceScale = (efficientRange - distance) / (float)baseAttack.Range;
                // bigger effect (penalty) beyond efficient range
                if (distance > efficientRange)
                    distanceScale *= 2;

                distanceMod = 1 + distanceScale;
            }
            float hit = (baseAttack.HitValue + hitMod) * distanceMod;
            float rapidHit1 = (baseAttack.Hit2Value + hitMod) * distanceMod;
            float rapidHit2 = (baseAttack.Hit3Value + hitMod) * distanceMod;

            float dmg = baseAttack.DamageValue + dmgBonus;

            //// sleep penalty.
            //if (IsActorExhausted())
            //{
            //    hit *= FIRING_WHEN_SLP_EXHAUSTED;
            //    rapidHit1 *= FIRING_WHEN_SLP_EXHAUSTED;
            //    rapidHit2 *= FIRING_WHEN_SLP_EXHAUSTED;
            //}
            //else if (IsActorSleepy())
            //{
            //    hit *= FIRING_WHEN_SLP_SLEEPY;
            //    rapidHit1 *= FIRING_WHEN_SLP_SLEEPY;
            //    rapidHit2 *= FIRING_WHEN_SLP_SLEEPY;
            //}

            //// stamina penalty.
            //if (actor.IsActorTired())
            //{
            //    hit *= FIRING_WHEN_STA_TIRED;
            //    rapidHit1 *= FIRING_WHEN_STA_TIRED;
            //    rapidHit2 *= FIRING_WHEN_STA_TIRED;
            //}
            //else if (actor.StaminaPoints < ActorMaxSTA(actor))
            //{
            //    hit *= FIRING_WHEN_STA_NOT_FULL;
            //    rapidHit1 *= FIRING_WHEN_STA_NOT_FULL;
            //    rapidHit2 *= FIRING_WHEN_STA_NOT_FULL;
            //}

            // return attack.
            return Attack.RangedAttack(baseAttack.Kind, baseAttack.Verb, (int)hit, (int)rapidHit1, (int)rapidHit2, (int)dmg, baseAttack.Range);
        }

        // alpha10
        /// <summary>
        /// Estimate chances to hit with a ranged attack. <br></br>
        /// Simulate a large number of rolls attack vs defence and returns % of hits.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="target"></param>
        /// <param name="shotCounter">0 for normal shot, 1 for 1st rapid fire shot, 2 for 2nd rapid fire shot</param>
        /// <returns>[0..100]</returns>
        public int ComputeChancesRangedHit(Actor target, int shotCounter)
        {
            Attack attack = ActorRangedAttack(CurrentRangedAttack, DistanceHelpers.GridDistance(Location.Position, target.Location.Position), target);
            Defence defence = ActorDefence(target.CurrentDefence);

            int hitValue = (shotCounter == 0 ? attack.HitValue : shotCounter == 1 ? attack.Hit2Value : attack.Hit3Value);
            int defValue = defence.Value;

            const int ROLLS = 1000;
            int hits = 0;
            for (int i = 0; i < ROLLS; i++)
            {
                int atkRoll = DiceRoller.RollSkill(hitValue);
                int defRoll = DiceRoller.RollSkill(defValue);
                if (atkRoll > defRoll)
                    hits++;
            }

            int percent = (100 * hits) / ROLLS;
            return percent;
        }

        public Defence ActorDefence(Defence baseDefence)
        {
            // Sleeping actors are defenceless.
            if (IsSleeping)
                return new Defence(0, 0, 0);

            // Base value + skill.
            //int defBonus = SKILL_AGILE_DEF_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.AGILE) +
            //    SKILL_ZAGILE_DEF_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_AGILE);
            float def = baseDefence.Value + 0;//+ defBonus;

            // Sleepy effect.
            if (IsActorExhausted())
                def /= 2f;
            else if (IsActorSleepy())
                def *= 3f / 4f;

            // done.
            return new Defence((int)def, baseDefence.Protection_Hit, baseDefence.Protection_Shot);
        }

        public bool IsActorSleepy()
        {
            return Model.Abilities.HasToSleep && SleepPoints <= SLEEP_SLEEPY_LEVEL;
        }

        public bool IsActorExhausted()
        {
            return Model.Abilities.HasToSleep && SleepPoints <= 0;
        }


        public bool IsOnCouch()
        {
            MapObject mapObj = Location.Map.GetMapObjectAt(Location.Position);
            if (mapObj == null)
                return false;

            return mapObj.IsCouch;
        }

        // alpha10
        public bool IsSafeFromTrap(ItemTrap trap)
        {
            if (trap.Owner == null)
                return false;
            if (trap.Owner == this)
                return true;
            return IsInGroupWith(trap.Owner);
        }

        public int CountBarricadingMaterial()
        {
            if (Inventory.IsEmpty)
                return 0;

            int count = 0;
            foreach (Item it in Inventory.Items)
                if (it is ItemBarricadeMaterial)
                    count += it.Quantity;
            return count;
        }

        public int ActorBarricadingMaterialNeedForFortification(bool isLarge)
        {
            int baseCost = isLarge ? 4 : 2;
            //int skillBonus = (builder.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CARPENTRY) >= 3 ? SKILL_CARPENTRY_LEVEL3_BUILD_BONUS : 0);
            int skillBonus = 0;
            return Math.Max(1, baseCost - skillBonus);
        }

        public int ActorMaxHPs()
        {
            //int skillBonus = (SKILL_TOUGH_HP_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.TOUGH))
            //    + (SKILL_ZTOUGH_HP_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_TOUGH));
            int skillBonus = 1;
            return Sheet.BaseHitPoints + skillBonus;
        }

        public int ActorMaxSanity()
        {
            return Sheet.BaseSanity;
        }

        public int ActorMaxThrowRange(int baseRange)
        {
            int bonus = 1;//SKILL_STRONG_THROW_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.STRONG);

            return baseRange + bonus;
        }

        public int ActorMaxInv()
        {
            int skillBonus = 1;//SKILL_HAULER_INV_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.HAULER);

            return Sheet.BaseInventoryCapacity + skillBonus;
        }

        public int ActorUnsuspicousChance(Actor actor)
        {
            // base = unsuspicious skill.
            int baseChance = 50;//SKILL_UNSUSPICIOUS_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.UNSUSPICIOUS);

            // bonus.
            int bonus = 0;

            // wearing some outfit.
            //ItemBodyArmor armor = actor.GetEquippedItem(DollPart.TORSO) as ItemBodyArmor;
            //if (armor != null)
            //{
            //    if (observer.Faction.ID == (int)GameFactions.IDs.ThePolice)
            //    {
            //        if (armor.IsHostileForCops())
            //            bonus -= UNSUSPICIOUS_BAD_OUTFIT_PENALTY;
            //        else if (armor.IsFriendlyForCops())
            //            bonus += UNSUSPICIOUS_GOOD_OUTFIT_BONUS;
            //    }
            //    else if (observer.Faction.ID == (int)GameFactions.IDs.TheBikers)
            //    {
            //        if (armor.IsHostileForBiker((GameGangs.IDs)observer.GangID))
            //            bonus -= UNSUSPICIOUS_BAD_OUTFIT_PENALTY;
            //        else if (armor.IsFriendlyForBiker((GameGangs.IDs)observer.GangID))
            //            bonus += UNSUSPICIOUS_GOOD_OUTFIT_BONUS;
            //    }
            //}

            return baseChance + bonus;
        }

        public int ActorSpotMurdererChance(Actor murderer)
        {
            int spotterBonus = MURDER_SPOTTING_MURDERCOUNTER_BONUS * murderer.MurdersCounter;
            int distancePenalty = MURDERER_SPOTTING_DISTANCE_PENALTY * DistanceHelpers.GridDistance(Location.Position, murderer.Location.Position);

            return MURDERER_SPOTTING_BASE_CHANCE + spotterBonus - distancePenalty;
        }

        public int ActorMaxFood()
        {
            int skillBonus = (int)(Sheet.BaseFoodPoints);// * SKILL_LIGHT_EATER_MAXFOOD_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LIGHT_EATER));

            return Sheet.BaseFoodPoints + skillBonus;
        }

        public int ActorMaxSTA()
        {
            int skillBonus = 1;// (SKILL_HIGH_STAMINA_STA_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.HIGH_STAMINA));

            return Sheet.BaseStaminaPoints + skillBonus;
        }

        public int ActorItemNutritionValue(int baseValue)
        {
            int skillBonus = (int)(baseValue); // * SKILL_LIGHT_EATER_FOOD_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LIGHT_EATER));

            return baseValue + skillBonus;
        }



        public int ActorMaxRot()
        {
            int skillBonus = (int)(Sheet.BaseFoodPoints);// * SKILL_ZLIGHT_EATER_MAXFOOD_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_LIGHT_EATER));

            return Sheet.BaseFoodPoints + skillBonus;
        }

        public int ActorMaxSleep()
        {
            int skillBonus = (int)(Sheet.BaseSleepPoints);// * SKILL_AWAKE_SLEEP_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.AWAKE));

            return (Sheet.BaseSleepPoints + skillBonus);
        }

        public int ActorSleepRegen(bool isOnCouch)
        {
            int baseRegen = isOnCouch ? SLEEP_COUCH_SLEEPING_REGEN : SLEEP_NOCOUCH_SLEEPING_REGEN;
            int skillBonus = (int)(baseRegen);// * SKILL_AWAKE_SLEEP_REGEN_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.AWAKE));

            return baseRegen + skillBonus;
        }

        public bool IsActorDisturbed()
        {
            return Model.Abilities.HasSanity && Sanity <= ActorDisturbedLevel();
        }

        public bool IsActorInsane()
        {
            return Model.Abilities.HasSanity && Sanity <= 0;
        }

        public int ActorDisturbedLevel()
        {
            float factor = 1.0f;// SKILL_STRONG_PSYCHE_LEVEL_BONUS * Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.STRONG_PSYCHE);
            return (int)(SANITY_UNSTABLE_LEVEL * factor);
        }

        public bool IsActorTrustingLeader()
        {
            if (!HasLeader)
                return false;

            return TrustInLeader >= TRUST_TRUSTING_THRESHOLD;
        }

        public bool CanActorRun(out string reason)
        {
            //////////////////////////////
            // Cant run if any is true:
            // 1. Has not CanRun ability.
            // 2. Stamina below min level.
            //////////////////////////////

            // 1. Has not CanRun ability.
            if (!Model.Abilities.CanRun)
            {
                reason = "no ability to run";
                return false;
            }

            // 2. Stamina below min level.
            if (StaminaPoints < STAMINA_MIN_FOR_ACTIVITY)
            {
                reason = "not enough stamina to run";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public int ActorSpeed()
        {
            float speed = Doll.Body.Speed;

            // stamina.
            if (IsActorTired())
                speed *= 2f / 3f;

            // sleep.
            if (IsActorExhausted())
                speed /= 2f;
            else if (IsActorSleepy())
                speed *= 2f / 3f;

            // wearing armor.
            //ItemBodyArmor armor = actor.GetEquippedItem(DollPart.TORSO) as ItemBodyArmor;
            //if (armor != null)
            //    speed -= armor.Weight;

            // dragging corpses.
            if (DraggedCorpse != null)
                speed /= 2f;

            // done, speed must be >= 0.
            return Math.Max((int)speed, 0);
        }

        /// <summary>
        /// If actor spend a turn now, will actor get the chance to act before other?
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool WillActorActAgainBefore(Actor other)
        {
            // if other can still act, nope.
            if (other.ActionPoints > 0)
                return false;

            // if other will be able to act next turn BEFORE actor, nope.
            if (other.ActionPoints + other.ActorSpeed() > 0 && Location.Map.IsActorBeforeInMapList(other, this))
                return false;

            // safe!
            return true;
        }

        public void DoEmote(string text, bool isDanger = false)
        {
           // if (IsVisibleToPlayer(actor))
           //     AddMessage(new Message(String.Format("{0} : {1}", actor.Name, text), actor.Location.Map.LocalTime.TurnCounter, isDanger ? SAYOREMOTE_DANGER_COLOR : SAYOREMOTE_NORMAL_COLOR));
        }

        public bool HasActorJumpAbility()
        {
            return Model.Abilities.CanJump;// ||
               // actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.AGILE) > 0 ||
              //  actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_AGILE) > 0;
        }

        public int ActorMaxFollowers()
        {
            return SKILL_LEADERSHIP_FOLLOWER_BONUS; //* actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LEADERSHIP);
        }
    }
}

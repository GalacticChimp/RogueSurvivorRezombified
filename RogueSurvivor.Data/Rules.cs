using System;
using System.Collections.Generic;
using System.Drawing;

using djack.RogueSurvivor.Data.Items;
using djack.RogueSurvivor.Data.Helpers;
using djack.RogueSurvivor.Data.Enums;
using djack.RogueSurvivor.Common;

namespace djack.RogueSurvivor.Data
{
    public static class Rules
    {
        public const int BASE_ACTION_COST = 100;
        public const int BASE_SPEED = BASE_ACTION_COST;

        /// <summary>
        /// Marker for infinite stamina, value doesnt matter since !Abilities.CanTire is checked instead.
        /// </summary>
        public const int STAMINA_INFINITE = 99;

        /// <summary>
        /// Minimum stamina for doing tiring actions (melee, running, jumping...)
        /// </summary>
        public const int STAMINA_MIN_FOR_ACTIVITY = 10;

        /// <summary>
        /// Stamina cost for running.
        /// </summary>
        public const int STAMINA_COST_RUNNING = 4;

        /// <summary>
        /// Bonus to stamina when waiting.
        /// </summary>
        public const int STAMINA_REGEN_WAIT = 2;

        /// <summary>
        /// Bonus to stamina each turn.
        /// </summary>
        public const int STAMINA_REGEN_PER_TURN = 2;

        /// <summary>
        /// Stamina cost for jumping.
        /// </summary>
        public const int STAMINA_COST_JUMP = 8;

        /// <summary>
        /// Stamina cost for melee fighting.
        /// </summary>
        public const int STAMINA_COST_MELEE_ATTACK = 8;

        /// <summary>
        /// Stamina cost for moving with a dragged corpse.
        /// </summary>
        public const int STAMINA_COST_MOVE_DRAGGED_CORPSE = 8;
        
        public const int JUMP_STUMBLE_CHANCE = 25;
        public const int JUMP_STUMBLE_ACTION_COST = BASE_ACTION_COST;

        /// <summary>
        /// How much scent living actors drop on their tile.
        /// </summary>
        public const int LIVING_SCENT_DROP = OdorScent.MAX_STRENGTH;

        /// <summary>
        /// How much scent undead masters drop on their tile.
        /// </summary>
        public const int UNDEAD_MASTER_SCENT_DROP = OdorScent.MAX_STRENGTH;

        const int MINIMAL_FOV = 2;

        public const int FOV_PENALTY_SUNSET = 1;
        public const int FOV_PENALTY_EVENING = 2;
        public const int FOV_PENALTY_MIDNIGHT = 3;
        public const int FOV_PENALTY_DEEP_NIGHT = 4;
        public const int FOV_PENALTY_SUNRISE = 2;
        public const int NIGHT_STA_PENALTY = 2;

        public const int FOV_PENALTY_RAIN = 1;
        public const int FOV_PENALTY_HEAVY_RAIN = 2;

        public const int MELEE_WEAPON_BREAK_CHANCE = 1;
        public const int MELEE_WEAPON_FRAGILE_BREAK_CHANCE = 3;
        public const int MELEE_DISARM_BASE_CHANCE = 5;  // alpha10
        public const int FIREARM_JAM_CHANCE_NO_RAIN = 1;
        public const int FIREARM_JAM_CHANCE_RAIN = 3;
        const float FIRING_WHEN_STA_TIRED = 0.75f;     // -25%
        const float FIRING_WHEN_STA_NOT_FULL = 0.90f;  // -10%
        // alpha10 made into constants
        const float FIRING_WHEN_SLP_EXHAUSTED = 0.50f; // -50%
        const float FIRING_WHEN_SLP_SLEEPY = 0.75f; // -25%

        public const int BLOOD_WALL_SPLAT_CHANCE = 20;

        public const int BODY_ARMOR_BREAK_CHANCE = 2;

        public const int FOOD_BASE_POINTS = WorldTime.TURNS_PER_HOUR * 48;
        public const int FOOD_HUNGRY_LEVEL = FOOD_BASE_POINTS / 2;

        public const int ROT_BASE_POINTS = WorldTime.TURNS_PER_DAY * 4;
        public const int ROT_HUNGRY_LEVEL = ROT_BASE_POINTS / 2;

        public const int SLEEP_BASE_POINTS = WorldTime.TURNS_PER_HOUR * 60;  // 60 = starting game at midnight => sleepy in late evening.
        public const int SLEEP_SLEEPY_LEVEL = SLEEP_BASE_POINTS / 2;

        public const int SANITY_BASE_POINTS = WorldTime.TURNS_PER_DAY * 4;
        public const int SANITY_UNSTABLE_LEVEL = SANITY_BASE_POINTS / 2;
        public const int SANITY_NIGHTMARE_CHANCE = 2;
        public const int SANITY_NIGHTMARE_SLP_LOSS = 2 * WorldTime.TURNS_PER_HOUR;
        public const int SANITY_NIGHTMARE_SAN_LOSS = WorldTime.TURNS_PER_HOUR;
        public const int SANITY_NIGHTMARE_STA_LOSS = 10 * STAMINA_COST_RUNNING;  // alpha10 -- worth running for 10 turns
        public const int SANITY_INSANE_ACTION_CHANCE = 5;

        public const int SANITY_HIT_BUTCHERING_CORPSE = WorldTime.TURNS_PER_HOUR;
        public const int SANITY_HIT_UNDEAD_EATING_CORPSE = 2 * WorldTime.TURNS_PER_HOUR;
        public const int SANITY_HIT_LIVING_EATING_CORPSE = 4 * WorldTime.TURNS_PER_HOUR;
        public const int SANITY_HIT_EATEN_ALIVE = 4 * WorldTime.TURNS_PER_HOUR;
        public const int SANITY_HIT_ZOMBIFY = 2 * WorldTime.TURNS_PER_HOUR;
        public const int SANITY_HIT_BOND_DEATH = 8 * WorldTime.TURNS_PER_HOUR;

        // alpha10 increased san recovery; chating & trading also recover san
        public const int SANITY_RECOVER_KILL_UNDEAD = 3 * WorldTime.TURNS_PER_HOUR;  // was 2h
        public const int SANITY_RECOVER_BOND_CHANCE = 5;
        public const int SANITY_RECOVER_BOND = 4 * WorldTime.TURNS_PER_HOUR;  // was 1h
        public const int SANITY_RECOVER_CHAT_OR_TRADE = 3 * WorldTime.TURNS_PER_HOUR;

        /// <summary>
        /// When starving, chance of dying from starvation each turn.
        /// </summary>
        public const int FOOD_STARVING_DEATH_CHANCE = 5;

        /// <summary>
        /// When eating expired food, chances of vomitting.
        /// </summary>
        public const int FOOD_EXPIRED_VOMIT_CHANCE = 25;

        /// <summary>
        /// Stamina lost for vomitting.
        /// </summary>
        public const int FOOD_VOMIT_STA_COST = 100;

        /// <summary>
        /// When rot-starving %% chance to loose an HP.
        /// </summary>
        public const int ROT_STARVING_HP_CHANCE = 5;

        /// <summary>
        /// When rot-hungry %% chance to loose a skill.
        /// </summary>
        public const int ROT_HUNGRY_SKILL_CHANCE = 5;

        /// <summary>
        /// When exhausted, chance of collapsing each turn.
        /// </summary>
        public const int SLEEP_EXHAUSTION_COLLAPSE_CHANCE = 5;

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

        /// <summary>
        /// When sleeping and healing, how many HPs regained.
        /// </summary>
        public const int SLEEP_HEAL_HITPOINTS = 2;

        public const int LOUD_NOISE_RADIUS = 5;
        const int LOUD_NOISE_BASE_WAKEUP_CHANCE = 10;
        const int LOUD_NOISE_DISTANCE_BONUS = 10;

        public const int VICTIM_DROP_GENERIC_ITEM_CHANCE = 50;
        public const int VICTIM_DROP_AMMOFOOD_ITEM_CHANCE = 100;

        public const int IMPROVED_WEAPONS_FROM_BROKEN_WOOD_CHANCE = 25;

        // alpha10 replaced with rapid fire attacks property of ranged weapons
        /*
        #region Rapid fire
        public const float RAPID_FIRE_FIRST_SHOT_ACCURACY = 0.70f;  // alpha9 was 0.50f;
        public const float RAPID_FIRE_SECOND_SHOT_ACCURACY = 0.50f;  // alpha9 was 0.30f;
        #endregion
        */

        public const int ZTRACKINGRADIUS = 6;

        public const int DEFAULT_ACTOR_WEIGHT = 10;

        /// <summary>
        /// When raining, chance per turn to test effects on fire on the map.
        /// 100 = will check every rain turn. Not recommended as it eats CPU.
        /// </summary>
        public const int FIRE_RAIN_TEST_CHANCE = 1;

        /// <summary>
        /// Chance for rain to put out a fire.
        /// </summary>
        public const int FIRE_RAIN_PUT_OUT_CHANCE = 10;

        public const int TRUST_NEUTRAL = 0;
        public const int TRUST_TRUSTING_THRESHOLD = 12 * WorldTime.TURNS_PER_HOUR; // 12h of sticking together.
        public const int TRUST_MIN = -TRUST_TRUSTING_THRESHOLD;
        public const int TRUST_MAX = 4 * TRUST_TRUSTING_THRESHOLD;
        public const int TRUST_BOND_THRESHOLD = TRUST_MAX;
        public const int TRUST_BASE_INCREASE = 1;                                       // 1pt per turn.
        public const int TRUST_GOOD_GIFT_INCREASE = 3 * WorldTime.TURNS_PER_HOUR;       // 3 trust-hours gained.
        public const int TRUST_MISC_GIFT_INCREASE = TRUST_BASE_INCREASE + TRUST_GOOD_GIFT_INCREASE / 10;
        public const int TRUST_GIVE_ITEM_ORDER_PENALTY = -WorldTime.TURNS_PER_HOUR;     // 1 trust-hours lost.
        public const int TRUST_LEADER_KILL_ENEMY = 3 * WorldTime.TURNS_PER_HOUR;        // 3 trust-hours gain.

        public const int MURDERER_SPOTTING_BASE_CHANCE = 5;
        public const int MURDERER_SPOTTING_DISTANCE_PENALTY = 1;
        public const int MURDER_SPOTTING_MURDERCOUNTER_BONUS = 5;

        const float INFECTION_BASE_FACTOR = 1.0f;

        public static int INFECTION_LEVEL_1_WEAK  = 10;
        public static int INFECTION_LEVEL_2_TIRED = 30;
        public static int INFECTION_LEVEL_3_VOMIT = 50;
        public static int INFECTION_LEVEL_4_BLEED = 75;
        public static int INFECTION_LEVEL_5_DEATH = 100;

        public static int INFECTION_LEVEL_1_WEAK_STA = 24;//16;
        public static int INFECTION_LEVEL_2_TIRED_STA = 24;//16;
        public static int INFECTION_LEVEL_2_TIRED_SLP = 3 * WorldTime.TURNS_PER_HOUR;//2 * WorldTime.TURNS_PER_HOUR;
        public static int INFECTION_LEVEL_4_BLEED_HP = 6;//4;

        public static int INFECTION_EFFECT_TRIGGER_CHANCE_1000 = (int)(1000 * 2.0f / WorldTime.TURNS_PER_DAY);

        const int CORPSE_ZOMBIFY_BASE_CHANCE = 0;
        const int CORPSE_ZOMBIFY_DELAY = 6 * WorldTime.TURNS_PER_HOUR;
        const float CORPSE_ZOMBIFY_INFECTIONP_FACTOR = 1f; // 1% infection = X% to raise when checked.
        const float CORPSE_ZOMBIFY_NIGHT_FACTOR = 2.0f;
        const float CORPSE_ZOMBIFY_DAY_FACTOR = 0.01f;
        const float CORPSE_ZOMBIFY_TIME_FACTOR = 1f / WorldTime.TURNS_PER_DAY; // -1% per day
        const float CORPSE_EATING_NUTRITION_FACTOR = 10.0f;
        const float CORPSE_EATING_INFECTION_FACTOR = 0.1f;

        const float CORPSE_DECAY_PER_TURN = 4f / WorldTime.TURNS_PER_DAY; // -1 HP per day ~ 1 week to rot at 30 HP.

        public const int GIVE_RARE_ITEM_DAY = 7;
        public const int GIVE_RARE_ITEM_CHANCE = 5;

        public const int UPGRADE_SKILLS_TO_CHOOSE_FROM = 5;
        public const int UNDEAD_UPGRADE_SKILLS_TO_CHOOSE_FROM = 2;

        public static int SKILL_AGILE_ATK_BONUS = 2;
        public static int SKILL_AGILE_DEF_BONUS = 4;

        public static float SKILL_AWAKE_SLEEP_BONUS = 0.10f;
        public static float SKILL_AWAKE_SLEEP_REGEN_BONUS = 0.15f;

        public static int SKILL_BOWS_ATK_BONUS = 10;
        public static int SKILL_BOWS_DMG_BONUS = 4;

        public static float SKILL_CARPENTRY_BARRICADING_BONUS = 0.15f;
        public static int SKILL_CARPENTRY_LEVEL3_BUILD_BONUS = 1;

        public static int SKILL_CHARISMATIC_TRUST_BONUS = 2;
        public static int SKILL_CHARISMATIC_TRADE_BONUS = 10;

        public static int SKILL_FIREARMS_ATK_BONUS = 10;
        public static int SKILL_FIREARMS_DMG_BONUS = 2;

        public static int SKILL_HARDY_HEAL_CHANCE_BONUS = 1;

        public static int SKILL_HAULER_INV_BONUS = 1;

        public static int SKILL_HIGH_STAMINA_STA_BONUS = 8;

        public static int SKILL_LEADERSHIP_FOLLOWER_BONUS = 1;

        public static float SKILL_LIGHT_EATER_MAXFOOD_BONUS = 0.10f;
        public static float SKILL_LIGHT_EATER_FOOD_BONUS = 0.15f;

        public static int SKILL_LIGHT_FEET_TRAP_BONUS = 15; // alpha10 prev value was 5

        public static int SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS = 20;  // alpha10 prev value was 10

        public static int SKILL_MARTIAL_ARTS_ATK_BONUS = 6;
        public static int SKILL_MARTIAL_ARTS_DMG_BONUS = 2;
        public static int SKILL_MARTIAL_ARTS_DISARM_BONUS = 10;  // alpha10

        public static float SKILL_MEDIC_BONUS = 0.15f;
        public static int SKILL_MEDIC_REVIVE_BONUS = 10;
        public static int SKILL_MEDIC_LEVEL_FOR_REVIVE_EST = 1;

        public static int SKILL_NECROLOGY_UNDEAD_BONUS = 2;
        public static int SKILL_NECROLOGY_CORPSE_BONUS = 4;
        public static int SKILL_NECROLOGY_LEVEL_FOR_INFECTION = 3;
        public static int SKILL_NECROLOGY_LEVEL_FOR_RISE = 5;

        public static float SKILL_STRONG_PSYCHE_LEVEL_BONUS = 0.15f;

        public static int SKILL_STRONG_DMG_BONUS = 2;
        public static int SKILL_STRONG_THROW_BONUS = 1;
        public static int SKILL_STRONG_RESIST_DISARM_BONUS = 5;  // alpha10

        public static int SKILL_TOUGH_HP_BONUS = 6;

        public static int SKILL_UNSUSPICIOUS_BONUS = 20;  // alpha10

        public static int UNSUSPICIOUS_BAD_OUTFIT_PENALTY = 75;  // alpha10 ; prev was 50
        public static int UNSUSPICIOUS_GOOD_OUTFIT_BONUS = 75; // alpha10 ; prev was 50

        public static int SKILL_ZAGILE_ATK_BONUS = 1;
        public static int SKILL_ZAGILE_DEF_BONUS = 2;

        public static int SKILL_ZSTRONG_DMG_BONUS = 2;

        public static int SKILL_ZTOUGH_HP_BONUS = 4;

        public static float SKILL_ZEATER_REGEN_BONUS = 0.20f;

        public static float SKILL_ZTRACKER_SMELL_BONUS = 0.10f;

        public static int SKILL_ZLIGHT_FEET_TRAP_BONUS = 3;

        public static int SKILL_ZGRAB_CHANCE = 4;  // alpha10 prev was 2

        public static float SKILL_ZINFECTOR_BONUS = 0.15f;

        public static float SKILL_ZLIGHT_EATER_MAXFOOD_BONUS = 0.15f;
        public static float SKILL_ZLIGHT_EATER_FOOD_BONUS = 0.10f;

        /// <summary>
        /// Apply a random deviation to a value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="deviation">amount (not percentage!) of deviation from value</param>
        /// <returns></returns>
        public static float Randomize(float value, float deviation)
        {
            float halfDeviation = deviation/2f;
            return value - halfDeviation * DiceRoller.RollFloat() + halfDeviation * DiceRoller.RollFloat();
        }

        public static int RollX(Map map)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            return DiceRoller.Roll(0, map.Width);
        }

        public static int RollY(Map map)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            return DiceRoller.Roll(0, map.Height);
        }



        /// <summary>
        /// [damageValue/2, damageValue] 
        /// </summary>
        /// <param name="damageValue"></param>
        /// <returns></returns>
        public static int RollDamage(int damageValue)
        {
            if (damageValue <= 0)
                return 0;
            return DiceRoller.Roll(damageValue / 2, damageValue + 1);
        }

        public static Location RollNeighbourInMap(Location from)
        {
            while (true)
            {
                Location next = from + DirectionHelpers.RollDirection();

                if (next.Map != from.Map)
                    continue;

                if (from.Map.IsInBounds(next.Position.X, next.Position.Y))
                    return next;
            }
        }

        public static bool RollValidMoveInMap(Actor actor, int maxTries, out Location nextLocation)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            for (int i = 0; i < maxTries; i++)
            {
                nextLocation = RollNeighbourInMap(actor.Location);

                if (actor.IsWalkableFor(nextLocation.Map, nextLocation.Position.X, nextLocation.Position.Y, out string reason))
                    return true;
            }

            nextLocation = actor.Location;
            return false;
        }

        public static bool RollValidMoveDirection(Actor actor, int maxTries, out Direction direction)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            for (int i = 0; i < maxTries; i++)
            {
                direction = DirectionHelpers.RollDirection();
                Location nextLocation = actor.Location + direction;
                if (nextLocation.Map != actor.Location.Map)
                    continue;
                if (actor.IsWalkableFor(nextLocation, out string reason))
                    return true;
            }

            direction = null;
            return false;
        }

        public static bool RollValidBumpDirection(Actor actor, int maxTries, out Direction direction)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            for (int i = 0; i < maxTries; i++)
            {
                direction = DirectionHelpers.RollDirection();
                Location nextLocation = actor.Location + direction;
                if (nextLocation.Map != actor.Location.Map)
                    continue;
                return true;
            }

            direction = null;
            return false;
        }

        public static bool IsItemBatteryPowered(Item it)
        {
            if (it == null) return false;
            return (it is ItemLight || it is ItemTracker);
        }

        public static bool IsItemBatteryFull(Item it)
        {
            if (it == null) return true;
            ItemLight light = it as ItemLight;
            if (light != null && light.IsFullyCharged) return true;
            ItemTracker tracker = it as ItemTracker;
            if (tracker != null && tracker.IsFullyCharged) return true;
            return false;
        }

        public static bool CanActorGiveItemTo(Actor actor, Actor target, Item gift, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (target == null)
                throw new ArgumentNullException("target");
            if (gift == null)
                throw new ArgumentNullException("gift");

            /////////////////////////////////
            // Can't if any is true:
            // 1. Target is enemy.
            // 2. Item is equipped.
            // 3. Target is sleeping.
            // 4. Target can't receive item.
            /////////////////////////////////

            // 1. Target is enemy.
            if (actor.IsEnemiesWith(target))
            {
                reason = "enemy";
                return false;
            }

            // 2. Item is equipped.
            if (gift.IsEquipped)
            {
                reason = "equipped";
                return false;
            }

            // 3. Target is sleeping.
            if (target.IsSleeping)
            {
                reason = "sleeping";
                return false;
            }

            // 4. Target can't receive item.
            if (!target.CanActorGetItem(gift, out reason))
            {
                return false;
            }

            // all clear.
            return true;
        }

        public static bool CanActorChatWith(Actor speaker, Actor target, out string reason)
        {
            if (speaker == null)
                throw new ArgumentNullException("speaker");
            if (target == null)
                throw new ArgumentNullException("target");

            //////////////////////////////
            // Can't if any is true:
            // 1. One of them can't talk.
            // 2. One of them is sleeping.
            //////////////////////////////

            // 1. One of them can't talk.
            if (!speaker.Model.Abilities.CanTalk)
            {
                reason = "can't talk";
                return false;
            }
            if (!target.Model.Abilities.CanTalk)
            {
                reason = String.Format("{0} can't talk", target.TheName);
                return false;
            }

            // 2. One of them is sleeping.
            if (speaker.IsSleeping)
            {
                reason = "sleeping";
                return false;
            }
            if (target.IsSleeping)
            {
                reason = String.Format("{0} is sleeping", target.TheName);
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        // alpha10
        public static bool CanPullActor(Actor actor, Actor other, Point moveToPos, out string reason)
        {
            /////////////////////////////////////////////
            // Basically check if can push and can walk.
            // 1. Actor cannot shove.
            // 2. Actor cannot walk to pos.
            /////////////////////////////////////////////

            // 1. Actor cannot shove.
            if (!CanActorShove(actor, other, out reason))
                return false;

            // 3. Actor cannot walk to pos.
            if (!actor.IsWalkableFor(new Location(actor.Location.Map, moveToPos), out reason))
                return false;

            // all clear
            reason = "";
            return true;
        }

        public static bool CanActorShove(Actor actor, Actor other, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (other == null)
                throw new ArgumentNullException("other");

            ///////////////////////////////
            // Not "shovable"
            // 1. Actor cannot push/pull.
            // 2. Actor is tired.
            // 3. Actor is dragging corpse  // alpha10
            ///////////////////////////////

            // 1. Actor cannot push/pull.
            if (!actor.HasActorPushAbility())
            {
                reason = "cannot shove people";
                return false;
            }

            // 2. Actor is tired.
            if (actor.IsActorTired())
            {
                reason = "tired";
                return false;
            }

            // 3. Actor is dragging corpse  // alpha10
            if (actor.DraggedCorpse != null)
            {
                reason = "dragging a corpse";
                return false;
            }

            // FIXME: in theory, should test if tile is walkable for the pusher. in practice, assume if other can walk here, we can too...
            //        this will cause problems in zombie mode (eg: undead player pushing living down from a car = move on the car...)

            // all clear
            reason = "";
            return true;
        }

        public static bool CanShoveActorTo(Actor actor, Point toPos, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            ///////////////////////////
            // Not pushable there:
            // 1. Out of bounds.
            // 2. Not walkable.
            // 3. Unwalkable object.
            // 4. An actor there.
            // 5. Actor is dragging corpse  // alpha10
            ///////////////////////////

            Map map = actor.Location.Map;

            // 1. Out of bounds.
            if (!map.IsInBounds(toPos))
            {
                reason = "out of map";
                return false;
            }

            // 2. Not walkable.
            if (!map.GetTileAt(toPos.X, toPos.Y).Model.IsWalkable)
            {
                reason = "blocked";
                return false;
            }

            // 3. Unwalkable object.
            MapObject obj = map.GetMapObjectAt(toPos);
            if (obj != null && !obj.IsWalkable)
            {
                reason = "blocked by an object";
                return false;
            }

            // 4. An actor there.
            if (map.GetActorAt(toPos) != null)
            {
                reason = "blocked by someone";
                return false;
            }

            // 5. Actor is dragging corpse  // alpha10
            if (actor.DraggedCorpse != null)
            {
                reason = "dragging a corpse";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        /// <summary>
        /// List enemies in fov, sorted by distance (closest first).
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="fov"></param>
        /// <returns></returns>
        public static List<Actor> GetEnemiesInFov(Actor actor, HashSet<Point> fov)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (fov == null)
                throw new ArgumentNullException("fov");

            List<Actor> list = null;

            foreach (Point pt in fov)
            {
                Actor other = actor.Location.Map.GetActorAt(pt);
                if (other == null)
                    continue;
                if (other == actor)
                    continue;
                if (!actor.IsEnemiesWith(other))
                    continue;

                if (list == null)
                    list = new List<Actor>(3);
                list.Add(other);
            }

            if (list != null)
            {
                list.Sort((a, b) =>
                    {
                        float dA = DistanceHelpers.StdDistance(a.Location.Position, actor.Location.Position);
                        float dB = DistanceHelpers.StdDistance(b.Location.Position, actor.Location.Position);

                        return dA < dB ? -1 :
                            dA > dB ? 1 :
                            0;
                    });
            }

            return list;
        }

        public static int SanityToHoursUntilUnstable(Actor a)
        {
            int left = a.Sanity - a.ActorDisturbedLevel();
            if (left <= 0) return 0;
            return left / WorldTime.TURNS_PER_HOUR;
        }

        public static bool CanActorCancelLead(Actor actor, Actor target, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (target == null)
                throw new ArgumentNullException("target");

            //////////////////////////////////
            // Can't if any is true:
            // 1. Target not a actor follower.
            // 2. Target is sleeping.
            //////////////////////////////////

            // 1. Target not a actor follower.
            if (target.Leader != actor)
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

        public static int ActorDamageVsCorpses(Actor a)
        {
            // base = melee HALVED.
            int dmg = a.CurrentMeleeAttack.DamageValue / 2;

            // Necrology.
            dmg += SKILL_NECROLOGY_CORPSE_BONUS;// * a.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.NECROLOGY);

            return dmg;
        }

        public static bool CanActorButcherCorpse(Actor actor, Corpse corpse)
        {
            string reason;
            return CanActorButcherCorpse(actor, corpse, out reason);
        }

        public static bool CanActorButcherCorpse(Actor actor, Corpse corpse, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (corpse == null)
                throw new ArgumentNullException("corpse");


            ////////////////////////////////////////
            // Can't if any is true:
            // 1. Actor tired.
            // 2. Corpse not in same tile as actor.
            ////////////////////////////////////////
            // 1. Actor tired.
            if (actor.IsActorTired())
            {
                reason = "tired";
                return false;
            }
            // 2. Corpse not in same tile as actor.
            if (corpse.Position != actor.Location.Position || !actor.Location.Map.HasCorpse(corpse))
            {
                reason = "not in same location";
                return false;
            }

            // ok
            reason = "";
            return true;
        }

        public static Actor GetNextActorToAct(Map map, int turnCounter)
        {
            if (map == null)
                return null;

            int n = map.CountActors;
            for (int i = map.CheckNextActorIndex; i < n; i++)
            {
                Actor a = map.GetActor(i);
                if (a.ActionPoints > 0 && !a.IsSleeping)
                {
                    map.CheckNextActorIndex = i;
                    return a;
                }
            }

            return null;
        }

        public static bool CanActorActThisTurn(Actor actor)
        {
            if (actor == null)
                return false;

            return actor.ActionPoints > 0;
        }

        public static bool CanActorActNextTurn(Actor actor)
        {
            if (actor == null)
                return false;

            return actor.ActionPoints + actor.ActorSpeed() > 0;
        }

        public static bool WillOtherActTwiceBefore(Actor actor, Actor other)
        {
            if (actor.Location.Map.IsActorBeforeInMapList(actor, other))
            {
                if (other.ActionPoints > BASE_ACTION_COST)
                    return true;
                else
                    return false;
            }
            else
            {
                if (other.ActionPoints + other.ActorSpeed() > BASE_ACTION_COST)
                    return true;
                else
                    return false;
            }
        }

        public static bool IsMurder(Actor killer, Actor victim)
        {
            if (killer == null || victim == null)
                return false;

            // killing an undead is never a murder (doh!)
            if (victim.Model.Abilities.IsUndead)
                return false;

            // a law enforcer killing a murderer is not a murder.
            if (killer.Model.Abilities.IsLawEnforcer && victim.MurdersCounter > 0)
                return false;

            // killing between enemy factions is allowed.
            if (killer.Faction.IsEnemyOf(victim.Faction))
                return false;

            // killing in self defence is not a murder.
            if (killer.IsSelfDefenceFrom(victim))
                return false;

            // all other cases are murders!
            return true;
        }

        public static int ActorDamageBonusVsUndeads(Actor actor)
        {
            return SKILL_NECROLOGY_UNDEAD_BONUS;// * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.NECROLOGY);
        }

        // alpha10 added mapobject param
        public static Attack ActorMeleeAttack(Actor actor, Attack baseAttack, Actor target, MapObject objToBreak=null)
        {
            float hit = baseAttack.HitValue;
            float dmg = baseAttack.DamageValue;
            int disarmBonus = 0;  // alpha10

            // skills bonuses.
            int hitBonus = 1;//SKILL_AGILE_ATK_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.AGILE) +
                             //SKILL_ZAGILE_ATK_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_AGILE);
            int dmgBonus = 1;// SKILL_STRONG_DMG_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.STRONG) +
                            //SKILL_ZSTRONG_DMG_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_STRONG);
            // martial arts apply only if no weapon equipped.
            if (actor.GetEquippedWeapon() == null)
            {
                int ma = 1;// actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.MARTIAL_ARTS);
                if (ma > 0)
                {
                    hitBonus += SKILL_MARTIAL_ARTS_ATK_BONUS * ma;
                    dmgBonus += SKILL_MARTIAL_ARTS_DMG_BONUS * ma;
                    disarmBonus += SKILL_MARTIAL_ARTS_DISARM_BONUS * ma;
                }
            }

            // necrology vs undeads.
            if (target != null && target.Model.Abilities.IsUndead)
                dmgBonus += ActorDamageBonusVsUndeads(actor);

            // alpha10
            // add tool damage bonus vs map objects
            if (objToBreak != null)
            {
                ItemMeleeWeapon eqMw = actor.GetEquippedMeleeWeapon();
                if (eqMw != null)
                {
                    dmgBonus += eqMw.ToolBashDamageBonus;
                }
            }

            hit += hitBonus;
            dmg += dmgBonus;

            // alpha10
            // disarm chance.
            float disarmChance = MELEE_DISARM_BASE_CHANCE;
            disarmChance += disarmBonus;
            // defender strong resist disarm
            if (target != null)
                disarmChance -= SKILL_STRONG_RESIST_DISARM_BONUS;// * target.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.STRONG);

            // sleepiness penalties.
            if (actor.IsActorExhausted())
            {
                hit /= 2f;
                disarmChance /= 2f;
            }
            else if (actor.IsActorSleepy())
            {
                hit *= 3f / 4f;
                disarmChance *= 3f / 4f;
            }

            // done.
            return Attack.MeleeAttack(baseAttack.Verb, (int)hit, (int)dmg, baseAttack.StaminaPenalty, (int)disarmChance);
        }

        public static int ActorMedicineEffect(Actor actor, int baseEffect)
        {
            int effectBonus = (int)SKILL_MEDIC_BONUS;// (int)(Math.Ceiling(SKILL_MEDIC_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.MEDIC) * baseEffect)) ;
            return baseEffect + effectBonus;
        }

        public static int ActorHealChanceBonus(Actor actor)
        {
            int chanceBonus = SKILL_HARDY_HEAL_CHANCE_BONUS;// * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.HARDY);
            return chanceBonus;
        }

        public static int ActorBarricadingPoints(Actor actor, int baseBarricadingPoints)
        {
            int barBonus = 0;

            // carpentry skill
            barBonus += (int)(baseBarricadingPoints * SKILL_CARPENTRY_BARRICADING_BONUS);// * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CARPENTRY));;

            // alpha10
            // tool build bonus
            ItemMeleeWeapon eqMw = actor.GetEquippedMeleeWeapon();
            if (eqMw != null && eqMw.ToolBuildBonus != 0)
            {
                barBonus += (int)(baseBarricadingPoints * eqMw.ToolBuildBonus);
            }

            return baseBarricadingPoints + barBonus;
        }

        public static int ActorLoudNoiseWakeupChance(Actor actor, int noiseDistance)
        {
            int baseChance = LOUD_NOISE_BASE_WAKEUP_CHANCE;
            int skillBonus = SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS;// * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LIGHT_SLEEPER);
            int distBonus = Math.Max(0, (LOUD_NOISE_RADIUS - noiseDistance) * LOUD_NOISE_DISTANCE_BONUS);

            return baseChance + skillBonus + distBonus;
        }



        public static int ActorTrustIncrease(Actor actor)
        {
            int skillBonus = SKILL_CHARISMATIC_TRUST_BONUS;// * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CHARISMATIC);

            return TRUST_BASE_INCREASE + skillBonus;
        }

        public static int ActorCharismaticTradeChance(Actor actor)
        {
            return SKILL_CHARISMATIC_TRADE_BONUS;// * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CHARISMATIC);
        }



#if false
        // alpha10  previous attempt
        // FIXME -- needing to pass game as arg is uggly. shouldnt be any reference to game in rules but we need to
        // call an ai method...
        public int ScoreNpcItemTradeValue(RogueGame game, Actor npc, Item it, bool fromNpcInventory, Actor otherTrader)
        {
            // base score
            int score = (npc.Controller as BaseAI).ScoreItemValue(game, it, fromNpcInventory);

            // modify by respective charismatic skills
            // a charismatic actor will make the opposing trader over-estimate the actor items.
            int addScore = 0;
            if (fromNpcInventory)
                addScore = (score * npc.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CHARISMATIC) * SKILL_CHARISMATIC_TRADE_BONUS) / 100;
            else
                addScore = (score * otherTrader.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CHARISMATIC) * SKILL_CHARISMATIC_TRADE_BONUS) / 100;
    
            score += addScore;

            return score;
        }
#endif 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="time"></param>
        /// <returns>penalty as number > 0</returns>
        public static int NightFovPenalty(Actor actor, WorldTime time)
        {
            if (actor.Model.Abilities.IsUndead)
                return 0;
            else
            {
                switch (time.Phase)
                {
                    case DayPhase.SUNSET: return FOV_PENALTY_SUNSET;
                    case DayPhase.EVENING: return FOV_PENALTY_EVENING;
                    case DayPhase.MIDNIGHT: return FOV_PENALTY_MIDNIGHT;
                    case DayPhase.DEEP_NIGHT: return FOV_PENALTY_DEEP_NIGHT;
                    case DayPhase.SUNRISE: return FOV_PENALTY_SUNRISE;
                    default: return 0;
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="weather"></param>
        /// <returns>penalty as number > 0</returns>
        public static int WeatherFovPenalty(Actor actor, Weather weather)
        {
            if (actor.Model.Abilities.IsUndead)
                return 0;
            else
            {
                switch (weather)
                {
                    case Weather.RAIN: return FOV_PENALTY_RAIN;
                    case Weather.HEAVY_RAIN: return FOV_PENALTY_HEAVY_RAIN;
                    default: return 0;
                }
            }
        }

        public static bool IsWeatherRain(Weather weather)
        {
            switch (weather)
            {
                case Weather.CLEAR:
                case Weather.CLOUDY:
                    return false;
                case Weather.HEAVY_RAIN:
                case Weather.RAIN:
                    return true;
                default: throw new ArgumentOutOfRangeException("unhandled weather");
            }
        }

        public static int DarknessFov(Actor actor)
        {
            if (actor.Model.Abilities.IsUndead)
                return actor.Sheet.BaseViewRange;
            else
                return MINIMAL_FOV;
        }


        // alpha10
        public static bool CanActorSeeSky(Actor actor)
        {
            if (actor.IsDead)
                return false;
            if (actor.IsSleeping)
                return false;
            return actor.Location.Map.Lighting == Lighting.OUTSIDE;
        }

        public static bool CanActorKnowTime(Actor actor)
        {
            if (actor.IsDead)
                return false;
            if (actor.IsSleeping)
                return false;
            if (actor.Location.Map.Lighting == Lighting.OUTSIDE)
                return true;

            ItemTracker eqTracker = actor.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
            if (eqTracker != null && eqTracker.HasClock && eqTracker.Batteries > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Compute normalized % of power level on the map. Each activated PowerGenerator map object count for one power unit.
        /// </summary>
        /// <param name="map"></param>
        /// <returns>0.0 = not powered at all to 1.0 = 100% power.</returns>
        public static float ComputeMapPowerRatio(Map map)
        {
            int totalOn, totalOff;

            totalOff = totalOn = 0;

            foreach (MapObject obj in map.MapObjects)
            {
                PowerGenerator powGen = obj as PowerGenerator;
                if (powGen == null)
                    continue;

                if (powGen.IsOn)
                    ++totalOn;
                else
                    ++totalOff;
            }

            int totalCount = totalOn + totalOff;
            if (totalCount == 0)
                return 0.0f;

            float ratio = (float)totalOn / (float)(totalCount);
            return ratio;
        }

        public static int ActorBiteHpRegen(Actor a, int dmg)
        {
            int bonus = (int)(Rules.SKILL_ZEATER_REGEN_BONUS);// * a.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_EATER) * dmg);
            return dmg + bonus;
        }

        public static int ActorBiteNutritionValue(Actor actor, int baseValue)
        {
            float zskillFactor = SKILL_ZLIGHT_EATER_FOOD_BONUS;// * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_LIGHT_EATER);
            float skillFactor = SKILL_LIGHT_EATER_FOOD_BONUS;// * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LIGHT_EATER);

            return (int)(CORPSE_EATING_NUTRITION_FACTOR + zskillFactor + skillFactor) * baseValue;
        }

        public static int CorpseEeatingInfectionTransmission(int infection)
        {
            return (int)(CORPSE_EATING_INFECTION_FACTOR * infection);
        }

        public static int ActorInfectionHPs(Actor a)
        {
            return a.ActorMaxHPs() + a.ActorMaxSTA();
        }

        public static int InfectionForDamage(Actor infector, int dmg)
        {
            float factor = INFECTION_BASE_FACTOR;// + infector.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_INFECTOR) * SKILL_ZINFECTOR_BONUS;
            return (int)(factor * dmg);
        }

        public static int ActorInfectionPercent(Actor a)
        {
            return (int)(100 * a.Infection / ActorInfectionHPs(a));
        }

        public static int InfectionEffectTriggerChance1000(int infectionPercent)
        {
            return INFECTION_EFFECT_TRIGGER_CHANCE_1000 + infectionPercent / 5;
        }

        public static int CorpseFreshnessPercent(Corpse c)
        {
            return (int)(100 * c.HitPoints / c.DeadGuy.ActorMaxHPs());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns>[0..5]</returns>
        public static int CorpseRotLevel(Corpse c)
        {
            int freshP = CorpseFreshnessPercent(c);
            if (freshP < 5)
                return 5;
            if (freshP < 25)
                return 4;
            if (freshP < 50)
                return 3;
            if (freshP < 75)
                return 2;
            if (freshP < 90)
                return 1;
            return 0;
        }

        public static float CorpseDecayPerTurn(Corpse c)
        {
            return CORPSE_DECAY_PER_TURN;
        }

        public static int CorpseZombifyChance(Corpse c, WorldTime timeNow, bool checkDelay = true)
        {
            float chance;

            // delay zombification.
            int dT = timeNow.TurnCounter - c.Turn;
            if (checkDelay && dT < CORPSE_ZOMBIFY_DELAY)
                return 0;

            // compute infection P
            int infP = ActorInfectionPercent(c.DeadGuy);

            // check only every X turns, higher frequency as infP rise.
            if (checkDelay)
            {
                int freq = (infP >= 100 ? 1 : 100 / (1 + infP));
                if (timeNow.TurnCounter % freq != 0)
                    return 0;
            }

            // base chance.
            chance = CORPSE_ZOMBIFY_BASE_CHANCE;

            // living infection
            chance += CORPSE_ZOMBIFY_INFECTIONP_FACTOR * infP;

            // less likey as time passes.
            chance -= (int)(CORPSE_ZOMBIFY_TIME_FACTOR * dT);

            // factor day & night.
            if (timeNow.IsNight)
                chance *= CORPSE_ZOMBIFY_NIGHT_FACTOR;
            else
                chance *= CORPSE_ZOMBIFY_DAY_FACTOR;

            // ok.
            int intChance = Math.Max(0, Math.Min(100, (int)chance));
            return intChance;
        }

        public static int CorpseReviveChance(Actor actor, Corpse corpse)
        {
            if (!actor.CanActorReviveCorpse(corpse, out string reason))
                return 0;
            int baseChance = CorpseFreshnessPercent(corpse) / 4;
            int skillBonus = 1;// actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.MEDIC) * SKILL_MEDIC_REVIVE_BONUS;
            return baseChance + skillBonus;
        }

        public static int CorpseReviveHPs(Actor actor, Corpse corpse)
        {
            int baseHps = 5;
            //int skillBonus = actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.MEDIC);
            return baseHps;// + skillBonus;
        }

        public static bool IsTrapTriggeringMapObjectThere(Map map, Point pos)
        {
            MapObject mobj = map.GetMapObjectAt(pos);
            if (mobj == null) return false;
            // mobj is NOT walkable and a door and NOT jumpable (eg:shelves,large fort)
            return !mobj.IsWalkable && !mobj.IsJumpable && !(mobj is DoorWindow);
        }

        public static bool HasImmediateZombification(GameMode mode)
        {
            return mode == GameMode.GM_STANDARD;
        }

        public static bool HasInfection(GameMode mode)
        {
            return mode != GameMode.GM_STANDARD;
        }

        public static bool HasCorpses(GameMode mode)
        {
            return mode != GameMode.GM_STANDARD;
        }

        public static bool HasEvolution(GameMode mode)
        {
            return mode != GameMode.GM_VINTAGE;
        }

        public static bool HasAllZombies(GameMode mode)
        {
            return mode != GameMode.GM_VINTAGE;
        }

        public static bool HasZombiesInBasements(GameMode mode)
        {
            return mode != GameMode.GM_VINTAGE;
        }

        public static bool HasZombiesInSewers(GameMode mode)
        {
            return mode != GameMode.GM_VINTAGE;
        }
    }
}

using djack.RogueSurvivor.Common;
using System;
using System.Drawing;

namespace djack.RogueSurvivor.Data.Items
{
    [Serializable]
    public class ItemTrap : Item
    {
        public const int TRAP_UNDEAD_ACTOR_TRIGGER_PENALTY = 30;
        public const int TRAP_SMALL_ACTOR_AVOID_BONUS = 90;
        public const int CRUSHING_GATES_DAMAGE = 60;  // alpha10.1

        bool m_IsActivated;
        bool m_IsTriggered;
        // alpha10
        Actor m_Owner;

        public bool IsActivated
        {
            get { return m_IsActivated;}
            //alpha10 set { m_IsActivated=value;}
        }

        public bool IsTriggered
        {
            get { return m_IsTriggered; }
            set { m_IsTriggered = value; }
        }

        public ItemTrapModel TrapModel { get { return Model as ItemTrapModel; } }

        // alpha10
        public Actor Owner
        {
            get
            {
                // cleanup dead owner reference
                if (m_Owner != null && m_Owner.IsDead)
                    m_Owner = null;

                return m_Owner;
            }
        }

        public ItemTrap(ItemModel model)
            : base(model)
        {
            if (!(model is ItemTrapModel))
                throw new ArgumentException("model is not a TrapModel");
        }

        /// <summary>
        /// A new trap of the same model, un-activated, no owner, un-triggered.
        /// </summary>
        /// <returns></returns>
        public ItemTrap Clone()
        {
            ItemTrap c = new ItemTrap(TrapModel);
            return c;
        }

        // alpha10
        public void Activate(Actor owner)
        {
            m_Owner = owner;
            m_IsActivated = true;
        }

        public void Desactivate()
        {
            m_Owner = null;
            m_IsActivated = false;
        }

        // alpha10
        public override void OptimizeBeforeSaving()
        {
            base.OptimizeBeforeSaving();

            // cleanup dead owner ref
            if (m_Owner != null && m_Owner.IsDead)
                m_Owner = null;
        }

        /// <summary>
        /// @return true if item must be removed (destroyed).
        /// </summary>
        /// <param name="trap"></param>
        /// <returns></returns>
        public bool TryTriggerTrap(Actor victim)
        {
            // check trigger chance.
            if (CheckTrapTriggers(victim))
                DoTriggerTrap(victim.Location.Map, victim.Location.Position, victim, null);
            else
            {
                // TODO
                //if (victim.IsVisibleToPlayer())
                //    AddMessage(MakeMessage(victim, String.Format("safely {0} {1}.", Conjugate(victim, VERB_AVOID), trap.TheName)));
            }
            // destroy?
            return Quantity == 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trap"></param>
        /// <param name="a"></param>
        /// <returns>true if trap triggers</returns>
        public bool CheckTrapTriggers(Actor a)
        {
            // alpha10 extracted and modified trigger chance formula
            int chance = GetTrapTriggerChance(a);
            return chance > 0 ? DiceRoller.RollChance(chance) : false;
        }

        public bool CheckTrapTriggers(MapObject mobj)
        {
            return DiceRoller.RollChance(TrapModel.TriggerChance * mobj.Weight);
        }

        public int GetTrapTriggerChance(Actor a)
        {
            // alpha10.1 bugfix - correctly has 0 chance to trigger safe traps (eg: followers traps etc...)
            if (a.IsSafeFromTrap(this))
                return 0;

            int baseChance;
            int avoidBonus;

            baseChance = TrapModel.TriggerChance * Quantity;

            avoidBonus = 0;
            if (a.Model.Abilities.IsUndead)
                avoidBonus -= TRAP_UNDEAD_ACTOR_TRIGGER_PENALTY;
            if (a.Model.Abilities.IsSmall)
                avoidBonus += TRAP_SMALL_ACTOR_AVOID_BONUS;
            // TODO
            //avoidBonus += a.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LIGHT_FEET) * SKILL_LIGHT_FEET_TRAP_BONUS;
            //avoidBonus += a.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_LIGHT_FEET) * SKILL_ZLIGHT_FEET_TRAP_BONUS;

            return baseChance - avoidBonus;
        }

        /// <summary>
        /// Trigger a trap by an actor or a map object.
        /// </summary>
        /// <param name="trap"></param>
        /// <param name="map"></param>
        /// <param name="pos"></param>
        /// <param name="victim"></param>
        /// <param name="mobj"></param>
        public void DoTriggerTrap(Map map, Point pos, Actor victim, MapObject mobj)
        {
            ItemTrapModel model = TrapModel;
            bool visible = map.IsVisibleToPlayer(pos);

            // flag.
            IsTriggered = true;

            // effect: damage on victim? (actor)
            int damage = model.Damage * Quantity;
            if (damage > 0 && victim != null)
            {
                victim.InflictDamage(damage);
                if (visible)
                {
                    //AddMessage(MakeMessage(victim, String.Format("is hurt by {0} for {1} damage!", trap.AName, damage)));
                    //AddOverlay(new OverlayImage(MapToScreen(victim.Location.Position), GameImages.ICON_MELEE_DAMAGE));
                    //AddOverlay(new OverlayText(MapToScreen(victim.Location.Position).Add(DAMAGE_DX, DAMAGE_DY), Color.White, damage.ToString(), Color.Black));
                    //RedrawPlayScreen();
                    //AnimDelay(victim.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
                    //ClearOverlays();
                    //RedrawPlayScreen();
                }
            }
            // effect: noise? (actor, mobj)
            if (model.IsNoisy)
            {
                if (visible)
                {
                    //if (victim != null)
                    //    AddMessage(MakeMessage(victim, String.Format("stepping on {0} makes a bunch of noise!", trap.AName)));
                    //else if (mobj != null)
                    //    AddMessage(new Message(String.Format("{0} makes a lot of noise!", Capitalize(trap.TheName)), map.LocalTime.TurnCounter));
                }
                map.OnLoudNoise(pos, model.NoiseName);
            }

            // if one time trigger = desactivate.
            if (model.IsOneTimeUse)
                Desactivate();  //alpha10 //trap.IsActivated = false;

            // then check break chance (actor, mobj)
            if (CheckTrapStepOnBreaks(mobj))
            {
                if (visible)
                {
                    //if (victim != null)
                    //    AddMessage(MakeMessage(victim, String.Format("{0} {1}.", Conjugate(victim, VERB_CRUSH), trap.TheName)));
                    //else if (mobj != null)
                    //    AddMessage(new Message(String.Format("{0} breaks the {1}.", Capitalize(mobj.TheName), trap.TheName), map.LocalTime.TurnCounter));
                }
                --Quantity;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trap"></param>
        /// <param name="victim"></param>
        /// <param name="isDestroyed"></param>
        /// <returns>true succesful escape</returns>
       public bool TryEscapeTrap(Actor victim, out bool isDestroyed)
        {
            isDestroyed = false;

            // no brainer.
            if (TrapModel.BlockChance <= 0)
                return true;

            bool visible = IsVisibleToPlayer(victim);
            bool canEscape = false;

            // check escape chance.
            if (CheckTrapEscape(victim))
            {
                // un-triggered and escape.
                IsTriggered = false;
                canEscape = true;

                // tell
                //if (visible)
                //    AddMessage(MakeMessage(victim, String.Format("{0} {1}.", Conjugate(victim, VERB_ESCAPE), trap.TheName)));

                // then check break on escape chance.
                if (CheckTrapEscape(victim))
                {
                    //if (visible)
                    //    AddMessage(MakeMessage(victim, String.Format("{0} {1}.", Conjugate(victim, VERB_BREAK), trap.TheName)));
                    --Quantity;
                    isDestroyed = Quantity <= 0;
                }
            }
            else
            {
                // tell
                //if (visible)
                //    AddMessage(MakeMessage(victim, String.Format("is trapped by {0}!", trap.TheName)));
            }

            // escape?
            return canEscape;
        }

        public bool CheckTrapEscapeBreaks()
        {
            return DiceRoller.RollChance(TrapModel.BreakChanceWhenEscape);
        }

        public bool CheckTrapEscape(Actor a)
        {
            // alpha10
            if (a.IsSafeFromTrap(this))
                return true;

            int escapeBonus = 0;

            //escapeBonus += a.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LIGHT_FEET) * SKILL_LIGHT_FEET_TRAP_BONUS
            //    + a.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_LIGHT_FEET) * SKILL_ZLIGHT_FEET_TRAP_BONUS;

            return DiceRoller.RollChance(escapeBonus + (100 - TrapModel.BlockChance * Quantity));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trap"></param>
        /// <param name="mobj">can be null</param>
        /// <returns></returns>
        public bool CheckTrapStepOnBreaks(MapObject mobj = null)
        {
            int chance = TrapModel.BreakChance;
            if (mobj != null) chance *= mobj.Weight;
            return DiceRoller.RollChance(chance);
        }
    }
}

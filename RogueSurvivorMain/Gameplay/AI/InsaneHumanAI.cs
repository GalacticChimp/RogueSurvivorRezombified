using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Data.Helpers;
using djack.RogueSurvivor.Common;

namespace djack.RogueSurvivor.Gameplay.AI
{
    [Serializable]
    /// <summary>
    /// InsaneHumanAI, used by Unique Enraged Patient & Asylum patients.
    /// Extremely simple : attack anyone in sight or shout insanities, wander.
    /// </summary>
    class InsaneHumanAI : BaseAI
    {
        #region Constants
        // const int ATTACK_CHANCE = 80;  // alpha10.1 obsolete
        const int SHOUT_CHANCE = 80;
        const int USE_EXIT_CHANCE = 50;

        string[] INSANITIES = new string[]
        {
            "WHY WALK THERE?",
            "WHAT MAKES YOU FUN?",
            "YOU WEAR TOO MUCH COLORS!",
            "YOU HAVE BAD HABITS!",
            "MOM DIDN'T HANG ME!",
            "LUCIE! LUCIA!",
            "WHAT EGGS AND PASTA?",
            "TURTLE CATS!",
            "I REMEMBER THE CRABS!",
            "IT WAS AFTER THAT NOW!",
            "CUT IT! CUT IT NOW!",
            "DECEASED TOES!",
            "ICE-CREAM COPS!",
            "I SAW THAT FUCKING TWICE! TWICE! TWICE!",
            "FUCK BASTARD SAUSAGE!",
            "HEY YOU! STOP MOVING THE FLOOR!",
            "DROP THE FUCKING EGGS NOW!",
            "YOU GO FIRST AFTER ME!",
            "IT HURTS BUT ITS OK!",
            "LAST TIME WAS OK...",
            "SHE ISN'T NOT YET!",
            "I WAS CRAWLING HAHA!",
            "ROLLING LIKE AN EGG!",
            "THAT IS NOT DECENT!",
            "JUMP LIKE A FLOWER!",
            "NIGGER TRIGGER!",
            "CHEESE LIKE THESE...",
            "ILL-ADVISED LOBSTER!",
            "SSSHHHH! SILENCE... DO YOU SMELL?",
            "NOTHING BEATS. NOTHING!",
            "GROWN-UP MEN DON'T DO THAT!",
            "BARN BUSTER!",
            "SUPER SUPER?",
            "ONE MORE PASTA CRAP!",
            "LAZY LADY!",
            "I HATE TAP WATER!",
            "STILL WANKING FOR FOOD?",
            "LOOK! IT FITS LIKE A HOLE!",
            "PESKY POLAR PRANKS!",
            "LITTLE BY LITTLE YOU DIE...",
            "PLEASE TIE YOUR NECK PROPERLY!",
            "I SEE WHAT I SHIT ALL THE TIME!",
            "THAT'S FUCKING ANNOYING!",
            "I UNLOCK THE WALLS!",
            "I'M NOT SO SURE NOW!?",
            "RUSTY BUT TRUSTY!",
            "CHEESE LICKER!",
            "LAUNDRY TIME AGAIN AND AGAIN!",
            "DON'T YOU SEE I'M ASSEMBLED?",
            "MEXICAN MIDGETS!",
            "RAZOR RASCALS!",
            "PUNCH MY BALLS!",
            "STUCK IN A VICIOUS SQUARE!",
            "HORSE HOLSTER!",
            "THAT WAS COMPLETLY UNCALLED FOR!",
            "ROBOTS WON'T FOOL ME!"
        };
        #endregion

        #region Fields
        LOSSensor m_LOSSensor;
        #endregion

        #region BaseAI
        protected override void CreateSensors()
        {
            m_LOSSensor = new LOSSensor(LOSSensor.SensingFilter.ACTORS);
        }

        protected override List<Percept> UpdateSensors(World world)
        {
            List<Percept> list = m_LOSSensor.Sense(world, m_Actor);
            return list;
        }

        protected override ActorAction SelectAction(World world, List<Percept> percepts)
        {
            HashSet<Point> fov = m_LOSSensor.FOV;
            List<Percept> mapPercepts = FilterSameMap(percepts);

            ///////////////////////////////////////////////////////////////////////
            // alpha10 OBSOLETE 1 equip weapon
            // alpha10 1 equip best items
            // 2 (chance) move closer to an enemy, nearest & visible enemies first
            // 3 (chance) shout insanities.
            // 4 (chance) use exit.
            // 5 wander
            ///////////////////////////////////////////////////////////////////////

            // alpha10
            m_Actor.IsRunning = false;

            // 1 equip best items
            ActorAction bestEquip = BehaviorEquipBestItems(false, false);
            if (bestEquip != null)
            {
                return bestEquip;
            }

            //// 1 equip weapon
            //ActorAction equipWpnAction = BehaviorEquipWeapon(game);
            //if (equipWpnAction != null)
            //{
            //    m_Actor.Activity = Activity.IDLE;
            //    return equipWpnAction;
            //}

            // 2 (chance) move closer to an enemy, nearest & visible enemies first
            #region
            // alpha10.1 always try to attack if (game.Rules.RollChance(ATTACK_CHANCE))
            {
                List<Percept> enemies = FilterEnemies(mapPercepts);
                if (enemies != null)
                {
                    // try visible enemies first, the closer the best.
                    List<Percept> visibleEnemies = Filter(enemies, (p) => p.Turn == m_Actor.Location.Map.LocalTime.TurnCounter);
                    if (visibleEnemies != null)
                    {
                        Percept bestEnemyPercept = null;
                        ActorAction bestBumpAction = null;
                        float closest = int.MaxValue;

                        foreach (Percept enemyP in visibleEnemies)
                        {
                            float distance = DistanceHelpers.GridDistance(m_Actor.Location.Position, enemyP.Location.Position);
                            if (distance < closest)
                            {
                                ActorAction bumpAction = BehaviorStupidBumpToward(enemyP.Location.Position, true, true);
                                if (bumpAction != null)
                                {
                                    closest = distance;
                                    bestEnemyPercept = enemyP;
                                    bestBumpAction = bumpAction;
                                }
                            }
                        }

                        if (bestBumpAction != null)
                        {
                            m_Actor.Activity = Activity.CHASING;
                            m_Actor.TargetActor = bestEnemyPercept.Percepted as Actor;
                            return bestBumpAction;
                        }
                    }

                    // then try rest, the closer the best.
                    List<Percept> oldEnemies = Filter(enemies, (p) => p.Turn != m_Actor.Location.Map.LocalTime.TurnCounter);
                    if (oldEnemies != null)
                    {
                        Percept bestEnemyPercept = null;
                        ActorAction bestBumpAction = null;
                        float closest = int.MaxValue;

                        foreach (Percept enemyP in oldEnemies)
                        {
                            float distance = DistanceHelpers.GridDistance(m_Actor.Location.Position, enemyP.Location.Position);
                            if (distance < closest)
                            {
                                ActorAction bumpAction = BehaviorStupidBumpToward(enemyP.Location.Position, true, true);
                                if (bumpAction != null)
                                {
                                    closest = distance;
                                    bestEnemyPercept = enemyP;
                                    bestBumpAction = bumpAction;
                                }
                            }
                        }

                        if (bestBumpAction != null)
                        {
                            m_Actor.Activity = Activity.CHASING;
                            m_Actor.TargetActor = bestEnemyPercept.Percepted as Actor;
                            return bestBumpAction;
                        }
                    }
                }
            }
            #endregion

            // 3 (chance) shout insanities.
            #region
            if (DiceRoller.RollChance(SHOUT_CHANCE))
            {
                string insanity = INSANITIES[DiceRoller.Roll(0, INSANITIES.Length)];
                m_Actor.Activity = Activity.IDLE;
                m_Actor.DoEmote(insanity, true);
            }
            #endregion

            // 4 (chance) use exit.
            if (DiceRoller.RollChance(USE_EXIT_CHANCE))
            {
                ActorAction useExit = BehaviorUseExit(UseExitFlags.ATTACK_BLOCKING_ENEMIES | UseExitFlags.BREAK_BLOCKING_OBJECTS);
                if (useExit != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return useExit;
                }
            }

            // 5 wander
            m_Actor.Activity = Activity.IDLE;
            return BehaviorWander(null);
        }
        #endregion
    }
}

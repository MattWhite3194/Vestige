using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TheGreen.Game.Entities.NPCs.Behaviors;

namespace TheGreen.Game.Entities.NPCs
{
    public static class NPCDatabase
    {

        private static Dictionary<int, object[]> _npcs = new Dictionary<int, object[]>
        {
            {0, [0, "Mutant Cricket", ContentLoader.EnemyTextures[0], new Vector2(69, 34), 100, 10, true, false, typeof(MutantCricketBehavior), new List<(int, int)> { (0, 3), (4, 4)}]}
        };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="npcID"></param>
        /// <returns>A new npc instance with the specified id</returns>
        public static NPC InstantiateNPCByID(int npcID)
        {
            //little bit of code smell (¬_¬)
            NPC npc = (NPC)Activator.CreateInstance(typeof(NPC), _npcs[npcID]);
            return npc;
        }
    }
}

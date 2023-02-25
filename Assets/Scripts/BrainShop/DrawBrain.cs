using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawBrain
{
   public struct BrainStatistics
   {
      public int level;
      public int intelligence;
      public int dexterity;
      public int strength;
      // public float sanity;
   }


   public static BrainStatistics DrawRandomBrain()
   {
      float levelRandomValue = Random.Range(0f, 1f);
      BrainStatistics brainStatistics = new BrainStatistics();

      brainStatistics.level = levelRandomValue < 0.1f ? 3 : (levelRandomValue < 0.5f ? 2 : 1);

      int statPoint = brainStatistics.level == 3 ? 7 : (brainStatistics.level == 2 ? 5 : 3);

      for (int i = 0; i < statPoint; i++)
      {
         int statisticRandomValue = Random.Range(0, 3);

         if (statisticRandomValue == 0)
         {
            brainStatistics.dexterity++;
         }
         else if (statisticRandomValue == 1)
         {
            brainStatistics.intelligence++;
         }
         else
         {
            brainStatistics.strength++;
         }
      }

      return brainStatistics;
   }
}

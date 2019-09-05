using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mediation.Interfaces;

namespace PlanRecognition
{
    /// <summary>
    /// The dimensions represented in the Event-Indexing Model.
    /// </summary>
    public enum EventIndexingDimension
    {
        /// <summary>
        /// The space index denotes the spatial region within which an event occurs.
        /// </summary>
        SPACE,

        /// <summary>
        /// The time index denotes the time frame within which an event occurs.
        /// </summary>
        TIME,

        /// <summary>
        /// The causality index denotes the causal relationship of one event with respect to another.
        /// </summary>
        CAUSALITY,

        /// <summary>
        /// The intentionality index denotes the intention of the character of an event (i.e. the goal 
        /// the character is trying to pursue).
        /// </summary>
        INTENTIONALITY,

        /// <summary>
        /// The entity denotes the characters / objects involved in the event.
        /// </summary>
        ENTITY
    }


    public static class Indexter
    {
        public static List<IOperator> filterNonSalient(Queue<IOperator> actions)
        {
            return null;
        }

        private static int indexOverlap(IOperator x, IOperator y)
        {
            int count = 0;

            // Space





            return count;
        }

        /// <summary>
        /// Checks whether the operators x and y happened in the same room.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static bool spaceOverlap(IOperator x, IOperator y)
        {
            return false;
        }

    }
}

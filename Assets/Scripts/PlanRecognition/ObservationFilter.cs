using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mediation.Interfaces;

namespace PlanRecognition
{
    /// <summary>
    /// The kind of observation filter desired.
    /// </summary>
    public enum FilterMode
    {
        /// <summary>
        /// This does not filter anything.
        /// </summary>
        INACTIVE,

        /// <summary>
        /// Filters on the basis of a sliding window of player actions. The window is
        /// currently hard-coded to be of size 5.
        /// </summary>
        WINDOWED, 

        /// <summary>
        /// Filters on the basis of a cognitive model of event working memory. This model
        /// is the Indexter model.
        /// </summary>
        COGNITIVE 
    }

    /// <summary>
    /// This class provides functionality to filter observations from a player log.  
    /// </summary>
    public static class ObservationFilter
    {
        private static int window_size = 5; // Arbitrary window size!

        /// <summary>
        /// Returns a list of player actions. This filter operates on the basis of the given filter
        /// mode, which is one of the following:
        /// INACTIVE, which does not filter anything.
        /// WINDOWED, which returns the last $window_size$ actions of the player log.
        /// COGNITIVE, which returns the actions deemed to be active in the mind of the player 
        /// according to some cognitive model (Indexter for the moment.)
        /// </summary>
        /// <param name="playerLog">A queue of player actions.</param>
        /// <param name="mode">The desired mode of operation of this filter.</param>
        /// <returns>A list of player actions.</returns>
        public static List<IOperator> filter(Queue<IOperator> playerLog, FilterMode mode)
        {
            switch(mode)
            {
                case FilterMode.INACTIVE:
                    return playerLog.ToList<IOperator>();
                    
                case FilterMode.WINDOWED:
                    int window_range = Math.Max(0, playerLog.Count() - window_size);
                    return playerLog.Skip(window_range).ToList<IOperator>();

                case FilterMode.COGNITIVE:
                    // This mode is not supported yet! Collection returned unchanged.
                    List<IOperator> cognitive = new List<IOperator>();
                    return cognitive;

                default:
                    throw new ArgumentException(mode.ToString());
            }
        }

    }
}

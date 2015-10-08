using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mastery.Utilities
{
    public static class CleverDialog
    {
        private static Random random = new Random();
        private static string[] data = new string[] 
            {
                "Nice work!",
                "Keep going!",
                "Another one down!",
                "Lets do one more!",
                "Your on a roll!",
                "Keep it up!",
                "Well done!",
                "Your doing great!",
                "No one knows your a dog!",
                "Looking good!",
                "You can't stop now!",
                "Sweet!",
                "Your the true Hero of Time!",
                "Time travelin'!",
                "Combo!",
                "A++++ will buy again.",
                "Your being productive!",
                "Just one more hour, come'on!",
                "Get back to WORK!",
                "Your just getting started!",
                "Don't forget to take a break.",
                "Finish what you start!",
                "Your one hour better!",
                "Keep GOING!",
                "Don't forget to support me!",
                "Just 1 hour till the next!",
                "Level up!",
                "Hadouken!",
                "Oh snap!",
                "I heard you like hours...",
                "Whatcha' doin'?",
                "Hello World!",
                "That's 0001 in binary.",
                "Just a reminder!",
                "Oops, did I disturb you?",
                "You know, no biggy.",
                "Another 60 minutes. Nice!",
                "You make this look good!",
                "Your pretty good.",
                "I can keep going, you?",
                "Wait, what are we doin'?",
                "Remember to sleep.",
                "Voyager traveled 38,610 miles.",
                ""
            };
        private static int cursor = 0;

        /// <summary>
        /// Get the next value from the ShuffleBag
        /// </summary>
        public static string Next()
        {
            if (cursor < 1)
            {
                cursor = data.Length - 1;
                if (data.Length < 1)
                    return default(string);
                return data[0];
            }
            int grab = (int)Math.Floor(random.NextDouble() * (cursor + 1));
            string temp = data[grab];
            data[grab] = data[cursor];
            data[cursor] = temp;
            cursor--;
            return temp;
        }
    }
}

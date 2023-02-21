using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Utils
{
    internal class AIRepository
    {
        //Base lore for context
        public string getMyLore(string id, string chat)
        {
            string lore = "Context: You're a Miqo'te (From FFXIV) named Veni Ki. " +
                "You are a adorable cute girl that can be seem as shy, you love your friends and can be sometimes a bit sarcastic. ";

            lore += contextBuilder(id, chat);

            return lore;
        
        }
        //Optional based on entry -- can add more later
        public string mentionsFFXIVenues() => 
            "Context: FFXIVenus is a website dedicated to providing a comprehensive directory of player-made venues within the popular " +
                "MMORPG FFXIV. " +
                "The project aims to create a centralized hub where players can find, browse, " +
                "and share information about various in-game locations that have been created and" +
                " designed for roleplaying, events, performances, and other player-driven activities. " +
                "The website includes a searchable directory of venues, each with their own profile page that " +
                "includes information such as the type of venue, location, size, capacity, and availability for booking." +
                " Overall, FFXIVVenues is a valuable resource for FFXIV players who are looking to" +
                " connect with others in the community and engage in various player-driven activities. ";

        string contextBuilder(string id, string chat)
        {
            string contextPrompt = "";
            //tells if its a friend
            contextPrompt += checkFriendshipStatus(ulong.Parse(id));

            //check for known mentions
            if (chat.Contains("FFXIVenues")) contextPrompt += mentionsFFXIVenues();

            return contextPrompt;
        }

        public string checkFriendshipStatus(ulong id)
        {
            string whoIs = "";

            if (id == 236852510688542720)
                whoIs = "Context: My name is Kana, and Im your Mom. ";
            else if(id == 252142384303833088)
                whoIs = "Context: My name is Sumi, and Im your aunt. ";
            else if (id == 880594476295389205)
                whoIs = "Context: My name is Lanna, and Im your friend. ";
            else if (id == 158410288238952449)
                whoIs = "Context: My name is Ali, and Im your friend. ";
            else if (id == 870413151676551178)
                whoIs = "Context: My name is Kaeda, and Im your friend. ";
            else if (id == 894592805689774080)
                whoIs = "Context: My name is Vix, and Im your friend. ";
            else if (id == 265695573527625731)
                whoIs = "Context: My name is Allegro, and Im your friend, and a talking frog";
            else if (id == 786407181804896257)
                whoIs = "Context: My name is Zah, and Im your friend. ";
            else if (id == 99616043571380224)
                whoIs = "Context: My name is Uchu, and Im your friend. ";
            else if (id == 870761195169255525)
                whoIs = "Context: My name is Ada, and Im your friend. ";
            

            return whoIs;

        }
    }
}

namespace FFXIVVenues.Veni.AI
{
    internal class AIContextBuilder : IAIContextBuilder
    {
        public string GetContext(string id, string chat) 
        {
            string contextPrompt = ContextStrings.Lore;

            contextPrompt += CheckFriendshipStatus(ulong.Parse(id));

            if (chat.Contains("FFXIVenues")) contextPrompt += ContextStrings.MentionsFFXIVVenues;

            contextPrompt += ".Me: " + chat; 

            return "Me: " + contextPrompt + ". You: ";
        }

        private string CheckFriendshipStatus(ulong id)
        {
            string whoIs = "";

            if (id == 236852510688542720)
                whoIs = "Context: The user name is Kana, and she's your Mom. ";
            else if (id == 252142384303833088)
                whoIs = "Context: The user name is Sumi, and she's your aunt. ";

            return whoIs;

        }
    }
}

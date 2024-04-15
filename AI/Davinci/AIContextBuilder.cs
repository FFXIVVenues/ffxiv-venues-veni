namespace FFXIVVenues.Veni.AI.Davinci;

internal class AiContextBuilder : IAIContextBuilder
{
    public string GetContext(string id, string chat) 
    {
        var contextPrompt = ContextStrings.PersonalityContext;
        contextPrompt += ContextStrings.FFXIVVenues;
        contextPrompt += CheckFriendshipStatus(ulong.Parse(id));

        contextPrompt += "\nMe: " + chat; 

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
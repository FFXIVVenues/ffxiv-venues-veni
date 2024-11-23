namespace FFXIVVenues.Veni.AI.Davinci
{
    internal interface IAiContextBuilder
    {
        string GetPrompt(string id, string chat);
    }
}
namespace FFXIVVenues.Veni.AI
{
    internal interface IAIContextBuilder
    {
        string GetContext(string id, string chat);
    }
}
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.AI
{
    internal interface IDavinciService
    {
        Task<string> AskTheAI(string prompt);

    }
}
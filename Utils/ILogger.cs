using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Utils
{
    internal interface ILogger
    {
        Task LogAsync(string type, string msg);
    }
}
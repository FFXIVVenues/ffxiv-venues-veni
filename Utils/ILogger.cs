using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Utils
{
    public interface ILogger
    {
        Task LogAsync(string type, string msg);
    }
}
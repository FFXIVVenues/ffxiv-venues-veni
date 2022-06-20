using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Luis
{
    internal interface ILuisClient
    {
        Task<Prediction> PredictAsync(string query);
    }
}
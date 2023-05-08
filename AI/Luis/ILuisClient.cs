using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;

namespace FFXIVVenues.Veni.AI.Luis
{
    internal interface ILuisClient
    {
        Task<Prediction> PredictAsync(string query);
    }
}
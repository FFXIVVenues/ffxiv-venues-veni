using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;

namespace FFXIVVenues.Veni.Services.Luis
{
    internal interface ILuisClient
    {
        Task<Prediction> PredictAsync(string query);
    }
}
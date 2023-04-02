using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Intents;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;

namespace FFXIVVenues.Veni.Services.Luis
{
    internal class LuisClient : IDisposable, ILuisClient
    {

        private const double PREDICTION_THRESHOLD = 0.2;

        private readonly LuisConfiguration _config;
        private LUISRuntimeClient _runtimeClient;
        private bool _disposedValue;

        public LuisClient(LuisConfiguration config)
        {
            _config = config;
            var credentials = new ApiKeyServiceClientCredentials(_config.RuntimeKey);
            _runtimeClient = new LUISRuntimeClient(credentials) { Endpoint = _config.PredictionEndpoint };
        }

        public async Task<Prediction> PredictAsync(string query)
        {
            if (query.Length > 500)
                return new Prediction { TopIntent = IntentNames.None };
            var request = new PredictionRequest { Query = query };
            var prediction = await _runtimeClient.Prediction
                .GetSlotPredictionAsync(new Guid(_config.AppId), _config.Slot, request);
            if (prediction.Prediction.Intents[prediction.Prediction.TopIntent].Score > PREDICTION_THRESHOLD)
                return prediction.Prediction;
            return new Prediction { TopIntent = IntentNames.None };
        }

        public void Dispose()
        {
            if (!_disposedValue)
            {
                _runtimeClient.Dispose();
                _disposedValue = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}

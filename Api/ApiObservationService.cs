using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FFXIVVenues.VenueModels.Observability;
using Microsoft.Extensions.DependencyInjection;
using NChronicle.Core.Interfaces;

namespace FFXIVVenues.Veni.Api
{
    internal class ApiObservationService : IApiObservationService, IDisposable
    {
        private readonly ObserveRequest _observeRequest;
        private readonly IChronicle _chronicle;
        private readonly IServiceProvider _serviceProvider;
        private readonly Uri _wsUri;
        private readonly List<(ObserveRequest Request, Type ObserverType)> _observers;
        private ClientWebSocket _wsClient;
        private Task _task;

        public ApiObservationService(IChronicle chronicle, ApiConfiguration config, IServiceProvider serviceProvider)
        {
            this._observeRequest = new (new ObservableOperation[] {  }, null, null);
            this._observers = new ();
            this._wsUri = new Uri(config.BaseUrl.Replace("http", "ws") + "/venue/observe");
            this._chronicle = chronicle;
            this._serviceProvider = serviceProvider;
        }

        public Task ObserveAsync() =>
            this._task ??= new TaskFactory().StartNew(Handle, TaskCreationOptions.LongRunning);
        
        public IApiObservationService Observe<T>(params ObservableOperation[] operations) where T : IApiObserver
        {
            this._observeRequest.OperationCriteria = this._observeRequest.OperationCriteria.Union(operations);
            _ = this.SendObservationRequestAsync();
            var @type = typeof(T);
            this._chronicle.Debug($"Observer {@type.Name} is observing.");
            this._observers.Add((new(operations, null, null), @type));
            return this;
        }
        
        private async Task Handle()
        {
            while (true)
            {
                try
                {
                    await ConnectToEndpoint();
                    _ = SendObservationRequestAsync();
                    await ListenToEndpoint();
                }
                catch (Exception e)
                {
                    this._chronicle.Critical(e);
                }
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        private async Task ListenToEndpoint()
        {
            this._chronicle.Debug("Listening for observations from observe endpoint.");
            var buffer = new byte[256];
            while (this._wsClient.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await this._wsClient.ReceiveAsync(buffer, CancellationToken.None);
                }
                catch (WebSocketException e)
                {
                    this._chronicle.Warning("Lost connection to observe endpoint.");
                    return;
                }
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    this._chronicle.Debug("Connection to observe endpoint closed by remote.");
                    await this._wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    continue;
                }

                var message = Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, 0, result.Count));
                var observation = JsonSerializer.Deserialize<Observation>(message);
                if (observation == null)
                    continue;

                this._chronicle.Debug($"Received observation from observe endpoint; {observation}.");
                _ = HandleObservation(observation);
            }
        }

        private async Task ConnectToEndpoint()
        {
            try
            {
                this._chronicle.Debug("Connecting to API observe ws endpoint.");
                this._wsClient = new();
                await this._wsClient.ConnectAsync(this._wsUri, CancellationToken.None);
                this._chronicle.Info("Connected to API observe ws endpoint.");
            }
            catch
            {
                this._chronicle.Warning("Failed to connect to API observe ws endpoint.");
                throw;
            }
        }

        private Task SendObservationRequestAsync()
        {
            if (this._wsClient?.State != WebSocketState.Open)
                return Task.CompletedTask;
            this._chronicle.Debug("Sending observation request to observe endpoint.");
            var requestMessage = JsonSerializer.Serialize(this._observeRequest);
            return this._wsClient.SendAsync(Encoding.ASCII.GetBytes(requestMessage),
                WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task HandleObservation(Observation observation)
        {
            foreach (var observer in this._observers)
            {
                if (!observer.Request.OperationCriteria.Contains(observation.Operation)) continue;
                var observerInstance = ActivatorUtilities.CreateInstance(this._serviceProvider, observer.ObserverType) as IApiObserver;
                if (observerInstance == null) continue;
                this._chronicle.Debug($"Observer {observer.ObserverType.Name} handling observation.");
                await observerInstance.Handle(observation);
            }
        }
        
        public void Dispose()
        {
            _wsClient?
                .CloseAsync(WebSocketCloseStatus.NormalClosure, "Observation ended", CancellationToken.None)
                .ContinueWith(_ => _wsClient?.Dispose());
            _task?.Dispose();
        }

    }

}
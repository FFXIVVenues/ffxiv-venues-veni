﻿using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using FFXIVVenues.Veni;
using FFXIVVenues.Veni.AI.Clu;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Abstractions;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.AI.Davinci;
using FFXIVVenues.Veni.AI.Luis;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.GuildEngagement;
using FFXIVVenues.Veni.Infrastructure;
using FFXIVVenues.Veni.Infrastructure.Presence;
using FFXIVVenues.Veni.Infrastructure.Tasks;
using FFXIVVenues.Veni.UserSupport;
using FFXIVVenues.Veni.VenueAuditing.MassAudit;
using FFXIVVenues.Veni.VenueAuditing.MassAudit.Exporting;
using FFXIVVenues.Veni.VenueAuditing.MassAuditNotice;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueApproval;
using FFXIVVenues.Veni.VenueDiscovery.Commands;
using FFXIVVenues.Veni.VenueObservations;
using FFXIVVenues.Veni.VenueRendering;
using OfficeOpenXml;

var serviceCollection = new ServiceCollection();
var config = Bootstrap.LoadConfiguration(serviceCollection);
Bootstrap.ConfigureLogging(serviceCollection, config);
Bootstrap.ConfigureApiClient(serviceCollection, config);
Bootstrap.ConfigureRepository(serviceCollection, config);
Bootstrap.ConfigureDiscordClient(serviceCollection, config);

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

serviceCollection.AddSingleton<ICommandBroker, CommandBroker>();
serviceCollection.AddSingleton<IComponentBroker, ComponentBroker>();
serviceCollection.AddSingleton<IApiService, ApiService>();
serviceCollection.AddSingleton<IAuthorizer, Authorizer>();
serviceCollection.AddSingleton<IGuildManager, GuildManager>();
serviceCollection.AddSingleton<IVenueApprovalService, VenueApprovalService>();
serviceCollection.AddSingleton<IAIHandler, AIHandler>();
serviceCollection.AddSingleton<IDavinciService, DavinciService>();
serviceCollection.AddSingleton<IAIContextBuilder, AiContextBuilder>();
serviceCollection.AddSingleton<IIntentHandlerProvider, IntentHandlerProvider>();
serviceCollection.AddSingleton<ISessionProvider, SessionProvider>();
serviceCollection.AddSingleton<IDiscordHandler, DiscordHandler>();
serviceCollection.AddSingleton<ICluClient, CluClient>();
serviceCollection.AddSingleton<IVenueAuditService, VenueAuditService>();
serviceCollection.AddSingleton<IVenueRenderer, VenueRenderer>();
serviceCollection.AddSingleton<IApiObservationService, ApiObservationService>();
serviceCollection.AddSingleton<IInteractionContextFactory, InteractionContextFactory>();
serviceCollection.AddSingleton<ICommandCartographer, CommandCartographer>();
serviceCollection.AddSingleton<IMassAuditService, MassAuditService>();
serviceCollection.AddSingleton<IMassAuditExporter, MassAuditExporter>();
serviceCollection.AddSingleton<MassNoticeService>();
serviceCollection.AddSingleton<IDiscordValidator, DiscordValidator>();
serviceCollection.AddSingleton<ISiteValidator, SiteValidator>();
serviceCollection.AddSingleton<IActivityManager, ActivityManager>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var commandBroker = serviceProvider.GetService<ICommandBroker>();
commandBroker.AddFromAssembly();
commandBroker.AddVenueControlCommands();
commandBroker.Add<HelpCommand.CommandFactory, HelpCommand.CommandHandler>(HelpCommand.COMMAND_NAME, isMasterGuildCommand: false);
commandBroker.Add<ShowOpenCommand.CommandFactory, ShowOpenCommand.CommandHandler>(ShowOpenCommand.COMMAND_NAME, isMasterGuildCommand: false);
commandBroker.Add<ShowForCommand.CommandFactory, ShowForCommand.CommandHandler>(ShowForCommand.COMMAND_NAME, isMasterGuildCommand: false);
commandBroker.Add<ShowMineCommand.CommandFactory, ShowMineCommand.CommandHandler>(ShowMineCommand.COMMAND_NAME, isMasterGuildCommand: false);
commandBroker.Add<ShowCountCommand.CommandFactory, ShowCountCommand.CommandHandler>(ShowCountCommand.COMMAND_NAME, isMasterGuildCommand: false);
commandBroker.Add<GetUnapprovedCommand.CommandFactory, GetUnapprovedCommand.CommandHandler>(GetUnapprovedCommand.COMMAND_NAME, isMasterGuildCommand: false);

serviceProvider.GetService<IComponentBroker>()
    .AddVenueObservationHandlers()
    .AddVenueAuditingHandlers()
    .AddVenueControlHandlers()
    .AddVenueRenderingHandlers();

_ = serviceProvider.GetService<IApiObservationService>()
    .AddVenueObservers()
    .ObserveAsync();

await serviceProvider.GetService<IDiscordHandler>().ListenAsync();
await Task.Delay(Timeout.Infinite);
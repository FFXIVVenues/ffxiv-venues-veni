using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Auditing;
using FFXIVVenues.Veni.Configuration;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Session;
using FFXIVVenues.Veni.People;
using FFXIVVenues.Veni.SessionStates;
using FFXIVVenues.VenueModels;
using moment.net;
using PrettyPrintNet;

namespace FFXIVVenues.Veni.Utils
{
    public class VenueRenderer : IVenueRenderer
    {
        private readonly IStaffService _staffService;
        private readonly IVenueAuditFactory _venueAuditFactory;
        private readonly UiConfiguration _uiConfig;
        private readonly ApiConfiguration _apiConfig;

        public VenueRenderer(IStaffService staffService, IVenueAuditFactory venueAuditFactory, UiConfiguration uiConfig, ApiConfiguration apiConfig)
        {
            this._staffService = staffService;
            _venueAuditFactory = venueAuditFactory;
            this._uiConfig = uiConfig;
            this._apiConfig = apiConfig;
        }

        public EmbedBuilder RenderEmbed(Venue venue, string bannerUrl = null)
        {
            var uiUrl = $"{this._uiConfig}/#{venue.Id}"; 
            bannerUrl ??= $"{this._apiConfig.BaseUrl}/venue/{venue.Id}/media";
            
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("**Created**: ");
            stringBuilder.AppendLine(venue.Added.FromNow()[0].ToString().ToUpper() + venue.Added.FromNow()[1..]);
            stringBuilder.Append("**Location**: ");
            stringBuilder.AppendLine(venue.Location.ToString());
            stringBuilder.Append("**SFW**: ");
            stringBuilder.AppendLine(venue.Sfw ? "Yes" : "No");
            stringBuilder.Append("**Website**: ");
            stringBuilder.AppendLine(venue.Website?.ToString() ?? "No website");
            stringBuilder.Append("**Discord**: ");
            stringBuilder.AppendLine(venue.Discord?.ToString() ?? "No discord");
            stringBuilder.Append("**Tags**: ");
            if (venue.Tags == null || venue.Tags.Count == 0)
                stringBuilder.AppendLine(" None");
            else
                stringBuilder.AppendLine(string.Join(", ", venue.Tags));


            var charsLeft = 1000;
            stringBuilder.AppendLine("**Description**: ");

            if (venue.Description != null)
                foreach (var paragraph in venue.Description)
                {
                    var trimmmedParagraph = paragraph;
                    if (paragraph.Length > charsLeft)
                        trimmmedParagraph = paragraph[..charsLeft];
                    stringBuilder.Append(paragraph);
                    charsLeft -= trimmmedParagraph.Length;

                    if (charsLeft < 10)
                    {
                        stringBuilder.AppendLine("...")
                            .AppendLine()
                            .AppendLine();
                        break;
                    }

                    stringBuilder.AppendLine();
                }
            else
                stringBuilder.AppendLine("No description")
                    .AppendLine();

            stringBuilder.AppendLine();

            if (venue.Openings == null || venue.Openings.Count == 0)
                stringBuilder.AppendLine("**Schedule**: ")
                    .AppendLine("No set schedule");
            else
            {
                stringBuilder.AppendLine("**Schedule**: ");
                foreach (var opening in venue.Openings)
                {
                    stringBuilder
                        .Append(opening.Day.ToString())
                        .Append("s, ")
                        .Append(opening.Start.Hour)
                        .Append(':')
                        .Append(opening.Start.Minute.ToString("00"))
                        .Append(" (")
                        .Append(opening.Start.TimeZone switch
                        {
                            "Eastern Standard Time" => "EST",
                            "Central Standard Time" => "CST",
                            "Mountain Standard Time" => "MST",
                            "Pacific Standard Time" => "PST",
                            "Atlantic Standard Time" => "AST",
                            "Central Europe Standard Time" => "CEST",
                            "E. Europe Standard Time" => "EEST",
                            "GMT Standard Time" => "GMT",
                            "UTC" => "Server Time",
                            _ => opening.Start.TimeZone
                        }).Append(")");
                    if (opening.Start.NextDay)
                    {
                        stringBuilder.Append(" (");
                        stringBuilder.Append(opening.Day.Next().ToShortName());
                        stringBuilder.Append(")");
                    }

                    if (opening.End != null)
                    {

                        stringBuilder
                            .Append(" - ")
                            .Append(opening.End.Hour)
                            .Append(':')
                            .Append(opening.End.Minute.ToString("00"))
                            .Append(" (")
                            .Append(opening.End.TimeZone switch
                            {
                                "Eastern Standard Time" => "EST",
                                "Central Standard Time" => "CST",
                                "Mountain Standard Time" => "MST",
                                "Pacific Standard Time" => "PST",
                                "Atlantic Standard Time" => "AST",
                                "Central Europe Standard Time" => "CEST",
                                "E. Europe Standard Time" => "EEST",
                                "GMT Standard Time" => "GMT",
                                "UTC" => "Server Time",
                                _ => opening.End.TimeZone
                            })
                            .Append(')');
                        if (opening.End.NextDay)
                        {
                            stringBuilder.Append(" (");
                            stringBuilder.Append(opening.Day.Next().ToShortName());
                            stringBuilder.Append(")");
                        }
                    }

                    stringBuilder.AppendLine();
                }
            }


            if (venue.OpenOverrides != null && venue.OpenOverrides.Any(o => o.End > DateTime.Now))
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("**Schedule Overrides**:");
                foreach (var @override in venue.OpenOverrides)
                {
                    if (@override.End < DateTime.Now)
                        continue;

                    stringBuilder.Append(@override.Open ? "Open " : "Closed ");

                    if (@override.Start < DateTime.Now)
                    {
                        stringBuilder.Append(" for ");
                        stringBuilder.AppendLine(@override.End.ToNow()[3..]);
                    }
                    else
                    {
                        stringBuilder.Append(" from ");
                        stringBuilder.Append(@override.Start.ToNow()[3..]);
                        stringBuilder.Append(" from now for ");
                        stringBuilder.AppendLine((@override.End - @override.Start).ToPrettyString());
                    }

                }
            }

            stringBuilder.AppendLine();

            stringBuilder.AppendLine("**Managers**: ");
            if (venue.Managers != null)
                foreach (var manager in venue.Managers)
                    stringBuilder.AppendLine(MentionUtils.MentionUser(ulong.Parse(manager)));
            else
                stringBuilder.AppendLine("No managers listed");

            if (venue.Open)
                stringBuilder.AppendLine().AppendLine(":green_circle: Open right now");
            else
                stringBuilder.AppendLine().AppendLine(":black_circle: Not open right now");

            var builder = new EmbedBuilder()
                .WithTitle(venue.Name)
                .WithDescription(stringBuilder.ToString())
                .WithUrl(uiUrl)
                .WithImageUrl(bannerUrl);

            return builder;
        }

        public ComponentBuilder RenderActionComponents(IVeniInteractionContext context, Venue venue, ulong user)
        {
            var isManager = venue.Managers.Contains(user.ToString());
            var isEditor = this._staffService.IsEditor(user);
            var isPhotographer = this._staffService.IsPhotographer(user);
            
            var builder = new ComponentBuilder();
                
            if (isEditor)
                builder.WithButton(new ButtonBuilder().WithLabel("Audit")
                    .WithSessionHandler(context.Session, ctx =>
                    {
                        var audit = this._venueAuditFactory.CreateAuditFor(venue, roundId:null);
                        return audit.AuditAsync();
                    }, ComponentPersistence.ClearRow)
                    .WithStyle(ButtonStyle.Secondary));

            if (isEditor || isManager)
            {
                builder.WithButton("Open", context.Session.RegisterComponentHandler(async cc =>
                {
                    cc.Session.SetItem("venue", venue);
                    await cc.Session.MoveStateAsync<OpenEntrySessionState>(cc);
                }, ComponentPersistence.ClearRow), ButtonStyle.Primary)
                .WithButton("Close", context.Session.RegisterComponentHandler(async cc =>
                {
                    cc.Session.SetItem("venue", venue);
                    await cc.Session.MoveStateAsync<CloseEntrySessionState>(cc);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("Edit", context.Session.RegisterComponentHandler(cc =>
                {
                    cc.Session.SetItem("venue", venue);
                    return cc.Session.MoveStateAsync<ModifyVenueSessionState>(cc);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("Delete", context.Session.RegisterComponentHandler(cm =>
                {
                    context.Session.SetItem("venue", venue);
                    return cm.Session.MoveStateAsync<DeleteVenueSessionState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                .WithButton("Do nothing", context.Session.RegisterComponentHandler(cm => Task.CompletedTask,
                    ComponentPersistence.ClearRow), ButtonStyle.Secondary);
            }
            else if (isPhotographer)
            {
                builder.WithButton("Edit Banner Photo", context.Session.RegisterComponentHandler(cm =>
                {
                    cm.Session.SetItem("venue", venue);
                    return cm.Session.MoveStateAsync<BannerEntrySessionState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary);
            }

            return builder;
        }
        
    }

    public interface IVenueRenderer
    {
        EmbedBuilder RenderEmbed(Venue venue, string bannerUrl = null);

        ComponentBuilder RenderActionComponents(IVeniInteractionContext context, Venue venue, ulong user);

    }
}


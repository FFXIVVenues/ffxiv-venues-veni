using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueAuditing.ComponentHandlers;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.ComponentHandlers;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.EditPropertyHandlers;
using FFXIVVenues.Veni.VenueControl.VenueClosing.ComponentHandlers;
using FFXIVVenues.Veni.VenueControl.VenueDeletion.ComponentHandlers;
using FFXIVVenues.Veni.VenueControl.VenueOpening.ComponentHandlers;
using FFXIVVenues.Veni.VenueRendering.ComponentHandlers;
using FFXIVVenues.VenueModels;
using moment.net;
using PrettyPrintNet;
using TimeZoneConverter;

namespace FFXIVVenues.Veni.VenueRendering
{
    public class VenueRenderer : IVenueRenderer
    {
        
        private readonly IAuthorizer _authorizer;
        private readonly UiConfiguration _uiConfig;
        
        public VenueRenderer(IAuthorizer authorizer, UiConfiguration uiConfig)
        {
            this._authorizer = authorizer;
            this._uiConfig = uiConfig;
        }

        public EmbedBuilder RenderEmbed(Venue venue, string bannerUrl = null)
        {
            var uiUrl = $"{this._uiConfig.BaseUrl}/#{venue.Id}"; 
            bannerUrl ??= venue.BannerUri?.ToString();
            
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
                            "America/New_York" => "EST",
                            "Central Standard Time" => "CST",
                            "America/Chicago" => "CST",
                            "Mountain Standard Time" => "MST",
                            "America/Denver" => "MST",
                            "Pacific Standard Time" => "PST",
                            "America/Los_Angeles" => "PST",
                            "Atlantic Standard Time" => "AST",
                            "America/Halifax" => "AST",
                            "Central Europe Standard Time" => "CEST",
                            "Europe/Budapest" => "CEST",
                            "E. Europe Standard Time" => "EEST",
                            "Europe/Chisinau" => "EEST",
                            "Greenwich Mean Time" => "GMT",
                            "GMT Standard Time" => "GMT",
                            "Europe/London" => "GMT",
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
                                "America/New_York" => "EST",
                                "Central Standard Time" => "CST",
                                "America/Chicago" => "CST",
                                "Mountain Standard Time" => "MST",
                                "America/Denver" => "MST",
                                "Pacific Standard Time" => "PST",
                                "America/Los_Angeles" => "PST",
                                "Atlantic Standard Time" => "AST",
                                "America/Halifax" => "AST",
                                "Central Europe Standard Time" => "CEST",
                                "Europe/Budapest" => "CEST",
                                "E. Europe Standard Time" => "EEST",
                                "Europe/Chisinau" => "EEST",
                                "Greenwich Mean Time" => "GMT",
                                "GMT Standard Time" => "GMT",
                                "Europe/London" => "GMT",
                                "UTC" => "Server Time",
                                _ => opening.Start.TimeZone
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
            var builder = new ComponentBuilder();
            var dropDown = new SelectMenuBuilder()
                .WithValueHandlers()
                .WithPlaceholder("What would you like to do?");

            if (this._authorizer.Authorize(user, Permission.AuditVenue, venue).Authorized)
                dropDown.AddOption(new SelectMenuOptionBuilder()
                    .WithLabel("Audit")
                    .WithEmote(new Emoji("🔍"))
                    .WithDescription("Message managers to confirm this venue's detail.")
                    .WithStaticHandler(AuditHandler.Key, venue.Id, "false", string.Empty));
            
            if (this._authorizer.Authorize(user, Permission.ViewAuditHistory, venue).Authorized)
                dropDown.AddOption(new SelectMenuOptionBuilder()
                    .WithLabel("View audits")
                    .WithEmote(new Emoji("🔍"))
                    .WithDescription("Get previous audits for this venue.")
                    .WithStaticHandler(GetAuditsHandler.Key, venue.Id));
            

            if (this._authorizer.Authorize(user, Permission.OpenVenue, venue).Authorized)
                dropDown.AddOption(new SelectMenuOptionBuilder()
                    .WithLabel("Open")
                    .WithEmote(new Emoji("📢"))
                    .WithDescription("Open this venue for a given amount of hours.")
                    .WithStaticHandler(OpenHandler.Key, venue.Id));
                
            if (this._authorizer.Authorize(user, Permission.CloseVenue, venue).Authorized)
                dropDown.AddOption(new SelectMenuOptionBuilder()
                    .WithLabel("Close")
                    .WithEmote(new Emoji("🔒"))
                    .WithDescription("Close current opening or go on hiatus.")
                    .WithStaticHandler(CloseHandler.Key, venue.Id));
                
            if (this._authorizer.Authorize(user, Permission.EditVenue, venue).Authorized)
                dropDown.AddOption(new SelectMenuOptionBuilder()
                    .WithLabel("Edit")
                    .WithEmote(new Emoji("✏️"))
                    .WithDescription("Update the details on this venue.")
                    .WithStaticHandler(EditHandler.Key, venue.Id));
            else {
                if (this._authorizer.Authorize(user, Permission.EditManagers, venue).Authorized)
                    dropDown.AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Edit Managers")
                        .WithEmote(new Emoji("📸"))
                        .WithDescription("Update the controlling managers on this venue.")
                        .WithStaticHandler(EditManagersHandler.Key, venue.Id));
                
                if (this._authorizer.Authorize(user, Permission.EditPhotography, venue).Authorized)
                    dropDown.AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Edit Photo")
                        .WithEmote(new Emoji("📸"))
                        .WithDescription("Update the banner on this venue.")
                        .WithStaticHandler(EditPhotoHandler.Key, venue.Id));
            }

            if (this._authorizer.Authorize(user, Permission.DeleteVenue, venue).Authorized)
                dropDown.AddOption(new SelectMenuOptionBuilder()
                    .WithLabel("Delete")
                    .WithEmote(new Emoji("❌"))
                    .WithDescription("Delete this venue completely.")
                    .WithStaticHandler(DeleteHandler.Key, venue.Id));
            
            if (dropDown.Options.Count > 0)
            {
                dropDown.AddOption(new SelectMenuOptionBuilder()
                    .WithLabel("Do nothing")
                    .WithStaticHandler(DismissHandler.Key));
                
                builder.WithSelectMenu(dropDown);
            }
            return builder;
        }

        public ComponentBuilder RenderEditComponents(Venue venue, ulong user)
        {
            var component = new ComponentBuilder();
            var selectMenu = new SelectMenuBuilder()
                .WithValueHandlers()
                .WithPlaceholder("What would you like to edit?");

            if (this._authorizer.Authorize(user, Permission.EditVenue, venue).Authorized)
                selectMenu
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Edit Name")
                        .WithEmote(new Emoji("🪪"))
                        .WithDescription("The name of your venue.")
                        .WithStaticHandler(EditNameHandler.Key, venue.Id))
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Edit Description")
                        .WithEmote(new Emoji("📃"))
                        .WithDescription("The blurb describing and advertising your venue.")
                        .WithStaticHandler(EditDescriptionHandler.Key, venue.Id))
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Edit Location")
                        .WithEmote(new Emoji("📍"))
                        .WithDescription("The primary address of your venue.")
                        .WithStaticHandler(EditLocationHandler.Key, venue.Id))
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Edit Schedule")
                        .WithEmote(new Emoji("📆"))
                        .WithDescription("The days and times your venue opens.")
                        .WithStaticHandler(EditScheduleHandler.Key, venue.Id))
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Edit N/SFW status")
                        .WithEmote(new Emoji("😳"))
                        .WithDescription("Whether your venue is openly NSFW or not.")
                        .WithStaticHandler(EditNsfwHandler.Key, venue.Id))
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Edit Tags")
                        .WithEmote(new Emoji("🏷️"))
                        .WithDescription("The categories and tags for your venue.")
                        .WithStaticHandler(EditTagsHandler.Key, venue.Id))
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Edit Website")
                        .WithEmote(new Emoji("🌐"))
                        .WithDescription("The primary address of your venue.")
                        .WithStaticHandler(EditWebsiteHandler.Key, venue.Id))
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Edit Discord")
                        .WithEmote(new Emoji("💬"))
                        .WithDescription("The permanent invite link to your venue's discord server.")
                        .WithStaticHandler(EditDiscordHandler.Key, venue.Id))
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Edit Splash Banner")
                        .WithEmote(new Emoji("📸"))
                        .WithDescription("The main splash images for your venue.")
                        .WithStaticHandler(EditPhotoHandler.Key, venue.Id))
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Edit Managers")
                        .WithEmote(new Emoji("👩‍💼"))
                        .WithDescription("The managers able to control this venue.")
                        .WithStaticHandler(EditManagersHandler.Key, venue.Id))
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Don't edit anything")
                        .WithStaticHandler(DismissHandler.Key));

            return component.WithSelectMenu(selectMenu);
        }

        public ComponentBuilder RenderVenueSelection(IEnumerable<Venue> venues, string handlerKey)
        {
            var componentBuilder = new ComponentBuilder();
            var selectMenuBuilder = new SelectMenuBuilder()
                .WithStaticHandler(handlerKey);
            foreach (var venue in venues.OrderBy(v => v.Name))
            {
                var selectMenuOption = new SelectMenuOptionBuilder
                {
                    Label = venue.Name,
                    Description = venue.Location.ToString(),
                    Value = venue.Id
                };
                selectMenuBuilder.AddOption(selectMenuOption);
            }
            componentBuilder.WithSelectMenu(selectMenuBuilder);

            return componentBuilder;
        }
        
    }

    public interface IVenueRenderer
    {
        EmbedBuilder RenderEmbed(Venue venue, string bannerUrl = null);

        ComponentBuilder RenderActionComponents(IVeniInteractionContext context, Venue venue, ulong user);

        ComponentBuilder RenderEditComponents(Venue venue, ulong user);

        ComponentBuilder RenderVenueSelection(IEnumerable<Venue> venues, string handlerKey);
    }
}


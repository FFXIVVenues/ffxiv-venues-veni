using System;
using System.Linq;
using System.Text;
using Discord;
using FFXIVVenues.VenueModels;
using moment.net;
using PrettyPrintNet;

namespace FFXIVVenues.Veni.Utils
{
    public static class VenueExtensions
    {

        public static EmbedBuilder ToEmbed(this Venue venue, string uiUrl, string bannerUrl)
        {
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

    }
}


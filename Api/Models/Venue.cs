using System;
using System.Linq;
using System.Text;
using Discord;
using FFXIVVenues.Veni.Utils;
using moment.net;
using PrettyPrintNet;

namespace FFXIVVenues.Veni.Api.Models
{
    public class Venue : VenueModels.V2022.Venue
    {

        public EmbedBuilder ToEmbed(string uiUrl, string bannerUrl)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("**Created**: ");
            stringBuilder.AppendLine(this.Added.FromNow()[0].ToString().ToUpper() + this.Added.FromNow()[1..]);
            stringBuilder.Append("**Location**: ");
            stringBuilder.AppendLine(this.Location.ToString());
            stringBuilder.Append("**SFW**: ");
            stringBuilder.AppendLine(this.Sfw ? "Yes" : "No");
            stringBuilder.Append("**Website**: ");
            stringBuilder.AppendLine(this.Website?.ToString() ?? "No website");
            stringBuilder.Append("**Discord**: ");
            stringBuilder.AppendLine(this.Discord?.ToString() ?? "No discord");
            stringBuilder.Append("**Tags**: ");
            if (this.Tags == null || this.Tags.Count == 0)
                stringBuilder.AppendLine(" None");
            else
                stringBuilder.AppendLine(string.Join(", ", this.Tags));


                var charsLeft = 1000;
                stringBuilder.AppendLine("**Description**: ");

            if (this.Description != null)
                foreach (var paragraph in this.Description)
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

            if (Openings == null || Openings.Count == 0)
                stringBuilder.AppendLine("**Schedule**: ")
                    .AppendLine("No set schedule");
            else
            {
                stringBuilder.AppendLine("**Schedule**: ");
                foreach (var opening in Openings)
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
                               _ => opening.Start.TimeZone
                           }).Append(" (ND/PMN)");
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
                                   _ => opening.End.TimeZone
                               })
                               .Append(')');
                        if (opening.Start.NextDay)
                        {
                            stringBuilder.Append(" (");
                            stringBuilder.Append(opening.Day.Next().ToShortName());
                            stringBuilder.Append(")");
                        }
                    }
                    stringBuilder.AppendLine();
                }
            }


            if (this.OpenOverrides != null && this.OpenOverrides.Any(o => o.End > DateTime.Now))
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("**Schedule Overrides**:");
                foreach (var @override in this.OpenOverrides)
                {
                    if (@override.End < DateTime.Now)
                        continue;

                    if (@override.IsNow)
                        stringBuilder.AppendLine("Open test");


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
            if (this.Managers != null)
                foreach (var manager in this.Managers)
                    stringBuilder.AppendLine(MentionUtils.MentionUser(ulong.Parse(manager)));
            else
                stringBuilder.AppendLine("No managers listed");

            if (this.Open)
                stringBuilder.AppendLine().AppendLine(":green_circle: Open right now");
            else
                stringBuilder.AppendLine().AppendLine(":black_circle: Not open right now");

            var builder = new EmbedBuilder()
                .WithTitle(this.Name)
                .WithDescription(stringBuilder.ToString())
                .WithUrl(uiUrl)
                .WithImageUrl(bannerUrl);

            return builder;
        }

    }
}


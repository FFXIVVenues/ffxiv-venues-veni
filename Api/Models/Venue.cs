using System.Text;
using Discord;

namespace FFXIVVenues.Veni.Api.Models
{
    public class Venue : VenueModels.V2022.Venue
    {

        public EmbedBuilder ToEmbed(string uiUrl, string bannerUrl)
        {
            var stringBuilder = new StringBuilder();
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
                    stringBuilder.AppendLine()
                                 .AppendLine();
                }
            else
                stringBuilder.AppendLine("No description")
                             .AppendLine();

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
                           }).Append(')');
                    if (opening.End != null)
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
                    stringBuilder.AppendLine();
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

            var builder = new EmbedBuilder()
                .WithTitle(this.Name)
                .WithDescription(stringBuilder.ToString())
                .WithUrl(uiUrl)
                .WithImageUrl(bannerUrl);

            return builder;
        }

    }
}

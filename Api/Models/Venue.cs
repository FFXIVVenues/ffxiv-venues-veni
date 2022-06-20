using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVVenues.Veni.Api.Models
{
    public class Venue
    {
        public string Id { get; set; }
        public string Name { get; set; } = "An mysterious venue";
        public List<string> Description { get; set; } = new();
        public Location Location { get; set; } = new();
        public Uri Website { get; set; }
        public Uri Discord { get; set; }
        public bool Sfw { get; set; }
        public bool Nsfw { get; set; }
        public List<Opening> Openings { get; set; } = new();
        public List<OpeningException> Exceptions { get; set; } = new();
        public List<Notice> Notices { get; set; } = new();
        public List<string> Contacts { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public bool Open { get; set; }

        public Venue() =>
            Id = GenerateId();

        public string GenerateId()
        {
            var chars = "BCDFGHJKLMNPQRSTVWXYZbcdfghjklmnpqrstvwxyz0123456789";
            var stringChars = new char[12];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        public override string ToString()
        {
            var summary = new StringBuilder();

            summary.Append(":id: : ")
                .AppendLine(Id)
                .Append(":placard:  : ")
                .AppendLine(Name)
                .Append($":hut:  : ")
                .Append(Location.DataCenter).Append(", ").Append(Location.World).Append(", ")
                .Append(Location.District).Append(", Ward ").Append(Location.Ward).Append(", ");

            if (Location.Apartment > 0)
            {
                summary.Append("Apartment #").Append(Location.Apartment);
                if (Location.Subdivision) summary.Append(" (subdivision)");
            }
            else
                summary.Append("Plot #").Append(Location.Plot);

            summary.AppendLine().Append(":hugging:  : ").AppendLine(Sfw ? "Is SFW" : "Is not SFW")
                                .Append(":underage:  : ").AppendLine(Nsfw ? "Provides NSFW services" : "Does not provide NSFW services")
                                .Append(":link:  : ").AppendLine(Website?.ToString() ?? "No website")
                                .Append(":speaking_head:  : ").AppendLine(Discord?.ToString() ?? "No discord");

            if (Open)
                summary.AppendLine("🟢 : *Currently open.*");
            else
                summary.AppendLine("🔴 : *Not currently open.*");

            if (Openings == null || Openings.Count == 0)
                summary.Append(":calendar: : ").AppendLine("No set schedule");
            else
            {
                summary.AppendLine(":calendar: : ");
                foreach (var opening in Openings)
                    summary.Append(opening.Day.ToString())
                           .Append(", ")
                           .Append(opening.Start.Hour)
                           .Append(":")
                           .Append(opening.Start.Minute.ToString("00"))
                           .Append(" (")
                           .Append(opening.Start.TimeZone)
                           .Append(") - ")
                           .Append(opening.End.Hour)
                           .Append(":")
                           .Append(opening.End.Minute.ToString("00"))
                           .Append(" (")
                           .Append(opening.End.TimeZone)
                           .AppendLine(")");

            }

            var charsLeft = 1000;
            summary.Append("📝 : ");
            foreach (var paragraph in Description)
            {
                if (charsLeft < 10)
                {
                    summary.AppendLine("...");
                    break;
                }

                var trimmmedParagraph = paragraph;
                if (paragraph.Length > charsLeft)
                {
                    trimmmedParagraph = paragraph.Substring(0, charsLeft);
                }
                summary.AppendLine(paragraph).AppendLine();
                charsLeft -= trimmmedParagraph.Length;
            }

            return summary.ToString();
        }
    }
}

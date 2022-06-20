using System;

namespace FFXIVVenues.Veni.Api.Models
{
    public class Notice
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public NoticeType Type { get; set; }
        public string Message { get; set; }
    }

}

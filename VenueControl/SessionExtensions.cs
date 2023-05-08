using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl;

public static class SessionExtensions
{
    public static Venue GetVenue(this Session session) =>
        session.GetItem<Venue>("venue");

    public static Session SetVenue(this Session session, Venue venue)
    {
        session.SetItem("venue", venue);
        return session;
    }
    
    public static bool InEditing(this Session session) =>
        session.GetItem<bool>("modifying");

    public static Session SetEditing(this Session session, bool editing = true)
    {
        session.SetItem("modifying", editing);
        return session;
    }
}
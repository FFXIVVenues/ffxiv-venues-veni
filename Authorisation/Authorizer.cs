using System.Linq;
using FFXIVVenues.Veni.Authorisation.Configuration;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Authorisation;

public class Authorizer : IAuthorizer
{
    
    public const string VENUE_SOURCE_KEY = "_VenueSource";
    public const string MASTER_SOURCE_KEY = "_Master";

    private readonly AuthorisationConfiguration _configuration;
    
    public Authorizer(AuthorisationConfiguration configuration) =>
        this._configuration = configuration;

    public AuthorizationResult Authorize(ulong user, Permission permission, Venue venue = null)
    {
        if (_configuration.Master.Contains(user))
            return new(true, MASTER_SOURCE_KEY, permission, user, venue);
        if (venue != null && venue.Managers.Contains(user.ToString()) 
                          && _configuration.ManagerPermissions.Contains(permission))
            return new(true, VENUE_SOURCE_KEY, permission, user, venue);
        foreach (var set in _configuration.PermissionSets)
            if (set.Permissions.Contains(permission) && set.Members.Contains(user))
                return new(true, set.Name, permission, user, venue);

        return new(false, null, permission, user, venue);
    }
    
}
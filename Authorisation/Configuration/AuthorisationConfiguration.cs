using System.Collections.Generic;

namespace FFXIVVenues.Veni.Authorisation.Configuration;

public class AuthorisationConfiguration
{
    public ulong[] Master { get; set; }
    public PermissionSet[] PermissionSets { get; set; }
    public Permission[] ManagerPermissions { get; set; }
}
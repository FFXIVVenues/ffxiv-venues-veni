namespace FFXIVVenues.Veni.Authorisation.Configuration;

public class PermissionSet
{
    public string Name { get; set; }
    public Permission[] Permissions { get; set; }
    public ulong[] Members { get; set; }
}
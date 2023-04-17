using System;

namespace FFXIVVenues.Veni.Authorisation;

[Flags]
public enum Permission
{
    ApproveVenue = 1,
    ViewAuditHistory = 2,
    AuditVenue = 4,
    EditVenue = 8,
    EditPhotography = 16,
    EditManagers = 32,
    OpenVenue = 64,
    CloseVenue = 128,
    HiatusVenue = 256,
    DeleteVenue = 512,
    Inspect = 1024,
    Blacklist = 2048
}
namespace Happenings.Model;

// Centralizovani nazivi rola � umjesto magic stringova ("Admin", "Organizer")
// razbacanih po atributima i servisima. const vrijednosti se mogu koristiti i u
// [Authorize(Roles = ...)] atributima.
public static class Roles
{
    public const string Admin = "Admin";
    public const string Organizer = "Organizer";
    public const string User = "User";

    // Kombinacija za [Authorize(Roles = ...)] (const + const je i dalje const)
    public const string OrganizerOrAdmin = Organizer + "," + Admin;
}

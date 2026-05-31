namespace Happenings.Model.Requests;

public class OrganizerRequestInsertRequest
{
    // Prazna klasa — userId se uzima iz JWT tokena
}

public class OrganizerRequestRejectRequest
{
    public string? Reason { get; set; }
}
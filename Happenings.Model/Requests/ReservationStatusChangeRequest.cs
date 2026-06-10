namespace Happenings.Model.Requests;

// Nosi stvarni razlog akcije (reject/cancel) koji upisuje korisnik ili admin,
// umjesto hardkodiranog teksta. Reason je opcionalan radi kompatibilnosti.
public class ReservationStatusChangeRequest
{
    public string? Reason { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace Happenings.Model.Requests
{
    public class ReservationUpdateRequest
    {
        [Range(1, 100000, ErrorMessage = "Quantity must be between 1 and 100000")]
        public int Quantity { get; set; }
    }
}

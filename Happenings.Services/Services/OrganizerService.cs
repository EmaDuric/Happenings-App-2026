using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Happenings.Model.DTOs;
using Happenings.Model.Entities;
using Happenings.Model.Requests;

namespace Happenings.Services.Services
{
    public class OrganizerService : IOrganizerService
    {
        private readonly HappeningsContext _context;

        public OrganizerService(HappeningsContext context)
        {
            _context = context;
        }

        public List<OrganizerDto> GetAll()
        {
            return _context.Organizers
                .Select(o => new OrganizerDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    ContactEmail = o.ContactEmail,
                    PhoneNumber = o.PhoneNumber
                }).ToList();
        }

        public OrganizerDto GetById(int id)
        {
            var entity = _context.Organizers.Find(id)
                ?? throw new Exception("Organizer not found");

            return new OrganizerDto
            {
                Id = entity.Id,
                Name = entity.Name,
                ContactEmail = entity.ContactEmail,
                PhoneNumber = entity.PhoneNumber
            };
        }

        public OrganizerDto Insert(OrganizerInsertRequest request)
        {
            // UserId je obavezan
            if (request.UserId <= 0)
                throw new Exception("UserId is required");

            // Provjeri da li korisnik postoji
            var user = _context.Users.Find(request.UserId)
                ?? throw new Exception("User not found");

            // Provjeri da li korisnik već ima organizer profil
            if (_context.Organizers.Any(o => o.UserId == request.UserId))
                throw new Exception("User already has an organizer profile");

            // Postavi IsOrganizer flag na useru
            user.IsOrganizer = true;

            var entity = new Organizer
            {
                Name = request.Name,
                ContactEmail = request.ContactEmail,
                PhoneNumber = request.PhoneNumber,
                UserId = request.UserId
            };

            _context.Organizers.Add(entity);
            _context.SaveChanges();

            return GetById(entity.Id);
        }

        // Ownership provjera za Update
        public OrganizerDto Update(int id, OrganizerUpdateRequest request, int userId, bool isAdmin)
        {
            var entity = _context.Organizers.Find(id)
                ?? throw new Exception("Organizer not found");

            if (!isAdmin && entity.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own organizer profile");

            entity.ContactEmail = request.ContactEmail;
            entity.PhoneNumber = request.PhoneNumber;

            _context.SaveChanges();

            return GetById(entity.Id);
        }

        public OrganizerDto Update(int id, OrganizerUpdateRequest request)
            => Update(id, request, 0, true);

        public void Delete(int id)
        {
            var entity = _context.Organizers.Find(id)
                ?? throw new Exception("Organizer not found");

            // Ukloni IsOrganizer flag sa usera
            var user = _context.Users.Find(entity.UserId);
            if (user != null)
                user.IsOrganizer = false;

            _context.Organizers.Remove(entity);
            _context.SaveChanges();
        }
    }
}
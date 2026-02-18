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
            var entity = new Organizer
            {
                Name = request.Name,
                ContactEmail = request.ContactEmail,
                PhoneNumber = request.PhoneNumber
            };

            _context.Organizers.Add(entity);
            _context.SaveChanges();

            return GetById(entity.Id);
        }

        public OrganizerDto Update(int id, OrganizerUpdateRequest request)
        {
            var entity = _context.Organizers.Find(id)
                ?? throw new Exception("Organizer not found");

            entity.ContactEmail = request.ContactEmail;
            entity.PhoneNumber = request.PhoneNumber;

            _context.SaveChanges();

            return GetById(entity.Id);
        }

        public void Delete(int id)
        {
            var entity = _context.Organizers.Find(id)
                ?? throw new Exception("Organizer not found");

            _context.Organizers.Remove(entity);
            _context.SaveChanges();
        }
    }
}

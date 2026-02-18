public class EventImageService : IEventImageService
{
    private readonly HappeningsContext _context;

    public EventImageService(HappeningsContext context)
    {
        _context = context;
    }

    public List<EventImageDto> GetByEvent(int eventId)
    {
        return _context.EventImages
            .Where(i => i.EventId == eventId)
            .Select(i => new EventImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                EventId = i.EventId
            }).ToList();
    }

    public EventImageDto Insert(EventImageInsertRequest request)
    {
        var entity = new EventImage
        {
            ImageUrl = request.ImageUrl,
            EventId = request.EventId
        };

        _context.EventImages.Add(entity);
        _context.SaveChanges();

        return new EventImageDto
        {
            Id = entity.Id,
            ImageUrl = entity.ImageUrl,
            EventId = entity.EventId
        };
    }
}

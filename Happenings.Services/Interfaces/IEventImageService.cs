public interface IEventImageService
{
    List<EventImageDto> GetByEvent(int eventId);
    EventImageDto Insert(EventImageInsertRequest request);
}

using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces;

public interface INotificationService
{
    List<NotificationDto> GetAll();
    List<NotificationDto> GetPending();

    NotificationDto Insert(NotificationInsertRequest request);
    NotificationDto? MarkAsSent(int id);
}

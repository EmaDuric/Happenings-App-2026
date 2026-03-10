using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Happenings.Model.Entities;
using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.Services.Services;

public class NotificationService : INotificationService
{
    private readonly HappeningsContext _context;

    public NotificationService(HappeningsContext context)
    {
        _context = context;
    }

    public List<NotificationDto> GetAll()
    {
        return _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                IsSent = n.IsSent,
                CreatedAt = n.CreatedAt
            })
            .ToList();
    }

    public List<NotificationDto> GetPending()
    {
        return _context.Notifications
            .Where(n => !n.IsSent)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                IsSent = n.IsSent,
                CreatedAt = n.CreatedAt
            })
            .ToList();
    }

    public NotificationDto Insert(NotificationInsertRequest request)
    {
        var entity = new Notification
        {
            Message = request.Message,
            IsSent = false,
            CreatedAt = DateTime.UtcNow,
            UserId = request.UserId,
            Title=request.Title

        };

        _context.Notifications.Add(entity);
        _context.SaveChanges();

        return new NotificationDto
        {
            Id = entity.Id,
            Message = entity.Message,
            IsSent = entity.IsSent,
            CreatedAt = entity.CreatedAt,
            UserId = entity.UserId,
            Title = entity.Title
        };
    }

    public NotificationDto? MarkAsSent(int id)
    {
        var entity = _context.Notifications.Find(id);
        if (entity == null)
            return null;

        entity.IsSent = true;
        _context.SaveChanges();

        return new NotificationDto
        {
            Id = entity.Id,
            Message = entity.Message,
            IsSent = entity.IsSent,
            CreatedAt = entity.CreatedAt
        };
    }
}

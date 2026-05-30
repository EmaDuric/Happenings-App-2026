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
                Title = n.Title,
                Message = n.Message,
                IsSent = n.IsSent,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                UserId = n.UserId
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
                Title = n.Title,
                Message = n.Message,
                IsSent = n.IsSent,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToList();
    }

    public NotificationDto Insert(NotificationInsertRequest request)
    {
        var entity = new Notification
        {
            Message = request.Message,
            Title = request.Title,
            IsSent = false,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            UserId = request.UserId
        };

        _context.Notifications.Add(entity);
        _context.SaveChanges();

        return new NotificationDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Message = entity.Message,
            IsSent = entity.IsSent,
            IsRead = entity.IsRead,
            CreatedAt = entity.CreatedAt,
            UserId = entity.UserId
        };
    }

    public NotificationDto? MarkAsSent(int id)
    {
        var entity = _context.Notifications.Find(id);
        if (entity == null) return null;

        entity.IsSent = true;
        _context.SaveChanges();

        return new NotificationDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Message = entity.Message,
            IsSent = entity.IsSent,
            IsRead = entity.IsRead,
            CreatedAt = entity.CreatedAt
        };
    }

    public List<NotificationDto> GetByUserId(int userId)
    {
        return _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                IsSent = n.IsSent,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToList();
    }

    public bool MarkAsRead(int id, int userId)
    {
        var entity = _context.Notifications
            .FirstOrDefault(n => n.Id == id && n.UserId == userId);
        if (entity == null) return false;

        entity.IsRead = true;
        entity.ReadAt = DateTime.UtcNow;
        _context.SaveChanges();
        return true;
    }

    public void MarkAllAsRead(int userId)
    {
        var notifications = _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToList();

        foreach (var n in notifications)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }

        _context.SaveChanges();
    }

    public void ClearByUserId(int userId)
    {
        var notifications = _context.Notifications
            .Where(n => n.UserId == userId)
            .ToList();

        _context.Notifications.RemoveRange(notifications);
        _context.SaveChanges();
    }
}
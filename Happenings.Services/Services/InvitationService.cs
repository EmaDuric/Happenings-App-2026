using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Happenings.Model.Entities;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Microsoft.EntityFrameworkCore;

namespace Happenings.Services.Services;

public class InvitationService : IInvitationService
{
    private readonly HappeningsContext _context;

    public InvitationService(HappeningsContext context)
    {
        _context = context;
    }

    public async Task<List<InvitationResponse>> GetAsync(int? receiverId)
    {
        var query = _context.Invitations
            .Include(x => x.Event)
            .Include(x => x.Sender)
            .Include(x => x.Receiver)
            .AsQueryable();

        if (receiverId.HasValue)
            query = query.Where(x => x.ReceiverId == receiverId);

        var list = await query.ToListAsync();

        return list.Select(x => new InvitationResponse
        {
            Id = x.Id,
            EventId = x.EventId,
            EventName = x.Event.Name,
            SenderId = x.SenderId,
            SenderName = x.Sender.Username,
            ReceiverId = x.ReceiverId,
            ReceiverName = x.Receiver.Username,
            Status = x.Status,
            SentAt = x.SentAt
        }).ToList();
    }

    public async Task<InvitationResponse> GetByIdAsync(int id)
    {
        var x = await _context.Invitations
            .Include(e => e.Event)
            .Include(s => s.Sender)
            .Include(r => r.Receiver)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (x == null) return null;

        return new InvitationResponse
        {
            Id = x.Id,
            EventId = x.EventId,
            EventName = x.Event.Name,
            SenderId = x.SenderId,
            SenderName = x.Sender.Username,
            ReceiverId = x.ReceiverId,
            ReceiverName = x.Receiver.Username,
            Status = x.Status,
            SentAt = x.SentAt
        };
    }

    public async Task<InvitationResponse> InsertAsync(InvitationInsertRequest request)
    {
        var entity = new Invitation
        {
            EventId = request.EventId,
            ReceiverId = request.ReceiverId,
            SenderId = 1, // TODO: uzeti iz auth usera
            Status = "Pending",
            SentAt = DateTime.Now
        };

        _context.Invitations.Add(entity);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(entity.Id);
    }

    //public async Task<InvitationResponse> UpdateAsync(int id, InvitationUpdateRequest request)
    //{
    //    var entity = await _context.Invitations.FindAsync(id);

    //    if (entity == null) return null;

    //    entity.Status = request.Status;

    //    await _context.SaveChangesAsync();

    //    return await GetByIdAsync(id);
    //}

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Invitations.FindAsync(id);

        if (entity == null) return false;

        _context.Invitations.Remove(entity);
        await _context.SaveChangesAsync();

        return true;
    }
}
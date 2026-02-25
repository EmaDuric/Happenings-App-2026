using System.Text;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Model.Search;
using Happenings.Model.Entities;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Happenings.Model.DTOs;

namespace Happenings.Services.Services;





public class TicketService : ITicketService
{
	private readonly HappeningsContext _context;

	public TicketService(HappeningsContext context)
	{
		_context = context;
	}

	public List<TicketDto> Get()
	{
		return _context.Tickets
			.Select(x => new TicketDto
			{
				Id = x.Id,
				ReservationId = x.ReservationId,
				QRCode = x.QRCode,
				IsUsed = x.IsUsed,
				GeneratedAt = x.GeneratedAt
			}).ToList();
	}

	public TicketDto? GetById(int id)
	{
		var entity = _context.Tickets.Find(id);
		if (entity == null) return null;

		return new TicketDto
		{
			Id = entity.Id,
			ReservationId = entity.ReservationId,
			QRCode = entity.QRCode,
			IsUsed = entity.IsUsed,
			GeneratedAt = entity.GeneratedAt
		};
	}

	public TicketDto Insert(TicketInsertRequest request)
	{
		var qrCode = Convert.ToBase64String(
			Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

		var entity = new Ticket
		{
			ReservationId = request.ReservationId,
			QRCode = qrCode,
			IsUsed = false
		};

		_context.Tickets.Add(entity);
		_context.SaveChanges();

		return new TicketDto
		{
			Id = entity.Id,
			ReservationId = entity.ReservationId,
			QRCode = entity.QRCode,
			IsUsed = entity.IsUsed,
			GeneratedAt = entity.GeneratedAt
		};
	}
}

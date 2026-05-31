using Happenings.Model.Entities;
using Happenings.Model.Responses;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Happenings.Services.Services;

public class OrganizerRequestService : IOrganizerRequestService
{
	private readonly HappeningsContext _context;

	public OrganizerRequestService(HappeningsContext context)
	{
		_context = context;
	}

	public async Task<OrganizerRequestDto> InsertAsync(int userId)
	{
		// Provjeri da korisnik veæ nije organizator
		var user = await _context.Users.FindAsync(userId)
			?? throw new Exception("User not found");

		if (user.IsOrganizer)
			throw new Exception("You are already an organizer");

		// Provjeri da veæ nema pending zahtjev
		var existing = await _context.OrganizerRequests
			.FirstOrDefaultAsync(r => r.UserId == userId && r.Status == "Pending");

		if (existing != null)
			throw new Exception("You already have a pending organizer request");

		var entity = new OrganizerRequest
		{
			UserId = userId,
			Status = "Pending",
			CreatedAt = DateTime.UtcNow
		};

		_context.OrganizerRequests.Add(entity);
		await _context.SaveChangesAsync();

		return MapToDto(entity, user);
	}

	public List<OrganizerRequestDto> GetAll()
	{
		return _context.OrganizerRequests
			.Include(r => r.User)
			.OrderByDescending(r => r.CreatedAt)
			.Select(r => new OrganizerRequestDto
			{
				Id = r.Id,
				UserId = r.UserId,
				Username = r.User.Username,
				Email = r.User.Email,
				Status = r.Status,
				CreatedAt = r.CreatedAt,
				ReviewedAt = r.ReviewedAt,
				RejectedReason = r.RejectedReason
			})
			.ToList();
	}

	public async Task ApproveAsync(int id, int adminUserId)
	{
		var request = await _context.OrganizerRequests
			.Include(r => r.User)
			.FirstOrDefaultAsync(r => r.Id == id)
			?? throw new Exception("Request not found");

		if (request.Status != "Pending")
			throw new Exception("Request already processed");

		// Ažuriraj request
		request.Status = "Approved";
		request.ReviewedAt = DateTime.UtcNow;
		request.ReviewedByUserId = adminUserId;

		// Kreiraj organizer profil
		if (!_context.Organizers.Any(o => o.UserId == request.UserId))
		{
			_context.Organizers.Add(new Organizer
			{
				UserId = request.UserId,
				Name = request.User.Username,
				ContactEmail = request.User.Email,
				PhoneNumber = ""
			});
		}

		// Postavi IsOrganizer flag
		request.User.IsOrganizer = true;

		await _context.SaveChangesAsync();
	}

	public async Task RejectAsync(int id, int adminUserId, string? reason)
	{
		var request = await _context.OrganizerRequests
			.FirstOrDefaultAsync(r => r.Id == id)
			?? throw new Exception("Request not found");

		if (request.Status != "Pending")
			throw new Exception("Request already processed");

		request.Status = "Rejected";
		request.ReviewedAt = DateTime.UtcNow;
		request.ReviewedByUserId = adminUserId;
		request.RejectedReason = reason;

		await _context.SaveChangesAsync();
	}

	private OrganizerRequestDto MapToDto(OrganizerRequest r, User user) => new()
	{
		Id = r.Id,
		UserId = r.UserId,
		Username = user.Username,
		Email = user.Email,
		Status = r.Status,
		CreatedAt = r.CreatedAt,
		ReviewedAt = r.ReviewedAt,
		RejectedReason = r.RejectedReason
	};
}
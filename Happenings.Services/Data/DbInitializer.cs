using Happenings.Model.Entities;
using Happenings.Services.Database;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Happenings.Services.Data
{
    public static class DbInitializer
    {
        public static void Seed(HappeningsContext context)
        {
            context.Database.Migrate();

            var random = new Random();

            // ADMIN USER
            if (!context.Users.Any(u => u.Email == "admin@mail.com"))
            {
                context.Users.Add(new User
                {
                    Username = "admin",
                    Email = "admin@mail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin2026"),
                    IsAdmin = true,
                    IsOrganizer = false,
                    CreatedAt = DateTime.UtcNow
                });
                context.SaveChanges();
            }

            // MOBILE USER
            if (!context.Users.Any(u => u.Email == "mobile@mail.com"))
            {
                context.Users.Add(new User
                {
                    Username = "mobile",
                    Email = "mobile@mail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("mobile2026"),
                    IsAdmin = false,
                    IsOrganizer = false,
                    CreatedAt = DateTime.UtcNow
                });
                context.SaveChanges();
            }

            // ORGANISER USER
            if (!context.Users.Any(u => u.Email == "organiser@mail.com"))
            {
                context.Users.Add(new User
                {
                    Username = "organiser",
                    Email = "organiser@mail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("organiser2026"),
                    IsAdmin = false,
                    IsOrganizer = true,
                    CreatedAt = DateTime.UtcNow
                });
                context.SaveChanges();
            }

            // USERS
            if (!context.Users.Any(u => u.Email == "amar@mail.com"))
            {
                var names = new List<string>
                {
                    "Amar","Lejla","Adnan","Selma","Tarik",
                    "Amina","Nermin","Sara","Haris","Jasmin"
                };

                var users = new List<User>();
                foreach (var name in names)
                {
                    users.Add(new User
                    {
                        Username = name.ToLower(),
                        Email = $"{name.ToLower()}@mail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("test"),
                        IsAdmin = false,
                        IsOrganizer = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            var usersList = context.Users
                .Where(u => !u.IsAdmin)
                .ToList();

            // CATEGORIES
            if (!context.EventCategories.Any())
            {
                var categories = new List<EventCategory>
                {
                    new EventCategory { Name="Music", Description="Concerts and music events"},
                    new EventCategory { Name="Technology", Description="Tech conferences and summits"},
                    new EventCategory { Name="Sport", Description="Sport and fitness events"},
                    new EventCategory { Name="Business", Description="Business and networking events"},
                    new EventCategory { Name="Art", Description="Art exhibitions and cultural events"}
                };
                context.EventCategories.AddRange(categories);
                context.SaveChanges();
            }

            var categoriesList = context.EventCategories.ToList();

            // LOCATIONS
            if (!context.Locations.Any())
            {
                var locations = new List<Location>
                {
                    new Location { Name="Skenderija Hall", Address="Terezija bb", City="Sarajevo"},
                    new Location { Name="Dom Mladih", Address="Skenderija 5", City="Sarajevo"},
                    new Location { Name="Mostar Arena", Address="Bulevar bb", City="Mostar"},
                    new Location { Name="Tuzla Sport Center", Address="Centar bb", City="Tuzla"},
                    new Location { Name="Banja Luka Conference Hall", Address="Centar bb", City="Banja Luka"},
                    new Location { Name="Zenica Cultural Center", Address="Trg Alije bb", City="Zenica"},
                    new Location { Name="Mostar Hall", Address="Rondo bb", City="Mostar"}
                };
                context.Locations.AddRange(locations);
                context.SaveChanges();
            }

            var locationsList = context.Locations.ToList();

            // ORGANIZERS
            if (!context.Organizers.Any())
            {
                // Kreiraj organizer za organiser@mail.com usera
                var organiserUser = context.Users.FirstOrDefault(u => u.Email == "organiser@mail.com");

                var organizers = new List<Organizer>
                {
                    new Organizer
                    {
                        Name = "EventPro",
                        UserId = usersList[0].Id,
                        ContactEmail = "eventpro@mail.com",
                        PhoneNumber = "061111111"
                    },
                    new Organizer
                    {
                        Name = "TechEvents",
                        UserId = usersList[1].Id,
                        ContactEmail = "techevents@mail.com",
                        PhoneNumber = "062222222"
                    }
                };

                if (organiserUser != null && !context.Organizers.Any(o => o.UserId == organiserUser.Id))
                {
                    organizers.Add(new Organizer
                    {
                        Name = "organiser",
                        UserId = organiserUser.Id,
                        ContactEmail = "organiser@mail.com",
                        PhoneNumber = "063333333"
                    });
                }

                context.Organizers.AddRange(organizers);
                context.SaveChanges();
            }

            var organizersList = context.Organizers.ToList();

            // EVENTS
            if (!context.Events.Any())
            {
                var eventNames = new List<string>
                {
                    "Sarajevo Music Festival",
                    "Tech Summit Bosnia",
                    "Startup Weekend Sarajevo",
                    "Jazz Night Sarajevo",
                    "Digital Marketing Conference",
                    "AI Conference Sarajevo",
                    "Sarajevo Marathon",
                    "Gaming Expo Balkan",
                    "Cyber Security Forum",
                    "Startup Pitch Night",
                    "Dino Merlin Concert",
                    "Kids Festival",
                    "Network Meetup Sarajevo"
                };

                var events = new List<Event>();
                foreach (var name in eventNames)
                {
                    events.Add(new Event
                    {
                        Name = name,
                        Description = $"Join us at {name}. Great speakers, entertainment and networking opportunities.",
                        EventDate = DateTime.UtcNow.AddDays(random.Next(5, 120)),
                        OrganizerId = organizersList[random.Next(organizersList.Count)].Id,
                        LocationId = locationsList[random.Next(locationsList.Count)].Id,
                        EventCategoryId = categoriesList[random.Next(categoriesList.Count)].Id
                    });
                }
                context.Events.AddRange(events);
                context.SaveChanges();
            }

            var eventsList = context.Events.ToList();

            // EVENT IMAGES
            if (!context.EventImages.Any())
            {
                var imageUrls = new List<string>
                {
                    "https://images.unsplash.com/photo-1501281668745-f7f57925c3b4",
                    "https://images.unsplash.com/photo-1540575467063-178a50c2df87",
                    "https://images.unsplash.com/photo-1492684223066-81342ee5ff30",
                    "https://images.unsplash.com/photo-1506157786151-b8491531f063",
                    "https://images.unsplash.com/photo-1518770660439-4636190af475"
                };

                var images = new List<EventImage>();
                foreach (var e in eventsList)
                {
                    images.Add(new EventImage
                    {
                        EventId = e.Id,
                        ImageUrl = imageUrls[random.Next(imageUrls.Count)]
                    });
                }
                context.EventImages.AddRange(images);
                context.SaveChanges();
            }

            // TICKET TYPES
            if (!context.EventTicketTypes.Any())
            {
                var ticketTypes = new List<EventTicketType>();
                foreach (var e in eventsList)
                {
                    ticketTypes.Add(new EventTicketType
                    {
                        EventId = e.Id,
                        Name = "Regular",
                        Price = random.Next(20, 50),
                        AvailableQuantity = 200
                    });
                    ticketTypes.Add(new EventTicketType
                    {
                        EventId = e.Id,
                        Name = "VIP",
                        Price = random.Next(80, 150),
                        AvailableQuantity = 50
                    });
                }
                context.EventTicketTypes.AddRange(ticketTypes);
                context.SaveChanges();
            }

            var ticketTypesList = context.EventTicketTypes.ToList();

            // RESERVATIONS
            if (!context.Reservations.Any())
            {
                var reservations = new List<Reservation>();
                for (int i = 0; i < 80; i++)
                {
                    var ticket = ticketTypesList[random.Next(ticketTypesList.Count)];
                    reservations.Add(new Reservation
                    {
                        UserId = usersList[random.Next(usersList.Count)].Id,
                        EventId = ticket.EventId,
                        EventTicketTypeId = ticket.Id,
                        Quantity = random.Next(1, 3),
                        Status = Happenings.Model.Enums.ReservationStatus.Pending,
                        ReservedAt = DateTime.UtcNow
                    });
                }
                context.Reservations.AddRange(reservations);
                context.SaveChanges();
            }

            var reservationsList = context.Reservations.ToList();

            // PAYMENTS
            if (!context.Payments.Any())
            {
                var payments = new List<Payment>();
                foreach (var r in reservationsList.Take(50))
                {
                    payments.Add(new Payment
                    {
                        ReservationId = r.Id,
                        Amount = random.Next(20, 120),
                        PaymentMethod = "Card",
                        Status = "Completed"
                    });
                }
                context.Payments.AddRange(payments);
                context.SaveChanges();
            }

            // TICKETS
            if (!context.Tickets.Any())
            {
                var tickets = new List<Ticket>();
                foreach (var r in reservationsList.Take(50))
                {
                    tickets.Add(new Ticket
                    {
                        ReservationId = r.Id,
                        EventId = r.EventId,
                        UserId = r.UserId,
                        QRCode = Guid.NewGuid().ToString(),
                        IsUsed = false
                    });
                }
                context.Tickets.AddRange(tickets);
                context.SaveChanges();
            }

            // REVIEWS
            if (!context.Reviews.Any())
            {
                var reviews = new List<Review>();
                foreach (var e in eventsList.Take(10))
                {
                    reviews.Add(new Review
                    {
                        UserId = usersList[random.Next(usersList.Count)].Id,
                        EventId = e.Id,
                        Rating = random.Next(3, 6),
                        Comment = "Great event, highly recommended!"
                    });
                }
                context.Reviews.AddRange(reviews);
                context.SaveChanges();
            }

            // ANNOUNCEMENTS
            if (!context.Announcements.Any())
            {
                var announcements = new List<Announcement>();
                foreach (var e in eventsList.Take(5))
                {
                    announcements.Add(new Announcement
                    {
                        EventId = e.Id,
                        Title = "Important update",
                        Content = $"New information regarding {e.Name}. Please check details.",
                        CreatedById = organizersList[0].UserId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                context.Announcements.AddRange(announcements);
                context.SaveChanges();
            }

            // INVITATIONS
            if (!context.Invitations.Any())
            {
                var invitations = new List<Invitation>();
                for (int i = 0; i < 5; i++)
                {
                    invitations.Add(new Invitation
                    {
                        EventId = eventsList[i].Id,
                        SenderId = organizersList[0].UserId,
                        ReceiverId = usersList[i + 2].Id,
                        Status = "Pending",
                        SentAt = DateTime.UtcNow
                    });
                }
                context.Invitations.AddRange(invitations);
                context.SaveChanges();
            }

            // EVENT VIEWS
            if (!context.EventViews.Any())
            {
                var views = new List<EventView>();
                for (int i = 0; i < 300; i++)
                {
                    views.Add(new EventView
                    {
                        UserId = usersList[random.Next(usersList.Count)].Id,
                        EventId = eventsList[random.Next(eventsList.Count)].Id
                    });
                }
                context.EventViews.AddRange(views);
                context.SaveChanges();
            }

            // USER PREFERENCES
            if (!context.UserPreferences.Any())
            {
                var preferences = new List<UserPreference>();
                foreach (var u in usersList.Take(5))
                {
                    preferences.Add(new UserPreference
                    {
                        UserId = u.Id,
                        PreferenceType = "Category",
                        PreferenceValue = "Music"
                    });
                }
                context.UserPreferences.AddRange(preferences);
                context.SaveChanges();
            }

            // NOTIFICATIONS
            if (!context.Notifications.Any())
            {
                var notifications = new List<Notification>();
                foreach (var u in usersList.Take(5))
                {
                    notifications.Add(new Notification
                    {
                        UserId = u.Id,
                        Title = "Welcome to Happenings!",
                        Message = "Your account has been created successfully."
                    });
                }
                context.Notifications.AddRange(notifications);
                context.SaveChanges();
            }
        }
    }
}
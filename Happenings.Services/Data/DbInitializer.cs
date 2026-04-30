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

            // USERS
            if (!context.Users.Any())
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
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("test")
                    });
                }

                context.Users.AddRange(users);
                context.SaveChanges();
            }

            var usersList = context.Users.ToList();


            // CATEGORIES
            if (!context.EventCategories.Any())
            {
                var categories = new List<EventCategory>
                {
                    new EventCategory { Name="Music", Description="Concerts"},
                    new EventCategory { Name="Technology", Description="Tech conferences"},
                    new EventCategory { Name="Sport", Description="Sport events"}
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
                    new Location { Name="Skenderija Hall", Address="Terezija", City="Sarajevo"},
                    new Location { Name="Dom Mladih", Address="Skenderija", City="Sarajevo"},
                    new Location { Name="Mostar Arena", Address="Bulevar", City="Mostar"},
                    new Location { Name="Tuzla Sport Center", Address="Centar", City="Tuzla"},
                    new Location { Name="Banja Luka Conference Hall", Address="Centar", City="Banja Luka"}
                };

                context.Locations.AddRange(locations);
                context.SaveChanges();
            }

            var locationsList = context.Locations.ToList();


            // ORGANIZERS
            if (!context.Organizers.Any())
            {
                var organizers = new List<Organizer>
{
                    new Organizer
                    {
                        Name="EventPro",
                        UserId = usersList[0].Id,
                        ContactEmail="eventpro@mail.com",
                        PhoneNumber="061111111"
                    },
                    new Organizer
                    {
                        Name="TechEvents",
                        UserId = usersList[1].Id,
                        ContactEmail="techevents@mail.com",
                        PhoneNumber="062222222"
                    }
                };      

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
                    "Startup Pitch Night"
                };

                var events = new List<Event>();

                foreach (var name in eventNames)
                {
                    events.Add(new Event
                    {
                        Name = name,
                        Description = $"Join us at {name}. Great speakers and networking.",
                        EventDate = DateTime.Now.AddDays(random.Next(5, 90)),
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
                var images = new List<EventImage>();

                foreach (var e in eventsList)
                {
                    images.Add(new EventImage
                    {
                        EventId = e.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1501281668745-f7f57925c3b4"
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
                        Price = random.Next(20, 40),
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
                        Quantity = random.Next(1, 3)
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
                        EventId = r.EventId,       // ← dodaj ovo
                        UserId = r.UserId,          // ← dodaj ovo
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
                        Rating = random.Next(3, 5),
                        Comment = "Great event!"
                    });
                }

                context.Reviews.AddRange(reviews);
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
                        Title = "Payment successful",
                        Message = "Your ticket purchase was successful."
                    });
                }

                context.Notifications.AddRange(notifications);
                context.SaveChanges();
            }
        }
    }
}
import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/event_dto.dart';
import 'auth_service.dart';

class ApiService {
  static const String baseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://localhost:5000/api',
  );

  // LOGIN
  static Future<Map<String, dynamic>> login(
      String email, String password) async {
    final response = await http.post(
      Uri.parse("$baseUrl/auth/login"),
      headers: {"Content-Type": "application/json"},
      body: jsonEncode({"email": email, "password": password}),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Login failed: ${response.body}");
    return jsonDecode(response.body);
  }

  // REGISTER — ne šalje isOrganizer (backend ga ne prima)
  static Future<Map<String, dynamic>> registerUser({
    required String username,
    required String email,
    required String password,
  }) async {
    final response = await http.post(
      Uri.parse("$baseUrl/auth/register"),
      headers: {"Content-Type": "application/json"},
      body: jsonEncode({
        "username": username,
        "email": email,
        "password": password,
      }),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Register failed: ${response.body}");
    return jsonDecode(response.body);
  }

  // EVENTS
  static Future<List<EventDto>> getEvents(String token, {String? name}) async {
    final uri = Uri.parse("$baseUrl/events").replace(
      queryParameters:
          (name != null && name.isNotEmpty) ? {"name": name} : null,
    );
    final response =
        await http.get(uri, headers: {"Authorization": "Bearer $token"});
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Loading events failed: ${response.body}");
    final data = jsonDecode(response.body);
    if (data is Map<String, dynamic> && data["items"] is List)
      return (data["items"] as List).map((e) => EventDto.fromJson(e)).toList();
    throw Exception("Unexpected events response format.");
  }

  static Future<Map<String, dynamic>> getEventDetails(
      int id, String token) async {
    final response = await http.get(
      Uri.parse("$baseUrl/events/$id"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Loading event failed");
    return jsonDecode(response.body);
  }

  static Future<Map<String, dynamic>> createEvent({
    required String name,
    required String description,
    required DateTime eventDate,
    required int eventCategoryId,
    required int locationId,
    required String token,
  }) async {
    final response = await http.post(
      Uri.parse("$baseUrl/events"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({
        "name": name,
        "description": description,
        "eventDate": eventDate.toIso8601String(),
        "eventCategoryId": eventCategoryId,
        "locationId": locationId,
      }),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Create event failed: ${response.body}");
    return jsonDecode(response.body);
  }

  static Future<Map<String, dynamic>> updateEvent({
    required int id,
    required String name,
    required String description,
    required DateTime eventDate,
    required int eventCategoryId,
    required int locationId,
    required String token,
  }) async {
    final response = await http.put(
      Uri.parse("$baseUrl/events/$id"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({
        "name": name,
        "description": description,
        "eventDate": eventDate.toIso8601String(),
        "eventCategoryId": eventCategoryId,
        "locationId": locationId,
      }),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Update event failed: ${response.body}");
    return jsonDecode(response.body);
  }

  // RESERVATIONS
  static Future<Map<String, dynamic>> createReservation({
    required int eventId,
    required int eventTicketTypeId,
    required int quantity,
    required String token,
  }) async {
    final response = await http.post(
      Uri.parse("$baseUrl/Reservations"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({
        "eventId": eventId,
        "eventTicketTypeId": eventTicketTypeId,
        "quantity": quantity
      }),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Reservation failed: ${response.body}");
    return jsonDecode(response.body);
  }

  static Future<List<dynamic>> getMyReservations(
      {required String token}) async {
    final response = await http.get(
      Uri.parse("$baseUrl/Reservations/my"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode == 200) return jsonDecode(response.body);
    return [];
  }

  // PAYMENTS
  static Future<Map<String, dynamic>> confirmPayment({
    required int reservationId,
    required String method,
    required String token,
  }) async {
    final response = await http.post(
      Uri.parse("$baseUrl/payments/confirm"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body:
          jsonEncode({"reservationId": reservationId, "paymentMethod": method}),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Payment failed: ${response.body}");
    return jsonDecode(response.body);
  }

  // TICKETS — korisnik koristi /my, ne admin listu
  static Future<List<dynamic>> getMyTickets(String token) async {
    final response = await http.get(
      Uri.parse("$baseUrl/Tickets/my"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode == 200) return jsonDecode(response.body);
    throw Exception("Failed to load tickets");
  }

  // TICKET TYPES
  static Future<List<dynamic>> getTicketTypes({
    required int eventId,
    required String token,
  }) async {
    final response = await http.get(
      Uri.parse("$baseUrl/eventtickettype?eventId=$eventId"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode != 200)
      throw Exception("Failed to load ticket types");
    return jsonDecode(response.body);
  }

  static Future<Map<String, dynamic>> createTicketType({
    required int eventId,
    required String name,
    required double price,
    required int availableQuantity,
    required String token,
  }) async {
    final response = await http.post(
      Uri.parse("$baseUrl/eventtickettype"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({
        "eventId": eventId,
        "name": name,
        "price": price,
        "availableQuantity": availableQuantity,
      }),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Failed to create ticket type: ${response.body}");
    return jsonDecode(response.body);
  }

  static Future<void> updateTicketType({
    required int id,
    required int eventId,
    required String name,
    required double price,
    required int availableQuantity,
    required String token,
  }) async {
    final response = await http.put(
      Uri.parse("$baseUrl/EventTicketType/$id"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({
        "eventId": eventId,
        "name": name,
        "price": price,
        "availableQuantity": availableQuantity,
      }),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Failed to update ticket type");
  }

  static Future<void> deleteTicketType(
      {required int id, required String token}) async {
    final response = await http.delete(
      Uri.parse("$baseUrl/EventTicketType/$id"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode != 204)
      throw Exception("Failed to delete ticket type");
  }

  // RECOMMENDATIONS — /my endpoint, userId iz JWT
  static Future<List<dynamic>> getRecommendations({
    required int userId,
    required String token,
  }) async {
    final response = await http.get(
      Uri.parse("$baseUrl/Recommendations/my"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode != 200)
      throw Exception("Failed to load recommendations");
    return jsonDecode(response.body);
  }

  // REVIEWS
  static Future<List<dynamic>> getEligibleEvents(String token) async {
    final response = await http.get(
      Uri.parse("$baseUrl/reviews/eligible-events"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode != 200)
      throw Exception("Failed to load eligible events");
    return jsonDecode(response.body);
  }

  static Future<void> submitReview({
    required int eventId,
    required int rating,
    required String comment,
    required String token,
  }) async {
    final response = await http.post(
      Uri.parse("$baseUrl/reviews"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({
        "eventId": eventId,
        "rating": rating,
        "comment": comment,
      }),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Failed to submit review: ${response.body}");
  }

  // ANNOUNCEMENTS — /my endpoint, userId iz JWT
  static Future<List<dynamic>> getMyAnnouncements(
      {required String token}) async {
    final response = await http.get(
      Uri.parse("$baseUrl/Announcement/my"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode == 200) return jsonDecode(response.body);
    return [];
  }

  static Future<List<dynamic>> getAnnouncements({
    required int eventId,
    required String token,
  }) async {
    final response = await http.get(
      Uri.parse("$baseUrl/Announcement/event/$eventId"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode == 200) return jsonDecode(response.body);
    return [];
  }

  static Future<void> createAnnouncement({
    required int eventId,
    required String title,
    required String content,
    required String token,
  }) async {
    final response = await http.post(
      Uri.parse("$baseUrl/Announcement"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body:
          jsonEncode({"eventId": eventId, "title": title, "content": content}),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Failed to create announcement: ${response.body}");
  }

  // INVITATIONS
  static Future<List<dynamic>> getMyInvitations({required String token}) async {
    final userId = await AuthService.getUserId();
    if (userId == null) return [];
    final response = await http.get(
      Uri.parse("$baseUrl/Invitation?receiverId=$userId"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode == 200) return jsonDecode(response.body);
    return [];
  }

  static Future<void> createInvitation({
    required int eventId,
    required int receiverId,
    required String token,
  }) async {
    final response = await http.post(
      Uri.parse("$baseUrl/Invitation"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({"eventId": eventId, "receiverId": receiverId}),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Failed to send invitation: ${response.body}");
  }

  static Future<void> respondToInvitation({
    required int invitationId,
    required String action,
    required String token,
  }) async {
    final response = await http.put(
      Uri.parse("$baseUrl/Invitation/$invitationId/$action"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Failed to $action invitation: ${response.body}");
  }

  // NOTIFICATIONS
  static Future<List<dynamic>> getMyNotifications(String token) async {
    final response = await http.get(
      Uri.parse("$baseUrl/notifications/my"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode != 200)
      throw Exception("Failed to load notifications");
    return jsonDecode(response.body);
  }

  static Future<void> clearMyNotifications(String token) async {
    final response = await http.delete(
      Uri.parse("$baseUrl/notifications/my/clear"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode != 204)
      throw Exception("Failed to clear notifications");
  }

  // EVENT IMAGES
  static Future<void> addEventImage({
    required int eventId,
    required String imageUrl,
    required String token,
  }) async {
    final response = await http.post(
      Uri.parse("$baseUrl/eventimages"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({"eventId": eventId, "imageUrl": imageUrl}),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Failed to add image: ${response.body}");
  }

  static Future<List<dynamic>> getEventImages({
    required int eventId,
    required String token,
  }) async {
    final response = await http.get(
      Uri.parse("$baseUrl/eventimages/by-event/$eventId"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode != 200) throw Exception("Failed to load images");
    return jsonDecode(response.body);
  }

  static Future<void> updateEventImage({
    required int id,
    required String imageUrl,
    required String token,
  }) async {
    final response = await http.put(
      Uri.parse("$baseUrl/eventimages/$id"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({"imageUrl": imageUrl}),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Failed to update image");
  }

  // CATEGORIES & LOCATIONS
  static Future<List<dynamic>> getCategories({required String token}) async {
    final response = await http.get(
      Uri.parse("$baseUrl/eventcategories"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode != 200)
      throw Exception("Failed to load categories");
    return jsonDecode(response.body);
  }

  static Future<List<dynamic>> getLocations({required String token}) async {
    final response = await http.get(
      Uri.parse("$baseUrl/locations"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode != 200) throw Exception("Failed to load locations");
    return jsonDecode(response.body);
  }

  // USERS
  static Future<List<dynamic>> getUsers({required String token}) async {
    final response = await http.get(
      Uri.parse("$baseUrl/Users"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode == 200) return jsonDecode(response.body);
    return [];
  }

  // EVENT VIEWS — userId iz JWT, ne iz requesta
  static Future<void> recordEventView({
    required int eventId,
    required String token,
  }) async {
    await http.post(
      Uri.parse("$baseUrl/EventViews"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({"eventId": eventId}),
    );
  }

  static Future<void> markNotificationAsRead(
      {required int id, required String token}) async {
    await http.put(
      Uri.parse("$baseUrl/notifications/$id/mark-read"),
      headers: {"Authorization": "Bearer $token"},
    );
  }

  static Future<void> markAllNotificationsAsRead(
      {required String token}) async {
    await http.put(
      Uri.parse("$baseUrl/notifications/my/mark-all-read"),
      headers: {"Authorization": "Bearer $token"},
    );
  }

  // Dodaj ove metode u ApiService.dart

  static Future<Map<String, dynamic>> getMyProfile(
      {required String token}) async {
    final response = await http.get(
      Uri.parse("$baseUrl/Users/my"),
      headers: {"Authorization": "Bearer $token"},
    );
    if (response.statusCode == 200) return jsonDecode(response.body);
    throw Exception("Failed to load profile");
  }

  static Future<void> updateMyProfile({
    required String username,
    required String email,
    required String token,
  }) async {
    final response = await http.put(
      Uri.parse("$baseUrl/Users/my"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({"username": username, "email": email}),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Failed to update profile: ${response.body}");
  }

  static Future<void> changePassword({
    required String currentPassword,
    required String newPassword,
    required String token,
  }) async {
    final response = await http.post(
      Uri.parse("$baseUrl/Auth/change-password"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode(
          {"currentPassword": currentPassword, "newPassword": newPassword}),
    );
    if (response.statusCode < 200 || response.statusCode >= 300)
      throw Exception("Failed to change password: ${response.body}");
  }

  static Future<String> createPayPalOrder(
      {required int reservationId, required String token}) async {
    final response = await http.post(
      Uri.parse("$baseUrl/payments/paypal/create-order"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({"reservationId": reservationId}),
    );
    if (response.statusCode != 200) throw Exception("Failed: ${response.body}");
    return jsonDecode(response.body)["approvalUrl"];
  }

  static Future<void> capturePayPalOrder(
      {required String orderId,
      required int reservationId,
      required String token}) async {
    final response = await http.post(
      Uri.parse("$baseUrl/payments/paypal/capture"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({"orderId": orderId, "reservationId": reservationId}),
    );
    if (response.statusCode != 200)
      throw Exception("Capture failed: ${response.body}");
  }

  static Future<Map<String, dynamic>> createStripePaymentIntent({
    required int reservationId,
    required String token,
  }) async {
    final response = await http.post(
      Uri.parse("$baseUrl/payments/stripe/create-intent"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({"reservationId": reservationId}),
    );
    if (response.statusCode != 200) throw Exception("Failed: ${response.body}");
    return jsonDecode(response.body);
  }

  static Future<void> confirmStripePayment({
    required String paymentIntentId,
    required int reservationId,
    required String token,
  }) async {
    final response = await http.post(
      Uri.parse("$baseUrl/payments/stripe/confirm"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode(
          {"paymentIntentId": paymentIntentId, "reservationId": reservationId}),
    );
    if (response.statusCode != 200)
      throw Exception("Confirm failed: ${response.body}");
  }
}

import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/event_dto.dart';

class ApiService {
  // za Flutter web
  static const String baseUrl = "http://localhost:5000/api";

  // LOGIN
  static Future<Map<String, dynamic>> login(
    String email,
    String password,
  ) async {
    final response = await http.post(
      Uri.parse("$baseUrl/auth/login"),
      headers: {"Content-Type": "application/json"},
      body: jsonEncode({"email": email, "password": password}),
    );

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception("Login failed: ${response.body}");
    }

    return jsonDecode(response.body);
  }

  // EVENTS
  static Future<List<EventDto>> getEvents(String token) async {
    final response = await http.get(
      Uri.parse("$baseUrl/events"),
      headers: {"Authorization": "Bearer $token"},
    );

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception("Loading events failed: ${response.body}");
    }

    final data = jsonDecode(response.body);

    // 🔥 Tvoj backend koristi "items"
    if (data is Map<String, dynamic> && data["items"] is List) {
      return (data["items"] as List).map((e) => EventDto.fromJson(e)).toList();
    }

    throw Exception("Unexpected events response format.");
  }

  // EVENT DETAILS (koristi se kasnije)
  static Future<Map<String, dynamic>> getEventDetails(
      int id, String token) async {
    final response = await http.get(
      Uri.parse("$baseUrl/events/$id"),
      headers: {"Authorization": "Bearer $token"},
    );

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception("Loading event failed");
    }

    return jsonDecode(response.body);
  }

  // RESERVATION
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

    if (response.statusCode < 200 || response.statusCode >= 300) {
      print("STATUS: ${response.statusCode}");
      print("BODY: ${response.body}");
      throw Exception("Reservation failed: ${response.body}");
    }

    return jsonDecode(response.body);
  }

  // PAYMENT (Card ili PayPal)
  static Future<Map<String, dynamic>> createPayment({
    required int reservationId,
    required double amount,
    required String paymentMethod,
    required String token,
  }) async {
    final response = await http.post(
      Uri.parse("$baseUrl/Payments"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer $token"
      },
      body: jsonEncode({
        "reservationId": reservationId,
        "amount": amount,
        "paymentMethod": paymentMethod
      }),
    );

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception("Payment failed: ${response.body}");
    }

    return jsonDecode(response.body);
  }

  // TICKETS
  static Future<List<dynamic>> getTickets(String token) async {
    final response = await http.get(
      Uri.parse("$baseUrl/tickets"),
      headers: {"Authorization": "Bearer $token"},
    );

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception("Tickets failed");
    }

    return jsonDecode(response.body);
  }

  // RECOMMENDATION
  static Future<List<dynamic>> getRecommendedEvents(
      int userId, String token) async {
    final response = await http.get(
      Uri.parse("$baseUrl/recommendation/$userId"),
      headers: {"Authorization": "Bearer $token"},
    );

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception("Recommendation failed");
    }

    return jsonDecode(response.body);
  }

  static Future<List<dynamic>> getTicketTypes(
    int eventId,
    String token,
  ) async {
    final response = await http.get(
      Uri.parse("$baseUrl/eventtickettype?eventId=$eventId"),
      headers: {
        "Authorization": "Bearer $token",
      },
    );

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception("Failed to load ticket types: ${response.body}");
    }

    return jsonDecode(response.body);
  }

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

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception("Register failed: ${response.body}");
    }

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
        "Authorization": "Bearer $token",
      },
      body: jsonEncode({
        "name": name,
        "description": description,
        "eventDate": eventDate.toIso8601String(),
        "eventCategoryId": eventCategoryId,
        "locationId": locationId,
      }),
    );

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception("Create event failed: ${response.body}");
    }

    return jsonDecode(response.body);
  }
}

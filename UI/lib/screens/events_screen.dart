import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../models/event_dto.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';

class EventsScreen extends StatefulWidget {
  const EventsScreen({super.key});

  @override
  State<EventsScreen> createState() => _EventsScreenState();
}

class _EventsScreenState extends State<EventsScreen> {
  List<EventDto> events = [];
  bool isLoading = true;
  String? errorMessage;

  @override
  void initState() {
    super.initState();
    loadEvents();
  }

  Future<void> loadEvents() async {
    try {
      final token = await AuthService.getToken();
      if (token == null) {
        throw Exception("User is not logged in.");
      }

      final result = await ApiService.getEvents(token);

      setState(() {
        events = result;
        isLoading = false;
      });
    } catch (e) {
      setState(() {
        errorMessage = e.toString();
        isLoading = false;
      });
    }
  }

  Future<void> reserveAndPay(EventDto event) async {
    try {
      final token = await AuthService.getToken();
      if (token == null) {
        throw Exception("User is not logged in.");
      }

      // privremeno hardcoded ticket type id = 1
      final reservation = await ApiService.createReservation(
        eventId: event.id,
        eventTicketTypeId: 1,
        quantity: 1,
        token: token,
      );

      final reservationId = reservation["id"] as int;

      await ApiService.createPayment(
        reservationId: reservationId,
        amount: 20,
        paymentMethod: "Card",
        token: token,
      );

      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text("Reservation and payment completed for ${event.name}"),
        ),
      );
    } catch (e) {
      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text("Error: $e"),
        ),
      );
    }
  }

  Future<void> logout() async {
    await AuthService.clearToken();
    if (!mounted) return;
    Navigator.pushReplacementNamed(context, "/");
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text("Events"),
        actions: [
          IconButton(
            onPressed: logout,
            icon: const Icon(Icons.logout),
          )
        ],
      ),
      body: isLoading
          ? const Center(child: CircularProgressIndicator())
          : errorMessage != null
              ? Center(child: Text(errorMessage!))
              : ListView.builder(
                  itemCount: events.length,
                  itemBuilder: (context, index) {
                    final event = events[index];

                    return Card(
                      margin: const EdgeInsets.symmetric(
                        horizontal: 12,
                        vertical: 8,
                      ),
                      child: Padding(
                        padding: const EdgeInsets.all(12),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              event.name,
                              style: const TextStyle(
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                            const SizedBox(height: 8),
                            Text(event.description),
                            const SizedBox(height: 8),
                            Text(
                              DateFormat("dd.MM.yyyy HH:mm")
                                  .format(event.eventDate),
                            ),
                            const SizedBox(height: 12),
                            ElevatedButton(
                              onPressed: () {
                                Navigator.pushNamed(
                                  context,
                                  "/event-details",
                                  arguments: event,
                                );
                              },
                              child: const Text("Reserve & Pay"),
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                ),
    );
  }
}

import 'package:flutter/material.dart';
import '../models/event_dto.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';
import 'tickets_screen.dart'; // 🔥 DODANO

class EventDetailsScreen extends StatefulWidget {
  const EventDetailsScreen({super.key});

  @override
  State<EventDetailsScreen> createState() => _EventDetailsScreenState();
}

class _EventDetailsScreenState extends State<EventDetailsScreen> {
  List ticketTypes = [];
  int? selectedTicketTypeId;

  String selectedPaymentMethod = "Card";
  bool isLoading = false;
  bool isLoadingTickets = true;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    loadTicketTypes();
  }

  Future<void> loadTicketTypes() async {
    try {
      final event = ModalRoute.of(context)!.settings.arguments as EventDto;

      final token = await AuthService.getToken();

      final data = await ApiService.getTicketTypes(
        event.id,
        token!,
      );

      setState(() {
        ticketTypes = data;
        selectedTicketTypeId = data.isNotEmpty ? data[0]["id"] : null;
        isLoadingTickets = false;
      });
    } catch (e) {
      setState(() {
        isLoadingTickets = false;
      });

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Error loading tickets: $e")),
      );
    }
  }

  Future<void> reserveAndPay(EventDto event) async {
    if (selectedTicketTypeId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Select ticket type")),
      );
      return;
    }

    setState(() {
      isLoading = true;
    });

    try {
      final token = await AuthService.getToken();

      if (token == null) {
        throw Exception("User not logged in");
      }

      final reservation = await ApiService.createReservation(
        eventId: event.id,
        eventTicketTypeId: selectedTicketTypeId!,
        quantity: 1,
        token: token,
      );

      final reservationId = reservation["id"];

      final selectedTicket = ticketTypes.firstWhere(
        (t) => t["id"] == selectedTicketTypeId,
      );

      await ApiService.createPayment(
        reservationId: reservationId,
        amount: (selectedTicket["price"] as num).toDouble(),
        paymentMethod: selectedPaymentMethod,
        token: token,
      );

      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text("🎉 Ticket successfully created!"),
        ),
      );

      // ✅ DIREKTNA NAVIGACIJA (ISPRAVNO)
      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (context) => const TicketsScreen(),
        ),
      );
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Error: $e")),
      );
    } finally {
      if (mounted) {
        setState(() {
          isLoading = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final event = ModalRoute.of(context)!.settings.arguments as EventDto;

    return Scaffold(
      appBar: AppBar(
        title: Text(event.name),
      ),
      body: isLoadingTickets
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    event.name,
                    style: const TextStyle(
                      fontSize: 22,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 10),
                  Text(
                    event.description,
                    style: const TextStyle(fontSize: 16),
                  ),
                  const SizedBox(height: 30),

                  // 🎟️ TICKET TYPE
                  const Text(
                    "Select Ticket Type",
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 16,
                    ),
                  ),
                  const SizedBox(height: 10),
                  DropdownButtonFormField<int>(
                    value: selectedTicketTypeId,
                    decoration: const InputDecoration(
                      border: OutlineInputBorder(),
                    ),
                    items: ticketTypes.map((t) {
                      return DropdownMenuItem<int>(
                        value: t["id"],
                        child: Text("${t["name"]} - ${t["price"]} KM"),
                      );
                    }).toList(),
                    onChanged: (value) {
                      setState(() {
                        selectedTicketTypeId = value!;
                      });
                    },
                  ),

                  const SizedBox(height: 30),

                  // 💳 PAYMENT
                  const Text(
                    "Payment Method",
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 16,
                    ),
                  ),
                  const SizedBox(height: 10),
                  DropdownButtonFormField<String>(
                    value: selectedPaymentMethod,
                    decoration: const InputDecoration(
                      border: OutlineInputBorder(),
                    ),
                    items: const [
                      DropdownMenuItem(
                        value: "Card",
                        child: Text("💳 Card"),
                      ),
                      DropdownMenuItem(
                        value: "PayPal",
                        child: Text("🟡 PayPal"),
                      ),
                    ],
                    onChanged: (value) {
                      setState(() {
                        selectedPaymentMethod = value!;
                      });
                    },
                  ),

                  const SizedBox(height: 40),

                  // 🔘 RESERVE BUTTON
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: isLoading ? null : () => reserveAndPay(event),
                      child: isLoading
                          ? const CircularProgressIndicator(
                              color: Colors.white,
                            )
                          : const Text("Reserve & Pay"),
                    ),
                  ),

                  const SizedBox(height: 15),

                  // 🔘 VIEW TICKETS (FIXED)
                  SizedBox(
                    width: double.infinity,
                    child: OutlinedButton(
                      onPressed: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (context) => const TicketsScreen(),
                          ),
                        );
                      },
                      child: const Text("View My Tickets 🎟️"),
                    ),
                  ),
                ],
              ),
            ),
    );
  }
}

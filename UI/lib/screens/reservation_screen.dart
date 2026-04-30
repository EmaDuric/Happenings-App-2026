import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../models/event_dto.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';
import 'payment_screen.dart';

class ReservationScreen extends StatefulWidget {
  final EventDto event;
  final bool goToPayment;

  const ReservationScreen(
      {super.key, required this.event, this.goToPayment = true});

  @override
  State<ReservationScreen> createState() => _ReservationScreenState();
}

class _ReservationScreenState extends State<ReservationScreen> {
  int quantity = 1;
  bool isLoading = false;
  bool isLoadingTicketTypes = true;

  final nameController = TextEditingController();
  final phoneController = TextEditingController();
  final emailController = TextEditingController();

  int? selectedTicketTypeId;
  List ticketTypes = [];

  @override
  void initState() {
    super.initState();
    loadTicketTypes();
  }

  Future<void> loadTicketTypes() async {
    try {
      final token = await AuthService.getToken();
      if (token == null) throw Exception("User not logged in");
      final result = await ApiService.getTicketTypes(
          eventId: widget.event.id, token: token);
      setState(() {
        ticketTypes = result;
        selectedTicketTypeId =
            ticketTypes.isNotEmpty ? ticketTypes.first["id"] : null;
        isLoadingTicketTypes = false;
      });
    } catch (e) {
      setState(() => isLoadingTicketTypes = false);
      ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("Error loading ticket types: $e")));
    }
  }

  double get selectedTicketPrice {
    if (selectedTicketTypeId == null) return 0;
    final selected = ticketTypes
        .firstWhere((t) => t["id"] == selectedTicketTypeId, orElse: () => null);
    if (selected == null) return 0;
    return (selected["price"] as num).toDouble();
  }

  double get total => selectedTicketPrice * quantity;

  Future<void> reserve() async {
    final name = nameController.text.trim();
    final phone = phoneController.text.trim();
    final email = emailController.text.trim();

    if (name.isEmpty || phone.isEmpty || email.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("All fields are required.")));
      return;
    }
    if (!RegExp(r'^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$').hasMatch(email)) {
      ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Please enter a valid email address.")));
      return;
    }
    if (!RegExp(r'^\+?[0-9]{8,15}$').hasMatch(phone)) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
          content:
              Text("Please enter a valid phone number (e.g. +38761123456).")));
      return;
    }
    if (selectedTicketTypeId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Please select ticket type")));
      return;
    }

    setState(() => isLoading = true);

    try {
      final token = await AuthService.getToken();
      if (token == null) throw Exception("User not logged in");

      final reservation = await ApiService.createReservation(
        eventId: widget.event.id,
        eventTicketTypeId: selectedTicketTypeId!,
        quantity: quantity,
        token: token,
      );

      final reservationId = reservation["id"];
      if (!mounted) return;

      if (widget.goToPayment) {
        Navigator.push(
            context,
            MaterialPageRoute(
                builder: (_) => PaymentScreen(reservationId: reservationId)));
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
              content: Text(
                  "Reservation created successfully! You can pay later from My Reservations.")),
        );
        Navigator.pop(context);
      }
    } catch (e) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text("Reservation failed: $e")));
    } finally {
      if (mounted) setState(() => isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final formattedDate =
        DateFormat("dd.MM.yyyy HH:mm").format(widget.event.eventDate);

    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      appBar: AppBar(
        title: Text(widget.goToPayment ? "Reserve & Pay" : "Reserve"),
        backgroundColor: const Color(0xFFF4D35E),
        foregroundColor: Colors.black,
        elevation: 0,
      ),
      body: isLoadingTicketTypes
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Column(
                children: [
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(18)),
                    child: Row(
                      children: [
                        Container(
                          width: 90,
                          height: 70,
                          decoration: BoxDecoration(
                              color: Colors.grey.shade300,
                              borderRadius: BorderRadius.circular(12)),
                          child: const Icon(Icons.event, size: 34),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(widget.event.name,
                                  style: const TextStyle(
                                      fontSize: 18,
                                      fontWeight: FontWeight.bold)),
                              const SizedBox(height: 6),
                              Text(formattedDate),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 20),
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(18)),
                    child: Column(
                      children: [
                        TextField(
                            controller: nameController,
                            decoration: const InputDecoration(
                                labelText: "Name",
                                border: OutlineInputBorder())),
                        const SizedBox(height: 12),
                        TextField(
                            controller: phoneController,
                            decoration: const InputDecoration(
                                labelText: "Mobile phone",
                                border: OutlineInputBorder())),
                        const SizedBox(height: 12),
                        TextField(
                            controller: emailController,
                            decoration: const InputDecoration(
                                labelText: "Email",
                                border: OutlineInputBorder())),
                      ],
                    ),
                  ),
                  const SizedBox(height: 20),
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(18)),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        const Text("Choose ticket type",
                            style: TextStyle(fontWeight: FontWeight.bold)),
                        const SizedBox(height: 12),
                        if (ticketTypes.isEmpty)
                          const Text("No ticket types available")
                        else
                          DropdownButtonFormField<int>(
                            value: selectedTicketTypeId,
                            decoration: const InputDecoration(
                                labelText: "Ticket type",
                                border: OutlineInputBorder()),
                            items: ticketTypes
                                .map<
                                    DropdownMenuItem<
                                        int>>((type) => DropdownMenuItem<int>(
                                    value: type["id"],
                                    child: Text(
                                        "${type["name"]} - ${type["price"]} KM")))
                                .toList(),
                            onChanged: (value) =>
                                setState(() => selectedTicketTypeId = value),
                          ),
                        const SizedBox(height: 20),
                        const Text("Choose quantity",
                            style: TextStyle(fontWeight: FontWeight.bold)),
                        const SizedBox(height: 12),
                        Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            IconButton(
                                onPressed: quantity > 1
                                    ? () => setState(() => quantity--)
                                    : null,
                                icon: const Icon(Icons.remove_circle_outline)),
                            Text(quantity.toString(),
                                style: const TextStyle(
                                    fontSize: 22, fontWeight: FontWeight.bold)),
                            IconButton(
                                onPressed: () => setState(() => quantity++),
                                icon: const Icon(Icons.add_circle_outline)),
                          ],
                        ),
                        const SizedBox(height: 12),
                        Text("Estimated total: ${total.toStringAsFixed(2)} KM",
                            textAlign: TextAlign.center,
                            style:
                                const TextStyle(fontWeight: FontWeight.bold)),
                      ],
                    ),
                  ),
                  const SizedBox(height: 24),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed:
                          isLoading || ticketTypes.isEmpty ? null : reserve,
                      style: ElevatedButton.styleFrom(
                        backgroundColor:
                            widget.goToPayment ? Colors.green : Colors.orange,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 16),
                      ),
                      child: isLoading
                          ? const CircularProgressIndicator(color: Colors.white)
                          : Text(widget.goToPayment
                              ? "CONTINUE TO PAYMENT"
                              : "RESERVE"),
                    ),
                  ),
                ],
              ),
            ),
    );
  }
}

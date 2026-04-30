import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';
import 'payment_screen.dart';

class MyReservationsScreen extends StatefulWidget {
  const MyReservationsScreen({super.key});

  @override
  State<MyReservationsScreen> createState() => _MyReservationsScreenState();
}

class _MyReservationsScreenState extends State<MyReservationsScreen> {
  List<dynamic> reservations = [];
  bool isLoading = true;

  @override
  void initState() {
    super.initState();
    loadReservations();
  }

  Future<void> loadReservations() async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return;
      final result = await ApiService.getMyReservations(token: token);
      setState(() {
        reservations = result;
        isLoading = false;
      });
    } catch (e) {
      setState(() => isLoading = false);
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text("Error: $e")));
    }
  }

  Color _statusColor(String? status) {
    switch (status?.toLowerCase()) {
      case "approved":
        return Colors.green;
      case "rejected":
        return Colors.red;
      case "cancelled":
        return Colors.grey;
      default:
        return Colors.orange;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      appBar: AppBar(
        title: const Text("My Reservations"),
        backgroundColor: const Color(0xFFF4D35E),
        foregroundColor: Colors.black,
        elevation: 0,
      ),
      body: isLoading
          ? const Center(child: CircularProgressIndicator())
          : reservations.isEmpty
              ? const Center(child: Text("No reservations yet"))
              : ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: reservations.length,
                  itemBuilder: (context, index) {
                    final r = reservations[index];
                    final status = r["status"] ?? "Pending";
                    final isPending = status.toLowerCase() == "pending";
                    final eventDate = r["eventDate"] != null
                        ? DateFormat("dd.MM.yyyy")
                            .format(DateTime.parse(r["eventDate"]))
                        : "";

                    return Container(
                      margin: const EdgeInsets.only(bottom: 14),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(18),
                        boxShadow: [
                          BoxShadow(
                              color: Colors.black12,
                              blurRadius: 6,
                              offset: const Offset(0, 3))
                        ],
                      ),
                      child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              children: [
                                Expanded(
                                  child: Text(r["eventName"] ?? "Event",
                                      style: const TextStyle(
                                          fontSize: 18,
                                          fontWeight: FontWeight.bold)),
                                ),
                                Container(
                                  padding: const EdgeInsets.symmetric(
                                      horizontal: 10, vertical: 4),
                                  decoration: BoxDecoration(
                                    color: _statusColor(status),
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                  child: Text(status,
                                      style: const TextStyle(
                                          color: Colors.white,
                                          fontSize: 12,
                                          fontWeight: FontWeight.bold)),
                                ),
                              ],
                            ),
                            const SizedBox(height: 8),
                            Row(children: [
                              const Icon(Icons.calendar_today,
                                  size: 14, color: Colors.black54),
                              const SizedBox(width: 6),
                              Text(eventDate,
                                  style:
                                      const TextStyle(color: Colors.black54)),
                            ]),
                            const SizedBox(height: 4),
                            Row(children: [
                              const Icon(Icons.confirmation_number,
                                  size: 14, color: Colors.black54),
                              const SizedBox(width: 6),
                              Text(
                                  "${r["ticketTypeName"] ?? "Ticket"} × ${r["quantity"]}",
                                  style:
                                      const TextStyle(color: Colors.black54)),
                            ]),
                            const SizedBox(height: 4),
                            Row(children: [
                              const Icon(Icons.access_time,
                                  size: 14, color: Colors.black54),
                              const SizedBox(width: 6),
                              Text(
                                  "Reserved: ${r["reservedAt"] != null ? DateFormat("dd.MM.yyyy HH:mm").format(DateTime.parse(r["reservedAt"])) : ""}",
                                  style: const TextStyle(
                                      color: Colors.black54, fontSize: 12)),
                            ]),
                            if (isPending) ...[
                              const SizedBox(height: 12),
                              SizedBox(
                                width: double.infinity,
                                child: ElevatedButton.icon(
                                  onPressed: () => Navigator.push(
                                    context,
                                    MaterialPageRoute(
                                        builder: (_) => PaymentScreen(
                                            reservationId: r["id"])),
                                  ).then((_) => loadReservations()),
                                  icon: const Icon(Icons.payment),
                                  label: const Text("PAY NOW"),
                                  style: ElevatedButton.styleFrom(
                                    backgroundColor: Colors.green,
                                    foregroundColor: Colors.white,
                                    padding: const EdgeInsets.symmetric(
                                        vertical: 12),
                                    shape: RoundedRectangleBorder(
                                        borderRadius:
                                            BorderRadius.circular(12)),
                                  ),
                                ),
                              ),
                            ],
                          ],
                        ),
                      ),
                    );
                  },
                ),
    );
  }
}

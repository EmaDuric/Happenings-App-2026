import 'package:flutter/material.dart';
import 'package:qr_flutter/qr_flutter.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';

class TicketsScreen extends StatefulWidget {
  const TicketsScreen({super.key});

  @override
  State<TicketsScreen> createState() => _TicketsScreenState();
}

class _TicketsScreenState extends State<TicketsScreen> {
  List tickets = [];
  bool isLoading = true;

  @override
  void initState() {
    super.initState();
    print("TICKETS SCREEN OPENED");
    loadTickets();
  }

  Future<void> loadTickets() async {
    try {
      final token = await AuthService.getToken();

      if (token == null) {
        throw Exception("User not logged in");
      }

      print("CALLING /tickets API...");

      final result = await ApiService.getTickets(token);

      print("TICKETS RESULT: $result");

      setState(() {
        tickets = result;
        isLoading = false;
      });
    } catch (e) {
      print("ERROR: $e");

      setState(() {
        isLoading = false;
      });

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("$e")),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text("My Tickets"),
      ),
      body: isLoading
          ? const Center(child: CircularProgressIndicator())
          : tickets.isEmpty
              ? const Center(
                  child: Text("No tickets found"),
                )
              : ListView.builder(
                  itemCount: tickets.length,
                  itemBuilder: (context, i) {
                    final ticket = tickets[i];

                    return Card(
                      margin: const EdgeInsets.all(10),
                      child: Padding(
                        padding: const EdgeInsets.all(12),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              "Ticket #${ticket["id"]}",
                              style: const TextStyle(
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                            const SizedBox(height: 10),

                            // 🔥 QR FIX (safe)
                            QrImageView(
                              data: ticket["qrCode"] ?? ticket["id"].toString(),
                              version: QrVersions.auto,
                              size: 200,
                            ),

                            const SizedBox(height: 10),

                            // 🔥 STATUS SAFE
                            Text(
                              (ticket["isUsed"] ?? false)
                                  ? "USED ❌"
                                  : "VALID ✅",
                              style: TextStyle(
                                color: (ticket["isUsed"] ?? false)
                                    ? Colors.red
                                    : Colors.green,
                              ),
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

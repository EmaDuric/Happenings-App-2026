import 'package:flutter/material.dart';
import 'package:qr_flutter/qr_flutter.dart';

class TicketDetailsScreen extends StatelessWidget {
  final dynamic ticket;

  const TicketDetailsScreen({super.key, required this.ticket});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      appBar: AppBar(
        backgroundColor: const Color(0xFFF4D35E),
        title: const Text("Your Ticket"),
      ),
      body: Center(
        child: Container(
          width: 350,
          padding: const EdgeInsets.all(20),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(20),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(
                ticket["eventName"] ?? "",
                style: const TextStyle(
                  fontSize: 22,
                  fontWeight: FontWeight.bold,
                ),
                textAlign: TextAlign.center,
              ),

              const SizedBox(height: 10),

              Text(ticket["location"] ?? ""),

              const SizedBox(height: 5),

              Text(ticket["eventDate"] ?? ""),

              const SizedBox(height: 20),

              // 🔥 BIG QR
              QrImageView(
                data: ticket["qrCode"] ?? ticket["id"].toString(),
                size: 220,
              ),

              const SizedBox(height: 20),

              Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 20,
                  vertical: 8,
                ),
                decoration: BoxDecoration(
                  color:
                      (ticket["isUsed"] ?? false) ? Colors.red : Colors.green,
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Text(
                  (ticket["isUsed"] ?? false) ? "USED" : "VALID",
                  style: const TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

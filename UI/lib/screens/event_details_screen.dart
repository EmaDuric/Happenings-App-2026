import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../models/event_dto.dart';
import '../services/auth_service.dart';
import '../services/api_service.dart';
import 'reservation_screen.dart';
import 'edit_event_screen.dart';

class EventDetailsScreen extends StatefulWidget {
  final EventDto event;

  const EventDetailsScreen({super.key, required this.event});

  @override
  State<EventDetailsScreen> createState() => _EventDetailsScreenState();
}

class _EventDetailsScreenState extends State<EventDetailsScreen> {
  bool isOrganizer = false;
  List<dynamic> announcements = [];

  @override
  void initState() {
    super.initState();
    checkRole();
    loadAnnouncements();
  }

  Future<void> checkRole() async {
    final org = await AuthService.isOrganizer();
    setState(() => isOrganizer = org);
  }

  Future<void> loadAnnouncements() async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return;
      final result = await ApiService.getAnnouncements(
        eventId: widget.event.id,
        token: token,
      );
      setState(() => announcements = result);
    } catch (_) {}
  }

  @override
  Widget build(BuildContext context) {
    final event = widget.event;

    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      body: CustomScrollView(
        slivers: [
          SliverAppBar(
            expandedHeight: 280,
            pinned: true,
            backgroundColor: const Color(0xFFF4D35E),
            foregroundColor: Colors.black,
            actions: [
              if (isOrganizer)
                IconButton(
                  icon: const Icon(Icons.edit, color: Colors.black),
                  onPressed: () async {
                    final updated = await Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (_) => EditEventScreen(event: event),
                      ),
                    );
                    if (updated == true && mounted) {
                      Navigator.pop(context, true);
                    }
                  },
                ),
            ],
            flexibleSpace: FlexibleSpaceBar(
              background: event.imageUrl != null && event.imageUrl!.isNotEmpty
                  ? Image.network(
                      event.imageUrl!,
                      fit: BoxFit.cover,
                      errorBuilder: (_, __, ___) => _placeholder(),
                    )
                  : _placeholder(),
            ),
          ),
          SliverToBoxAdapter(
            child: Container(
              decoration: const BoxDecoration(color: Color(0xFFF4D35E)),
              padding: const EdgeInsets.all(20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  if (event.categoryName != null)
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 12, vertical: 6),
                      decoration: BoxDecoration(
                        color: Colors.black,
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Text(
                        event.categoryName!,
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 12,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                  const SizedBox(height: 12),
                  Text(
                    event.name,
                    style: const TextStyle(
                        fontSize: 28, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 20),
                  Row(
                    children: [
                      _infoCard(
                        icon: Icons.calendar_today,
                        label: "Date",
                        value: DateFormat("dd.MM.yyyy").format(event.eventDate),
                      ),
                      const SizedBox(width: 12),
                      _infoCard(
                        icon: Icons.access_time,
                        label: "Time",
                        value: DateFormat("HH:mm").format(event.eventDate),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),
                  if (event.locationName != null)
                    _infoCard(
                      icon: Icons.location_on,
                      label: "Location",
                      value: event.locationName!,
                      fullWidth: true,
                    ),
                  const SizedBox(height: 24),
                  const Text(
                    "About this event",
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 10),
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(16),
                    ),
                    child: Text(
                      event.description,
                      style: const TextStyle(
                          fontSize: 15, height: 1.6, color: Colors.black87),
                    ),
                  ),

                  // ANNOUNCEMENTS
                  if (announcements.isNotEmpty) ...[
                    const SizedBox(height: 24),
                    const Text(
                      "Announcements",
                      style:
                          TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                    ),
                    const SizedBox(height: 10),
                    ...announcements.map((a) => Container(
                          margin: const EdgeInsets.only(bottom: 10),
                          padding: const EdgeInsets.all(14),
                          decoration: BoxDecoration(
                            color: Colors.white,
                            borderRadius: BorderRadius.circular(14),
                            border: Border.all(color: Colors.orange.shade200),
                          ),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                children: [
                                  const Icon(Icons.campaign,
                                      color: Colors.orange, size: 20),
                                  const SizedBox(width: 8),
                                  Expanded(
                                    child: Text(
                                      a["title"] ?? "",
                                      style: const TextStyle(
                                          fontWeight: FontWeight.bold,
                                          fontSize: 15),
                                    ),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 6),
                              Text(
                                a["content"] ?? "",
                                style: const TextStyle(fontSize: 13),
                              ),
                            ],
                          ),
                        )),
                  ],

                  const SizedBox(height: 32),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => ReservationScreen(event: event),
                          ),
                        );
                      },
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.green,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 16),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(14),
                        ),
                      ),
                      child: const Text(
                        "Reserve & Pay",
                        style: TextStyle(
                            fontSize: 16, fontWeight: FontWeight.bold),
                      ),
                    ),
                  ),
                  const SizedBox(height: 20),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _infoCard({
    required IconData icon,
    required String label,
    required String value,
    bool fullWidth = false,
  }) {
    final card = Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
      ),
      child: Row(
        children: [
          Icon(icon, size: 20, color: Colors.black54),
          const SizedBox(width: 10),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(label,
                    style:
                        const TextStyle(fontSize: 11, color: Colors.black45)),
                Text(value,
                    style: const TextStyle(
                        fontSize: 14, fontWeight: FontWeight.bold),
                    overflow: TextOverflow.ellipsis),
              ],
            ),
          ),
        ],
      ),
    );

    if (fullWidth) return SizedBox(width: double.infinity, child: card);
    return Expanded(child: card);
  }

  Widget _placeholder() {
    return Container(
      color: Colors.grey.shade300,
      child: const Icon(Icons.event, size: 80, color: Colors.grey),
    );
  }
}

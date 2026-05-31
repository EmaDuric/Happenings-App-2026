import 'package:flutter/material.dart';
import '../services/auth_service.dart';
import '../services/api_service.dart';
import '../models/event_dto.dart';
import 'event_details_screen.dart';
import 'edit_event_screen.dart';
import 'create_event_screen.dart';

class MyEventsScreen extends StatefulWidget {
  const MyEventsScreen({super.key});

  @override
  State<MyEventsScreen> createState() => _MyEventsScreenState();
}

class _MyEventsScreenState extends State<MyEventsScreen> {
  List<dynamic> myEvents = [];
  bool isLoading = true;
  int? myUserId;

  @override
  void initState() {
    super.initState();
    loadMyEvents();
  }

  Future<void> loadMyEvents() async {
    try {
      final token = await AuthService.getToken();
      final id = await AuthService.getUserId();
      if (token == null) return;
      setState(() => myUserId = id);
      final allEvents = await ApiService.getEvents(token);
      final filtered = allEvents.where((e) => e.organizerUserId == id).toList();
      setState(() {
        myEvents = filtered;
        isLoading = false;
      });
    } catch (e) {
      setState(() => isLoading = false);
    }
  }

  Future<void> _showAnnouncementDialog(int eventId) async {
    final titleController = TextEditingController();
    final contentController = TextEditingController();
    await showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text("Create Announcement"),
        content: Column(mainAxisSize: MainAxisSize.min, children: [
          TextField(
              controller: titleController,
              decoration: const InputDecoration(
                  labelText: "Title", border: OutlineInputBorder())),
          const SizedBox(height: 12),
          TextField(
              controller: contentController,
              maxLines: 3,
              decoration: const InputDecoration(
                  labelText: "Content", border: OutlineInputBorder())),
        ]),
        actions: [
          TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text("Cancel")),
          ElevatedButton(
            onPressed: () async {
              if (titleController.text.trim().isEmpty ||
                  contentController.text.trim().isEmpty) return;
              Navigator.pop(context);
              try {
                final token = await AuthService.getToken();
                if (token == null) return;
                await ApiService.createAnnouncement(
                    eventId: eventId,
                    title: titleController.text.trim(),
                    content: contentController.text.trim(),
                    token: token);
                if (!mounted) return;
                ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text("Announcement created!")));
              } catch (e) {
                if (!mounted) return;
                ScaffoldMessenger.of(context)
                    .showSnackBar(SnackBar(content: Text("Error: $e")));
              }
            },
            style: ElevatedButton.styleFrom(backgroundColor: Colors.orange),
            child: const Text("Create", style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  Future<void> _showInvitationDialog(int eventId) async {
    List<dynamic> users = [];
    try {
      final token = await AuthService.getToken();
      if (token == null) return;
      users =
          await ApiService.getUsersForInvitation(token: token); // ← ISPRAVLJENO
    } catch (_) {}
    int? selectedUserId;
    if (!mounted) return;
    await showDialog(
      context: context,
      builder: (_) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          title: const Text("Send Invitation"),
          content: users.isEmpty
              ? const Text("No users available.")
              : DropdownButtonFormField<int>(
                  decoration: const InputDecoration(
                      labelText: "Select user", border: OutlineInputBorder()),
                  items: users
                      .map<DropdownMenuItem<int>>((u) => DropdownMenuItem<int>(
                            value: u["id"],
                            child: Text("${u["username"]} (${u["email"]})"),
                          ))
                      .toList(),
                  onChanged: (value) =>
                      setDialogState(() => selectedUserId = value),
                ),
          actions: [
            TextButton(
                onPressed: () => Navigator.pop(context),
                child: const Text("Cancel")),
            ElevatedButton(
              onPressed: selectedUserId == null
                  ? null
                  : () async {
                      Navigator.pop(context);
                      try {
                        final token = await AuthService.getToken();
                        if (token == null) return;
                        await ApiService.createInvitation(
                            eventId: eventId,
                            receiverId: selectedUserId!,
                            token: token);
                        if (!mounted) return;
                        ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(content: Text("Invitation sent!")));
                      } catch (e) {
                        if (!mounted) return;
                        ScaffoldMessenger.of(context)
                            .showSnackBar(SnackBar(content: Text("Error: $e")));
                      }
                    },
              style: ElevatedButton.styleFrom(backgroundColor: Colors.blue),
              child: const Text("Send", style: TextStyle(color: Colors.white)),
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      appBar: AppBar(
        backgroundColor: const Color(0xFFF4D35E),
        foregroundColor: Colors.black,
        elevation: 0,
        title: const Text("My Events",
            style: TextStyle(fontWeight: FontWeight.bold)),
        actions: [
          IconButton(
            icon: const Icon(Icons.add, color: Colors.black),
            tooltip: "Create new event",
            onPressed: () async {
              await Navigator.push(context,
                  MaterialPageRoute(builder: (_) => const CreateEventScreen()));
              loadMyEvents();
            },
          ),
        ],
      ),
      body: isLoading
          ? const Center(child: CircularProgressIndicator())
          : myEvents.isEmpty
              ? const Center(
                  child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                      Icon(Icons.event_busy, size: 64, color: Colors.black38),
                      SizedBox(height: 16),
                      Text("No events created yet",
                          style:
                              TextStyle(color: Colors.black54, fontSize: 16)),
                    ]))
              : RefreshIndicator(
                  onRefresh: loadMyEvents,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: myEvents.length,
                    itemBuilder: (context, index) {
                      final e = myEvents[index] as EventDto;
                      return Container(
                        margin: const EdgeInsets.only(bottom: 14),
                        decoration: BoxDecoration(
                          color: Colors.white,
                          borderRadius: BorderRadius.circular(16),
                          boxShadow: [
                            BoxShadow(
                                color: Colors.black.withOpacity(0.06),
                                blurRadius: 8,
                                offset: const Offset(0, 3))
                          ],
                        ),
                        child: Column(children: [
                          if (e.imageUrl != null)
                            ClipRRect(
                              borderRadius: const BorderRadius.only(
                                  topLeft: Radius.circular(16),
                                  topRight: Radius.circular(16)),
                              child: Image.network(e.imageUrl!,
                                  height: 140,
                                  width: double.infinity,
                                  fit: BoxFit.cover,
                                  errorBuilder: (_, __, ___) =>
                                      const SizedBox.shrink()),
                            ),
                          Padding(
                            padding: const EdgeInsets.all(14),
                            child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(e.name,
                                      style: const TextStyle(
                                          fontWeight: FontWeight.bold,
                                          fontSize: 16)),
                                  const SizedBox(height: 6),
                                  if (e.locationName != null)
                                    Row(children: [
                                      const Icon(Icons.location_on,
                                          size: 14, color: Colors.grey),
                                      const SizedBox(width: 4),
                                      Text(e.locationName!,
                                          style: const TextStyle(
                                              fontSize: 13,
                                              color: Colors.black54))
                                    ]),
                                  Row(children: [
                                    const Icon(Icons.calendar_today,
                                        size: 14, color: Colors.grey),
                                    const SizedBox(width: 4),
                                    Text(
                                        "${e.eventDate.day}.${e.eventDate.month}.${e.eventDate.year}",
                                        style: const TextStyle(
                                            fontSize: 13,
                                            color: Colors.black54))
                                  ]),
                                  const SizedBox(height: 12),
                                  const Divider(height: 1),
                                  const SizedBox(height: 10),
                                  Row(children: [
                                    Expanded(
                                        child: OutlinedButton.icon(
                                      onPressed: () =>
                                          _showAnnouncementDialog(e.id),
                                      icon: const Icon(Icons.campaign,
                                          size: 15, color: Colors.orange),
                                      label: const Text("Announce",
                                          style: TextStyle(
                                              fontSize: 11,
                                              color: Colors.orange)),
                                      style: OutlinedButton.styleFrom(
                                          side: const BorderSide(
                                              color: Colors.orange),
                                          padding: const EdgeInsets.symmetric(
                                              vertical: 6)),
                                    )),
                                    const SizedBox(width: 6),
                                    Expanded(
                                        child: OutlinedButton.icon(
                                      onPressed: () =>
                                          _showInvitationDialog(e.id),
                                      icon: const Icon(Icons.person_add,
                                          size: 15, color: Colors.blue),
                                      label: const Text("Invite",
                                          style: TextStyle(
                                              fontSize: 11,
                                              color: Colors.blue)),
                                      style: OutlinedButton.styleFrom(
                                          side: const BorderSide(
                                              color: Colors.blue),
                                          padding: const EdgeInsets.symmetric(
                                              vertical: 6)),
                                    )),
                                    const SizedBox(width: 6),
                                    Expanded(
                                        child: OutlinedButton.icon(
                                      onPressed: () async {
                                        final updated = await Navigator.push(
                                            context,
                                            MaterialPageRoute(
                                                builder: (_) =>
                                                    EditEventScreen(event: e)));
                                        if (updated == true) loadMyEvents();
                                      },
                                      icon: const Icon(Icons.edit,
                                          size: 15, color: Colors.green),
                                      label: const Text("Edit",
                                          style: TextStyle(
                                              fontSize: 11,
                                              color: Colors.green)),
                                      style: OutlinedButton.styleFrom(
                                          side: const BorderSide(
                                              color: Colors.green),
                                          padding: const EdgeInsets.symmetric(
                                              vertical: 6)),
                                    )),
                                  ]),
                                ]),
                          ),
                        ]),
                      );
                    },
                  ),
                ),
    );
  }
}

import 'package:flutter/material.dart';
import '../services/auth_service.dart';
import '../services/api_service.dart';
import 'events_screen.dart';
import 'create_event_screen.dart';
import 'edit_event_screen.dart';
import 'login_screen.dart';
import 'notification_screen.dart';
import 'event_details_screen.dart';
import '../models/event_dto.dart';
import 'my_reservations_screen.dart';
import 'profile_screen.dart';
import 'tickets_screen.dart';
import 'my_events_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  bool isOrganizerUser = false;
  String? username;
  int? userId;
  bool isLoading = true;

  List<dynamic> recommended = [];
  List<dynamic> eligibleEvents = [];
  List<dynamic> invitations = [];
  List<dynamic> announcements = [];
  List<dynamic> myEvents = [];

  @override
  void initState() {
    super.initState();
    loadAll();
  }

  Future<void> loadAll() async {
    final isOrg = await AuthService.isOrganizer();
    final user = await AuthService.getUsername();
    final id = await AuthService.getUserId();
    final token = await AuthService.getToken();

    setState(() {
      isOrganizerUser = isOrg;
      username = user;
      userId = id;
    });

    if (token != null && id != null) {
      try {
        final rec =
            await ApiService.getRecommendations(userId: id, token: token);
        final eligible = await ApiService.getEligibleEvents(token);
        final invs = await ApiService.getMyInvitations(token: token);
        final anns = await ApiService.getMyAnnouncements(token: token);

        List<dynamic> events = [];
        if (isOrg) {
          try {
            final allEvents = await ApiService.getEvents(token);
            // Filtriraj samo evente ovog organizatora
            events = allEvents.where((e) => e.organizerUserId == id).toList();
          } catch (_) {}
        }

        setState(() {
          recommended = rec;
          eligibleEvents = eligible;
          invitations = invs;
          announcements = anns;
          myEvents = events;
          isLoading = false;
        });
      } catch (e) {
        setState(() => isLoading = false);
      }
    } else {
      setState(() => isLoading = false);
    }
  }

  Future<void> logout() async {
    await AuthService.clearToken();
    if (!mounted) return;
    Navigator.pushReplacement(
        context, MaterialPageRoute(builder: (_) => const LoginScreen()));
  }

  Future<void> respondToInvitation(int invitationId, String action) async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return;
      await ApiService.respondToInvitation(
          invitationId: invitationId, action: action, token: token);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text(
                "Invitation ${action == 'accept' ? 'accepted' : 'declined'}!")),
      );
      loadAll();
    } catch (e) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text("Error: $e")));
    }
  }

  Future<void> submitReview(int eventId, int rating, String comment) async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return;
      await ApiService.submitReview(
          eventId: eventId, rating: rating, comment: comment, token: token);
      ScaffoldMessenger.of(context)
          .showSnackBar(const SnackBar(content: Text("Review submitted!")));
      loadAll();
    } catch (e) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text("Error: $e")));
    }
  }

  void showReviewDialog(Map<String, dynamic> event) {
    int selectedRating = event["existingRating"] ?? 0;
    final commentController =
        TextEditingController(text: event["existingComment"] ?? "");

    showDialog(
      context: context,
      builder: (_) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          title: Text("Review: ${event["eventName"]}"),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: List.generate(
                    5,
                    (i) => IconButton(
                          icon: Icon(
                              i < selectedRating
                                  ? Icons.star
                                  : Icons.star_border,
                              color: Colors.amber,
                              size: 32),
                          onPressed: () =>
                              setDialogState(() => selectedRating = i + 1),
                        )),
              ),
              const SizedBox(height: 12),
              TextField(
                  controller: commentController,
                  maxLines: 3,
                  decoration: const InputDecoration(
                      hintText: "Leave a comment...",
                      border: OutlineInputBorder())),
            ],
          ),
          actions: [
            TextButton(
                onPressed: () => Navigator.pop(context),
                child: const Text("Cancel")),
            ElevatedButton(
              onPressed: selectedRating == 0
                  ? null
                  : () {
                      Navigator.pop(context);
                      submitReview(event["eventId"], selectedRating,
                          commentController.text.trim());
                    },
              style: ElevatedButton.styleFrom(backgroundColor: Colors.green),
              child:
                  const Text("Submit", style: TextStyle(color: Colors.white)),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _showAnnouncementDialog(int eventId) async {
    final titleController = TextEditingController();
    final contentController = TextEditingController();

    await showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text("Create Announcement"),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
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
          ],
        ),
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
      users = await ApiService.getUsersForInvitation(token: token);
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
        elevation: 0,
        titleSpacing: 8,
        title: const Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.location_on, color: Colors.red, size: 18),
            SizedBox(width: 2),
            Text("HAPPENINGS",
                style: TextStyle(
                    color: Colors.black,
                    fontWeight: FontWeight.bold,
                    fontSize: 14,
                    letterSpacing: 1)),
          ],
        ),
        actions: [
          if (username != null)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 2),
              child: Center(
                  child: Text(
                username!.length > 8
                    ? "${username!.substring(0, 8)}..."
                    : username!.toUpperCase(),
                style: const TextStyle(
                    color: Colors.black,
                    fontWeight: FontWeight.bold,
                    fontSize: 11),
              )),
            ),
          IconButton(
              icon: const Icon(Icons.account_circle,
                  color: Colors.black, size: 22),
              tooltip: "Profile",
              padding: EdgeInsets.zero,
              onPressed: () => Navigator.push(context,
                  MaterialPageRoute(builder: (_) => const ProfileScreen()))),
          IconButton(
              icon: const Icon(Icons.confirmation_number,
                  color: Colors.black, size: 22),
              tooltip: "My Tickets",
              padding: EdgeInsets.zero,
              onPressed: () => Navigator.push(context,
                  MaterialPageRoute(builder: (_) => const TicketsScreen()))),
          IconButton(
              icon: const Icon(Icons.bookmark, color: Colors.black, size: 22),
              tooltip: "My Reservations",
              padding: EdgeInsets.zero,
              onPressed: () => Navigator.push(
                  context,
                  MaterialPageRoute(
                      builder: (_) => const MyReservationsScreen()))),
          IconButton(
              icon: const Icon(Icons.notifications,
                  color: Colors.black, size: 22),
              tooltip: "Notifications",
              padding: EdgeInsets.zero,
              onPressed: () => Navigator.push(
                  context,
                  MaterialPageRoute(
                      builder: (_) => const NotificationsScreen()))),
          // My Events tab — samo za organizatora
          if (isOrganizerUser)
            IconButton(
              icon: const Icon(Icons.event_note, color: Colors.black, size: 22),
              tooltip: "My Events",
              padding: EdgeInsets.zero,
              onPressed: () => Navigator.push(context,
                  MaterialPageRoute(builder: (_) => const MyEventsScreen())),
            ),
          IconButton(
              icon: const Icon(Icons.logout, color: Colors.black, size: 22),
              padding: EdgeInsets.zero,
              onPressed: logout),
        ],
      ),
      body: isLoading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(children: [
                    Expanded(
                        child: GestureDetector(
                      onTap: () => Navigator.push(
                          context,
                          MaterialPageRoute(
                              builder: (_) => const EventsScreen())),
                      child: Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 16, vertical: 12),
                        decoration: BoxDecoration(
                            color: Colors.white.withOpacity(0.6),
                            borderRadius: BorderRadius.circular(30)),
                        child: const Row(children: [
                          Icon(Icons.search, color: Colors.black54),
                          SizedBox(width: 8),
                          Text("Explore",
                              style: TextStyle(color: Colors.black54))
                        ]),
                      ),
                    )),
                    const SizedBox(width: 10),
                    GestureDetector(
                      onTap: () => Navigator.push(
                          context,
                          MaterialPageRoute(
                              builder: (_) => const NotificationsScreen())),
                      child: Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 16, vertical: 12),
                        decoration: BoxDecoration(
                            color: Colors.white.withOpacity(0.6),
                            borderRadius: BorderRadius.circular(30)),
                        child: const Row(children: [
                          Icon(Icons.notifications_none, color: Colors.black54),
                          SizedBox(width: 6),
                          Text("NOTIFICATIONS",
                              style: TextStyle(
                                  color: Colors.black54, fontSize: 12))
                        ]),
                      ),
                    ),
                  ]),
                  const SizedBox(height: 24),

                  const Text("RECOMMENDED FOR YOU",
                      style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                          letterSpacing: 1)),
                  const SizedBox(height: 12),
                  if (recommended.isEmpty)
                    Container(
                        height: 160,
                        decoration: BoxDecoration(
                            color: Colors.white.withOpacity(0.4),
                            borderRadius: BorderRadius.circular(16)),
                        child:
                            const Center(child: Text("No recommendations yet")))
                  else
                    GridView.builder(
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      itemCount:
                          recommended.length > 6 ? 6 : recommended.length,
                      gridDelegate:
                          const SliverGridDelegateWithFixedCrossAxisCount(
                              crossAxisCount: 3,
                              crossAxisSpacing: 8,
                              mainAxisSpacing: 8,
                              childAspectRatio: 0.75),
                      itemBuilder: (context, index) =>
                          _recommendedCard(recommended[index]),
                    ),

                  // ORGANIZER — MY EVENTS (samo 3, bez see all)
                  if (isOrganizerUser) ...[
                    const SizedBox(height: 24),
                    Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          const Text("MY EVENTS",
                              style: TextStyle(
                                  fontSize: 18,
                                  fontWeight: FontWeight.bold,
                                  letterSpacing: 1)),
                          TextButton.icon(
                            onPressed: () => Navigator.push(
                                context,
                                MaterialPageRoute(
                                    builder: (_) => const CreateEventScreen())),
                            icon: const Icon(Icons.add,
                                size: 18, color: Colors.green),
                            label: const Text("New",
                                style: TextStyle(
                                    color: Colors.green,
                                    fontWeight: FontWeight.bold)),
                          ),
                        ]),
                    const SizedBox(height: 8),
                    if (myEvents.isEmpty)
                      Container(
                          padding: const EdgeInsets.all(16),
                          decoration: BoxDecoration(
                              color: Colors.white.withOpacity(0.5),
                              borderRadius: BorderRadius.circular(12)),
                          child: const Center(
                              child: Text("No events created yet")))
                    else
                      // Samo 3 eventa, bez see all
                      ...myEvents.take(3).map((e) => _myEventCard(e)),
                  ],

                  // ANNOUNCEMENTS
                  if (announcements.isNotEmpty) ...[
                    const SizedBox(height: 28),
                    const Text("YOUR ANNOUNCEMENTS",
                        style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            letterSpacing: 1)),
                    const SizedBox(height: 12),
                    ...announcements.map((a) => Container(
                          margin: const EdgeInsets.only(bottom: 10),
                          padding: const EdgeInsets.all(14),
                          decoration: BoxDecoration(
                              color: Colors.white.withOpacity(0.85),
                              borderRadius: BorderRadius.circular(16),
                              border:
                                  Border.all(color: Colors.orange.shade200)),
                          child: Row(children: [
                            Container(
                                padding: const EdgeInsets.all(10),
                                decoration: BoxDecoration(
                                    color: Colors.orange.shade100,
                                    borderRadius: BorderRadius.circular(12)),
                                child: const Icon(Icons.campaign,
                                    color: Colors.orange, size: 28)),
                            const SizedBox(width: 12),
                            Expanded(
                                child: Column(
                                    crossAxisAlignment:
                                        CrossAxisAlignment.start,
                                    children: [
                                  Text(a["eventName"] ?? "",
                                      style: const TextStyle(
                                          color: Colors.black54, fontSize: 12)),
                                  Text(a["title"] ?? "",
                                      style: const TextStyle(
                                          fontWeight: FontWeight.bold,
                                          fontSize: 14)),
                                  Text(a["content"] ?? "",
                                      style: const TextStyle(fontSize: 13),
                                      maxLines: 2,
                                      overflow: TextOverflow.ellipsis),
                                ])),
                          ]),
                        )),
                  ],

                  // INVITATIONS
                  if (invitations.isNotEmpty) ...[
                    const SizedBox(height: 28),
                    const Text("YOUR INVITATIONS",
                        style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            letterSpacing: 1)),
                    const SizedBox(height: 12),
                    ...invitations.map((inv) {
                      final isPending =
                          (inv["status"] ?? "").toLowerCase() == "pending";
                      return Container(
                        margin: const EdgeInsets.only(bottom: 10),
                        padding: const EdgeInsets.all(14),
                        decoration: BoxDecoration(
                            color: Colors.white.withOpacity(0.85),
                            borderRadius: BorderRadius.circular(16),
                            border: Border.all(color: Colors.orange.shade200)),
                        child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(children: [
                                Container(
                                    padding: const EdgeInsets.all(10),
                                    decoration: BoxDecoration(
                                        color: Colors.orange.shade100,
                                        borderRadius:
                                            BorderRadius.circular(12)),
                                    child: const Icon(Icons.mail_outline,
                                        color: Colors.orange, size: 28)),
                                const SizedBox(width: 12),
                                Expanded(
                                    child: Column(
                                        crossAxisAlignment:
                                            CrossAxisAlignment.start,
                                        children: [
                                      Text(
                                          inv["eventName"] ??
                                              "Event invitation",
                                          style: const TextStyle(
                                              fontWeight: FontWeight.bold,
                                              fontSize: 14)),
                                      const SizedBox(height: 4),
                                      Container(
                                          padding: const EdgeInsets.symmetric(
                                              horizontal: 8, vertical: 2),
                                          decoration: BoxDecoration(
                                              color: _invitationStatusColor(
                                                  inv["status"]),
                                              borderRadius:
                                                  BorderRadius.circular(10)),
                                          child: Text(
                                              inv["status"] ?? "Pending",
                                              style: const TextStyle(
                                                  color: Colors.white,
                                                  fontSize: 11))),
                                    ])),
                              ]),
                              if (isPending) ...[
                                const SizedBox(height: 10),
                                Row(children: [
                                  Expanded(
                                      child: OutlinedButton.icon(
                                          onPressed: () => respondToInvitation(
                                              inv["id"], "decline"),
                                          icon: const Icon(Icons.close,
                                              color: Colors.red, size: 16),
                                          label: const Text("Decline",
                                              style:
                                                  TextStyle(color: Colors.red)),
                                          style: OutlinedButton.styleFrom(
                                              side: const BorderSide(
                                                  color: Colors.red)))),
                                  const SizedBox(width: 10),
                                  Expanded(
                                      child: ElevatedButton.icon(
                                          onPressed: () => respondToInvitation(
                                              inv["id"], "accept"),
                                          icon:
                                              const Icon(Icons.check, size: 16),
                                          label: const Text("Accept"),
                                          style: ElevatedButton.styleFrom(
                                              backgroundColor: Colors.green,
                                              foregroundColor: Colors.white))),
                                ]),
                              ],
                            ]),
                      );
                    }),
                  ],

                  const SizedBox(height: 28),
                  if (eligibleEvents.isNotEmpty) ...[
                    const Text("Review events you visited:",
                        style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            fontStyle: FontStyle.italic)),
                    const SizedBox(height: 12),
                    ...eligibleEvents.map((e) => _reviewCard(e)),
                  ],
                ],
              ),
            ),
    );
  }

  Color _invitationStatusColor(String? status) {
    switch (status?.toLowerCase()) {
      case "accepted":
        return Colors.green;
      case "declined":
        return Colors.red;
      default:
        return Colors.orange;
    }
  }

  Widget _myEventCard(dynamic e) {
    final name = e is EventDto ? e.name : (e["name"] ?? "");
    final date =
        e is EventDto ? e.eventDate : DateTime.tryParse(e["eventDate"] ?? "");
    final location = e is EventDto ? e.locationName : e["locationName"];
    final imageUrl = e is EventDto ? e.imageUrl : e["imageUrl"];
    final id = e is EventDto ? e.id : e["id"];
    final organizerUserId =
        e is EventDto ? e.organizerUserId : e["organizerUserId"];

    final event = e is EventDto
        ? e
        : EventDto(
            id: id,
            name: name,
            description: e["description"] ?? "",
            eventDate: date ?? DateTime.now(),
            locationName: location,
            categoryName: e["categoryName"],
            imageUrl: imageUrl,
            organizerUserId: organizerUserId,
          );

    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
          color: Colors.white.withOpacity(0.9),
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: Colors.orange.shade100)),
      child: Column(children: [
        GestureDetector(
          onTap: () => Navigator.push(
              context,
              MaterialPageRoute(
                  builder: (_) => EventDetailsScreen(event: event))),
          child: Row(children: [
            ClipRRect(
              borderRadius: BorderRadius.circular(10),
              child: imageUrl != null
                  ? Image.network(imageUrl,
                      width: 60,
                      height: 60,
                      fit: BoxFit.cover,
                      errorBuilder: (_, __, ___) => _eventPlaceholder())
                  : _eventPlaceholder(),
            ),
            const SizedBox(width: 12),
            Expanded(
                child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                  Text(name,
                      style: const TextStyle(
                          fontWeight: FontWeight.bold, fontSize: 14),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis),
                  if (location != null)
                    Text("📍 $location",
                        style: const TextStyle(
                            fontSize: 12, color: Colors.black54)),
                  if (date != null)
                    Text("📅 ${date.day}.${date.month}.${date.year}",
                        style: const TextStyle(
                            fontSize: 12, color: Colors.black54)),
                ])),
            const Icon(Icons.chevron_right, color: Colors.black38),
          ]),
        ),
        const SizedBox(height: 8),
        const Divider(height: 1),
        const SizedBox(height: 8),
        Row(children: [
          Expanded(
              child: OutlinedButton.icon(
            onPressed: () => _showAnnouncementDialog(id),
            icon: const Icon(Icons.campaign, size: 15, color: Colors.orange),
            label: const Text("Announce",
                style: TextStyle(fontSize: 11, color: Colors.orange)),
            style: OutlinedButton.styleFrom(
                side: const BorderSide(color: Colors.orange),
                padding: const EdgeInsets.symmetric(vertical: 6)),
          )),
          const SizedBox(width: 6),
          Expanded(
              child: OutlinedButton.icon(
            onPressed: () => _showInvitationDialog(id),
            icon: const Icon(Icons.person_add, size: 15, color: Colors.blue),
            label: const Text("Invite",
                style: TextStyle(fontSize: 11, color: Colors.blue)),
            style: OutlinedButton.styleFrom(
                side: const BorderSide(color: Colors.blue),
                padding: const EdgeInsets.symmetric(vertical: 6)),
          )),
          const SizedBox(width: 6),
          Expanded(
              child: OutlinedButton.icon(
            onPressed: () async {
              final updated = await Navigator.push(
                  context,
                  MaterialPageRoute(
                      builder: (_) => EditEventScreen(event: event)));
              if (updated == true) loadAll();
            },
            icon: const Icon(Icons.edit, size: 15, color: Colors.green),
            label: const Text("Edit",
                style: TextStyle(fontSize: 11, color: Colors.green)),
            style: OutlinedButton.styleFrom(
                side: const BorderSide(color: Colors.green),
                padding: const EdgeInsets.symmetric(vertical: 6)),
          )),
        ]),
      ]),
    );
  }

  Widget _eventPlaceholder() {
    return Container(
        width: 60,
        height: 60,
        color: Colors.grey.shade200,
        child: const Icon(Icons.event, color: Colors.grey));
  }

  Widget _recommendedCard(Map<String, dynamic> rec) {
    return GestureDetector(
      onTap: () {
        final event = EventDto(
            id: rec["eventId"],
            name: rec["eventName"] ?? "",
            description: "",
            eventDate:
                DateTime.tryParse(rec["eventDate"] ?? "") ?? DateTime.now(),
            locationName: rec["locationName"],
            categoryName: rec["categoryName"],
            imageUrl: rec["imageUrl"]);
        Navigator.push(
            context,
            MaterialPageRoute(
                builder: (_) => EventDetailsScreen(event: event)));
      },
      child: ClipRRect(
        borderRadius: BorderRadius.circular(16),
        child: Stack(fit: StackFit.expand, children: [
          rec["imageUrl"] != null
              ? Image.network(rec["imageUrl"],
                  fit: BoxFit.cover,
                  errorBuilder: (_, __, ___) => Container(
                      color: Colors.grey.shade300,
                      child: const Icon(Icons.event,
                          color: Colors.grey, size: 40)))
              : Container(
                  color: Colors.grey.shade300,
                  child: const Icon(Icons.event, color: Colors.grey, size: 40)),
          Container(
              decoration: BoxDecoration(
                  gradient: LinearGradient(
                      begin: Alignment.topCenter,
                      end: Alignment.bottomCenter,
                      colors: [
                Colors.transparent,
                Colors.black.withOpacity(0.7)
              ]))),
          Positioned(
              bottom: 12,
              left: 10,
              right: 10,
              child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(rec["eventName"] ?? "",
                        style: const TextStyle(
                            color: Colors.white,
                            fontWeight: FontWeight.bold,
                            fontSize: 11),
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis),
                    if (rec["locationName"] != null)
                      Text(rec["locationName"],
                          style: const TextStyle(
                              color: Colors.white70, fontSize: 9),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis),
                    if (rec["reason"] != null &&
                        (rec["reason"] as String).trim().isNotEmpty)
                      Padding(
                        padding: const EdgeInsets.only(top: 3),
                        child: Row(children: [
                          const Icon(Icons.auto_awesome,
                              color: Colors.amberAccent, size: 10),
                          const SizedBox(width: 3),
                          Expanded(
                            child: Text(rec["reason"],
                                style: const TextStyle(
                                    color: Colors.amberAccent,
                                    fontSize: 8,
                                    fontStyle: FontStyle.italic),
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis),
                          ),
                        ]),
                      ),
                  ])),
        ]),
      ),
    );
  }

  Widget _reviewCard(Map<String, dynamic> event) {
    final hasReview = event["existingRating"] != null;
    final rating = event["existingRating"] ?? 0;
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
          color: Colors.white.withOpacity(0.85),
          borderRadius: BorderRadius.circular(16)),
      child: Row(children: [
        ClipRRect(
            borderRadius: BorderRadius.circular(10),
            child: event["imageUrl"] != null
                ? Image.network(event["imageUrl"],
                    width: 70,
                    height: 70,
                    fit: BoxFit.cover,
                    errorBuilder: (_, __, ___) => _reviewPlaceholder())
                : _reviewPlaceholder()),
        const SizedBox(width: 12),
        Expanded(
            child:
                Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Text(event["eventName"] ?? "",
              style:
                  const TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
          const SizedBox(height: 4),
          Row(
              children: List.generate(
                  5,
                  (i) => Icon(i < rating ? Icons.star : Icons.star_border,
                      color: Colors.amber, size: 18))),
          if (hasReview && event["existingComment"] != null)
            Text(event["existingComment"],
                style: const TextStyle(color: Colors.black54, fontSize: 12),
                maxLines: 1,
                overflow: TextOverflow.ellipsis),
        ])),
        IconButton(
            icon: Icon(hasReview ? Icons.edit : Icons.rate_review,
                color: Colors.black54),
            onPressed: () => showReviewDialog(event)),
      ]),
    );
  }

  Widget _reviewPlaceholder() {
    return Container(
        width: 70,
        height: 70,
        color: Colors.grey.shade200,
        child: const Icon(Icons.event, color: Colors.grey));
  }
}

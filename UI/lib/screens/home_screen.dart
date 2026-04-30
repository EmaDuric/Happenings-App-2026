import 'package:flutter/material.dart';
import '../services/auth_service.dart';
import '../services/api_service.dart';
import 'events_screen.dart';
import 'create_event_screen.dart';
import 'login_screen.dart';
import 'notification_screen.dart';
import 'event_details_screen.dart';
import '../models/event_dto.dart';
import 'my_reservations_screen.dart';

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

        setState(() {
          recommended = rec;
          eligibleEvents = eligible;
          invitations = invs;
          announcements = anns;
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
                    border: OutlineInputBorder()),
              ),
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

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      appBar: AppBar(
        backgroundColor: const Color(0xFFF4D35E),
        elevation: 0,
        title: const Row(
          children: [
            Icon(Icons.location_on, color: Colors.red, size: 20),
            SizedBox(width: 4),
            Text("HAPPENINGS",
                style: TextStyle(
                    color: Colors.black,
                    fontWeight: FontWeight.bold,
                    letterSpacing: 1.2)),
          ],
        ),
        actions: [
          if (username != null)
            Padding(
              padding: const EdgeInsets.only(right: 4),
              child: Center(
                  child: Text(username!.toUpperCase(),
                      style: const TextStyle(
                          color: Colors.black,
                          fontWeight: FontWeight.bold,
                          fontSize: 12))),
            ),
          IconButton(
              icon: const Icon(Icons.account_circle, color: Colors.black),
              onPressed: () {}),
          IconButton(
            icon: const Icon(Icons.bookmark, color: Colors.black),
            onPressed: () => Navigator.push(
                context,
                MaterialPageRoute(
                    builder: (_) => const MyReservationsScreen())),
          ),
          IconButton(
            icon: const Icon(Icons.notifications, color: Colors.black),
            onPressed: () => Navigator.push(context,
                MaterialPageRoute(builder: (_) => const NotificationsScreen())),
          ),
          IconButton(
              icon: const Icon(Icons.logout, color: Colors.black),
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
                  // SEARCH + EXPLORE
                  Row(
                    children: [
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
                        ),
                      ),
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
                            Icon(Icons.notifications_none,
                                color: Colors.black54),
                            SizedBox(width: 6),
                            Text("NOTIFICATIONS",
                                style: TextStyle(
                                    color: Colors.black54, fontSize: 12))
                          ]),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 24),

                  // RECOMMENDED
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
                          const Center(child: Text("No recommendations yet")),
                    )
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
                        childAspectRatio: 0.75,
                      ),
                      itemBuilder: (context, index) =>
                          _recommendedCard(recommended[index]),
                    ),

                  // ORGANIZER DUGME
                  if (isOrganizerUser) ...[
                    const SizedBox(height: 16),
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton.icon(
                        onPressed: () => Navigator.push(
                            context,
                            MaterialPageRoute(
                                builder: (_) => const CreateEventScreen())),
                        icon: const Icon(Icons.add),
                        label: const Text("Create an event"),
                        style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.green,
                            foregroundColor: Colors.white,
                            padding: const EdgeInsets.symmetric(vertical: 14),
                            shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(12))),
                      ),
                    ),
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
                            border: Border.all(color: Colors.orange.shade200),
                          ),
                          child: Row(
                            children: [
                              Container(
                                padding: const EdgeInsets.all(10),
                                decoration: BoxDecoration(
                                    color: Colors.orange.shade100,
                                    borderRadius: BorderRadius.circular(12)),
                                child: const Icon(Icons.campaign,
                                    color: Colors.orange, size: 28),
                              ),
                              const SizedBox(width: 12),
                              Expanded(
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(a["eventName"] ?? "",
                                        style: const TextStyle(
                                            color: Colors.black54,
                                            fontSize: 12)),
                                    Text(a["title"] ?? "",
                                        style: const TextStyle(
                                            fontWeight: FontWeight.bold,
                                            fontSize: 14)),
                                    Text(a["content"] ?? "",
                                        style: const TextStyle(fontSize: 13),
                                        maxLines: 2,
                                        overflow: TextOverflow.ellipsis),
                                  ],
                                ),
                              ),
                            ],
                          ),
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
                          border: Border.all(color: Colors.orange.shade200),
                        ),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              children: [
                                Container(
                                  padding: const EdgeInsets.all(10),
                                  decoration: BoxDecoration(
                                      color: Colors.orange.shade100,
                                      borderRadius: BorderRadius.circular(12)),
                                  child: const Icon(Icons.mail_outline,
                                      color: Colors.orange, size: 28),
                                ),
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
                                        child: Text(inv["status"] ?? "Pending",
                                            style: const TextStyle(
                                                color: Colors.white,
                                                fontSize: 11)),
                                      ),
                                    ],
                                  ),
                                ),
                              ],
                            ),
                            if (isPending) ...[
                              const SizedBox(height: 10),
                              Row(
                                children: [
                                  Expanded(
                                    child: OutlinedButton.icon(
                                      onPressed: () => respondToInvitation(
                                          inv["id"], "decline"),
                                      icon: const Icon(Icons.close,
                                          color: Colors.red, size: 16),
                                      label: const Text("Decline",
                                          style: TextStyle(color: Colors.red)),
                                      style: OutlinedButton.styleFrom(
                                          side: const BorderSide(
                                              color: Colors.red)),
                                    ),
                                  ),
                                  const SizedBox(width: 10),
                                  Expanded(
                                    child: ElevatedButton.icon(
                                      onPressed: () => respondToInvitation(
                                          inv["id"], "accept"),
                                      icon: const Icon(Icons.check, size: 16),
                                      label: const Text("Accept"),
                                      style: ElevatedButton.styleFrom(
                                          backgroundColor: Colors.green,
                                          foregroundColor: Colors.white),
                                    ),
                                  ),
                                ],
                              ),
                            ],
                          ],
                        ),
                      );
                    }),
                  ],

                  const SizedBox(height: 28),

                  // REVIEW SECTION
                  if (eligibleEvents.isNotEmpty) ...[
                    const Text("Review events you visited:",
                        style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            fontStyle: FontStyle.italic)),
                    const SizedBox(height: 12),
                    ...eligibleEvents.map((e) => _reviewCard(e)).toList(),
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

  Widget _recommendedCard(Map<String, dynamic> rec) {
    return GestureDetector(
      onTap: () {
        final event = EventDto(
          id: rec["eventId"],
          name: rec["eventName"] ?? "",
          description: "",
          eventDate:
              DateTime.tryParse(rec["eventDate"] ?? "") ?? DateTime.UtcNow(),
          locationName: rec["locationName"],
          categoryName: rec["categoryName"],
          imageUrl: rec["imageUrl"],
        );
        Navigator.push(
            context,
            MaterialPageRoute(
                builder: (_) => EventDetailsScreen(event: event)));
      },
      child: ClipRRect(
        borderRadius: BorderRadius.circular(16),
        child: Stack(
          fit: StackFit.expand,
          children: [
            rec["imageUrl"] != null
                ? Image.network(rec["imageUrl"],
                    fit: BoxFit.cover,
                    errorBuilder: (_, __, ___) => Container(
                        color: Colors.grey.shade300,
                        child: const Icon(Icons.event,
                            color: Colors.grey, size: 40)))
                : Container(
                    color: Colors.grey.shade300,
                    child:
                        const Icon(Icons.event, color: Colors.grey, size: 40)),
            Container(
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topCenter,
                  end: Alignment.bottomCenter,
                  colors: [Colors.transparent, Colors.black.withOpacity(0.7)],
                ),
              ),
            ),
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
                        style:
                            const TextStyle(color: Colors.white70, fontSize: 9),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis),
                ],
              ),
            ),
          ],
        ),
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
      child: Row(
        children: [
          ClipRRect(
            borderRadius: BorderRadius.circular(10),
            child: event["imageUrl"] != null
                ? Image.network(event["imageUrl"],
                    width: 70,
                    height: 70,
                    fit: BoxFit.cover,
                    errorBuilder: (_, __, ___) => _reviewPlaceholder())
                : _reviewPlaceholder(),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(event["eventName"] ?? "",
                    style: const TextStyle(
                        fontWeight: FontWeight.bold, fontSize: 14)),
                const SizedBox(height: 4),
                Row(
                    children: List.generate(
                        5,
                        (i) => Icon(i < rating ? Icons.star : Icons.star_border,
                            color: Colors.amber, size: 18))),
                if (hasReview && event["existingComment"] != null)
                  Text(event["existingComment"],
                      style:
                          const TextStyle(color: Colors.black54, fontSize: 12),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis),
              ],
            ),
          ),
          IconButton(
            icon: Icon(hasReview ? Icons.edit : Icons.rate_review,
                color: Colors.black54),
            onPressed: () => showReviewDialog(event),
          ),
        ],
      ),
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

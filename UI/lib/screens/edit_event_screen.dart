import 'package:flutter/material.dart';
import '../models/event_dto.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';

class ExistingTicketTypeEntry {
  final int id;
  final TextEditingController nameController;
  final TextEditingController priceController;
  final TextEditingController quantityController;
  bool isDeleted = false;

  ExistingTicketTypeEntry({
    required this.id,
    required String name,
    required double price,
    required int quantity,
  })  : nameController = TextEditingController(text: name),
        priceController = TextEditingController(text: price.toString()),
        quantityController = TextEditingController(text: quantity.toString());

  void dispose() {
    nameController.dispose();
    priceController.dispose();
    quantityController.dispose();
  }
}

class NewTicketTypeEntry {
  final nameController = TextEditingController();
  final priceController = TextEditingController();
  final quantityController = TextEditingController();

  void dispose() {
    nameController.dispose();
    priceController.dispose();
    quantityController.dispose();
  }
}

class EditEventScreen extends StatefulWidget {
  final EventDto event;

  const EditEventScreen({super.key, required this.event});

  @override
  State<EditEventScreen> createState() => _EditEventScreenState();
}

class _EditEventScreenState extends State<EditEventScreen> {
  late final TextEditingController _nameController;
  late final TextEditingController _descriptionController;
  late final TextEditingController _imageUrlController;

  DateTime? selectedDate;
  int? selectedCategoryId;
  int? selectedLocationId;

  List<dynamic> categories = [];
  List<dynamic> locations = [];

  List<ExistingTicketTypeEntry> existingTicketTypes = [];
  List<NewTicketTypeEntry> newTicketTypes = [];

  bool isLoading = false;
  bool isLoadingDropdowns = true;

  @override
  void initState() {
    super.initState();
    _nameController = TextEditingController(text: widget.event.name);
    _descriptionController =
        TextEditingController(text: widget.event.description);
    _imageUrlController =
        TextEditingController(text: widget.event.imageUrl ?? "");
    selectedDate = widget.event.eventDate;
    selectedCategoryId = widget.event.eventCategoryId;
    selectedLocationId = widget.event.locationId;
    loadDropdowns();
    loadTicketTypes();
  }

  @override
  void dispose() {
    _nameController.dispose();
    _descriptionController.dispose();
    for (final t in existingTicketTypes) t.dispose();
    for (final t in newTicketTypes) t.dispose();
    super.dispose();
  }

  Future<void> loadDropdowns() async {
    try {
      final token = await AuthService.getToken();
      if (token == null) throw Exception("Not logged in");
      final cats = await ApiService.getCategories(token: token);
      final locs = await ApiService.getLocations(token: token);
      setState(() {
        categories = cats;
        locations = locs;
        if (!cats.any((c) => c["id"] == selectedCategoryId)) {
          selectedCategoryId = cats.isNotEmpty ? cats.first["id"] : null;
        }
        if (!locs.any((l) => l["id"] == selectedLocationId)) {
          selectedLocationId = locs.isNotEmpty ? locs.first["id"] : null;
        }
        isLoadingDropdowns = false;
      });
    } catch (e) {
      setState(() => isLoadingDropdowns = false);
    }
  }

  Future<void> loadTicketTypes() async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return;
      final types = await ApiService.getTicketTypes(
          eventId: widget.event.id, token: token);
      setState(() {
        existingTicketTypes = types
            .map((t) => ExistingTicketTypeEntry(
                  id: t["id"],
                  name: t["name"] ?? "",
                  price: (t["price"] as num).toDouble(),
                  quantity: t["availableQuantity"] as int,
                ))
            .toList();
      });
    } catch (_) {}
  }

  Future<void> pickDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: selectedDate ?? DateTime.UtcNow(),
      firstDate: DateTime.UtcNow(),
      lastDate: DateTime(2100),
    );
    if (picked != null) setState(() => selectedDate = picked);
  }

  Future<void> saveEvent() async {
    if (_nameController.text.trim().isEmpty ||
        _descriptionController.text.trim().isEmpty ||
        selectedDate == null ||
        selectedCategoryId == null ||
        selectedLocationId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("All event fields are required.")),
      );
      return;
    }

    for (int i = 0; i < newTicketTypes.length; i++) {
      final t = newTicketTypes[i];
      if (t.nameController.text.trim().isEmpty ||
          t.priceController.text.trim().isEmpty ||
          t.quantityController.text.trim().isEmpty) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
              content: Text("New ticket type ${i + 1}: all fields required.")),
        );
        return;
      }
    }

    setState(() => isLoading = true);

    try {
      final token = await AuthService.getToken();
      if (token == null) throw Exception("Not logged in");

      await ApiService.updateEvent(
        id: widget.event.id,
        name: _nameController.text.trim(),
        description: _descriptionController.text.trim(),
        eventDate: selectedDate!,
        eventCategoryId: selectedCategoryId!,
        locationId: selectedLocationId!,
        token: token,
      );

      final imageUrl = _imageUrlController.text.trim();
      if (imageUrl.isNotEmpty) {
        final existingImages = await ApiService.getEventImages(
            eventId: widget.event.id, token: token);
        if (existingImages.isEmpty) {
          await ApiService.addEventImage(
              eventId: widget.event.id, imageUrl: imageUrl, token: token);
        } else {
          await ApiService.updateEventImage(
              id: existingImages.first["id"], imageUrl: imageUrl, token: token);
        }
      }

      for (final t in existingTicketTypes) {
        if (t.isDeleted) {
          await ApiService.deleteTicketType(id: t.id, token: token);
        } else {
          await ApiService.updateTicketType(
            id: t.id,
            eventId: widget.event.id,
            name: t.nameController.text.trim(),
            price: double.tryParse(t.priceController.text.trim()) ?? 0,
            availableQuantity:
                int.tryParse(t.quantityController.text.trim()) ?? 0,
            token: token,
          );
        }
      }

      for (final t in newTicketTypes) {
        await ApiService.createTicketType(
          eventId: widget.event.id,
          name: t.nameController.text.trim(),
          price: double.parse(t.priceController.text.trim()),
          availableQuantity: int.parse(t.quantityController.text.trim()),
          token: token,
        );
      }

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Event updated successfully!")),
      );
      Navigator.pop(context, true);
    } catch (e) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text("Error: $e")));
    } finally {
      if (mounted) setState(() => isLoading = false);
    }
  }

  void showCreateAnnouncementDialog() {
    final titleController = TextEditingController();
    final contentController = TextEditingController();

    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text("Create Announcement"),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(
              controller: titleController,
              decoration: const InputDecoration(
                labelText: "Title",
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: contentController,
              maxLines: 3,
              decoration: const InputDecoration(
                labelText: "Content",
                border: OutlineInputBorder(),
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text("Cancel"),
          ),
          ElevatedButton(
            onPressed: () async {
              if (titleController.text.trim().isEmpty ||
                  contentController.text.trim().isEmpty) {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                      content: Text("Title and content are required.")),
                );
                return;
              }
              Navigator.pop(context);
              try {
                final token = await AuthService.getToken();
                if (token == null) return;
                await ApiService.createAnnouncement(
                  eventId: widget.event.id,
                  title: titleController.text.trim(),
                  content: contentController.text.trim(),
                  token: token,
                );
                if (!mounted) return;
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                      content: Text("Announcement created successfully!")),
                );
              } catch (e) {
                if (!mounted) return;
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(content: Text("Error: $e")),
                );
              }
            },
            style: ElevatedButton.styleFrom(backgroundColor: Colors.orange),
            child: const Text("Create", style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  void showCreateInvitationDialog() async {
    // Učitaj korisnike
    List<dynamic> users = [];
    try {
      final token = await AuthService.getToken();
      if (token == null) return;
      users = await ApiService.getUsers(token: token);
    } catch (_) {}

    int? selectedUserId;

    if (!mounted) return;

    showDialog(
      context: context,
      builder: (_) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          title: const Text("Send Invitation"),
          content: users.isEmpty
              ? const Text("No users available.")
              : DropdownButtonFormField<int>(
                  decoration: const InputDecoration(
                    labelText: "Select user",
                    border: OutlineInputBorder(),
                  ),
                  items: users.map<DropdownMenuItem<int>>((u) {
                    return DropdownMenuItem<int>(
                      value: u["id"],
                      child: Text("${u["username"]} (${u["email"]})"),
                    );
                  }).toList(),
                  onChanged: (value) =>
                      setDialogState(() => selectedUserId = value),
                ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text("Cancel"),
            ),
            ElevatedButton(
              onPressed: selectedUserId == null
                  ? null
                  : () async {
                      Navigator.pop(context);
                      try {
                        final token = await AuthService.getToken();
                        if (token == null) return;
                        await ApiService.createInvitation(
                          eventId: widget.event.id,
                          receiverId: selectedUserId!,
                          token: token,
                        );
                        if (!mounted) return;
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(
                              content: Text("Invitation sent successfully!")),
                        );
                      } catch (e) {
                        if (!mounted) return;
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(content: Text("Error: $e")),
                        );
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

  Widget buildExistingTicketCard(ExistingTicketTypeEntry t, int index) {
    if (t.isDeleted) return const SizedBox.shrink();
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        border: Border.all(color: Colors.grey.shade300),
        borderRadius: BorderRadius.circular(12),
        color: Colors.grey.shade50,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text("Ticket Type ${index + 1}",
                  style: const TextStyle(fontWeight: FontWeight.bold)),
              IconButton(
                icon: const Icon(Icons.delete, color: Colors.red),
                onPressed: () => setState(() => t.isDeleted = true),
              ),
            ],
          ),
          const SizedBox(height: 10),
          TextField(
              controller: t.nameController,
              decoration: const InputDecoration(
                  labelText: "Name", border: OutlineInputBorder())),
          const SizedBox(height: 10),
          Row(
            children: [
              Expanded(
                  child: TextField(
                      controller: t.priceController,
                      keyboardType: TextInputType.number,
                      decoration: const InputDecoration(
                          labelText: "Price (KM)",
                          border: OutlineInputBorder()))),
              const SizedBox(width: 10),
              Expanded(
                  child: TextField(
                      controller: t.quantityController,
                      keyboardType: TextInputType.number,
                      decoration: const InputDecoration(
                          labelText: "Quantity",
                          border: OutlineInputBorder()))),
            ],
          ),
        ],
      ),
    );
  }

  Widget buildNewTicketCard(NewTicketTypeEntry t, int index) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        border: Border.all(color: Colors.green.shade300),
        borderRadius: BorderRadius.circular(12),
        color: Colors.green.shade50,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text("New Ticket Type ${index + 1}",
                  style: const TextStyle(
                      fontWeight: FontWeight.bold, color: Colors.green)),
              IconButton(
                icon: const Icon(Icons.delete, color: Colors.red),
                onPressed: () => setState(() {
                  t.dispose();
                  newTicketTypes.removeAt(index);
                }),
              ),
            ],
          ),
          const SizedBox(height: 10),
          TextField(
              controller: t.nameController,
              decoration: const InputDecoration(
                  labelText: "Name (e.g. VIP, Regular)",
                  border: OutlineInputBorder())),
          const SizedBox(height: 10),
          Row(
            children: [
              Expanded(
                  child: TextField(
                      controller: t.priceController,
                      keyboardType: TextInputType.number,
                      decoration: const InputDecoration(
                          labelText: "Price (KM)",
                          border: OutlineInputBorder()))),
              const SizedBox(width: 10),
              Expanded(
                  child: TextField(
                      controller: t.quantityController,
                      keyboardType: TextInputType.number,
                      decoration: const InputDecoration(
                          labelText: "Quantity",
                          border: OutlineInputBorder()))),
            ],
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (isLoadingDropdowns) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      appBar: AppBar(
        title: const Text("Edit Event"),
        backgroundColor: const Color(0xFFF4D35E),
        foregroundColor: Colors.black,
        elevation: 0,
      ),
      body: Center(
        child: SingleChildScrollView(
          child: Container(
            width: 450,
            padding: const EdgeInsets.all(24),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Text("Edit Event",
                    style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
                    textAlign: TextAlign.center),
                const SizedBox(height: 25),

                TextField(
                    controller: _nameController,
                    decoration: const InputDecoration(
                        labelText: "Event name",
                        border: OutlineInputBorder(),
                        filled: true,
                        fillColor: Colors.white)),
                const SizedBox(height: 15),

                TextField(
                    controller: _descriptionController,
                    maxLines: 3,
                    decoration: const InputDecoration(
                        labelText: "Description",
                        border: OutlineInputBorder(),
                        filled: true,
                        fillColor: Colors.white)),
                const SizedBox(height: 15),

                TextField(
                  controller: _imageUrlController,
                  decoration: const InputDecoration(
                      labelText: "Image URL (optional)",
                      hintText: "https://example.com/image.jpg",
                      border: OutlineInputBorder(),
                      filled: true,
                      fillColor: Colors.white,
                      prefixIcon: Icon(Icons.image)),
                  onChanged: (_) => setState(() {}),
                ),

                if (_imageUrlController.text.trim().isNotEmpty)
                  Padding(
                    padding: const EdgeInsets.only(top: 10),
                    child: ClipRRect(
                      borderRadius: BorderRadius.circular(12),
                      child: Image.network(_imageUrlController.text.trim(),
                          height: 160,
                          width: double.infinity,
                          fit: BoxFit.cover,
                          errorBuilder: (_, __, ___) => Container(
                              height: 50,
                              color: Colors.grey.shade200,
                              child: const Center(
                                  child: Text("Invalid image URL",
                                      style: TextStyle(color: Colors.grey))))),
                    ),
                  ),

                const SizedBox(height: 15),
                DropdownButtonFormField<int>(
                  value: selectedCategoryId,
                  decoration: const InputDecoration(
                      labelText: "Category",
                      border: OutlineInputBorder(),
                      filled: true,
                      fillColor: Colors.white),
                  items: categories
                      .map<DropdownMenuItem<int>>((cat) =>
                          DropdownMenuItem<int>(
                              value: cat["id"], child: Text(cat["name"] ?? "")))
                      .toList(),
                  onChanged: (value) =>
                      setState(() => selectedCategoryId = value),
                ),
                const SizedBox(height: 15),

                DropdownButtonFormField<int>(
                  value: selectedLocationId,
                  decoration: const InputDecoration(
                      labelText: "Location",
                      border: OutlineInputBorder(),
                      filled: true,
                      fillColor: Colors.white),
                  items: locations
                      .map<DropdownMenuItem<int>>((loc) =>
                          DropdownMenuItem<int>(
                              value: loc["id"], child: Text(loc["name"] ?? "")))
                      .toList(),
                  onChanged: (value) =>
                      setState(() => selectedLocationId = value),
                ),
                const SizedBox(height: 15),

                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(8),
                      border: Border.all(color: Colors.grey.shade400)),
                  child: Row(
                    children: [
                      const Icon(Icons.calendar_today, color: Colors.grey),
                      const SizedBox(width: 12),
                      Expanded(
                          child: Text(selectedDate == null
                              ? "No date selected"
                              : "Date: ${selectedDate!.day}.${selectedDate!.month}.${selectedDate!.year}")),
                      ElevatedButton(
                          onPressed: pickDate, child: const Text("Pick date")),
                    ],
                  ),
                ),

                const SizedBox(height: 30),
                const Divider(),
                const SizedBox(height: 10),

                const Text("Ticket Types",
                    style:
                        TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                const SizedBox(height: 12),

                ...existingTicketTypes
                    .asMap()
                    .entries
                    .map((e) => buildExistingTicketCard(e.value, e.key)),
                ...newTicketTypes
                    .asMap()
                    .entries
                    .map((e) => buildNewTicketCard(e.value, e.key)),

                TextButton.icon(
                  onPressed: () =>
                      setState(() => newTicketTypes.add(NewTicketTypeEntry())),
                  icon: const Icon(Icons.add, color: Colors.green),
                  label: const Text("Add ticket type",
                      style: TextStyle(color: Colors.green)),
                ),

                const SizedBox(height: 25),

                ElevatedButton(
                  onPressed: isLoading ? null : saveEvent,
                  style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.green,
                      foregroundColor: Colors.white,
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12))),
                  child: isLoading
                      ? const CircularProgressIndicator(color: Colors.white)
                      : const Text("Save Changes",
                          style: TextStyle(
                              fontSize: 16, fontWeight: FontWeight.bold)),
                ),

                const SizedBox(height: 30),
                const Divider(),
                const SizedBox(height: 10),

                // ORGANIZER AKCIJE
                const Text("Organizer Actions",
                    style:
                        TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                const SizedBox(height: 12),

                // ANNOUNCEMENT
                Container(
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: Colors.orange.shade50,
                    borderRadius: BorderRadius.circular(14),
                    border: Border.all(color: Colors.orange.shade200),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Row(
                        children: [
                          Icon(Icons.campaign, color: Colors.orange),
                          SizedBox(width: 8),
                          Text("Announcements",
                              style: TextStyle(
                                  fontWeight: FontWeight.bold, fontSize: 16)),
                        ],
                      ),
                      const SizedBox(height: 8),
                      const Text(
                          "Notify attendees about important updates for this event.",
                          style:
                              TextStyle(color: Colors.black54, fontSize: 13)),
                      const SizedBox(height: 12),
                      SizedBox(
                        width: double.infinity,
                        child: ElevatedButton.icon(
                          onPressed: showCreateAnnouncementDialog,
                          icon: const Icon(Icons.add),
                          label: const Text("Create Announcement"),
                          style: ElevatedButton.styleFrom(
                              backgroundColor: Colors.orange,
                              foregroundColor: Colors.white),
                        ),
                      ),
                    ],
                  ),
                ),

                const SizedBox(height: 16),

                // INVITATION
                Container(
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: Colors.blue.shade50,
                    borderRadius: BorderRadius.circular(14),
                    border: Border.all(color: Colors.blue.shade200),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Row(
                        children: [
                          Icon(Icons.mail_outline, color: Colors.blue),
                          SizedBox(width: 8),
                          Text("Invitations",
                              style: TextStyle(
                                  fontWeight: FontWeight.bold, fontSize: 16)),
                        ],
                      ),
                      const SizedBox(height: 8),
                      const Text(
                          "Invite users to this event by their email address.",
                          style:
                              TextStyle(color: Colors.black54, fontSize: 13)),
                      const SizedBox(height: 12),
                      SizedBox(
                        width: double.infinity,
                        child: ElevatedButton.icon(
                          onPressed: showCreateInvitationDialog,
                          icon: const Icon(Icons.person_add),
                          label: const Text("Send Invitation"),
                          style: ElevatedButton.styleFrom(
                              backgroundColor: Colors.blue,
                              foregroundColor: Colors.white),
                        ),
                      ),
                    ],
                  ),
                ),

                const SizedBox(height: 20),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

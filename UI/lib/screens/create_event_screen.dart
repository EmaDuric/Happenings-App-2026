import 'package:flutter/material.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';

class TicketTypeEntry {
  final nameController = TextEditingController();
  final priceController = TextEditingController();
  final quantityController = TextEditingController();

  void dispose() {
    nameController.dispose();
    priceController.dispose();
    quantityController.dispose();
  }
}

class CreateEventScreen extends StatefulWidget {
  const CreateEventScreen({super.key});

  @override
  State<CreateEventScreen> createState() => _CreateEventScreenState();
}

class _CreateEventScreenState extends State<CreateEventScreen> {
  final _nameController = TextEditingController();
  final _descriptionController = TextEditingController();
  final _imageUrlController =
      TextEditingController(); // ✅ OVDJE, ne u TicketTypeEntry

  DateTime? selectedDate;
  int? selectedCategoryId;
  int? selectedLocationId;

  List<dynamic> categories = [];
  List<dynamic> locations = [];
  List<TicketTypeEntry> ticketTypes = [TicketTypeEntry()];

  bool isLoading = false;
  bool isOrganizer = false;
  bool isLoadingDropdowns = true;

  @override
  void initState() {
    super.initState();
    checkRole();
  }

  @override
  void dispose() {
    _nameController.dispose();
    _descriptionController.dispose();
    _imageUrlController.dispose();
    for (final t in ticketTypes) {
      t.dispose();
    }
    super.dispose();
  }

  Future<void> checkRole() async {
    final org = await AuthService.isOrganizer();
    if (!org) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Only organizers can create events")),
      );
      Navigator.pushReplacementNamed(context, "/home");
      return;
    }
    setState(() => isOrganizer = true);
    await loadDropdowns();
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
        selectedCategoryId = cats.isNotEmpty ? cats.first["id"] : null;
        selectedLocationId = locs.isNotEmpty ? locs.first["id"] : null;
        isLoadingDropdowns = false;
      });
    } catch (e) {
      setState(() => isLoadingDropdowns = false);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Error loading data: $e")),
      );
    }
  }

  Future<void> pickDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: DateTime.UtcNow().add(const Duration(days: 1)),
      firstDate: DateTime.UtcNow(),
      lastDate: DateTime(2100),
    );
    if (picked != null) setState(() => selectedDate = picked);
  }

  Future<void> createEvent() async {
    if (_nameController.text.trim().isEmpty ||
        _descriptionController.text.trim().isEmpty ||
        selectedDate == null ||
        selectedCategoryId == null ||
        selectedLocationId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("All event fields are required")),
      );
      return;
    }

    for (int i = 0; i < ticketTypes.length; i++) {
      final t = ticketTypes[i];
      if (t.nameController.text.trim().isEmpty ||
          t.priceController.text.trim().isEmpty ||
          t.quantityController.text.trim().isEmpty) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("Ticket type ${i + 1}: all fields required")),
        );
        return;
      }
      if (double.tryParse(t.priceController.text.trim()) == null) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("Ticket type ${i + 1}: invalid price")),
        );
        return;
      }
      if (int.tryParse(t.quantityController.text.trim()) == null) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("Ticket type ${i + 1}: invalid quantity")),
        );
        return;
      }
    }

    setState(() => isLoading = true);

    try {
      final token = await AuthService.getToken();
      if (token == null) throw Exception("User not logged in");

      // 1. Kreiraj event
      final event = await ApiService.createEvent(
        name: _nameController.text.trim(),
        description: _descriptionController.text.trim(),
        eventDate: selectedDate!,
        eventCategoryId: selectedCategoryId!,
        locationId: selectedLocationId!,
        token: token,
      );

      final eventId = event["id"];

      // 2. Kreiraj ticket types
      for (final t in ticketTypes) {
        await ApiService.createTicketType(
          eventId: eventId,
          name: t.nameController.text.trim(),
          price: double.parse(t.priceController.text.trim()),
          availableQuantity: int.parse(t.quantityController.text.trim()),
          token: token,
        );
      }

      // 3. Dodaj sliku ako je unesena
      final imageUrl = _imageUrlController.text.trim();
      if (imageUrl.isNotEmpty) {
        await ApiService.addEventImage(
          eventId: eventId,
          imageUrl: imageUrl,
          token: token,
        );
      }

      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Event created successfully!")),
      );

      Navigator.pushReplacementNamed(context, "/events");
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Error: $e")),
      );
    } finally {
      if (mounted) setState(() => isLoading = false);
    }
  }

  Widget buildTicketTypeCard(int index) {
    final t = ticketTypes[index];
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
              Text(
                "Ticket Type ${index + 1}",
                style: const TextStyle(fontWeight: FontWeight.bold),
              ),
              if (ticketTypes.length > 1)
                IconButton(
                  icon: const Icon(Icons.delete, color: Colors.red),
                  onPressed: () => setState(() {
                    t.dispose();
                    ticketTypes.removeAt(index);
                  }),
                ),
            ],
          ),
          const SizedBox(height: 10),
          TextField(
            controller: t.nameController,
            decoration: const InputDecoration(
              labelText: "Name (e.g. VIP, Regular)",
              border: OutlineInputBorder(),
            ),
          ),
          const SizedBox(height: 10),
          Row(
            children: [
              Expanded(
                child: TextField(
                  controller: t.priceController,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(
                    labelText: "Price (KM)",
                    border: OutlineInputBorder(),
                  ),
                ),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: TextField(
                  controller: t.quantityController,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(
                    labelText: "Quantity",
                    border: OutlineInputBorder(),
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (!isOrganizer || isLoadingDropdowns) {
      return const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      );
    }

    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      appBar: AppBar(
        title: const Text("Create Event"),
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
                const Text(
                  "Create New Event",
                  style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 25),

                // EVENT POLJA
                TextField(
                  controller: _nameController,
                  decoration: const InputDecoration(
                    labelText: "Event name",
                    border: OutlineInputBorder(),
                    filled: true,
                    fillColor: Colors.white,
                  ),
                ),
                const SizedBox(height: 15),
                TextField(
                  controller: _descriptionController,
                  maxLines: 3,
                  decoration: const InputDecoration(
                    labelText: "Description",
                    border: OutlineInputBorder(),
                    filled: true,
                    fillColor: Colors.white,
                  ),
                ),
                const SizedBox(height: 15),
                DropdownButtonFormField<int>(
                  value: selectedCategoryId,
                  decoration: const InputDecoration(
                    labelText: "Category",
                    border: OutlineInputBorder(),
                    filled: true,
                    fillColor: Colors.white,
                  ),
                  items: categories.map<DropdownMenuItem<int>>((cat) {
                    return DropdownMenuItem<int>(
                      value: cat["id"],
                      child: Text(cat["name"] ?? ""),
                    );
                  }).toList(),
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
                    fillColor: Colors.white,
                  ),
                  items: locations.map<DropdownMenuItem<int>>((loc) {
                    return DropdownMenuItem<int>(
                      value: loc["id"],
                      child: Text(loc["name"] ?? ""),
                    );
                  }).toList(),
                  onChanged: (value) =>
                      setState(() => selectedLocationId = value),
                ),
                const SizedBox(height: 15),

                // DATUM
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: Colors.grey.shade400),
                  ),
                  child: Row(
                    children: [
                      const Icon(Icons.calendar_today, color: Colors.grey),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Text(
                          selectedDate == null
                              ? "No date selected"
                              : "Date: ${selectedDate!.day}.${selectedDate!.month}.${selectedDate!.year}",
                        ),
                      ),
                      ElevatedButton(
                        onPressed: pickDate,
                        child: const Text("Pick date"),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 15),

                // IMAGE URL
                TextField(
                  controller: _imageUrlController,
                  decoration: const InputDecoration(
                    labelText: "Image URL (optional)",
                    hintText: "https://example.com/image.jpg",
                    border: OutlineInputBorder(),
                    filled: true,
                    fillColor: Colors.white,
                    prefixIcon: Icon(Icons.image),
                  ),
                  onChanged: (_) => setState(() {}),
                ),

                // IMAGE PREVIEW
                if (_imageUrlController.text.trim().isNotEmpty)
                  Padding(
                    padding: const EdgeInsets.only(top: 10),
                    child: ClipRRect(
                      borderRadius: BorderRadius.circular(12),
                      child: Image.network(
                        _imageUrlController.text.trim(),
                        height: 160,
                        width: double.infinity,
                        fit: BoxFit.cover,
                        errorBuilder: (_, __, ___) => Container(
                          height: 50,
                          decoration: BoxDecoration(
                            color: Colors.grey.shade200,
                            borderRadius: BorderRadius.circular(12),
                          ),
                          child: const Center(
                            child: Text(
                              "Invalid image URL",
                              style: TextStyle(color: Colors.grey),
                            ),
                          ),
                        ),
                      ),
                    ),
                  ),

                const SizedBox(height: 30),
                const Divider(),
                const SizedBox(height: 10),

                // TICKET TYPES
                const Text(
                  "Ticket Types",
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 12),

                ...List.generate(
                  ticketTypes.length,
                  (index) => buildTicketTypeCard(index),
                ),

                TextButton.icon(
                  onPressed: () =>
                      setState(() => ticketTypes.add(TicketTypeEntry())),
                  icon: const Icon(Icons.add),
                  label: const Text("Add ticket type"),
                ),

                const SizedBox(height: 25),

                ElevatedButton(
                  onPressed: isLoading ? null : createEvent,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.green,
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                  child: isLoading
                      ? const CircularProgressIndicator(color: Colors.white)
                      : const Text(
                          "Create Event",
                          style: TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                ),
                const SizedBox(height: 20),
              ],
            ),
          ),
        ),
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: 1,
        onTap: (index) {
          if (index == 0)
            Navigator.pushReplacementNamed(context, "/home");
          else if (index == 1)
            Navigator.pushReplacementNamed(context, "/events");
          else if (index == 2)
            Navigator.pushReplacementNamed(context, "/tickets");
        },
        items: const [
          BottomNavigationBarItem(icon: Icon(Icons.home), label: "Home"),
          BottomNavigationBarItem(icon: Icon(Icons.event), label: "Events"),
          BottomNavigationBarItem(
              icon: Icon(Icons.confirmation_number), label: "Tickets"),
        ],
      ),
    );
  }
}

import 'package:flutter/material.dart';
import '../models/event_dto.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';
import 'login_screen.dart';
import 'reservation_screen.dart';
import 'event_details_screen.dart';

class EventsScreen extends StatefulWidget {
  const EventsScreen({super.key});

  @override
  State<EventsScreen> createState() => _EventsScreenState();
}

class _EventsScreenState extends State<EventsScreen> {
  List<EventDto> events = [];
  List<EventDto> filteredEvents = [];
  bool isLoading = true;
  String? errorMessage;

  // Filters
  final searchController = TextEditingController();
  String? selectedCategory;
  String? selectedLocation;
  DateTime? selectedDate;

  List<String> categories = [];
  List<String> locations = [];

  @override
  void initState() {
    super.initState();
    loadEvents();
    searchController.addListener(_applyFilters);
  }

  @override
  void dispose() {
    searchController.dispose();
    super.dispose();
  }

  Future<void> loadEvents() async {
    try {
      final token = await AuthService.getToken();
      if (token == null) throw Exception("User is not logged in.");
      final result = await ApiService.getEvents(token);
      setState(() {
        events = result;
        filteredEvents = result;
        categories = result
            .map((e) => e.categoryName ?? "")
            .where((c) => c.isNotEmpty)
            .toSet()
            .toList();
        locations = result
            .map((e) => e.locationName ?? "")
            .where((l) => l.isNotEmpty)
            .toSet()
            .toList();
        isLoading = false;
      });
    } catch (e) {
      setState(() {
        errorMessage = "Failed to load events";
        isLoading = false;
      });
    }
  }

  void _applyFilters() {
    setState(() {
      filteredEvents = events.where((e) {
        final matchesSearch = searchController.text.isEmpty ||
            e.name
                .toLowerCase()
                .contains(searchController.text.toLowerCase()) ||
            (e.description
                .toLowerCase()
                .contains(searchController.text.toLowerCase()));
        final matchesCategory =
            selectedCategory == null || e.categoryName == selectedCategory;
        final matchesLocation =
            selectedLocation == null || e.locationName == selectedLocation;
        final matchesDate = selectedDate == null ||
            (e.eventDate.year == selectedDate!.year &&
                e.eventDate.month == selectedDate!.month &&
                e.eventDate.day == selectedDate!.day);
        return matchesSearch &&
            matchesCategory &&
            matchesLocation &&
            matchesDate;
      }).toList();
    });
  }

  void _clearFilters() {
    setState(() {
      searchController.clear();
      selectedCategory = null;
      selectedLocation = null;
      selectedDate = null;
      filteredEvents = events;
    });
  }

  Future<void> _pickDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: DateTime.now(),
      firstDate: DateTime.now().subtract(const Duration(days: 365)),
      lastDate: DateTime.now().add(const Duration(days: 365 * 2)),
    );
    if (picked != null) {
      setState(() => selectedDate = picked);
      _applyFilters();
    }
  }

  Future<void> logout() async {
    await AuthService.clearToken();
    if (!mounted) return;
    Navigator.pushReplacement(
        context, MaterialPageRoute(builder: (_) => const LoginScreen()));
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      appBar: AppBar(
        title: const Text("Events"),
        backgroundColor: const Color(0xFFF4D35E),
        foregroundColor: Colors.black,
        elevation: 0,
        actions: [
          IconButton(onPressed: logout, icon: const Icon(Icons.logout))
        ],
      ),
      body: isLoading
          ? const Center(child: CircularProgressIndicator())
          : errorMessage != null
              ? Center(child: Text(errorMessage!))
              : Column(
                  children: [
                    // FILTERS
                    Container(
                      color: const Color(0xFFF4D35E),
                      padding: const EdgeInsets.fromLTRB(12, 0, 12, 12),
                      child: Column(
                        children: [
                          // Search
                          TextField(
                            controller: searchController,
                            decoration: InputDecoration(
                              hintText: "Search events...",
                              prefixIcon: const Icon(Icons.search),
                              suffixIcon: searchController.text.isNotEmpty
                                  ? IconButton(
                                      icon: const Icon(Icons.clear),
                                      onPressed: () {
                                        searchController.clear();
                                        _applyFilters();
                                      })
                                  : null,
                              filled: true,
                              fillColor: Colors.white,
                              border: OutlineInputBorder(
                                  borderRadius: BorderRadius.circular(30),
                                  borderSide: BorderSide.none),
                              contentPadding:
                                  const EdgeInsets.symmetric(vertical: 10),
                            ),
                          ),
                          const SizedBox(height: 8),
                          // Category + Location + Date
                          Row(
                            children: [
                              Expanded(
                                child: DropdownButtonFormField<String>(
                                  initialValue: selectedCategory,
                                  hint: const Text("Category",
                                      style: TextStyle(fontSize: 12)),
                                  decoration: InputDecoration(
                                    filled: true,
                                    fillColor: Colors.white,
                                    border: OutlineInputBorder(
                                        borderRadius: BorderRadius.circular(12),
                                        borderSide: BorderSide.none),
                                    contentPadding: const EdgeInsets.symmetric(
                                        horizontal: 10, vertical: 6),
                                  ),
                                  items: [
                                    const DropdownMenuItem(
                                        value: null,
                                        child: Text("All",
                                            style: TextStyle(fontSize: 12))),
                                    ...categories.map((c) => DropdownMenuItem(
                                        value: c,
                                        child: Text(c,
                                            style: const TextStyle(
                                                fontSize: 12)))),
                                  ],
                                  onChanged: (v) {
                                    setState(() => selectedCategory = v);
                                    _applyFilters();
                                  },
                                ),
                              ),
                              const SizedBox(width: 8),
                              Expanded(
                                child: DropdownButtonFormField<String>(
                                  initialValue: selectedLocation,
                                  hint: const Text("Location",
                                      style: TextStyle(fontSize: 12)),
                                  decoration: InputDecoration(
                                    filled: true,
                                    fillColor: Colors.white,
                                    border: OutlineInputBorder(
                                        borderRadius: BorderRadius.circular(12),
                                        borderSide: BorderSide.none),
                                    contentPadding: const EdgeInsets.symmetric(
                                        horizontal: 10, vertical: 6),
                                  ),
                                  items: [
                                    const DropdownMenuItem(
                                        value: null,
                                        child: Text("All",
                                            style: TextStyle(fontSize: 12))),
                                    ...locations.map((l) => DropdownMenuItem(
                                        value: l,
                                        child: Text(l,
                                            style: const TextStyle(
                                                fontSize: 12)))),
                                  ],
                                  onChanged: (v) {
                                    setState(() => selectedLocation = v);
                                    _applyFilters();
                                  },
                                ),
                              ),
                              const SizedBox(width: 8),
                              GestureDetector(
                                onTap: _pickDate,
                                child: Container(
                                  padding: const EdgeInsets.symmetric(
                                      horizontal: 10, vertical: 10),
                                  decoration: BoxDecoration(
                                      color: Colors.white,
                                      borderRadius: BorderRadius.circular(12)),
                                  child: Row(
                                    children: [
                                      const Icon(Icons.calendar_today,
                                          size: 16),
                                      if (selectedDate != null) ...[
                                        const SizedBox(width: 4),
                                        Text(
                                            "${selectedDate!.day}.${selectedDate!.month}",
                                            style:
                                                const TextStyle(fontSize: 12)),
                                      ]
                                    ],
                                  ),
                                ),
                              ),
                              if (selectedCategory != null ||
                                  selectedLocation != null ||
                                  selectedDate != null ||
                                  searchController.text.isNotEmpty)
                                IconButton(
                                    icon: const Icon(Icons.clear, size: 18),
                                    onPressed: _clearFilters),
                            ],
                          ),
                          const SizedBox(height: 4),
                          Text("${filteredEvents.length} events found",
                              style: const TextStyle(
                                  fontSize: 12, color: Colors.black54)),
                        ],
                      ),
                    ),

                    // LIST
                    Expanded(
                      child: filteredEvents.isEmpty
                          ? const Center(child: Text("No events found"))
                          : ListView.builder(
                              padding: const EdgeInsets.all(12),
                              itemCount: filteredEvents.length,
                              itemBuilder: (context, index) {
                                final event = filteredEvents[index];
                                return Card(
                                  elevation: 3,
                                  margin: const EdgeInsets.only(bottom: 12),
                                  shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(16)),
                                  child: Column(
                                    crossAxisAlignment:
                                        CrossAxisAlignment.start,
                                    children: [
                                      ClipRRect(
                                        borderRadius: const BorderRadius.only(
                                            topLeft: Radius.circular(16),
                                            topRight: Radius.circular(16)),
                                        child: event.imageUrl != null &&
                                                event.imageUrl!.isNotEmpty
                                            ? Image.network(event.imageUrl!,
                                                height: 160,
                                                width: double.infinity,
                                                fit: BoxFit.cover,
                                                errorBuilder: (_, __, ___) =>
                                                    Container(
                                                        height: 160,
                                                        color: Colors
                                                            .grey.shade300,
                                                        child: const Icon(
                                                            Icons.event,
                                                            size: 60,
                                                            color:
                                                                Colors.grey)))
                                            : Container(
                                                height: 160,
                                                color: Colors.grey.shade300,
                                                child: const Icon(Icons.event,
                                                    size: 60,
                                                    color: Colors.grey)),
                                      ),
                                      Padding(
                                        padding: const EdgeInsets.all(16),
                                        child: Column(
                                          crossAxisAlignment:
                                              CrossAxisAlignment.start,
                                          children: [
                                            Text(event.name,
                                                style: const TextStyle(
                                                    fontSize: 20,
                                                    fontWeight:
                                                        FontWeight.bold)),
                                            if (event.categoryName != null ||
                                                event.locationName != null) ...[
                                              const SizedBox(height: 4),
                                              Row(children: [
                                                if (event.categoryName !=
                                                    null) ...[
                                                  const Icon(Icons.category,
                                                      size: 14,
                                                      color: Colors.black54),
                                                  const SizedBox(width: 4),
                                                  Text(event.categoryName!,
                                                      style: const TextStyle(
                                                          fontSize: 12,
                                                          color:
                                                              Colors.black54)),
                                                  const SizedBox(width: 12),
                                                ],
                                                if (event.locationName !=
                                                    null) ...[
                                                  const Icon(Icons.location_on,
                                                      size: 14,
                                                      color: Colors.black54),
                                                  const SizedBox(width: 4),
                                                  Text(event.locationName!,
                                                      style: const TextStyle(
                                                          fontSize: 12,
                                                          color:
                                                              Colors.black54)),
                                                ],
                                              ]),
                                            ],
                                            const SizedBox(height: 8),
                                            Text(event.description,
                                                maxLines: 2,
                                                overflow:
                                                    TextOverflow.ellipsis),
                                            const SizedBox(height: 8),
                                            Row(
                                              children: [
                                                Expanded(
                                                  child: OutlinedButton(
                                                    onPressed: () => Navigator.push(
                                                        context,
                                                        MaterialPageRoute(
                                                            builder: (_) =>
                                                                EventDetailsScreen(
                                                                    event:
                                                                        event))),
                                                    style: OutlinedButton.styleFrom(
                                                        padding:
                                                            const EdgeInsets
                                                                .symmetric(
                                                                vertical: 14),
                                                        shape: RoundedRectangleBorder(
                                                            borderRadius:
                                                                BorderRadius
                                                                    .circular(
                                                                        12))),
                                                    child:
                                                        const Text("Details"),
                                                  ),
                                                ),
                                                const SizedBox(width: 8),
                                                Expanded(
                                                  child: ElevatedButton(
                                                    onPressed: () => Navigator.push(
                                                        context,
                                                        MaterialPageRoute(
                                                            builder: (_) =>
                                                                ReservationScreen(
                                                                    event:
                                                                        event,
                                                                    goToPayment:
                                                                        false))),
                                                    style: ElevatedButton.styleFrom(
                                                        backgroundColor:
                                                            Colors.orange,
                                                        foregroundColor:
                                                            Colors.white,
                                                        padding:
                                                            const EdgeInsets
                                                                .symmetric(
                                                                vertical: 14),
                                                        shape: RoundedRectangleBorder(
                                                            borderRadius:
                                                                BorderRadius
                                                                    .circular(
                                                                        12))),
                                                    child: const Text("Reserve",
                                                        style: TextStyle(
                                                            fontWeight:
                                                                FontWeight
                                                                    .bold)),
                                                  ),
                                                ),
                                                const SizedBox(width: 8),
                                                Expanded(
                                                  child: ElevatedButton(
                                                    onPressed: () => Navigator.push(
                                                        context,
                                                        MaterialPageRoute(
                                                            builder: (_) =>
                                                                ReservationScreen(
                                                                    event:
                                                                        event,
                                                                    goToPayment:
                                                                        true))),
                                                    style: ElevatedButton.styleFrom(
                                                        backgroundColor:
                                                            Colors.green,
                                                        foregroundColor:
                                                            Colors.white,
                                                        padding:
                                                            const EdgeInsets
                                                                .symmetric(
                                                                vertical: 14),
                                                        shape: RoundedRectangleBorder(
                                                            borderRadius:
                                                                BorderRadius
                                                                    .circular(
                                                                        12))),
                                                    child: const Text(
                                                        "Reserve & Pay",
                                                        style: TextStyle(
                                                            fontWeight:
                                                                FontWeight
                                                                    .bold)),
                                                  ),
                                                ),
                                              ],
                                            ),
                                          ],
                                        ),
                                      ),
                                    ],
                                  ),
                                );
                              },
                            ),
                    ),
                  ],
                ),
    );
  }
}

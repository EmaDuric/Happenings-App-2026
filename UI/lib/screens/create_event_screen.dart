import 'package:flutter/material.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';

class CreateEventScreen extends StatefulWidget {
  const CreateEventScreen({super.key});

  @override
  State<CreateEventScreen> createState() => _CreateEventScreenState();
}

class _CreateEventScreenState extends State<CreateEventScreen> {
  final _nameController = TextEditingController();
  final _descriptionController = TextEditingController();

  DateTime? selectedDate;

  int selectedCategoryId = 4;
  int selectedLocationId = 2;

  bool isLoading = false;

  Future<void> pickDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: DateTime.now().add(const Duration(days: 1)),
      firstDate: DateTime.now(),
      lastDate: DateTime(2100),
    );

    if (picked != null) {
      setState(() {
        selectedDate = picked;
      });
    }
  }

  Future<void> createEvent() async {
    if (_nameController.text.trim().isEmpty ||
        _descriptionController.text.trim().isEmpty ||
        selectedDate == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("All fields are required")),
      );
      return;
    }

    setState(() => isLoading = true);

    try {
      final token = await AuthService.getToken();

      if (token == null) {
        throw Exception("User not logged in");
      }

      await ApiService.createEvent(
        name: _nameController.text.trim(),
        description: _descriptionController.text.trim(),
        eventDate: selectedDate!,
        eventCategoryId: selectedCategoryId,
        locationId: selectedLocationId,
        token: token,
      );

      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Event created successfully")),
      );

      Navigator.pushReplacementNamed(context, "/events");
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("$e")),
      );
    } finally {
      if (mounted) {
        setState(() => isLoading = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text("Create Event"),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            TextField(
              controller: _nameController,
              decoration: const InputDecoration(
                labelText: "Event name",
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _descriptionController,
              maxLines: 3,
              decoration: const InputDecoration(
                labelText: "Description",
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 12),
            DropdownButtonFormField<int>(
              value: selectedCategoryId,
              decoration: const InputDecoration(
                labelText: "Category",
                border: OutlineInputBorder(),
              ),
              items: const [
                DropdownMenuItem(value: 4, child: Text("Music")),
                DropdownMenuItem(value: 5, child: Text("Technology")),
                DropdownMenuItem(value: 6, child: Text("Sport")),
              ],
              onChanged: (value) {
                setState(() {
                  selectedCategoryId = value!;
                });
              },
            ),
            const SizedBox(height: 12),
            DropdownButtonFormField<int>(
              value: selectedLocationId,
              decoration: const InputDecoration(
                labelText: "Location",
                border: OutlineInputBorder(),
              ),
              items: const [
                DropdownMenuItem(value: 2, child: Text("Arena Sarajevo")),
                DropdownMenuItem(value: 3, child: Text("Mostar Hall")),
                DropdownMenuItem(value: 4, child: Text("Tuzla Center")),
              ],
              onChanged: (value) {
                setState(() {
                  selectedLocationId = value!;
                });
              },
            ),
            const SizedBox(height: 12),
            Row(
              children: [
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
            const SizedBox(height: 24),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: isLoading ? null : createEvent,
                child: isLoading
                    ? const CircularProgressIndicator(color: Colors.white)
                    : const Text("Create Event"),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

import 'package:flutter/material.dart';
import '../services/api_service.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _usernameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();

  bool isLoading = false;
  bool requestOrganizer = false; // ← checkbox stanje

  Future<void> register() async {
    final username = _usernameController.text.trim();
    final email = _emailController.text.trim();
    final password = _passwordController.text;

    if (username.isEmpty || email.isEmpty || password.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("All fields are required.")),
      );
      return;
    }

    if (!RegExp(r'^[^@]+@[^@]+\.[^@]+$').hasMatch(email)) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Please enter a valid email address.")),
      );
      return;
    }

    if (password.length < 6) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text("Password must be at least 6 characters.")),
      );
      return;
    }

    setState(() => isLoading = true);

    try {
      // 1. Registruj korisnika
      final result = await ApiService.registerUser(
        username: username,
        email: email,
        password: password,
      );

      // 2. Ako je korisnik čekirao organizer request, logiraj se i pošalji zahtjev
      if (requestOrganizer) {
        try {
          final loginResult = await ApiService.login(email, password);
          final token = loginResult["token"] as String?;

          if (token != null && token.isNotEmpty) {
            await ApiService.sendOrganizerRequest(token: token);
          }
        } catch (e) {
          // Prikaži grešku korisniku umjesto da je sakriješ
          if (mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(content: Text("Organizer request failed: $e")),
            );
          }
        }
      }

      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text(requestOrganizer
                ? "Account created! Organizer request sent to admin."
                : "Account created successfully")),
      );

      Navigator.pop(context);
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("$e")),
      );
    } finally {
      if (mounted) setState(() => isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      body: Center(
        child: Container(
          width: 350,
          padding: const EdgeInsets.all(24),
          decoration: BoxDecoration(
            border: Border.all(color: Colors.orange, width: 2),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.person_add, size: 80),
              const SizedBox(height: 20),
              const Text("REGISTER",
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 20)),
              const SizedBox(height: 20),
              TextField(
                controller: _usernameController,
                decoration: const InputDecoration(
                    hintText: "Username",
                    filled: true,
                    fillColor: Colors.white),
              ),
              const SizedBox(height: 10),
              TextField(
                controller: _emailController,
                keyboardType: TextInputType.emailAddress,
                decoration: const InputDecoration(
                    hintText: "Email", filled: true, fillColor: Colors.white),
              ),
              const SizedBox(height: 10),
              TextField(
                controller: _passwordController,
                obscureText: true,
                decoration: const InputDecoration(
                    hintText: "Password",
                    filled: true,
                    fillColor: Colors.white),
              ),
              const SizedBox(height: 14),

              // Organizer request checkbox
              Container(
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.orange.shade200),
                ),
                child: CheckboxListTile(
                  value: requestOrganizer,
                  onChanged: (val) =>
                      setState(() => requestOrganizer = val ?? false),
                  title: const Text("Request organizer role",
                      style:
                          TextStyle(fontSize: 14, fontWeight: FontWeight.w500)),
                  subtitle: const Text("Admin will review your request",
                      style: TextStyle(fontSize: 11, color: Colors.black54)),
                  controlAffinity: ListTileControlAffinity.leading,
                  activeColor: Colors.orange,
                  contentPadding: const EdgeInsets.symmetric(horizontal: 8),
                  dense: true,
                ),
              ),

              const SizedBox(height: 20),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: isLoading ? null : register,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.grey[300],
                    foregroundColor: Colors.black,
                  ),
                  child: isLoading
                      ? const CircularProgressIndicator()
                      : const Text("CREATE ACCOUNT"),
                ),
              ),
              const SizedBox(height: 10),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: () => Navigator.pop(context),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.grey[300],
                    foregroundColor: Colors.black,
                  ),
                  child: const Text("BACK TO LOGIN"),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

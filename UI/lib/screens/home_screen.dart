import 'package:flutter/material.dart';

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text("Happenings"),
        centerTitle: true,
      ),
      body: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Text(
              "Welcome to Happenings",
              style: TextStyle(
                fontSize: 24,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 40),

            // LOGIN
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: () {
                  Navigator.pushNamed(context, "/");
                },
                child: const Text("Login"),
              ),
            ),

            const SizedBox(height: 10),

            // REGISTER
            SizedBox(
              width: double.infinity,
              child: OutlinedButton(
                onPressed: () {
                  Navigator.pushNamed(context, "/register");
                },
                child: const Text("Create an Account"),
              ),
            ),

            const SizedBox(height: 30),

            // FIND EVENTS
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: () {
                  Navigator.pushNamed(context, "/events");
                },
                child: const Text("Find Your Next Event"),
              ),
            ),

            const SizedBox(height: 10),

            // CREATE EVENT (za organizatora)
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: () {
                  Navigator.pushNamed(context, "/create-event");
                },
                child: const Text("Create Event"),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

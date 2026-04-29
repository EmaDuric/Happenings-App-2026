import 'package:flutter/material.dart';
import 'screens/login_screen.dart';
import 'screens/home_screen.dart';
import 'screens/payment_screen.dart';
import 'screens/notification_screen.dart';
import 'screens/create_event_screen.dart';
import 'screens/tickets_screen.dart';
import 'services/auth_service.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      home: const RootScreen(),
      routes: {
        "/home": (context) => const HomeScreen(),
        "/tickets": (context) => const TicketsScreen(),
        "/notifications": (context) => const NotificationsScreen(),
        "/create-event": (context) => const CreateEventScreen(),
      },
    );
  }
}

class RootScreen extends StatefulWidget {
  const RootScreen({super.key});

  @override
  State<RootScreen> createState() => _RootScreenState();
}

class _RootScreenState extends State<RootScreen> {
  bool isLoading = true;
  bool isLoggedIn = false;

  @override
  void initState() {
    super.initState();
    checkLogin();
  }

  Future<void> checkLogin() async {
    final logged = await AuthService.isLoggedIn();

    setState(() {
      isLoggedIn = logged;
      isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    if (isLoading) {
      return const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      );
    }

    return isLoggedIn ? const HomeScreen() : const LoginScreen();
  }
}

import 'package:flutter/material.dart';
import 'screens/login_screen.dart';
import 'screens/events_screen.dart';
import 'screens/event_details_screen.dart';
import 'screens/tickets_screen.dart';
import 'screens/register_screen.dart';
import 'screens/home_screen.dart';
import 'screens/create_event_screen.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Happenings',
      debugShowCheckedModeBanner: false,
      initialRoute: '/home',
      routes: {
        '/home': (context) => const HomeScreen(),
        '/': (context) => const LoginScreen(),
        '/events': (context) => const EventsScreen(),
        "/event-details": (context) => const EventDetailsScreen(),
        '/tickets': (context) => const TicketsScreen(),
        '/register': (context) => const RegisterScreen(),
        '/create-event': (context) => const CreateEventScreen()
      },
    );
  }
}

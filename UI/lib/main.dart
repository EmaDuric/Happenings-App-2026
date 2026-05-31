import 'package:flutter/material.dart';
import 'screens/login_screen.dart';
import 'screens/home_screen.dart';
import 'screens/notification_screen.dart';
import 'screens/create_event_screen.dart';
import 'screens/tickets_screen.dart';
import 'services/auth_service.dart';
import 'screens/my_reservations_screen.dart';
import 'screens/profile_screen.dart';
import 'screens/payment_success_screen.dart';
import 'screens/my_events_screen.dart';
import 'package:flutter_stripe/flutter_stripe.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  const stripeKey =
      String.fromEnvironment('STRIPE_PUBLISHABLE_KEY', defaultValue: '');
  if (stripeKey.isNotEmpty) {
    Stripe.publishableKey = stripeKey;
  }
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      home: const RootScreen(),
      onGenerateRoute: (settings) {
        final uri = Uri.parse(settings.name ?? "/");

        if (uri.path == "/payment-success") {
          return MaterialPageRoute(
            builder: (_) => const PaymentSuccessScreen(),
            settings: settings,
          );
        }

        if (uri.path == "/payment-cancel") {
          return MaterialPageRoute(
            builder: (_) => const PaymentCancelScreen(),
            settings: settings,
          );
        }

        switch (uri.path) {
          case "/home":
            return MaterialPageRoute(builder: (_) => const HomeScreen());
          case "/tickets":
            return MaterialPageRoute(builder: (_) => const TicketsScreen());
          case "/notifications":
            return MaterialPageRoute(
                builder: (_) => const NotificationsScreen());
          case "/create-event":
            return MaterialPageRoute(builder: (_) => const CreateEventScreen());
          case "/my-reservations":
            return MaterialPageRoute(
                builder: (_) => const MyReservationsScreen());
          case "/profile":
            return MaterialPageRoute(builder: (_) => const ProfileScreen());
          case "/my-events":
            return MaterialPageRoute(builder: (_) => const MyEventsScreen());
          default:
            return MaterialPageRoute(builder: (_) => const RootScreen());
        }
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
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }
    return isLoggedIn ? const HomeScreen() : const LoginScreen();
  }
}

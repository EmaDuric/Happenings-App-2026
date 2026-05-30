import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';

class PaymentSuccessScreen extends StatefulWidget {
  const PaymentSuccessScreen({super.key});

  @override
  State<PaymentSuccessScreen> createState() => _PaymentSuccessScreenState();
}

class _PaymentSuccessScreenState extends State<PaymentSuccessScreen> {
  bool isLoading = true;
  bool isSuccess = false;
  String? errorMessage;

  @override
  void initState() {
    super.initState();
    _capturePayment();
  }

  Future<void> _capturePayment() async {
    try {
      String? token;
      String? reservationIdStr;

      if (kIsWeb) {
        // Flutter web sa hash routing — parametri su u Uri.base fragment
        // URL: localhost:64020/#/payment-success?reservationId=95&token=XXX
        final fullUrl = Uri.base.toString();
        final hashIndex = fullUrl.indexOf('#');
        if (hashIndex != -1) {
          final fragment = fullUrl.substring(hashIndex + 1);
          final fragmentUri = Uri.parse(fragment);
          token = fragmentUri.queryParameters["token"];
          reservationIdStr = fragmentUri.queryParameters["reservationId"];
        }
      } else {
        // Mobile — normalni URL parametri
        final uri = Uri.base;
        token = uri.queryParameters["token"];
        reservationIdStr = uri.queryParameters["reservationId"];
      }

      if (token == null || token.isEmpty) {
        setState(() {
          isLoading = false;
          errorMessage = "Payment token not found in URL";
        });
        return;
      }

      if (reservationIdStr == null) {
        setState(() {
          isLoading = false;
          errorMessage = "Reservation ID not found in URL";
        });
        return;
      }

      final reservationId = int.tryParse(reservationIdStr);
      if (reservationId == null) {
        setState(() {
          isLoading = false;
          errorMessage = "Invalid reservation ID";
        });
        return;
      }

      final authToken = await AuthService.getToken();
      if (authToken == null) {
        setState(() {
          isLoading = false;
          errorMessage = "Not logged in";
        });
        return;
      }

      // Server-side capture — verifikacija na backendu
      await ApiService.capturePayPalOrder(
        orderId: token,
        reservationId: reservationId,
        token: authToken,
      );

      setState(() {
        isLoading = false;
        isSuccess = true;
      });
    } catch (e) {
      setState(() {
        isLoading = false;
        errorMessage = e.toString();
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      body: Center(
        child: Container(
          width: 350,
          padding: const EdgeInsets.all(32),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(20),
          ),
          child: isLoading
              ? const Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    CircularProgressIndicator(),
                    SizedBox(height: 20),
                    Text("Verifying payment...",
                        style: TextStyle(fontSize: 16)),
                  ],
                )
              : isSuccess
                  ? Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.check_circle,
                            color: Colors.green, size: 80),
                        const SizedBox(height: 20),
                        const Text("Payment Successful!",
                            style: TextStyle(
                                fontSize: 22, fontWeight: FontWeight.bold)),
                        const SizedBox(height: 10),
                        const Text("Your ticket has been created.",
                            style: TextStyle(color: Colors.black54),
                            textAlign: TextAlign.center),
                        const SizedBox(height: 24),
                        SizedBox(
                          width: double.infinity,
                          child: ElevatedButton(
                            onPressed: () => Navigator.pushNamedAndRemoveUntil(
                                context, "/tickets", (route) => false),
                            style: ElevatedButton.styleFrom(
                              backgroundColor: Colors.green,
                              foregroundColor: Colors.white,
                              padding: const EdgeInsets.symmetric(vertical: 14),
                              shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(12)),
                            ),
                            child: const Text("View My Tickets",
                                style: TextStyle(fontWeight: FontWeight.bold)),
                          ),
                        ),
                        const SizedBox(height: 12),
                        TextButton(
                          onPressed: () => Navigator.pushNamedAndRemoveUntil(
                              context, "/home", (route) => false),
                          child: const Text("Back to Home"),
                        ),
                      ],
                    )
                  : Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.error, color: Colors.red, size: 80),
                        const SizedBox(height: 20),
                        const Text("Payment Failed",
                            style: TextStyle(
                                fontSize: 22, fontWeight: FontWeight.bold)),
                        const SizedBox(height: 10),
                        Text(errorMessage ?? "Unknown error",
                            style: const TextStyle(color: Colors.black54),
                            textAlign: TextAlign.center),
                        const SizedBox(height: 24),
                        SizedBox(
                          width: double.infinity,
                          child: ElevatedButton(
                            onPressed: () => Navigator.pushNamedAndRemoveUntil(
                                context, "/home", (route) => false),
                            style: ElevatedButton.styleFrom(
                              backgroundColor: Colors.orange,
                              foregroundColor: Colors.white,
                              padding: const EdgeInsets.symmetric(vertical: 14),
                            ),
                            child: const Text("Back to Home"),
                          ),
                        ),
                      ],
                    ),
        ),
      ),
    );
  }
}

class PaymentCancelScreen extends StatelessWidget {
  const PaymentCancelScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      body: Center(
        child: Container(
          width: 350,
          padding: const EdgeInsets.all(32),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(20),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.cancel, color: Colors.orange, size: 80),
              const SizedBox(height: 20),
              const Text("Payment Cancelled",
                  style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold)),
              const SizedBox(height: 10),
              const Text("You cancelled the PayPal payment.",
                  style: TextStyle(color: Colors.black54),
                  textAlign: TextAlign.center),
              const SizedBox(height: 24),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: () => Navigator.pushNamedAndRemoveUntil(
                      context, "/home", (route) => false),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.orange,
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12)),
                  ),
                  child: const Text("Back to Home"),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

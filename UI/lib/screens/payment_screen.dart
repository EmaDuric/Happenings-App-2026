import 'package:flutter/material.dart';
import 'package:flutter_stripe/flutter_stripe.dart';
import 'package:url_launcher/url_launcher.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';
import 'tickets_screen.dart';

class PaymentScreen extends StatefulWidget {
  final int reservationId;

  const PaymentScreen({super.key, required this.reservationId});

  @override
  State<PaymentScreen> createState() => _PaymentScreenState();
}

class _PaymentScreenState extends State<PaymentScreen> {
  String selectedMethod = "Card";
  bool isLoading = false;
  bool isPaid = false;

  Future<void> payWithStripe() async {
    setState(() => isLoading = true);
    try {
      final token = await AuthService.getToken();
      if (token == null) throw Exception("Not logged in");

      final result = await ApiService.createStripePaymentIntent(
        reservationId: widget.reservationId,
        token: token,
      );

      final clientSecret = result["clientSecret"] as String;
      final publishableKey = result["publishableKey"] as String;

      Stripe.publishableKey = publishableKey;

      // Ispravljeni naziv parametra
      await Stripe.instance.initPaymentSheet(
        paymentSheetParameters: SetupPaymentSheetParameters(
            paymentIntentClientSecret: clientSecret,
            merchantDisplayName: "Happenings",
            style: ThemeMode.light),
      );

      await Stripe.instance.presentPaymentSheet();

      // Server-side verifikacija
      final paymentIntentId = clientSecret.split("_secret_")[0];
      await ApiService.confirmStripePayment(
        paymentIntentId: paymentIntentId,
        reservationId: widget.reservationId,
        token: token,
      );

      if (!mounted) return;
      setState(() => isPaid = true);
      _showSuccessDialog();
    } on StripeException catch (e) {
      if (!mounted) return;
      if (e.error.code == FailureCode.Canceled) {
        ScaffoldMessenger.of(context)
            .showSnackBar(const SnackBar(content: Text("Payment cancelled.")));
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text("Stripe error: ${e.error.message}")));
      }
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text("Error: $e")));
    } finally {
      if (mounted) setState(() => isLoading = false);
    }
  }

  Future<void> payWithPayPal() async {
    setState(() => isLoading = true);
    try {
      final token = await AuthService.getToken();
      if (token == null) throw Exception("Not logged in");

      final approvalUrl = await ApiService.createPayPalOrder(
        reservationId: widget.reservationId,
        token: token,
      );

      final uri = Uri.parse(approvalUrl);
      if (await canLaunchUrl(uri)) {
        await launchUrl(uri, mode: LaunchMode.externalApplication);
      }
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text("Error: $e")));
    } finally {
      if (mounted) setState(() => isLoading = false);
    }
  }

  void _showSuccessDialog() {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (_) => AlertDialog(
        title: const Text("Payment successful! ✅"),
        content: const Text("Your ticket has been created."),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              Navigator.pushReplacement(context,
                  MaterialPageRoute(builder: (_) => const TicketsScreen()));
            },
            child: const Text("View tickets"),
          ),
        ],
      ),
    );
  }

  Widget _paymentMethodCard(String method, IconData icon) {
    final selected = selectedMethod == method;
    return Expanded(
      child: GestureDetector(
        onTap: isPaid ? null : () => setState(() => selectedMethod = method),
        child: Container(
          height: 80,
          decoration: BoxDecoration(
            color: selected ? Colors.green : Colors.white,
            borderRadius: BorderRadius.circular(16),
            border: Border.all(
                color: selected ? Colors.green : Colors.black26, width: 2),
          ),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(icon, color: selected ? Colors.white : Colors.black),
              const SizedBox(height: 6),
              Text(method,
                  style: TextStyle(
                      color: selected ? Colors.white : Colors.black,
                      fontWeight: FontWeight.bold)),
            ],
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      appBar: AppBar(
        title: const Text("Payment"),
        backgroundColor: const Color(0xFFF4D35E),
        foregroundColor: Colors.black,
        elevation: 0,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(18),
        child: Column(
          children: [
            Container(
              width: double.infinity,
              margin: const EdgeInsets.symmetric(vertical: 20),
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(20),
                gradient: const LinearGradient(
                    colors: [Color(0xFF2E1065), Color(0xFF7E22CE)]),
              ),
              child: const Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text("HAPPENINGS PAYMENT",
                      style: TextStyle(color: Colors.white70)),
                  SizedBox(height: 30),
                  Text("SECURE CHECKOUT",
                      style: TextStyle(
                          color: Colors.white, fontSize: 18, letterSpacing: 2)),
                  SizedBox(height: 18),
                  Text("Stripe / PayPal",
                      style: TextStyle(color: Colors.white70)),
                ],
              ),
            ),
            Row(children: [
              _paymentMethodCard("Card", Icons.credit_card),
              const SizedBox(width: 12),
              _paymentMethodCard("PayPal", Icons.account_balance_wallet),
            ]),
            if (selectedMethod == "Card") ...[
              const SizedBox(height: 16),
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: Colors.grey.shade200),
                ),
                child: const Row(
                  children: [
                    Icon(Icons.lock, color: Colors.green, size: 20),
                    SizedBox(width: 10),
                    Expanded(
                      child: Text(
                        "Secure card payment via Stripe. Your card details are handled securely.",
                        style: TextStyle(fontSize: 13, color: Colors.black54),
                      ),
                    ),
                  ],
                ),
              ),
            ],
            if (selectedMethod == "PayPal") ...[
              const SizedBox(height: 16),
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: Colors.grey.shade200),
                ),
                child: const Row(
                  children: [
                    Icon(Icons.info_outline, color: Colors.blue, size: 20),
                    SizedBox(width: 10),
                    Expanded(
                      child: Text(
                        "You will be redirected to PayPal to complete your payment and returned to the app.",
                        style: TextStyle(fontSize: 13, color: Colors.black54),
                      ),
                    ),
                  ],
                ),
              ),
            ],
            const SizedBox(height: 24),
            if (isPaid)
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                    color: Colors.green,
                    borderRadius: BorderRadius.circular(16)),
                child: const Center(
                    child: Text("PAID ✅",
                        style: TextStyle(
                            color: Colors.white,
                            fontWeight: FontWeight.bold,
                            fontSize: 18))),
              )
            else
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: isLoading
                      ? null
                      : (selectedMethod == "Card"
                          ? payWithStripe
                          : payWithPayPal),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.green,
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12)),
                  ),
                  child: isLoading
                      ? const CircularProgressIndicator(color: Colors.white)
                      : Text(
                          selectedMethod == "Card"
                              ? "PAY WITH CARD"
                              : "PAY WITH PAYPAL",
                          style: const TextStyle(
                              fontSize: 16, fontWeight: FontWeight.bold)),
                ),
              ),
          ],
        ),
      ),
    );
  }
}

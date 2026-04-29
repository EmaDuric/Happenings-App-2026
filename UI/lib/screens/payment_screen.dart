import 'package:flutter/material.dart';
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

  final cardNumberController = TextEditingController();
  final expiryController = TextEditingController();
  final cvvController = TextEditingController();

  Future<void> pay() async {
    if (selectedMethod == "Card") {
      if (cardNumberController.text.trim().isEmpty ||
          expiryController.text.trim().isEmpty ||
          cvvController.text.trim().isEmpty) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Please enter card details")),
        );
        return;
      }
    }

    setState(() => isLoading = true);

    try {
      final token = await AuthService.getToken();

      if (token == null) {
        throw Exception("User not logged in");
      }

      await ApiService.confirmPayment(
        reservationId: widget.reservationId,
        method: selectedMethod,
        token: token,
      );

      if (!mounted) return;

      setState(() {
        isPaid = true;
      });

      showDialog(
        context: context,
        barrierDismissible: false,
        builder: (_) => AlertDialog(
          title: const Text("Payment successful"),
          content: const Text("Your ticket has been created."),
          actions: [
            TextButton(
              onPressed: () {
                Navigator.pop(context);

                Navigator.pushReplacement(
                  context,
                  MaterialPageRoute(
                    builder: (_) => const TicketsScreen(),
                  ),
                );
              },
              child: const Text("View tickets"),
            ),
          ],
        ),
      );
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Payment failed: $e")),
      );
    } finally {
      if (mounted) {
        setState(() => isLoading = false);
      }
    }
  }

  Widget paymentMethodCard(String method, IconData icon) {
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
              color: selected ? Colors.green : Colors.black26,
              width: 2,
            ),
          ),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                icon,
                color: selected ? Colors.white : Colors.black,
              ),
              const SizedBox(height: 6),
              Text(
                method,
                style: TextStyle(
                  color: selected ? Colors.white : Colors.black,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget cardForm() {
    if (selectedMethod != "Card") return const SizedBox.shrink();

    return Column(
      children: [
        const SizedBox(height: 16),
        TextField(
          controller: cardNumberController,
          keyboardType: TextInputType.number,
          decoration: const InputDecoration(
            labelText: "Card number",
            hintText: "4242 4242 4242 4242",
            border: OutlineInputBorder(),
          ),
        ),
        const SizedBox(height: 12),
        Row(
          children: [
            Expanded(
              child: TextField(
                controller: expiryController,
                decoration: const InputDecoration(
                  labelText: "MM/YY",
                  border: OutlineInputBorder(),
                ),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: TextField(
                controller: cvvController,
                keyboardType: TextInputType.number,
                obscureText: true,
                decoration: const InputDecoration(
                  labelText: "CVV",
                  border: OutlineInputBorder(),
                ),
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget cardPreview() {
    return Container(
      width: double.infinity,
      margin: const EdgeInsets.symmetric(vertical: 20),
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(20),
        gradient: const LinearGradient(
          colors: [Color(0xFF2E1065), Color(0xFF7E22CE)],
        ),
      ),
      child: const Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            "HAPPENINGS CARD",
            style: TextStyle(color: Colors.white70),
          ),
          SizedBox(height: 30),
          Text(
            "**** **** **** 4242",
            style: TextStyle(
              color: Colors.white,
              fontSize: 20,
              letterSpacing: 2,
            ),
          ),
          SizedBox(height: 18),
          Text(
            "CARD / PAYPAL PAYMENT",
            style: TextStyle(color: Colors.white70),
          ),
        ],
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
            cardPreview(),
            Row(
              children: [
                paymentMethodCard("Card", Icons.credit_card),
                const SizedBox(width: 12),
                paymentMethodCard("PayPal", Icons.account_balance_wallet),
              ],
            ),
            cardForm(),
            const SizedBox(height: 24),
            if (isPaid)
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Colors.green,
                  borderRadius: BorderRadius.circular(16),
                ),
                child: const Center(
                  child: Text(
                    "PAID ✅",
                    style: TextStyle(
                      color: Colors.white,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ),
            if (!isPaid)
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: isLoading ? null : pay,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.green,
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 16),
                  ),
                  child: isLoading
                      ? const CircularProgressIndicator(color: Colors.white)
                      : const Text("PAY NOW"),
                ),
              ),
          ],
        ),
      ),
    );
  }
}

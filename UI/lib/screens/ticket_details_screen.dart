import 'package:flutter/material.dart';
import 'package:qr_flutter/qr_flutter.dart';
import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;
import 'package:printing/printing.dart';

class TicketDetailsScreen extends StatelessWidget {
  final dynamic ticket;

  const TicketDetailsScreen({super.key, required this.ticket});

  Future<void> generatePdf(BuildContext context) async {
    final pdf = pw.Document();

    final qrCode = ticket["qrCode"] ?? ticket["id"].toString();
    final eventName = ticket["eventName"] ?? "Event";
    final location = ticket["location"] ?? "";
    final eventDate = ticket["eventDate"] ?? "";
    final isUsed = ticket["isUsed"] ?? false;

    pdf.addPage(
      pw.Page(
        pageFormat: PdfPageFormat.a4,
        build: (pw.Context context) {
          return pw.Center(
            child: pw.Container(
              width: 350,
              padding: const pw.EdgeInsets.all(30),
              decoration: pw.BoxDecoration(
                border: pw.Border.all(color: PdfColors.orange, width: 2),
                borderRadius: pw.BorderRadius.circular(16),
              ),
              child: pw.Column(
                mainAxisSize: pw.MainAxisSize.min,
                crossAxisAlignment: pw.CrossAxisAlignment.center,
                children: [
                  pw.Text(
                    "HAPPENINGS",
                    style: pw.TextStyle(
                      fontSize: 14,
                      fontWeight: pw.FontWeight.bold,
                      color: PdfColors.orange,
                    ),
                  ),
                  pw.SizedBox(height: 4),
                  pw.Divider(color: PdfColors.orange),
                  pw.SizedBox(height: 12),
                  pw.Text(
                    eventName,
                    style: pw.TextStyle(
                        fontSize: 20, fontWeight: pw.FontWeight.bold),
                    textAlign: pw.TextAlign.center,
                  ),
                  pw.SizedBox(height: 10),
                  if (location.isNotEmpty)
                    pw.Row(
                      mainAxisAlignment: pw.MainAxisAlignment.center,
                      children: [
                        pw.Text("📍 ", style: const pw.TextStyle(fontSize: 12)),
                        pw.Text(location,
                            style: const pw.TextStyle(fontSize: 12)),
                      ],
                    ),
                  pw.SizedBox(height: 4),
                  if (eventDate.isNotEmpty)
                    pw.Row(
                      mainAxisAlignment: pw.MainAxisAlignment.center,
                      children: [
                        pw.Text("📅 ", style: const pw.TextStyle(fontSize: 12)),
                        pw.Text(eventDate,
                            style: const pw.TextStyle(fontSize: 12)),
                      ],
                    ),
                  pw.SizedBox(height: 20),
                  // QR kod
                  pw.BarcodeWidget(
                    barcode: pw.Barcode.qrCode(),
                    data: qrCode,
                    width: 180,
                    height: 180,
                  ),
                  pw.SizedBox(height: 16),
                  pw.Text(
                    qrCode,
                    style:
                        const pw.TextStyle(fontSize: 9, color: PdfColors.grey),
                    textAlign: pw.TextAlign.center,
                  ),
                  pw.SizedBox(height: 16),
                  pw.Container(
                    padding: const pw.EdgeInsets.symmetric(
                        horizontal: 20, vertical: 8),
                    decoration: pw.BoxDecoration(
                      color: isUsed ? PdfColors.red : PdfColors.green,
                      borderRadius: pw.BorderRadius.circular(20),
                    ),
                    child: pw.Text(
                      isUsed ? "USED" : "VALID",
                      style: pw.TextStyle(
                        color: PdfColors.white,
                        fontWeight: pw.FontWeight.bold,
                        fontSize: 14,
                      ),
                    ),
                  ),
                  pw.SizedBox(height: 16),
                  pw.Divider(color: PdfColors.grey300),
                  pw.SizedBox(height: 8),
                  pw.Text(
                    "This ticket is your entry pass. Present QR code at the entrance.",
                    style:
                        const pw.TextStyle(fontSize: 9, color: PdfColors.grey),
                    textAlign: pw.TextAlign.center,
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );

    await Printing.layoutPdf(
      onLayout: (PdfPageFormat format) async => pdf.save(),
      name: "ticket_${eventName.replaceAll(' ', '_')}.pdf",
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      appBar: AppBar(
        backgroundColor: const Color(0xFFF4D35E),
        foregroundColor: Colors.black,
        title: const Text("Your Ticket",
            style: TextStyle(fontWeight: FontWeight.bold)),
        actions: [
          IconButton(
            icon: const Icon(Icons.picture_as_pdf, color: Colors.black),
            tooltip: "Download PDF",
            onPressed: () => generatePdf(context),
          ),
        ],
      ),
      body: Center(
        child: Container(
          width: 350,
          padding: const EdgeInsets.all(20),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(20),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(
                ticket["eventName"] ?? "",
                style:
                    const TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 10),
              if ((ticket["location"] ?? "").isNotEmpty)
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Icon(Icons.location_on, size: 16, color: Colors.grey),
                    const SizedBox(width: 4),
                    Text(ticket["location"] ?? "",
                        style: const TextStyle(color: Colors.grey)),
                  ],
                ),
              const SizedBox(height: 5),
              if ((ticket["eventDate"] ?? "").isNotEmpty)
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Icon(Icons.calendar_today,
                        size: 16, color: Colors.grey),
                    const SizedBox(width: 4),
                    Text(ticket["eventDate"] ?? "",
                        style: const TextStyle(color: Colors.grey)),
                  ],
                ),
              const SizedBox(height: 20),
              QrImageView(
                data: ticket["qrCode"] ?? ticket["id"].toString(),
                size: 220,
              ),
              const SizedBox(height: 20),
              Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
                decoration: BoxDecoration(
                  color:
                      (ticket["isUsed"] ?? false) ? Colors.red : Colors.green,
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Text(
                  (ticket["isUsed"] ?? false) ? "USED" : "VALID",
                  style: const TextStyle(
                      color: Colors.white, fontWeight: FontWeight.bold),
                ),
              ),
              const SizedBox(height: 16),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton.icon(
                  onPressed: () => generatePdf(context),
                  icon: const Icon(Icons.picture_as_pdf),
                  label: const Text("Download PDF Ticket"),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.black,
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12)),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

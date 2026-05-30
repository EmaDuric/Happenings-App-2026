import 'dart:async';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({super.key});

  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  List<dynamic> notifications = [];
  bool isLoading = true;
  Timer? _refreshTimer;

  @override
  void initState() {
    super.initState();
    loadNotifications();
    // Auto-refresh svakih 30 sekundi
    _refreshTimer = Timer.periodic(const Duration(seconds: 30), (_) {
      loadNotifications();
    });
  }

  @override
  void dispose() {
    _refreshTimer?.cancel();
    super.dispose();
  }

  Future<void> loadNotifications() async {
    try {
      final token = await AuthService.getToken();
      if (token == null) throw Exception("Not logged in");
      final result = await ApiService.getMyNotifications(token);
      if (!mounted) return;
      setState(() {
        notifications = result;
        isLoading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() => isLoading = false);
    }
  }

  Future<void> markAsRead(int id) async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return;
      await ApiService.markNotificationAsRead(id: id, token: token);
      loadNotifications();
    } catch (_) {}
  }

  Future<void> markAllAsRead() async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return;
      await ApiService.markAllNotificationsAsRead(token: token);
      loadNotifications();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text("Error: $e")));
    }
  }

  Future<void> clearAll() async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text("Clear notifications"),
        content:
            const Text("Are you sure you want to clear all notifications?"),
        actions: [
          TextButton(
              onPressed: () => Navigator.pop(context, false),
              child: const Text("Cancel")),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text("Clear", style: TextStyle(color: Colors.red)),
          ),
        ],
      ),
    );

    if (confirm != true) return;

    try {
      final token = await AuthService.getToken();
      if (token == null) throw Exception("Not logged in");
      await ApiService.clearMyNotifications(token);
      setState(() => notifications = []);
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text("Error: $e")));
    }
  }

  Widget buildNotificationCard(Map<String, dynamic> notif) {
    final createdAt = DateTime.tryParse(notif["createdAt"] ?? "");
    final formatted = createdAt != null
        ? DateFormat("dd.MM.yyyy HH:mm").format(createdAt.toLocal())
        : "";
    final isRead = notif["isRead"] ?? false;

    return GestureDetector(
      onTap: () {
        if (!isRead) markAsRead(notif["id"]);
      },
      child: Container(
        margin: const EdgeInsets.only(bottom: 12),
        decoration: BoxDecoration(
          color: isRead ? Colors.white : Colors.white,
          borderRadius: BorderRadius.circular(16),
          border: isRead
              ? null
              : Border.all(color: Colors.orange.shade300, width: 1.5),
          boxShadow: [
            BoxShadow(
                color: Colors.black.withOpacity(0.06),
                blurRadius: 8,
                offset: const Offset(0, 3)),
          ],
        ),
        child: ListTile(
          contentPadding:
              const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
          leading: Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: isRead
                  ? const Color(0xFFF4D35E).withOpacity(0.3)
                  : const Color(0xFFF4D35E).withOpacity(0.6),
              shape: BoxShape.circle,
            ),
            child: Icon(
              isRead ? Icons.notifications : Icons.notifications_active,
              color: isRead ? Colors.black54 : Colors.black87,
            ),
          ),
          title: Text(
            notif["title"] ?? "",
            style: TextStyle(
              fontWeight: isRead ? FontWeight.normal : FontWeight.bold,
              fontSize: 15,
            ),
          ),
          subtitle: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const SizedBox(height: 4),
              Text(notif["message"] ?? ""),
              const SizedBox(height: 6),
              Row(
                children: [
                  Text(formatted,
                      style: const TextStyle(fontSize: 12, color: Colors.grey)),
                  if (!isRead) ...[
                    const SizedBox(width: 8),
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 6, vertical: 2),
                      decoration: BoxDecoration(
                        color: Colors.orange,
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: const Text("NEW",
                          style: TextStyle(
                              color: Colors.white,
                              fontSize: 10,
                              fontWeight: FontWeight.bold)),
                    ),
                  ],
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final unreadCount =
        notifications.where((n) => !(n["isRead"] ?? false)).length;

    return Scaffold(
      backgroundColor: const Color(0xFFF4D35E),
      appBar: AppBar(
        title: Row(
          children: [
            const Icon(Icons.notifications, color: Colors.black),
            const SizedBox(width: 8),
            const Text("NOTIFICATIONS",
                style: TextStyle(
                    fontWeight: FontWeight.bold,
                    letterSpacing: 1.2,
                    color: Colors.black)),
            if (unreadCount > 0) ...[
              const SizedBox(width: 8),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                decoration: BoxDecoration(
                    color: Colors.red, borderRadius: BorderRadius.circular(12)),
                child: Text("$unreadCount",
                    style: const TextStyle(
                        color: Colors.white,
                        fontSize: 12,
                        fontWeight: FontWeight.bold)),
              ),
            ],
          ],
        ),
        backgroundColor: const Color(0xFFF4D35E),
        elevation: 0,
        foregroundColor: Colors.black,
        actions: [
          if (unreadCount > 0)
            TextButton(
              onPressed: markAllAsRead,
              child: const Text("Mark all read",
                  style: TextStyle(color: Colors.black87, fontSize: 12)),
            ),
        ],
      ),
      body: isLoading
          ? const Center(child: CircularProgressIndicator())
          : notifications.isEmpty
              ? const Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Icon(Icons.notifications_off,
                          size: 64, color: Colors.black38),
                      SizedBox(height: 16),
                      Text("No notifications yet",
                          style:
                              TextStyle(color: Colors.black54, fontSize: 16)),
                    ],
                  ),
                )
              : Column(
                  children: [
                    Expanded(
                      child: RefreshIndicator(
                        onRefresh: loadNotifications,
                        child: ListView.builder(
                          padding: const EdgeInsets.all(16),
                          itemCount: notifications.length,
                          itemBuilder: (context, index) =>
                              buildNotificationCard(notifications[index]),
                        ),
                      ),
                    ),
                    Padding(
                      padding: const EdgeInsets.all(16),
                      child: SizedBox(
                        width: double.infinity,
                        child: ElevatedButton(
                          onPressed: clearAll,
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.green,
                            foregroundColor: Colors.white,
                            padding: const EdgeInsets.symmetric(vertical: 14),
                            shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(12)),
                          ),
                          child: const Text("CLEAR ALL",
                              style: TextStyle(
                                  fontWeight: FontWeight.bold,
                                  letterSpacing: 1.2)),
                        ),
                      ),
                    ),
                  ],
                ),
    );
  }
}

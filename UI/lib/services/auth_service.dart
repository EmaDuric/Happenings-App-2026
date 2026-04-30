import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthService {
  static const _storage = FlutterSecureStorage();
  static const _tokenKey = 'jwt_token';

  static Future<int?> getUserId() async {
    final token = await getToken();
    if (token == null) return null;
    final parts = token.split('.');
    if (parts.length != 3) return null;
    final payload =
        utf8.decode(base64Url.decode(base64Url.normalize(parts[1])));
    final data = jsonDecode(payload);
    final id = data[
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
    return int.tryParse(id.toString());
  }

  static Future<void> saveToken(String token) async {
    await _storage.write(key: _tokenKey, value: token);
  }

  static Future<String?> getToken() async {
    return await _storage.read(key: _tokenKey);
  }

  static Future<void> clearToken() async {
    await _storage.delete(key: _tokenKey);
  }

  static Future<bool> isLoggedIn() async {
    final token = await getToken();
    return token != null && token.isNotEmpty;
  }

  static Map<String, dynamic>? _parseToken(String token) {
    try {
      final parts = token.split('.');
      if (parts.length != 3) return null;
      final payload =
          utf8.decode(base64Url.decode(base64Url.normalize(parts[1])));
      return jsonDecode(payload);
    } catch (e) {
      return null;
    }
  }

  static Future<String?> getUsername() async {
    final token = await getToken();
    if (token == null) return null;
    final data = _parseToken(token);
    return data?["unique_name"] ?? data?["name"];
  }

  static Future<bool> isOrganizer() async {
    final token = await getToken();
    if (token == null) return false;
    final data = _parseToken(token);
    return data?["isOrganizer"] == true ||
        data?["isOrganizer"] == "True" ||
        data?["role"] == "Organizer" ||
        data?["role"] == "Admin";
  }
}

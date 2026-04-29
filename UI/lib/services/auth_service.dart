import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'dart:html' as html;

class AuthService {
  static const _storage = FlutterSecureStorage();
  static const _tokenKey = 'jwt_token';

  static Future<int?> getUserId() async {
    final token = await getToken();
    if (token == null) return null;
    // parsiraj JWT i izvuci userId claim
    final parts = token.split('.');
    if (parts.length != 3) return null;
    final payload =
        utf8.decode(base64Url.decode(base64Url.normalize(parts[1])));
    final data = jsonDecode(payload);
    final id = data[
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
    return int.tryParse(id.toString());
  }

  // 🔐 SAVE TOKEN
  static Future<void> saveToken(String token) async {
    if (kIsWeb) {
      html.window.localStorage[_tokenKey] = token;
    } else {
      await _storage.write(key: _tokenKey, value: token);
    }
  }

  // 🔐 GET TOKEN
  static Future<String?> getToken() async {
    if (kIsWeb) {
      return html.window.localStorage[_tokenKey];
    } else {
      return await _storage.read(key: _tokenKey);
    }
  }

  // 🚪 LOGOUT
  static Future<void> clearToken() async {
    if (kIsWeb) {
      html.window.localStorage.remove(_tokenKey);
    } else {
      await _storage.delete(key: _tokenKey);
    }
  }

  // 🔎 DA LI JE LOGOVAN
  static Future<bool> isLoggedIn() async {
    final token = await getToken();
    return token != null && token.isNotEmpty;
  }

  // 🔥 PARSIRAJ TOKEN
  static Map<String, dynamic>? _parseToken(String token) {
    try {
      final parts = token.split('.');
      if (parts.length != 3) return null;

      final payload = utf8.decode(
        base64Url.decode(base64Url.normalize(parts[1])),
      );

      return jsonDecode(payload);
    } catch (e) {
      return null;
    }
  }

  // 👤 USERNAME
  static Future<String?> getUsername() async {
    final token = await getToken();
    if (token == null) return null;

    final data = _parseToken(token);
    return data?["unique_name"] ?? data?["name"];
  }

  // 🔥 ORGANIZER CHECK (ROBUSTAN)
  static Future<bool> isOrganizer() async {
    final token = await getToken();
    if (token == null) return false;

    final data = _parseToken(token);

    print("JWT DATA: $data");

    return data?["isOrganizer"] == true ||
        data?["isOrganizer"] == "True" ||
        data?["role"] == "Organizer" ||
        data?["role"] == "Admin";
  }
}

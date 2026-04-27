import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'dart:html' as html; // 👈 VAŽNO

class AuthService {
  static const _storage = FlutterSecureStorage();
  static const _tokenKey = 'jwt_token';

  static Future<void> saveToken(String token) async {
    if (kIsWeb) {
      html.window.localStorage[_tokenKey] = token;
      print("TOKEN SAVED (WEB): $token");
    } else {
      await _storage.write(
        key: _tokenKey,
        value: token,
      );
    }
  }

  static Future<String?> getToken() async {
    if (kIsWeb) {
      final token = html.window.localStorage[_tokenKey];
      print("TOKEN READ (WEB): $token");
      return token;
    } else {
      return await _storage.read(key: _tokenKey);
    }
  }

  static Future<void> clearToken() async {
    if (kIsWeb) {
      html.window.localStorage.remove(_tokenKey);
    } else {
      await _storage.delete(key: _tokenKey);
    }
  }

  static Future<bool> isLoggedIn() async {
    final token = await getToken();
    return token != null && token.isNotEmpty;
  }
}

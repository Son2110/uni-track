import 'package:flutter/material.dart';

/// FPT Identity & UniTrack Color Palette
class AppColors {
  AppColors._();

  // Primary Colors
  static const Color primary = Color(0xFFF27123); // Orange - Action/Active
  static const Color secondary = Color(0xFF1E5BB8); // Blue - Header

  // Background Colors
  static const Color background = Color(0xFFF3F4F6);
  static const Color surface = Color(0xFFFFFFFF);

  // Text Colors
  static const Color textPrimary = Color(0xFF1F2937);
  static const Color textSecondary = Color(0xFF6B7280);
  static const Color textDisabled = Color(0xFF9CA3AF);

  // Navigation Colors
  static const Color navSelected = primary;
  static const Color navUnselected = Color(0xFF9CA3AF);
  static const Color navBackground = surface;

  // Status Colors
  static const Color success = Color(0xFF10B981);
  static const Color warning = Color(0xFFFBBF24);
  static const Color error = Color(0xFFEF4444);
  static const Color info = Color(0xFF3B82F6);
}

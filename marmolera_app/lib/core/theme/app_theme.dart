import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

class AppTheme {
  // Paleta corporativa (NUEVA IDENTIDAD VISUAL):
  static const Color primaryBlue = Color(0xFF00468B); // Azul corporativo
  static const Color accentGrey = Color(0xFF808285);  // Gris Piedra
  static const Color backgroundWhite = Color(0xFFFFFFFF); // Blanco
  
  static const Color textPrimary = primaryBlue; // Texto principal azul corporativo
  static const Color textSecondary = Color(0xFF4A7BAA); // Texto secundario (azul claro)
  static const Color errorColor = Color(0xFFE74C3C);
  static const Color cardColor = Color(0xFFFFFFFF); // Tarjetas blancas

  // Aliases para mantener compatibilidad con pantallas no actualizadas (evita errores de compilación):
  static const Color primaryDark = backgroundWhite; 
  static const Color accentGold = primaryBlue;
  static const Color surfaceColor = backgroundWhite;

  static ThemeData get lightTheme {
    return ThemeData(
      useMaterial3: true,
      brightness: Brightness.light,
      scaffoldBackgroundColor: backgroundWhite,
      colorScheme: const ColorScheme.light(
        primary: primaryBlue,
        secondary: accentGrey,
        surface: backgroundWhite,
        error: errorColor,
      ),
      textTheme: GoogleFonts.poppinsTextTheme(),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: const Color(0xFFF5F7FA), // Gris muy claro para inputs
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: accentGrey, width: 1.0),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: accentGrey, width: 1.0),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: primaryBlue, width: 2),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: errorColor, width: 1.5),
        ),
        labelStyle: const TextStyle(color: textSecondary),
        prefixIconColor: textSecondary,
        suffixIconColor: textSecondary,
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: primaryBlue,
          foregroundColor: Colors.white,
          minimumSize: const Size(double.infinity, 54),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
          textStyle: GoogleFonts.poppins(
            fontSize: 16,
            fontWeight: FontWeight.w600,
            letterSpacing: 0.5,
          ),
        ),
      ),
    );
  }
  
  // Alias por si alguna pantalla sigue pidiendo darkTheme
  static ThemeData get darkTheme => lightTheme;
}
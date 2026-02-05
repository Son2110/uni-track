import 'package:flutter/material.dart';
import 'constants/app_colors.dart';
import 'routing/scaffold_with_navbar.dart';

/// The main application widget
class App extends StatelessWidget {
  const App({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'UniTrack',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        // Color Scheme
        colorScheme: ColorScheme.fromSeed(
          seedColor: AppColors.primary,
          primary: AppColors.primary,
          secondary: AppColors.secondary,
          surface: AppColors.surface,
        ),

        // Scaffold Background
        scaffoldBackgroundColor: AppColors.background,

        // App Bar Theme
        appBarTheme: const AppBarTheme(
          backgroundColor: AppColors.secondary,
          foregroundColor: Colors.white,
          elevation: 0,
          centerTitle: false,
        ),

        // Bottom Navigation Bar Theme
        bottomNavigationBarTheme: const BottomNavigationBarThemeData(
          backgroundColor: AppColors.navBackground,
          selectedItemColor: AppColors.navSelected,
          unselectedItemColor: AppColors.navUnselected,
          type: BottomNavigationBarType.fixed,
          elevation: 0,
        ),

        // Text Theme
        textTheme: const TextTheme(
          bodyLarge: TextStyle(color: AppColors.textPrimary),
          bodyMedium: TextStyle(color: AppColors.textSecondary),
        ),

        // Use Material 3
        useMaterial3: true,
      ),
      home: const ScaffoldWithNavBar(),
    );
  }
}

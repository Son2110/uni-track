import 'package:flutter/material.dart';
import 'constants/app_colors.dart';
import 'features/auth/data/models/auth_models.dart';
import 'features/auth/data/repositories/auth_repository.dart';
import 'features/auth/presentation/login_screen.dart';
import 'routing/scaffold_with_navbar.dart';

/// The main application widget
class App extends StatefulWidget {
  const App({super.key});

  @override
  State<App> createState() => _AppState();
}

class _AppState extends State<App> {
  final _authRepository = AuthRepository();
  AuthUser? _currentUser;
  bool _authChecked = false;

  @override
  void initState() {
    super.initState();
    _checkSavedAuth();
  }

  Future<void> _checkSavedAuth() async {
    final user = await _authRepository.getSavedUser();
    setState(() {
      _currentUser = user;
      _authChecked = true;
    });
  }

  void _onLoginSuccess(AuthUser user) {
    setState(() => _currentUser = user);
  }

  void _onLogout() async {
    await _authRepository.logout();
    setState(() => _currentUser = null);
  }

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
      home: !_authChecked
          ? const _SplashScreen()
          : _currentUser != null
              ? ScaffoldWithNavBar(
                  currentUser: _currentUser!,
                  onLogout: _onLogout,
                )
              : LoginScreen(onLoginSuccess: _onLoginSuccess),
    );
  }
}

class _SplashScreen extends StatelessWidget {
  const _SplashScreen();

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      backgroundColor: AppColors.secondary,
      body: Center(
        child: CircularProgressIndicator(color: Colors.white),
      ),
    );
  }
}

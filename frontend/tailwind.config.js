/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        // Login page colors (from HTML)
        "primary": "#1E5BB8",
        "background-light": "#F9FAFB",
        "background-dark": "#111827",
        "fpt-orange": "#F37021",
        "google-red": "#EA4335",
        
        // Admin dashboard colors (keeping for compatibility)
        "primary-dark": "#154288",
        "sidebar-blue": "#1E5BB8",
        "sidebar-hover": "#154288",
        "card-light": "#ffffff",
        "card-dark": "#111827",
      },
      fontFamily: {
        "display": ["Inter", "sans-serif"]
      },
    },
  },
  plugins: [],
}

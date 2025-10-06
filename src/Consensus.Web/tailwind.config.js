/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['"Inter"', '"Segoe UI"', '"Roboto"', 'sans-serif'],
      },
      colors: {
        primary: '#10a37f',
        'primary-hover': '#0d8a68',
      },
    },
  },
  plugins: [],
}

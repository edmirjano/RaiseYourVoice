/** @type {import('next').NextConfig} */
const { i18n } = require('./next-i18n.config');

const nextConfig = {
  reactStrictMode: true,
  i18n,
  output: 'standalone',
  images: {
    domains: ['localhost', 'raiseyourvoice.example.com'],
  },
  async redirects() {
    return [
      {
        source: '/home',
        destination: '/',
        permanent: true,
      },
    ];
  },
};

module.exports = nextConfig;
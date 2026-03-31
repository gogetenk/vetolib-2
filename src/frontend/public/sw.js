// Vetara PWA Service Worker — app shell caching only
// Does NOT cache API responses; only static assets for offline shell.

const CACHE_NAME = 'vetara-shell-v1';

const SHELL_ASSETS = [
  '/',
  '/en/portal',
  '/ar/portal',
];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => cache.addAll(SHELL_ASSETS))
  );
  self.skipWaiting();
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(
        keys
          .filter((key) => key !== CACHE_NAME)
          .map((key) => caches.delete(key))
      )
    )
  );
  self.clients.claim();
});

self.addEventListener('fetch', (event) => {
  const { request } = event;

  // Only cache GET requests for navigation and static assets
  if (request.method !== 'GET') return;

  // Never cache API calls
  if (request.url.includes('/api/')) return;

  event.respondWith(
    caches.match(request).then((cached) => {
      // Network-first for navigation, cache-first for static assets
      if (request.mode === 'navigate') {
        return fetch(request)
          .then((response) => {
            if (response.ok) {
              const clone = response.clone();
              caches.open(CACHE_NAME).then((cache) => cache.put(request, clone));
            }
            return response;
          })
          .catch(() => cached || caches.match('/en/portal'));
      }

      // For static assets (JS/CSS/images): cache-first
      if (
        request.destination === 'script' ||
        request.destination === 'style' ||
        request.destination === 'image' ||
        request.destination === 'font'
      ) {
        return (
          cached ||
          fetch(request).then((response) => {
            if (response.ok) {
              const clone = response.clone();
              caches.open(CACHE_NAME).then((cache) => cache.put(request, clone));
            }
            return response;
          })
        );
      }

      return fetch(request);
    })
  );
});

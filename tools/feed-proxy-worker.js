// Cloudflare Worker relay for feeds whose CDN blocks this app's cloud egress IP (see FeedProxyOptions).
// Deploy: Cloudflare dashboard -> Workers -> Create -> paste this -> Deploy, then set the app setting
// FeedProxy__BaseUrl to the worker URL (e.g. https://feed-proxy.<you>.workers.dev). Not an open proxy:
// only the allowlisted hosts below are forwarded.
const ALLOWED_HOSTS = new Set([
  "indianexpress.com", "www.indianexpress.com",
  "indiatoday.in", "www.indiatoday.in",
  "theprint.in", "www.theprint.in",
  "organiser.org", "www.organiser.org",
  "shipmin.gov.in", "www.shipmin.gov.in",
]);

export default {
  async fetch(request) {
    const target = new URL(new URL(request.url).searchParams.get("url") ?? "about:blank");
    if (target.protocol !== "https:" || !ALLOWED_HOSTS.has(target.hostname)) {
      return new Response("host not allowed", { status: 400 });
    }
    return fetch(target, {
      headers: {
        "User-Agent": "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36",
        Accept: "application/rss+xml,application/xml,text/xml,*/*;q=0.8",
      },
    });
  },
};

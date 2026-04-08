using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Resources;
using static System.Reflection.Metadata.BlobBuilder;

namespace OrderAPI.Middleware
{
	public class SecurityHeadersMiddleware
	{
		/*
		 * What is Security Headers Hardening?
		 * Adding HTTP response headers to reduce common web vulnerabilities like XSS, clickjacking, MIME attacks, etc.
		 * These headers instruct the browser how to behave securely.
		 * Modifying HTTP response headers before sending to client
		 */

		private readonly RequestDelegate _next;
		
		public SecurityHeadersMiddleware(RequestDelegate next)
		{
			_next = next;
		}
		
		public async Task InvokeAsync(HttpContext context)
		{
			var headers = context.Response.Headers;

			// Prevent XSS
			
			// X-XSS-Protection is a legacy header and modern browsers have better built-in XSS protections,
			// but it's still good to include for older browsers.
			
			// Purpose: Protect against Cross - Site Scripting(XSS) attacks
			// Behavior: Browser detects XSS → blocks page rendering
			headers["X-XSS-Protection"] = "1; mode=block";

			// Prevent clickjacking
			
			// X-Frame-Options is a legacy header, but still widely supported.
			// For modern browsers, Content Security Policy's frame-ancestors directive is more flexible and secure.
			
			// Purpose: Prevent Clickjacking attacks
			// Attack example: Your site loaded inside an invisible iframe → user unknowingly clicks something
			// Options: DENY → No iframe allowed ✅ (most secure)
			// SAMEORIGIN → Only same domain allowed
			headers["X-Frame-Options"] = "DENY";

			// Prevent MIME sniffing
			// X-Content-Type-Options is a simple but effective header to prevent MIME sniffing attacks.
			// MIME sniffing is when the browser tries to guess the content type of a file based on its content rather than trusting the Content-Type header sent by the server.
			// This can lead to security vulnerabilities, especially if a malicious file is disguised as a safe one (e.g., a .jpg file that contains JavaScript).
			// Purpose: Prevent MIME sniffing attacks
			// Problem: Browser guesses file type instead of trusting server
			// Attack: Malicious JS disguised as image → executed
			// Solution: nosniff → Browser must trust Content-Type header
			headers["X-Content-Type-Options"] = "nosniff";

			// Referrer policy
			// Referrer-Policy controls how much referrer information is sent with requests from your site to other sites.
			// Purpose: Control referrer information sent to other sites / Control how much referrer info is shared
			// Example: User clicks a link from your site to another site → Referrer header is sent with the request
			// Options: no-referrer → send nothing ✅ (most secure)
			//          strict-origin-when-cross-origin → balanced (recommended for web apps)
			// Trade-off: You lose analytics data
			headers["Referrer-Policy"] = "no-referrer";

			// Content Security Policy (basic for API)
			// Content-Security-Policy is a powerful header that allows you to control which resources (scripts, styles, images, etc.) can be loaded by the browser when visiting your site.
			// For an API, you typically want to restrict everything since you don't serve any frontend assets.
			// Purpose: Control which resources can be loaded by the browser
			// Prevents:
			// - XSS (by blocking malicious scripts)
			// - Data injection attacks (by blocking untrusted sources)
			// - Malicious scripts from being executed

			// Your Policy Meaning:
			// default - src 'self': Only allow resources from same origin
			// frame - ancestors 'none':
			//		- Prevent embedding in iframe(clickjacking protection)
			//		- More modern than X-Frame - Options

			// Since APIs don’t serve UI, block everything
			headers["Content-Security-Policy"] = "default-src 'self'; frame-ancestors 'none';";

			// Improve CSP (if UI exists)
			//headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; object-src 'none';";

			// Enforce HTTPS
			// Strict-Transport-Security (HSTS) tells the browser to only connect to your site over HTTPS for a specified period of time.
			// Purpose: Enforce HTTPS connections
			// Prevent:
			// - Man in-the-middle attacks (by ensuring all communication is encrypted)
			// - Downgrade attacks (by preventing fallback to HTTP)
			// max-age=31536000 → 1 year
			// includeSubDomains → Apply to all subdomains (recommended)
			headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

			// Cache-Control:
			// no-store → Prevents caching of sensitive data in browser or intermediary caches
			// no-cache → Forces revalidation with server before using cached data
			// must-revalidate → Ensures stale data is not used without checking with server
			headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
			// Pragma and Expires are older headers that also control caching behavior,
			// but Cache-Control is more modern and widely supported.
			// Including them for compatibility with older clients.
			headers["Pragma"] = "no-cache";
			// Expires: 0 → Indicates that the response is already expired and should not be cached
			headers["Expires"] = "0";

			await _next(context);
		}

	}
}

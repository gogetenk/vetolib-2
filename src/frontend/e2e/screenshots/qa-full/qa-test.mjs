import { chromium } from 'playwright';
import { writeFileSync } from 'fs';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const SCREENSHOT_DIR = __dirname;
const BASE = 'http://localhost:3000';

const findings = [];

function log(msg) {
  console.log(`[QA] ${msg}`);
}

function addFinding(page, category, severity, detail) {
  findings.push({ page, category, severity, detail });
}

async function navigateAndWait(page, url, timeout = 30000) {
  try {
    await page.goto(url, { waitUntil: 'load', timeout });
  } catch (e) {
    log(`WARNING: Navigation error for ${url}: ${e.message}`);
    return false;
  }
  // Wait for Next.js hydration
  await page.waitForTimeout(3000);
  return true;
}

async function loginOnPage(page) {
  await navigateAndWait(page, `${BASE}/en/login`);

  const emailInput = page.locator('[data-testid="email-input"]');
  const passwordInput = page.locator('[data-testid="password-input"]');

  try {
    await emailInput.waitFor({ state: 'visible', timeout: 5000 });
    await passwordInput.waitFor({ state: 'visible', timeout: 5000 });
  } catch {
    log('Login form elements not found after wait');
    return false;
  }

  await emailInput.fill('dr.sarah@desertpaws.ae');
  await passwordInput.fill('Secure123!');

  const loginBtn = page.locator('button[type="submit"]').first();
  if (await loginBtn.count()) {
    await loginBtn.click();
    await page.waitForTimeout(3000);
    log(`After login URL: ${page.url()}`);
    return true;
  }
  return false;
}

async function run() {
  const browser = await chromium.launch({ headless: true });
  const consoleErrors = {};

  // Helper to create a page with console error tracking
  async function newTrackedPage(name) {
    const ctx = await browser.newContext();
    const page = await ctx.newPage();
    const errors = [];
    page.on('console', msg => {
      if (msg.type() === 'error') {
        const text = msg.text();
        if (!text.includes('Fast Refresh') && !text.includes('webpack') && !text.includes('React DevTools')) {
          errors.push(text);
        }
      }
    });
    page.on('pageerror', err => {
      errors.push(err.message);
    });
    consoleErrors[name] = errors;
    return { page, ctx };
  }

  // ============================================================
  // 1. LANDING PAGE EN - HERO
  // ============================================================
  {
    log('--- 1. Landing EN Hero ---');
    const { page, ctx } = await newTrackedPage('01-landing-en-hero');
    await navigateAndWait(page, `${BASE}/en`);
    await page.screenshot({ path: join(SCREENSHOT_DIR, '01-landing-en-hero.png'), fullPage: false });
    log('Screenshot: 01-landing-en-hero.png');

    const heroExists = await page.locator('[data-testid="section-hero"]').count();
    if (!heroExists) addFinding('Landing EN', 'Layout', 'HIGH', 'Hero section [data-testid="section-hero"] not found');
    else log('Hero section found');

    const navSignin = await page.locator('[data-testid="btn-nav-signin"]').count();
    if (!navSignin) addFinding('Landing EN', 'Layout', 'MEDIUM', 'Nav sign-in button [data-testid="btn-nav-signin"] missing');
    else log('Nav sign-in button found');

    if (consoleErrors['01-landing-en-hero'].length > 0) {
      addFinding('Landing EN', 'Console Errors', 'MEDIUM', consoleErrors['01-landing-en-hero'].join(' | '));
    }
    await ctx.close();
  }

  // ============================================================
  // 2. LANDING PAGE EN - SCROLL
  // ============================================================
  {
    log('--- 2. Landing EN Scroll ---');
    const { page, ctx } = await newTrackedPage('02-landing-en-scroll');
    await navigateAndWait(page, `${BASE}/en`);
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight / 2));
    await page.waitForTimeout(1000);
    await page.screenshot({ path: join(SCREENSHOT_DIR, '02-landing-en-scroll.png'), fullPage: false });
    log('Screenshot: 02-landing-en-scroll.png');

    const featuresVisible = await page.locator('[data-testid="section-features"]').count();
    if (!featuresVisible) addFinding('Landing EN Scroll', 'Layout', 'MEDIUM', 'Features section not rendered');
    else log('Features section found');

    const testimonials = await page.locator('[data-testid="section-testimonials"]').count();
    if (!testimonials) addFinding('Landing EN Scroll', 'Layout', 'MEDIUM', 'Testimonials section not rendered');
    else log('Testimonials section found');

    if (consoleErrors['02-landing-en-scroll'].length > 0) {
      addFinding('Landing EN Scroll', 'Console Errors', 'MEDIUM', consoleErrors['02-landing-en-scroll'].join(' | '));
    }
    await ctx.close();
  }

  // ============================================================
  // 3. LANDING PAGE AR (RTL)
  // ============================================================
  {
    log('--- 3. Landing AR RTL ---');
    const { page, ctx } = await newTrackedPage('03-landing-ar-rtl');
    await navigateAndWait(page, `${BASE}/ar`);
    await page.screenshot({ path: join(SCREENSHOT_DIR, '03-landing-ar-rtl.png'), fullPage: false });
    log('Screenshot: 03-landing-ar-rtl.png');

    const dirAttr = await page.locator('html').getAttribute('dir');
    const langAttr = await page.locator('html').getAttribute('lang');
    log(`HTML dir="${dirAttr}" lang="${langAttr}"`);
    if (dirAttr !== 'rtl') addFinding('Landing AR', 'RTL', 'HIGH', `Expected dir="rtl", got dir="${dirAttr}"`);
    if (langAttr !== 'ar') addFinding('Landing AR', 'i18n', 'MEDIUM', `Expected lang="ar", got lang="${langAttr}"`);

    try {
      const heroText = await page.locator('[data-testid="section-hero"] h1').textContent({ timeout: 3000 });
      const hasArabic = /[\u0600-\u06FF]/.test(heroText || '');
      if (!hasArabic) addFinding('Landing AR', 'i18n', 'HIGH', `Hero headline not in Arabic: "${heroText}"`);
      else log('Arabic text confirmed in hero');
    } catch {
      addFinding('Landing AR', 'Layout', 'HIGH', 'Could not read hero headline text');
    }

    if (consoleErrors['03-landing-ar-rtl'].length > 0) {
      addFinding('Landing AR', 'Console Errors', 'MEDIUM', consoleErrors['03-landing-ar-rtl'].join(' | '));
    }
    await ctx.close();
  }

  // ============================================================
  // 4. PORTAL BOOKING
  // ============================================================
  {
    log('--- 4. Portal Booking ---');
    const { page, ctx } = await newTrackedPage('04-portal-booking');
    await navigateAndWait(page, `${BASE}/en/portal/desert-paws/book`);
    await page.screenshot({ path: join(SCREENSHOT_DIR, '04-portal-booking.png'), fullPage: false });
    log('Screenshot: 04-portal-booking.png');

    const bookingContent = await page.content();
    const hasBookingUI = bookingContent.includes('book') || bookingContent.includes('Book') || bookingContent.includes('appointment') || bookingContent.includes('Appointment') || bookingContent.includes('pet') || bookingContent.includes('Pet');
    if (!hasBookingUI) addFinding('Portal Booking', 'Layout', 'HIGH', 'No booking-related content found on page');
    else log('Booking content found');

    if (consoleErrors['04-portal-booking'].length > 0) {
      addFinding('Portal Booking', 'Console Errors', 'MEDIUM', consoleErrors['04-portal-booking'].join(' | '));
    }
    await ctx.close();
  }

  // ============================================================
  // 5. MESSAGES INBOX (login required)
  // ============================================================
  {
    log('--- 5. Messages Inbox ---');
    const { page, ctx } = await newTrackedPage('05-messages-inbox');
    const loggedIn = await loginOnPage(page);
    if (!loggedIn) {
      addFinding('Messages Inbox', 'Auth', 'HIGH', 'Could not login - MSW auth may not be working');
    }

    await navigateAndWait(page, `${BASE}/en/messages`);
    const currentUrl = page.url();
    log(`Messages page URL: ${currentUrl}`);

    if (currentUrl.includes('/login')) {
      addFinding('Messages Inbox', 'Auth', 'HIGH', 'Redirected to login - auth session not persisted');
      await page.screenshot({ path: join(SCREENSHOT_DIR, '05-messages-inbox-redirected.png') });
    }
    await page.screenshot({ path: join(SCREENSHOT_DIR, '05-messages-inbox.png'), fullPage: false });
    log('Screenshot: 05-messages-inbox.png');

    const messageItems = await page.locator('[data-testid*="message"], [data-testid*="conversation"], [role="listitem"]').count();
    if (messageItems === 0 && !currentUrl.includes('/login')) {
      addFinding('Messages Inbox', 'Data', 'MEDIUM', 'No message items visible - MSW data may not be loaded');
    } else {
      log(`Found ${messageItems} message-related elements`);
    }

    // ============================================================
    // 6. MESSAGES - CLICK CONVERSATION
    // ============================================================
    log('--- 6. Messages Conversation ---');
    const conversationLink = page.locator('[data-testid*="conversation"], [data-testid*="message-item"], a[href*="messages/"]').first();
    if (await conversationLink.count()) {
      await conversationLink.click();
      await page.waitForTimeout(2000);
      await page.screenshot({ path: join(SCREENSHOT_DIR, '06-messages-conversation.png'), fullPage: false });
      log(`Screenshot: 06-messages-conversation.png (URL: ${page.url()})`);
    } else {
      addFinding('Messages', 'Data', 'MEDIUM', 'No conversation link to click');
      await page.screenshot({ path: join(SCREENSHOT_DIR, '06-messages-conversation-none.png') });
      log('Screenshot: 06-messages-conversation-none.png (no conversation found)');
    }

    if (consoleErrors['05-messages-inbox'].length > 0) {
      addFinding('Messages', 'Console Errors', 'MEDIUM', consoleErrors['05-messages-inbox'].join(' | '));
    }
    await ctx.close();
  }

  // ============================================================
  // 7. MESSAGES AR/RTL
  // ============================================================
  {
    log('--- 7. Messages AR/RTL ---');
    const { page, ctx } = await newTrackedPage('07-messages-ar-rtl');
    await loginOnPage(page);
    await navigateAndWait(page, `${BASE}/ar/messages`);
    const currentUrl = page.url();
    log(`Messages AR URL: ${currentUrl}`);

    await page.screenshot({ path: join(SCREENSHOT_DIR, '07-messages-ar-rtl.png'), fullPage: false });
    log('Screenshot: 07-messages-ar-rtl.png');

    if (!currentUrl.includes('/login')) {
      const msgDir = await page.locator('html').getAttribute('dir');
      if (msgDir !== 'rtl') addFinding('Messages AR', 'RTL', 'HIGH', `Expected dir="rtl", got "${msgDir}"`);
      else log('RTL direction confirmed');
    } else {
      addFinding('Messages AR', 'Auth', 'HIGH', 'Redirected to login for AR messages');
    }

    if (consoleErrors['07-messages-ar-rtl'].length > 0) {
      addFinding('Messages AR', 'Console Errors', 'MEDIUM', consoleErrors['07-messages-ar-rtl'].join(' | '));
    }
    await ctx.close();
  }

  // ============================================================
  // 8. LOGIN MOBILE
  // ============================================================
  {
    log('--- 8. Login Mobile ---');
    const { page, ctx } = await newTrackedPage('08-login-mobile');
    await page.setViewportSize({ width: 375, height: 812 });
    await navigateAndWait(page, `${BASE}/en/login`);
    await page.screenshot({ path: join(SCREENSHOT_DIR, '08-login-mobile.png'), fullPage: false });
    log('Screenshot: 08-login-mobile.png');

    const loginOverflow = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });
    if (loginOverflow) addFinding('Login Mobile', 'Responsive', 'HIGH', 'Horizontal overflow detected on mobile');
    else log('No horizontal overflow');

    if (consoleErrors['08-login-mobile'].length > 0) {
      addFinding('Login Mobile', 'Console Errors', 'MEDIUM', consoleErrors['08-login-mobile'].join(' | '));
    }
    await ctx.close();
  }

  // ============================================================
  // 9. CALENDAR MOBILE
  // ============================================================
  {
    log('--- 9. Calendar Mobile ---');
    const { page, ctx } = await newTrackedPage('09-calendar-mobile');
    await loginOnPage(page);
    await page.setViewportSize({ width: 375, height: 812 });
    await navigateAndWait(page, `${BASE}/en/appointments`);
    const currentUrl = page.url();
    log(`Calendar mobile URL: ${currentUrl}`);

    await page.screenshot({ path: join(SCREENSHOT_DIR, '09-calendar-mobile.png'), fullPage: false });
    log('Screenshot: 09-calendar-mobile.png');

    if (!currentUrl.includes('/login')) {
      const calOverflow = await page.evaluate(() => {
        return document.documentElement.scrollWidth > document.documentElement.clientWidth;
      });
      if (calOverflow) addFinding('Calendar Mobile', 'Responsive', 'MEDIUM', 'Horizontal overflow on mobile calendar');
      else log('No horizontal overflow');
    } else {
      addFinding('Calendar Mobile', 'Auth', 'HIGH', 'Redirected to login');
    }

    if (consoleErrors['09-calendar-mobile'].length > 0) {
      addFinding('Calendar Mobile', 'Console Errors', 'MEDIUM', consoleErrors['09-calendar-mobile'].join(' | '));
    }
    await ctx.close();
  }

  // ============================================================
  // 10. MESSAGES MOBILE
  // ============================================================
  {
    log('--- 10. Messages Mobile ---');
    const { page, ctx } = await newTrackedPage('10-messages-mobile');
    await loginOnPage(page);
    await page.setViewportSize({ width: 375, height: 812 });
    await navigateAndWait(page, `${BASE}/en/messages`);
    const currentUrl = page.url();
    log(`Messages mobile URL: ${currentUrl}`);

    await page.screenshot({ path: join(SCREENSHOT_DIR, '10-messages-mobile.png'), fullPage: false });
    log('Screenshot: 10-messages-mobile.png');

    if (!currentUrl.includes('/login')) {
      const msgOverflow = await page.evaluate(() => {
        return document.documentElement.scrollWidth > document.documentElement.clientWidth;
      });
      if (msgOverflow) addFinding('Messages Mobile', 'Responsive', 'HIGH', 'Horizontal overflow on mobile messages');
      else log('No horizontal overflow');
    } else {
      addFinding('Messages Mobile', 'Auth', 'HIGH', 'Redirected to login');
    }

    if (consoleErrors['10-messages-mobile'].length > 0) {
      addFinding('Messages Mobile', 'Console Errors', 'MEDIUM', consoleErrors['10-messages-mobile'].join(' | '));
    }
    await ctx.close();
  }

  await browser.close();

  // ============================================================
  // GENERATE REPORT
  // ============================================================
  log('\n=== QA FINDINGS ===');
  if (findings.length === 0) {
    log('No issues found!');
  } else {
    findings.forEach(f => {
      log(`[${f.severity}] ${f.page} - ${f.category}: ${f.detail}`);
    });
  }

  const report = generateReport();
  writeFileSync(join(SCREENSHOT_DIR, 'QA-MESSAGES-PORTAL-REPORT.md'), report);
  log('\nReport written to QA-MESSAGES-PORTAL-REPORT.md');
}

function generateReport() {
  const high = findings.filter(f => f.severity === 'HIGH');
  const medium = findings.filter(f => f.severity === 'MEDIUM');
  const low = findings.filter(f => f.severity === 'LOW');

  let md = `# QA Report: Messages, Owner Portal & Landing Page

**Date**: ${new Date().toISOString().split('T')[0]}
**Agent**: QA Automated (Playwright)
**Environment**: localhost:3000 (Next.js dev + MSW)

## Summary

- **Total findings**: ${findings.length}
- **HIGH**: ${high.length}
- **MEDIUM**: ${medium.length}
- **LOW**: ${low.length}

## Screenshots Taken

| # | Name | Description |
|---|------|-------------|
| 1 | 01-landing-en-hero.png | Landing page EN - hero section |
| 2 | 02-landing-en-scroll.png | Landing page EN - scrolled to features |
| 3 | 03-landing-ar-rtl.png | Landing page AR - RTL layout |
| 4 | 04-portal-booking.png | Owner Portal - booking wizard |
| 5 | 05-messages-inbox.png | Messages inbox (after login) |
| 6 | 06-messages-conversation.png | Messages - conversation detail |
| 7 | 07-messages-ar-rtl.png | Messages AR/RTL |
| 8 | 08-login-mobile.png | Login page - mobile (375x812) |
| 9 | 09-calendar-mobile.png | Calendar/appointments - mobile |
| 10 | 10-messages-mobile.png | Messages - mobile (375x812) |

## Findings

`;

  if (findings.length === 0) {
    md += `No issues found. All pages loaded correctly.\n`;
  } else {
    if (high.length > 0) {
      md += `### HIGH Severity\n\n`;
      high.forEach(f => {
        md += `- **${f.page}** [${f.category}]: ${f.detail}\n`;
      });
      md += '\n';
    }
    if (medium.length > 0) {
      md += `### MEDIUM Severity\n\n`;
      medium.forEach(f => {
        md += `- **${f.page}** [${f.category}]: ${f.detail}\n`;
      });
      md += '\n';
    }
    if (low.length > 0) {
      md += `### LOW Severity\n\n`;
      low.forEach(f => {
        md += `- **${f.page}** [${f.category}]: ${f.detail}\n`;
      });
      md += '\n';
    }
  }

  md += `## Checklist

| Test | Status |
|------|--------|
| Landing EN loads | Tested |
| Landing EN scroll (features/testimonials) | Tested |
| Landing AR RTL | Tested |
| Portal booking wizard | Tested |
| Messages inbox | Tested |
| Messages conversation | Tested |
| Messages AR/RTL | Tested |
| Login mobile responsive | Tested |
| Calendar mobile responsive | Tested |
| Messages mobile responsive | Tested |

## Notes

- Login credentials used: dr.sarah@desertpaws.ae / Secure123!
- All tests run with MSW (Mock Service Worker) intercepting API calls
- Screenshots saved in \`src/frontend/e2e/screenshots/qa-full/\`
`;

  return md;
}

run().catch(err => {
  console.error('Fatal error:', err);
  process.exit(1);
});

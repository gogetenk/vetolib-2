# Skill: Playwright — Tests E2E

## Rôle dans le projet

Les tests Playwright valident les Golden Paths depuis la perspective de l'utilisateur final.
Ils sont écrits par l'Agent QA après chaque PR, et une vidéo est attachée à chaque run.
L'Agent PO regarde la vidéo pour valider le comportement avant de donner son approbation.

## Setup

```bash
# Installer Playwright (TypeScript)
npm init playwright@latest
npx playwright install chromium firefox  # navigateurs nécessaires

# Ou via le projet Next.js frontend
cd vetolib-frontend
npm install -D @playwright/test
npx playwright install
```

## Configuration — playwright.config.ts

```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
    testDir: './e2e',
    fullyParallel: true,
    reporter: [
        ['html'],
        ['json', { outputFile: 'test-results.json' }]
    ],
    use: {
        baseURL: process.env.BASE_URL || 'http://localhost:3000',
        // Vidéo sur chaque test — obligatoire pour la review PO
        video: 'on',
        screenshot: 'only-on-failure',
        trace: 'retain-on-failure',
    },
    projects: [
        { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    ],
    // Lancer le serveur de dev automatiquement si pas déjà lancé
    webServer: {
        command: 'npm run dev',
        url: 'http://localhost:3000',
        reuseExistingServer: !process.env.CI,
    },
});
```

## Convention data-testid — OBLIGATOIRE

Tous les éléments interactifs doivent avoir un `data-testid`. C'est la seule façon
sûre de sélectionner des éléments dans les tests (les classes Tailwind changent).

```tsx
// Dans les composants Next.js/shadcn
<Button data-testid="create-appointment-btn" onClick={handleCreate}>
    Prendre rendez-vous
</Button>

<Input data-testid="appointment-date-input" {...field} />

<TableRow data-testid={`appointment-row-${appointment.id}`}>
    <TableCell data-testid="appointment-status">{appointment.status}</TableCell>
</TableRow>
```

```typescript
// Dans Playwright — toujours utiliser data-testid
await page.getByTestId('create-appointment-btn').click();
await page.getByTestId('appointment-date-input').fill('2025-06-15');
await expect(page.getByTestId('appointment-status')).toHaveText('Confirmed');
```

## Page Object Model — structure

```typescript
// e2e/pages/AppointmentsPage.ts
export class AppointmentsPage {
    private readonly page: Page;

    constructor(page: Page) {
        this.page = page;
    }

    // Locators
    get createButton() { return this.page.getByTestId('create-appointment-btn'); }
    get dateInput() { return this.page.getByTestId('appointment-date-input'); }
    get vetSelect() { return this.page.getByTestId('vet-select'); }
    get submitButton() { return this.page.getByTestId('appointment-submit-btn'); }
    get successToast() { return this.page.getByTestId('toast-success'); }
    get appointmentRows() { return this.page.getByTestId(/appointment-row-.*/); }

    // Actions
    async goto() {
        await this.page.goto('/appointments');
        await this.page.waitForLoadState('networkidle');
    }

    async createAppointment(opts: { vetName: string; date: string; time: string }) {
        await this.createButton.click();
        await this.dateInput.fill(opts.date);
        await this.vetSelect.selectOption({ label: opts.vetName });
        await this.submitButton.click();
    }

    // Assertions
    async expectAppointmentVisible(id: string) {
        await expect(this.page.getByTestId(`appointment-row-${id}`)).toBeVisible();
    }
}
```

## Tests — un fichier par Golden Path

```typescript
// e2e/tests/gp-01-appointment-booking.spec.ts
import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { AppointmentsPage } from '../pages/AppointmentsPage';

test.describe('GP-01: Appointment Booking', () => {
    let loginPage: LoginPage;
    let appointmentsPage: AppointmentsPage;

    test.beforeEach(async ({ page }) => {
        loginPage = new LoginPage(page);
        appointmentsPage = new AppointmentsPage(page);

        // Se connecter comme vétérinaire
        await loginPage.goto();
        await loginPage.loginAs('vet@clinic-dubai.com', 'password123');
        await expect(page).toHaveURL('/dashboard');
    });

    test('vet can book an appointment for a patient', async ({ page }) => {
        await appointmentsPage.goto();
        await appointmentsPage.createAppointment({
            vetName: 'Dr. Smith',
            date: '2025-12-15',
            time: '10:00'
        });

        // Assertion : succès visible
        await expect(appointmentsPage.successToast).toBeVisible();
        await expect(appointmentsPage.successToast).toContainText('Appointment created');
    });

    test('cannot book at an already-taken slot', async ({ page }) => {
        // Setup : créer un premier RDV
        await appointmentsPage.goto();
        await appointmentsPage.createAppointment({
            vetName: 'Dr. Smith',
            date: '2025-12-15',
            time: '10:00'
        });

        // Tenter de créer le même créneau
        await appointmentsPage.createAppointment({
            vetName: 'Dr. Smith',
            date: '2025-12-15',
            time: '10:00'
        });

        await expect(page.getByTestId('error-message')).toContainText('slot is already booked');
    });
});

// e2e/tests/gp-03-multi-tenant-isolation.spec.ts
test.describe('GP-03: Multi-tenant isolation', () => {
    test('clinic A cannot see clinic B appointments', async ({ browser }) => {
        // Deux contextes de navigateur = deux cliniques
        const contextA = await browser.newContext();
        const contextB = await browser.newContext();

        const pageA = await contextA.newPage();
        const pageB = await contextB.newPage();

        // Se connecter avec deux cliniques différentes
        await loginAs(pageA, 'vet@clinic-a.com');
        await loginAs(pageB, 'vet@clinic-b.com');

        // Clinique A crée un RDV
        const appointmentsA = new AppointmentsPage(pageA);
        await appointmentsA.goto();
        await appointmentsA.createAppointment({ vetName: 'Dr. A', date: '2025-12-20', time: '14:00' });

        // Clinique B ne doit pas voir ce RDV
        const appointmentsB = new AppointmentsPage(pageB);
        await appointmentsB.goto();
        const rows = appointmentsB.appointmentRows;
        await expect(rows).toHaveCount(0);  // ou n'inclut pas le RDV de A

        await contextA.close();
        await contextB.close();
    });
});
```

## Authentification — helper fixture

```typescript
// e2e/fixtures/auth.ts
import { test as base } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';

// Fixture pour les tests authentifiés
export const test = base.extend<{
    authenticatedPage: Page;
    vetPage: Page;
}>({
    authenticatedPage: async ({ page }, use) => {
        const loginPage = new LoginPage(page);
        await loginPage.goto();
        await loginPage.loginAs(
            process.env.TEST_VET_EMAIL || 'vet@test.com',
            process.env.TEST_VET_PASSWORD || 'Test1234!'
        );
        await use(page);
    },
});

// Utilisation dans les tests
import { test } from '../fixtures/auth';
test('appointment page loads', async ({ authenticatedPage }) => {
    await authenticatedPage.goto('/appointments');
    // ...
});
```

## Vidéo — workflow review PO

Les vidéos sont générées automatiquement dans `test-results/`.
L'Agent QA doit les joindre à la PR comme artefacts :

```yaml
# .github/workflows/e2e.yml
- name: Upload test videos
  uses: actions/upload-artifact@v4
  if: always()
  with:
    name: playwright-videos
    path: test-results/
    retention-days: 7
```

## Commandes

```bash
# Lancer tous les tests E2E
npx playwright test

# Lancer un fichier spécifique
npx playwright test e2e/tests/gp-01-appointment-booking.spec.ts

# Mode UI (debug interactif)
npx playwright test --ui

# Générer le rapport HTML
npx playwright show-report

# Générer les vidéos uniquement pour les tests échoués
# (configurer failed-only dans playwright.config.ts)
```

## Conventions

```
✅ data-testid sur tous les éléments interactifs
✅ Page Object Model — un fichier par page
✅ Un fichier spec par Golden Path
✅ Vidéo activée sur tous les tests (pour review PO)
✅ waitForLoadState('networkidle') après navigation
✅ Test data via API (pas via UI) pour le setup Given

❌ Pas de sleep (utiliser expect avec timeout ou waitFor)
❌ Pas de sélecteurs par classe CSS ou texte (sauf cas exceptionnels)
❌ Pas de logique conditionnelle dans les tests
❌ Pas d'accès direct à la DB dans les tests E2E
```

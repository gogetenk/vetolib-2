# Skill: shadcn/ui + Next.js 15 App Router

## Conventions de base

- App Router uniquement (`app/` directory)
- Server Components par défaut, `"use client"` uniquement si nécessaire
- shadcn/ui pour tous les composants UI (pas de composants custom si shadcn existe)
- `data-testid` obligatoire sur tous les éléments interactifs (pour Playwright)
- Tailwind CSS uniquement pour le styling

## Structure du projet frontend

```
vetolib-frontend/
├── app/
│   ├── (auth)/              ← route group — layout sans header
│   │   ├── login/
│   │   │   └── page.tsx
│   │   └── layout.tsx
│   ├── (dashboard)/         ← route group — layout avec header/sidebar
│   │   ├── appointments/
│   │   │   ├── page.tsx         ← Server Component — liste
│   │   │   ├── [id]/
│   │   │   │   └── page.tsx     ← Server Component — détail
│   │   │   └── new/
│   │   │       └── page.tsx     ← Server Component wrapping Client Form
│   │   ├── patients/
│   │   ├── medical-records/
│   │   └── billing/
│   ├── layout.tsx           ← Root layout
│   └── globals.css
├── components/
│   ├── ui/                  ← shadcn/ui (généré automatiquement, ne pas modifier)
│   └── features/
│       ├── appointments/
│       │   ├── AppointmentForm.tsx     ← "use client"
│       │   ├── AppointmentsTable.tsx   ← "use client"
│       │   └── AppointmentCard.tsx
│       └── patients/
├── lib/
│   ├── api/                 ← fonctions fetch vers le backend
│   │   ├── appointments.ts
│   │   └── patients.ts
│   └── utils.ts             ← cn() helper (shadcn)
└── e2e/                     ← Tests Playwright
```

## Composants shadcn — lesquels utiliser

| Besoin | Composant shadcn |
|---|---|
| Tableau de données | `DataTable` + `tanstack/react-table` |
| Formulaire | `Form` + `react-hook-form` + `zod` |
| Calendrier/Date | `Calendar` + `Popover` = DatePicker |
| Modal/Dialog | `Dialog` |
| Notifications | `Sonner` (toast) |
| Loader | `Skeleton` |
| Select/Combobox | `Select` ou `Command` + `Popover` |
| Badge statut | `Badge` avec variant |
| Navigation | `NavigationMenu` + `Sidebar` |

## Pattern Server Component + Client Form

```tsx
// app/(dashboard)/appointments/new/page.tsx — Server Component
import { AppointmentForm } from '@/components/features/appointments/AppointmentForm';
import { getVets } from '@/lib/api/appointments';

// Les données sont chargées côté serveur
export default async function NewAppointmentPage() {
    const vets = await getVets();  // fetch serveur, pas de loading state
    return (
        <div className="container mx-auto py-6">
            <h1 className="text-2xl font-bold mb-6">Nouveau rendez-vous</h1>
            <AppointmentForm vets={vets} />
        </div>
    );
}
```

```tsx
// components/features/appointments/AppointmentForm.tsx — Client Component
"use client";

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { toast } from 'sonner';
import { useRouter } from 'next/navigation';

const appointmentSchema = z.object({
    vetId: z.string().uuid('Please select a vet'),
    scheduledAt: z.string().datetime(),
    durationMinutes: z.number().int().min(15).max(240),
    notes: z.string().optional(),
});

type AppointmentFormData = z.infer<typeof appointmentSchema>;

interface Props {
    vets: VetDto[];
}

export function AppointmentForm({ vets }: Props) {
    const router = useRouter();
    const form = useForm<AppointmentFormData>({
        resolver: zodResolver(appointmentSchema),
        defaultValues: { durationMinutes: 30 },
    });

    async function onSubmit(data: AppointmentFormData) {
        const response = await fetch('/api/appointments', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data),
        });

        if (!response.ok) {
            const error = await response.json();
            toast.error(error.title || 'Failed to create appointment');
            return;
        }

        toast.success('Appointment created successfully');
        router.push('/appointments');
        router.refresh();
    }

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
                <FormField
                    control={form.control}
                    name="vetId"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Veterinarian</FormLabel>
                            <Select
                                onValueChange={field.onChange}
                                defaultValue={field.value}
                            >
                                <FormControl>
                                    <SelectTrigger data-testid="vet-select">
                                        <SelectValue placeholder="Select a vet" />
                                    </SelectTrigger>
                                </FormControl>
                                <SelectContent>
                                    {vets.map(vet => (
                                        <SelectItem key={vet.id} value={vet.id}>
                                            {vet.name}
                                        </SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <Button
                    type="submit"
                    disabled={form.formState.isSubmitting}
                    data-testid="appointment-submit-btn"
                >
                    {form.formState.isSubmitting ? 'Creating...' : 'Create Appointment'}
                </Button>
            </form>
        </Form>
    );
}
```

## DataTable — pattern pour les listes

```tsx
// components/features/appointments/AppointmentsTable.tsx
"use client";

import { ColumnDef } from '@tanstack/react-table';
import { DataTable } from '@/components/ui/data-table';
import { Badge } from '@/components/ui/badge';

const columns: ColumnDef<AppointmentDto>[] = [
    {
        accessorKey: 'scheduledAt',
        header: 'Date & Time',
        cell: ({ row }) => (
            <span data-testid="appointment-date">
                {formatDateTime(row.original.scheduledAt)}
            </span>
        ),
    },
    {
        accessorKey: 'vetName',
        header: 'Veterinarian',
    },
    {
        accessorKey: 'status',
        header: 'Status',
        cell: ({ row }) => (
            <Badge
                data-testid="appointment-status"
                variant={statusVariant(row.original.status)}
            >
                {row.original.status}
            </Badge>
        ),
    },
];

export function AppointmentsTable({ appointments }: { appointments: AppointmentDto[] }) {
    return (
        <DataTable
            columns={columns}
            data={appointments}
            // data-testid sur les rows via meta
        />
    );
}

function statusVariant(status: string) {
    return {
        Pending: 'secondary',
        Confirmed: 'default',
        Done: 'outline',
        Cancelled: 'destructive',
    }[status] ?? 'secondary';
}
```

## Fetch API — lib/api/

```typescript
// lib/api/appointments.ts
const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

export async function getAppointments(params?: {
    from?: string;
    to?: string;
    vetId?: string;
}): Promise<AppointmentDto[]> {
    const url = new URL(`${API_BASE}/api/appointments`);
    if (params?.from) url.searchParams.set('from', params.from);
    if (params?.to) url.searchParams.set('to', params.to);

    const res = await fetch(url, { cache: 'no-store' });  // pas de cache sur les listes
    if (!res.ok) throw new Error('Failed to fetch appointments');
    return res.json();
}

export async function createAppointment(data: CreateAppointmentRequest): Promise<AppointmentDto> {
    const res = await fetch(`${API_BASE}/api/appointments`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
    });
    if (!res.ok) {
        const error = await res.json();
        throw new ApiError(res.status, error);
    }
    return res.json();
}
```

## Conventions

```
✅ Server Components pour les pages (fetch serveur)
✅ "use client" uniquement pour les formulaires et interactions
✅ data-testid sur tous les boutons, inputs, rows, statuts
✅ shadcn/ui pour les composants (ne pas réinventer)
✅ react-hook-form + zod pour tous les formulaires
✅ Sonner pour les toasts (toast.success / toast.error)
✅ router.refresh() après mutation pour réactualiser les Server Components

❌ Pas de useState pour les données serveur (fetch dans Server Components)
❌ Pas de classe CSS custom si Tailwind suffit
❌ Pas de composants UI custom si shadcn en a un
❌ Pas de fetch dans les Server Components sans error boundary
```

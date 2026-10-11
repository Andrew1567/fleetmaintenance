# Архітектура

## Шари

| Шар | Проєкт | Що містить | Що йому заборонено |
|---|---|---|---|
| Домен | FleetMaintenance.Domain | сутності, перелічення, таблиця переходів, власні винятки | знати про файли, консоль, JSON, інші шари |
| Прикладний | FleetMaintenance.Application | сервіси сценаріїв, інтерфейси сховищ і журналу, політики розрахунку, валідація | знати про формат збереження й про консоль |
| Інфраструктура | FleetMaintenance.Infrastructure | JSON-репозиторії, DTO, файловий журнал, FixedTimeProvider | містити правила домену |
| Застосунок | FleetMaintenance.App | меню, читання вводу, форматування виводу, композиційний корінь | містити розрахунки й правила |

## Правило залежностей

```
App -> Infrastructure -> Application -> Domain
```

Залежності спрямовані всередину. Домен не має жодного посилання на інші проєкти; Application посилається лише на Domain; Infrastructure реалізує інтерфейси, оголошені в Application (`IVehicleRepository`, `IServiceOrderRepository`, `IAppLogger`). Правило закріплено посиланнями .csproj (компілятор не дозволить зворотну залежність) і тестами `ArchitectureTests`.

## Композиційний корінь

Єдине місце створення конкретних реалізацій — `Program.Compose` (`src/FleetMaintenance.App/Program.cs`).

## Точки розширення

1. Нова політика ціни — нова реалізація `IPricingPolicy`; змінюється один рядок у `Program.Compose`.
2. Новий тип ТЗ чи інтервал — нові рядки таблиці `IntervalTable` (та коефіцієнт у `PricingPolicy`).
3. Нове сховище (CSV, база даних) — нова реалізація `IVehicleRepository` / `IServiceOrderRepository`; наявні класи не змінюються.

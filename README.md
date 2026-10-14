# FleetMaintenance

Консольний застосунок обліку пробігу автопарку, який визначає, яким машинам час на технічне обслуговування, і планує замовлення на ТО.

**Варіант 16 · ПРІЗВИЩЕ Ім'я · група ____ · дисципліна «Конструювання програмного забезпечення»**

## Швидкий старт

```
git clone https://github.com/<логін>/fleetmaintenance.git && cd fleetmaintenance
dotnet build FleetMaintenance.sln -c Release
dotnet run --project src/FleetMaintenance.App
```

## Структура рішення

- `src/FleetMaintenance.Domain` — сутності, стани замовлення, власні винятки.
- `src/FleetMaintenance.Application` — сценарії, політики розрахунку, валідація, інтерфейси сховищ.
- `src/FleetMaintenance.Infrastructure` — JSON-сховища, файловий журнал, фіксований годинник.
- `src/FleetMaintenance.App` — консольне меню й композиційний корінь.
- `tests/FleetMaintenance.Tests` — модульні тести xUnit.
- `data/` — умовні дані (60 ТЗ); `docs/` — документація; `scripts/` — локальна перевірка.

## Можливості

Сценарії описано в [docs/spec.md](docs/spec.md): UC-01 зареєструвати пробіг, UC-02 запланувати обслуговування, UC-03 змінити стан замовлення, UC-04 звіт «машини, що потребують ТО», UC-05 знайти ТЗ, UC-06 додати ТЗ, UC-07 зберегти й завантажити дані. Посібник — [docs/user-guide.md](docs/user-guide.md).

## Тести й покриття

```
dotnet test FleetMaintenance.sln /p:CollectCoverage=true
```

Покриття міряється лише для бібліотеки логіки (проєкти Domain і Application; межа задана в `tests/FleetMaintenance.Tests/FleetMaintenance.Tests.csproj`). Поточне число: рядкове __ %, гілок __ %. Усі перевірки конвеєра однією командою: `./scripts/check.sh` (Linux/macOS) або `./scripts/check.ps1` (Windows).

## Технології

.NET 8, C# 12, xUnit, coverlet, аналізатори Roslyn (рівень latest, попередження як помилки), `dotnet format`, GitHub Actions.

## Обмеження й відомий борг

- Дані зберігаються у двох файлах JSON, що читаються цілком; для десятків тисяч ТЗ потрібне інше сховище.
- Інтервали й ціни зашито в код (TD-04), новий вид ТО потребує змін у коді (TD-05).
- Консольне меню не покрите автотестами (TD-01).
- Місяць рахується за календарем: від 31 січня до 28 лютого — 0 повних місяців.
- Застосунок однокористувацький: паралельний запис у файли не підтримується.

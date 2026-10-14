# План комітів (рекомендований порядок)

Не менше 25 комітів за конвенцією `тип(область): суть` не менш ніж за 6 різних днів. Перед кожним комітом — `dotnet build` і `dotnet test`. Кожен рядок — окремий коміт; розподіл по днях — орієнтир.

| День | Коміти |
|---|---|
| 1 | `chore: додано .gitignore, .editorconfig і Directory.Build.props`; `chore: створено рішення й проєкти`; `docs(spec): межі системи й словник`; `docs(spec): сценарії UC-01…UC-07` |
| 2 | `docs(spec): таблиці рішень і переходів`; `feat(domain): додано перелічення й ServicePoint`; `feat(domain): сутність Vehicle`; `test(domain): тести пробігу` |
| 3 | `feat(domain): таблиця переходів OrderTransitions`; `feat(domain): сутність ServiceOrder`; `test(domain): тести дозволених і заборонених переходів`; `docs(architecture): шари й правило залежностей` |
| 4 | `feat(app): IntervalTable і MaintenancePolicy`; `test(app): таблиця рішень стану ТО`; `feat(app): PricingPolicy і ServiceAdvisor`; `test(app): тести вартості` |
| 5 | `feat(app): InputValidator`; `test(app): тести валідації`; `feat(app): сервіси UC-01 і UC-06`; `test(app): сценарії UC-01 і UC-06` |
| 6 | `feat(app): сервіс UC-02 і UC-03`; `test(app): сценарії UC-02 і UC-03`; `feat(app): ReportService і пошук`; `test(app): звіт і пошук` |
| 7 | `feat(infra): JSON-репозиторії й журнал`; `test(infra): збереження й завантаження`; `feat(app): меню й композиційний корінь`; `data: умовні дані автопарку` |
| 8 | `ci: додано конвеєр GitHub Actions`; `docs(readme): запуск і структура`; `docs: посібник користувача`; `docs: журнал рішень і самооцінка`; тег `v1.0` |

Гілка `ci-demo` створюється окремо (розділ 5.2): навмисно зламайте форматування, надішліть гілку й не зливайте її.

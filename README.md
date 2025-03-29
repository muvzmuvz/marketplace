marketplace-api
📖 Описание проекта
marketplace-api — это серверная часть маркетплейса на ASP.NET Core 7, реализующая RESTful API для управления пользователями, товарами, корзиной, заказами, отзывами и историей просмотров. Проект включает ролевую модель (Admin, Seller, User), аутентификацию через JWT, фоновые задачи и интеграцию с Redis.

🚀 Ключевой функционал
🔐 Аутентификация и авторизация
Регистрация/вход для пользователей, продавцов и администраторов.

Ролевая модель с доступом к эндпоинтам через [Authorize(Roles = "...")].

Кастомные фильтры валидации (например, CustomAdminValidateFilter).

🛒 Корзина
Добавление/удаление товаров.

Просмотр корзины с пагинацией.

Кэширование корзины через Redis (RedisCartService).

📦 Товары
CRUD-операции для товаров с валидацией через FluentValidation.

История просмотров товаров (автоматическое добавление при просмотре).

Поиск по названию, получение топовых товаров.

📦 Заказы
Создание заказа с автоматической отправкой email через Hangfire.

Обновление статуса заказа (только для администратора).

Фильтрация заказов по дате.

✉️ Уведомления
Отправка email при регистрации, входе, оформлении заказа (фоновые задачи Hangfire).

📝 Отзывы
Добавление/обновление/удаление отзывов.

Фильтрация отзывов по пользователю или товару.

📊 Дополнительно
Логирование действий через ILogger.

Глобальная обработка ошибок с кастомными исключениями (NotFoundExeption, UserAlreadyExistsException).

Автоматическое маппинги DTO с AutoMapper.

🛠️ Технологии
Backend: ASP.NET Core 7

База данных: PostgreSQL + Entity Framework Core (миграции)

Кэширование: Redis

Аутентификация: JWT

Валидация: FluentValidation

Фоновые задачи: Hangfire

Маппинг: AutoMapper

Логирование: ILogger (встроенный)

Контейнеризация: Docker

Тестирование: xUnit (планируется)

📂 Структура проекта
Copy
marketplace-api/
├── Controllers/              # API-контроллеры (Auth, Cart, Product, Order и др.)
├── Services/                 # Бизнес-логика (AuthService, CartService, MailService...)
├── Repository/               # Репозитории для работы с БД (Cart, Order, User...)
├── Models/                   # Сущности БД (User, Product, Cart...)
├── ModelsDto/                # DTO для запросов/ответов
├── Middleware/               # Кастомное middleware
├── Extensions/               # Расширения (например, для DI)
├── Data/                     # Контекст БД и конфигурации
├── Migrations/               # Миграции Entity Framework
├── MappingProfiles/          # Профили AutoMapper
├── CustomExeption/           # Кастомные исключения
├── CustomFilter/             # Фильтры валидации
├── wwwroot/                  # Статические файлы
├── appsettings.json          # Конфигурация приложения
├── Dockerfile                # Конфигурация Docker
├── docker-compose.yml        # Оркестрация сервисов (API, PostgreSQL, Redis)
└── Program.cs                # Точка входа
🚀 Установка и запуск
1. Клонирование и настройка
bash
Copy
git clone https://github.com/ваш-репозиторий/marketplace-api.git
cd marketplace-api
2. Настройка окружения
Создайте appsettings.Development.json:

json
Copy
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=marketplace;Username=postgres;Password=12345;",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Secret": "your_super_secret_key",
    "ExpiryInMinutes": 120
  },
  "Hangfire": {
    "ConnectionString": "Host=localhost;Database=hangfire;Username=postgres;Password=12345;"
  }
}
3. Запуск через Docker
bash
Copy
docker-compose up --build
Сервисы:

API: http://localhost:5000

PostgreSQL: порт 5432

Redis: порт 6379

Hangfire Dashboard: http://localhost:5000/hangfire

📚 Документация API
Доступна через Swagger UI:
http://localhost:5000/swagger

Примеры эндпоинтов:

Аутентификация:

POST /api/admin/auth/login (только для админа с кодом 3347MM__rer).

POST /authseller/login (для продавцов).

POST /authuser/reg (регистрация пользователя).

Корзина:

POST /cart/add_pdouct_cart/{productId} (добавить товар).

GET /cart/products (просмотр корзины).

Товары:

GET /product/top_product (топовые товары).

POST /product/create (только для продавца/админа).

Заказы:

POST /order/orders (создать заказ).

PUT /order/orders/{orderId} (обновить статус заказа).

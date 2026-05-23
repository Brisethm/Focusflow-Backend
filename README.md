# 🧠 FocusFlow API — Backend (.NET 8 + ASP.NET Core)

API RESTful desarrollada en **ASP.NET Core Web API (.NET 8)** para la plataforma **FocusFlow**, enfocada en productividad, seguimiento emocional, gestión de tareas y acompañamiento personalizado.

El backend implementa autenticación con **Supabase Auth + JWT**, persistencia con **PostgreSQL (Supabase)** mediante **Entity Framework Core**, pruebas automatizadas y soporte para comunicación en tiempo real mediante **SignalR**.

---

# 🚀 Tecnologías Utilizadas

| Tecnología | Uso |
|------------|-----|
| ASP.NET Core Web API (.NET 8) | Desarrollo backend |
| Entity Framework Core | ORM y acceso a datos |
| PostgreSQL + Supabase | Base de datos |
| JWT Bearer Authentication | Autenticación |
| Supabase Auth | Gestión de usuarios |
| SignalR | Comunicación en tiempo real |
| xUnit | Testing |
| Coverlet | Cobertura |
| Docker | Contenedorización |
| Swagger | Documentación interactiva |

---

# 📂 Estructura del Proyecto

La solución se organiza en dos proyectos principales:

```plaintext
Focusflow Backend.sln
│
├── FocusFlowAPI/
│   ├── Controllers/      → Endpoints HTTP
│   ├── Services/         → Lógica de negocio
│   ├── Models/           → Entidades del dominio
│   ├── DTOs/             → Objetos de transferencia
│   ├── Security/         → Seguridad y validación JWT
│   ├── Extensions/       → Helpers/extensiones
│   ├── Hubs/             → SignalR
│   ├── Serialization/    → Conversores personalizados
│   └── Program.cs        → Configuración global
│
└── FocusFlow.Tests/
    ├── Controllers/      → Tests de endpoints
    ├── Services/         → Tests lógica negocio
    ├── Security/         → Tests autenticación
    └── Extensions/       → Tests helpers
```

---

# 🧩 Arquitectura General

El flujo principal de la aplicación sigue esta estructura:

```plaintext
Cliente (Frontend)
        │
        ▼
Controllers (API)
        │
        ▼
Services (Lógica negocio)
        │
        ▼
Entity Framework Core
        │
        ▼
PostgreSQL (Supabase)
```

---

# 🔐 Autenticación

El sistema utiliza:

- JWT Bearer Authentication
- Tokens emitidos mediante Supabase Auth
- Claims personalizados
- Validación automática en middleware ASP.NET Core

Ejemplo:

```csharp
builder.Services
.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(...);
```

Claims disponibles:

```json
{
   "sub":"user-id",
   "role":"authenticated",
   "aud":"authenticated"
}
```

---

# 📡 Comunicación en Tiempo Real

El backend implementa **SignalR** mediante:

```plaintext
Hubs/
└── TicketHub.cs
```

Permite:

- Actualización instantánea de tickets
- Comunicación en tiempo real
- Notificaciones

---

# 🗂 Funcionalidades Implementadas

Actualmente la API incluye módulos para:

### 👤 Usuarios
- Perfil de usuario
- Autenticación
- Gestión de datos

### ✅ Productividad
- Tareas
- Recordatorios
- Sesiones de enfoque
- Planes personalizados

### 🧠 Bienestar emocional
- Registros emocionales
- Cuestionarios

### 🎫 Soporte
- Tickets
- Respuestas

### 💳 Monetización
- Transacciones

---

# ⚙ Variables de Entorno

Configurar:

```json
{
  "ConnectionStrings": {
      "DefaultConnection": ""
  },

  "Jwt": {
      "Key":"",
      "Issuer":"",
      "Audience":""
  },

  "Supabase": {
      "Url":"",
      "AnonKey":""
  }
}
```

Se recomienda usar:

```plaintext
appsettings.Development.json
.env
```

Ambos excluidos mediante:

```plaintext
.gitignore
```

---

# 🐳 Docker

Construir:

```bash
docker build -t focusflow-api .
```

Ejecutar:

```bash
docker compose up
```

Configuración:

```plaintext
Dockerfile
compose.yaml
```

---

# 🚀 Instalación Local

Clonar:

```bash
git clone https://github.com/TU-USUARIO/focusflow-backend.git
```

Entrar:

```bash
cd focusflow-backend
```

Restaurar paquetes:

```bash
dotnet restore
```

Compilar:

```bash
dotnet build
```

Ejecutar:

```bash
dotnet run --project FocusFlowAPI
```

La API iniciará en:

```plaintext
https://localhost:5001
```

Swagger:

```plaintext
/swagger
```

---

# 🧪 Testing

Ejecutar pruebas:

```bash
dotnet test
```

Generar cobertura:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

Incluye pruebas para:

✔ Controllers  
✔ Services  
✔ Seguridad JWT  
✔ Extensions  

---

# 📈 Cobertura

Los reportes se generan automáticamente:

```plaintext
FocusFlow.Tests/TestResults/
```

Ejemplo:

```plaintext
coverage.cobertura.xml
```

---

# 🔒 Seguridad

El backend implementa:

- JWT
- Middleware ASP.NET
- Validación de Claims
- Gestión segura de secretos
- Exclusión de credenciales del repositorio

---

# 👩‍💻 Desarrollo

Proyecto desarrollado para **FocusFlow**, plataforma orientada a productividad, gestión emocional y acompañamiento personalizado.

---

## Licencia

Uso académico / privado.

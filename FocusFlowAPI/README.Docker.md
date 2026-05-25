🐳 Guía de Despliegue con Docker - FocusFlow Backend (.NET)

Este documento contiene las instrucciones necesarias para construir, configurar y ejecutar de forma segura la API backend de FocusFlow basada en .NET utilizando Docker y contenedores optimizados para producción.
📋 Prerrequisitos

Antes de comenzar, asegúrate de tener instalado lo siguiente en tu sistema:

    Docker Desktop (versión 20.10 o superior) o Docker Engine en Linux.

    Docker Compose (generalmente incluido en Docker Desktop).

🔒 Configuración del Entorno (.env)

El backend de FocusFlow depende de variables de entorno críticas para conectarse a PostgreSQL en Supabase, inicializar la validación de tokens JWT y configurar los entornos de ejecución en el contenedor.

Por estrictas razones de seguridad, el archivo .env real está ignorado en el repositorio. Sigue estos pasos para configurarlo en tu entorno local o servidor:

    En la raíz del proyecto backend, crea un nuevo archivo llamado .env.

    Copia la estructura de la siguiente plantilla y rellena los campos con tus credenciales e instancias reales:

Fragmento de código


# FOCUSFLOW BACKEND - CONFIGURACIÓN DE VARIABLES DE ENTORNO


# Configuración del Entorno de ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Conexión y Seguridad de Supabase
Supabase__Url=tu_url_de_supabase_aquí
Supabase__AnonKey=tu_anon_key_de_supabase_aquí

# Configuración del Token JWT
Jwt__Key=tu_clave_secreta_jwt_super_segura_aquí
Jwt__Issuer=tu_emisor_jwt_aquí
Jwt__Audience=authenticated

# Cadena de Conexión Estricta a la Base de Datos PostgreSQL (Supabase)
ConnectionStrings__SupabaseDb="Host=tu_host_aquí;Port=5432;Database=postgres;User Id=tu_usuario_aquí;Password=tu_contraseña_aquí;Ssl Mode=Require;Trust Server Certificate=true;Multiplexing=false;Keepalive=10;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=10;No Reset On Close=true"

    💡 Tip de .NET en Docker: En entornos Linux (como las imágenes Alpine de Docker), la estructura Seccion__Propiedad (con doble guion bajo) mapea automáticamente el objeto de configuración en tu archivo appsettings.json.

🛠️ Arquitectura y Seguridad del Contenedor

Para cumplir con las pautas de arquitectura limpia de SonarLint (DevSecOps) y garantizar un despliegue blindado en producción, el entorno se diseñó bajo las siguientes especificaciones:

    Construcción Multi-etapa (Multi-stage build): El código se compila usando el SDK completo de .NET, pero la imagen final solo contiene el ASP.NET Runtime (mucho más ligero y seguro).

    Puerto No-Root Alternativo: La aplicación está configurada a través de ASPNETCORE_URLS para escuchar exclusivamente en el puerto 8080. Esto evita el uso del puerto tradicional 80 que requiere privilegios elevados de administrador de red en Linux.

    Optimización de Base de Datos: La cadena de conexión incluye Pooling=true y un límite de conexiones ajustado (Maximum Pool Size=10) para no agotar los recursos del plan de Supabase cuando levantes múltiples contenedores.

🚀 Comandos de Ejecución

Si cuentas con un archivo docker-compose.yml unificado en la raíz, puedes automatizar todo el proceso.
1. Construir y Levantar con Docker Compose (Recomendado)

Este comando lee las variables de tu archivo .env, compila los contenedores necesarios y los pone a correr en segundo plano:
Bash

docker compose up --build -d

2. Construir la Imagen de .NET manualmente

Si prefieres compilar la imagen del backend de forma individual sin usar compose:
Bash

docker build -t focusflow-backend .

3. Ejecutar el Contenedor de la API

Para correr la API mapeando el puerto 8080 de forma segura hacia el exterior:
Bash

docker run -d -p 8080:8080 --env-file .env --name focusflow-api-container focusflow-backend

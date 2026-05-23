# Guía de Despliegue con Docker - FocusFlow

Este documento contiene las instrucciones necesarias para construir, configurar y ejecutar la aplicación utilizando **Docker** y **Docker Compose**.

## Prerrequisitos

Antes de comenzar, asegúrate de tener instalado lo siguiente en tu sistema:

*   [Docker Desktop](https://www.docker.com/products/docker-desktop/) (versión 20.10 o superior)
*   [Docker Compose](https://docs.docker.com/compose/install/) (incluido en Docker Desktop)

---

## Configuración del Entorno (`.env`)

La aplicación depende de variables de entorno para conectarse a los servicios (Base de datos, Supabase, JWT, etc.). 

Por seguridad, el archivo `.env` **no está incluido en el repositorio**. Sigue estos pasos para configurarlo:

1. En la raíz del proyecto,crea un nuevo archivollamado `.env`.
2. Define las variables necesarias. Un ejemplo de la estructura requerida es:

```env
# Configuración del Servidor
ASPNETCORE_ENVIRONMENT=Production

# Conexión y Seguridad (Ejemplo)
Supabase__Url="tu_url_de_supabase"
Supabase__AnonKey="tu_anon_key"
Jwt__Key="tu_jwt_key"
Jwt__Issuer="tu_jwt_issuer"
Jwt__Audience="authenticated"
ConnectionStrings__SupabaseDb="Host=tu_host;Port=tu_puerto;Database=tu_base_de_datos;User Id=tu_usuario;Password=tu_contraseña;Ssl Mode=Require;Trust Server Certificate=true;Multiplexing=false;Keepalive=10;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=10;No Reset On Close=true"
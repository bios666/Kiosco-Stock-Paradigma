# StockVentas

Aplicación de escritorio (Avalonia UI) para gestión de stock y ventas, con persistencia en MySQL/MariaDB.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- MySQL o MariaDB corriendo localmente (o accesible por red)

## Puesta en marcha (primera vez)

1. **Clonar el repo** y pararse en la carpeta del proyecto.

2. **Crear la base de datos y cargar el esquema:**

   ```bash
   sudo mariadb -e "CREATE DATABASE stockventas;"
   sudo mariadb stockventas < Database/schema.sql
   ```

3. **Crear un usuario de MySQL para la app** (o reusar uno existente con permisos sobre `stockventas`):

   ```sql
   CREATE USER 'stockventas_app'@'localhost' IDENTIFIED BY 'TU_PASSWORD';
   GRANT ALL PRIVILEGES ON stockventas.* TO 'stockventas_app'@'localhost';
   FLUSH PRIVILEGES;
   ```

4. **Configurar la cadena de conexión:** copiar `db.config.example` a `db.config` y completar la contraseña que usaste en el paso anterior.

   ```bash
   cp db.config.example db.config
   ```

   > `db.config` está en `.gitignore` a propósito porque contiene una contraseña: **no se sube al repo** y cada persona debe crear el suyo localmente.

5. **Ejecutar la aplicación** desde la raíz del proyecto (no desde `bin/`, ya que `db.config` se busca en el directorio de trabajo actual):

   ```bash
   dotnet run
   ```

6. **Primer inicio de sesión:** si todavía no existe ningún usuario con rol "Dueño", la app crea automáticamente uno de arranque:

   - Usuario: `admin`
   - Contraseña: `admin123`

   Se recomienda cambiar esa contraseña después de ingresar.

## Problemas comunes

- **"No se encontró 'db.config'"**: falta el paso 4 (copiar y completar `db.config.example`).
- **Error de conexión a MySQL**: verificar que el servicio de MySQL/MariaDB esté corriendo y que el usuario/contraseña/puerto en `db.config` coincidan con los configurados en el paso 3.
- **Tablas inexistentes / error de SQL**: falta cargar `Database/schema.sql` (paso 2).

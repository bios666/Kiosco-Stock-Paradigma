# StockVentas

Aplicación de escritorio (Avalonia UI) para gestión de stock y ventas, con persistencia en MySQL/MariaDB.

## Requisitos

La aplicación está hecha con Avalonia UI, que es **multiplataforma**: corre igual en Linux, Windows y macOS. No hay ninguna dependencia específica de un sistema operativo.

- [.NET SDK 10](https://dotnet.microsoft.com/download) (Windows, Linux o macOS, según corresponda)
- MySQL o MariaDB corriendo localmente (o accesible por red)
  - **Linux:** `mariadb` desde el gestor de paquetes de tu distro (`pacman -S mariadb` en CachyOS/Arch, `sudo apt install mariadb-server` en Zorin OS/Ubuntu-Debian).
  - **Windows:** [MySQL Installer / MySQL Community Server](https://dev.mysql.com/downloads/installer/) (incluye el cliente `mysql` de línea de comandos).

## Puesta en marcha (primera vez)

1. **Clonar el repo** y pararse en la carpeta del proyecto.

2. **Crear la base de datos y cargar el esquema:**

   - **Linux** (CachyOS, Zorin OS, cualquier distro con `mariadb`/`mysql` instalado):

     ```bash
     sudo mariadb -e "CREATE DATABASE stockventas;"
     sudo mariadb stockventas < Database/schema.sql
     ```

   - **Windows 10** (PowerShell o CMD, con el `mysql` de MySQL Installer en el PATH):

     ```powershell
     mysql -u root -p -e "CREATE DATABASE stockventas;"
     mysql -u root -p stockventas < Database\schema.sql
     ```

     (pide la contraseña del usuario `root` de MySQL que configuraste al instalarlo; no existe `sudo` en Windows).

3. **Crear un usuario de MySQL para la app** (o reusar uno existente con permisos sobre `stockventas`). Entrar a la consola SQL con `sudo mariadb` (Linux) o `mysql -u root -p` (Windows), y ejecutar:

   ```sql
   CREATE USER 'stockventas_app'@'localhost' IDENTIFIED BY 'TU_PASSWORD';
   GRANT ALL PRIVILEGES ON stockventas.* TO 'stockventas_app'@'localhost';
   FLUSH PRIVILEGES;
   ```

4. **Configurar la cadena de conexión:** copiar `db.config.example` a `db.config` y completar la contraseña que usaste en el paso anterior.

   - **Linux:** `cp db.config.example db.config`
   - **Windows (PowerShell/CMD):** `copy db.config.example db.config`

   > `db.config` está en `.gitignore` a propósito porque contiene una contraseña: **no se sube al repo** y cada persona debe crear el suyo localmente.

5. **Ejecutar la aplicación** desde la raíz del proyecto (no desde `bin/`, ya que `db.config` se busca en el directorio de trabajo actual). El comando es el mismo en cualquier sistema operativo:

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
- **"¿Esto corre en Windows?"**: sí, sin cambios de código. Cada persona del equipo (Linux o Windows) sigue los mismos pasos de este README, usando los comandos de su sistema operativo.

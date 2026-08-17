# README - Backend de Aplicación de Emergencia y Alerta (Tgestiona)

Este repositorio contiene el código fuente del **Backend API** desarrollado en **.NET** para la plataforma de gestión de emergencias y evacuación de colaboradores. El sistema está diseñado con una arquitectura robusta, de alta velocidad y orientada a eventos, utilizando **Dapper** para la persistencia de datos.

---

## 🎯 Objetivos del Proyecto

1. **Gestión Centralizada de Emergencias**: Permitir que los oficiales de seguridad activen alertas de evacuación masivas de forma instantánea.
2. **Monitoreo y Trazabilidad**: Registrar y recibir de manera segura la confirmación de estado de seguridad y geolocalización (latitud y longitud) de cada colaborador.
3. **Alto Rendimiento y Escalabilidad**: Utilizar un micro-ORM (Dapper) para garantizar consultas rápidas a la base de datos y un bajo consumo de recursos.
4. **Arquitectura Limpia y Mantenible**: Organizar la lógica de negocio en controladores, servicios y repositorios de forma simple y directa para mantener un código limpio, mantenible y fácil de probar.
5. **Comunicación Crítica**: Integración con OneSignal para disparar notificaciones push de alta prioridad (Data Messages) capaces de saltar bloqueos de pantalla y modos de silencio en dispositivos móviles.

---

## 🛠️ Tecnologías y Arquitectura

* **Framework**: .NET 10 (ASP.NET Core Web API)
* **Acceso a Datos**: **Dapper** (Micro-ORM para SQL Server)
* **Patrón de Arquitectura**: Arquitectura en capas (Controladores → Servicios → Repositorios)
* **Notificaciones Push**: **OneSignal** para envío de alertas de emergencia
* **Base de Datos**: SQL Server (Esquema `SOS`)

### Estructura del Proyecto

```text
SOS-Backend/
│
├── Models/                     # Entidades de C# que mapean las tablas SQL
│   ├── AlertaEvacuacion.cs
│   └── ConfirmacionSeguridad.cs
│
├── Repositories/               # Capa de acceso a datos con Dapper
│   ├── AlertasRepository.cs
│   └── ConfirmacionesRepository.cs
│
├── Services/                   # Lógica de negocio y servicios
│   ├── OneSignalNotificationService.cs    # Envío de notificaciones push
│   └── LoggerService.cs                   # Logging centralizado
│
├── Controllers/                # Controladores API REST
│   └── EmergencyController.cs   # Endpoints de emergencias
│
├── appsettings.json            # Configuración y cadenas de conexión
└── Program.cs                  # Configuración de servicios e inyección de dependencias

```

---

## 📊 Modelo de Base de Datos

El backend interactúa directamente con dos tablas principales ubicadas en el esquema `SOS`:

1. **`SOS.AlertasEvacuacion`**: Almacena los registros de las alarmas disparadas por los oficiales.
2. **`SOS.ConfirmacionesSeguridad`**: Registra la respuesta de los colaboradores, incluyendo su ubicación GPS (latitud y longitud), estado reportado (`A salvo`, `En peligro`) y comentarios.

---

## 🚀 Endpoints Principales de la API

### 1. Activar Alerta de Evacuación

* **Método**: `POST`
* **Ruta**: `/api/Emergency/activate`
* **Descripción**: Inicia una nueva alerta de emergencia a nivel corporativo.
* **Cuerpo de la Solicitud (JSON)**:
```json
{
  "idUsuario": "SEC_001",
  "tipoAlerta": "Sismo",
  "mensajeAlerta": "Alerta de sismo activada. ¡Evacúen de inmediato!",
  "latitudActivacion": -12.0464,
  "longitudActivacion": -77.0428,
  "descripcionUbicacionActivacion": "Oficina principal, segundo piso"
}

```



### 2. Confirmar Seguridad del Colaborador

* **Método**: `POST`
* **Ruta**: `/api/Emergency/confirm-safety`
* **Descripción**: Permite al colaborador enviar su ubicación actual y confirmar que se encuentra a salvo o requiere asistencia.
* **Cuerpo de la Solicitud (JSON)**:
```json
{
  "idUsuario": 12345,
  "alertaEvacuacionId": 1,
  "latitud": -12.0450,
  "longitud": -77.0435,
  "estadoReportado": "A salvo",
  "comentario": "Me encuentro en el punto de encuentro externo."
}

```



### 3. Obtener Última Alerta Activa

* **Método**: `GET`
* **Ruta**: `/api/Emergency/latest-alert`
* **Descripción**: Devuelve los detalles de la emergencia activa más reciente para que la aplicación móvil actúe en consecuencia.

---

## ⚙️ Configuración y Ejecución Local

1. **Clonar el repositorio** y abrir la solución en Visual Studio o tu IDE favorito compatible con .NET.
2. **Configurar la base de datos**: Actualiza la cadena de conexión en el archivo `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR;Database=TU_BASEDATOS;User Id=TU_USUARIO;Password=TU_PASSWORD;Trusted_Connection=False;TrustServerCertificate=True"
}

```


3. **Instalar dependencias necesarias** (si no se restauran automáticamente):
```bash
dotnet add package Dapper
dotnet add package RestSharp
dotnet add package System.Data.SqlClient

```


4. **Ejecutar el proyecto**: Presiona `F5` o ejecuta en la terminal:
```bash
dotnet run
```
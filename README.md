# Connect4GameG5
Juego Connect4 

# Proyecto Connect4

## Integrantes del grupo

| Nombre de usuario | Correo electrónico | Carné |
|------------------|-----------------|-------|
| dylaannep        | despinoza50014@ufide.ac.cr | FH23013870 |
| TayronGC         | tguzman30879@ufide.ac.cr   | FI23032601 |
| Mauma1306        | losmau1306@gmail.com       | FH23013896 |

## Frameworks y herramientas utilizadas

- .NET 8.0 Core
- Entity Framework Core 9
- SQL Server
- Visual Studio Code
- Bootstrap 5 (para el frontend)
- Mermaid (para diagramas)

## Tipo de aplicación

- MPA (Multi-Page Application)

## Arquitectura utilizada

- MVC (Modelo-Vista-Controlador)

## Diagrama de la base de datos

```mermaid
erDiagram
    JUGADOR {
        int Id PK "Clave primaria"
        int Identificacion "Identificación única de 9 dígitos"
        string Nombre "Nombre del jugador"
        int Marcador
        int Victorias
        int Derrotas
        int Empates
    }

    PARTIDA {
        int Id PK "Clave primaria"
        int Jugador1Id FK "Referencia a JUGADOR"
        int Jugador2Id FK "Referencia a JUGADOR"
        string Tablero "JSON de la matriz del tablero"
        int GanadorId FK "Referencia a JUGADOR (opcional)"
        int TurnoGuardado
        string Estado "EnCurso o Finalizada"
        datetime Fecha
    }

    JUGADOR ||--o{ PARTIDA : "Jugador1"
    JUGADOR ||--o{ PARTIDA : "Jugador2"
    JUGADOR ||--o{ PARTIDA : "Ganador"
```  

## Frameworks y herramientas utilizadas

### Clonar el repositorio
git clone [URL del repositorio]
cd [nombre_del_proyecto]

## Instalar dependencias
dotnet restore

## Compilar el proyecto
dotnet build


## Ejecutar Aplicacion
dotnet run
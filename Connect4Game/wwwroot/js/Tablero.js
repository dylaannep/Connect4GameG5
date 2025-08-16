const filas = 6;
const columnas = 7;
let turno = 1;
let juegoTerminado = false;
const tablero = Array(filas).fill().map(() => Array(columnas).fill(0));
console.log(tablero);
//console.log(jugador1.Nombre);
console.log("Desde Tablero.js");
console.log("Jugador 1:", jugador1);
console.log("Jugador 2:", jugador2);
console.log("Partida ID:", partidaIdcs);

function jugar(columna) {
    if (juegoTerminado) return;

    for (let fila = filas - 1; fila >= 0; fila--) {
        if (tablero[fila][columna] === 0) {
            tablero[fila][columna] = turno;

            colocarFicha(fila, columna, turno); // Sin animación

            // Actualiza el tablero a la BD con AJAX
            fetch(`/Game/ActualizarTablero`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    partidaId: partidaIdcs, // Asegúrate de que el ID de la partida esté disponible
                    tablero: tablero,
                    turno: turno,
                })
            })
            .then(response => { response.json(); })
            .then(data => {
                console.log("Tablero actualizado en la BD:", data);
            })
            .catch(error => {
                console.error("Error al actualizar el tablero:", error);
            });


            if (verificarVictoria(fila, columna, turno)) {
                juegoTerminado = true;
                const jugadorGanador = turno === 1 ? jugador1 : jugador2;
                setTimeout(() => alert(`¡Ganó el Jugador ${jugadorGanador.nombre} (${jugadorGanador.color})!`), 100);
                return;
            }

            if (esEmpate()) {
                juegoTerminado = true;
                setTimeout(() => alert("¡Empate!"), 100);
                return;
            }

            cambiarTurno();
            return;
        }
    }

    alert("¡Columna llena!");
}

function colocarFicha(fila, columna, jugador) {
    const celda = document.getElementById(`celda-${fila}-${columna}`);
    const ficha = document.createElement("div");

    ficha.classList.add("ficha");
    ficha.style.backgroundColor = jugador === 1 ? "red" : "yellow";

    celda.innerHTML = ''; // Quitar disco gris base
    celda.appendChild(ficha);
}

function cambiarTurno() {
    turno = turno === 1 ? 2 : 1;
    document.getElementById("turnoActual").textContent = `Jugador ${turno} (${turno === 1 ? jugador1.nombre : jugador2.nombre}
    (${turno === 1 ? jugador1.color : jugador2.color}))`;
}

function verificarVictoria(fila, columna, jugador) {
    return (
        contarEnDireccion(fila, columna, jugador, 0, 1) >= 4 || // Horizontal
        contarEnDireccion(fila, columna, jugador, 1, 0) >= 4 || // Vertical
        contarEnDireccion(fila, columna, jugador, 1, 1) >= 4 || // Diagonal ↘
        contarEnDireccion(fila, columna, jugador, 1, -1) >= 4   // Diagonal ↙
    );
}

function contarEnDireccion(fila, columna, jugador, dirFila, dirCol) {
    let total = 1;
    total += contar(fila, columna, jugador, dirFila, dirCol);
    total += contar(fila, columna, jugador, -dirFila, -dirCol);
    return total;
}

function contar(fila, columna, jugador, dirFila, dirCol) {
    let count = 0;
    let f = fila + dirFila;
    let c = columna + dirCol;
    while (f >= 0 && f < filas && c >= 0 && c < columnas && tablero[f][c] === jugador) {
        count++;
        f += dirFila;
        c += dirCol;
    }
    return count;
}

function esEmpate() {
    for (let fila = 0; fila < filas; fila++) {
        for (let col = 0; col < columnas; col++) {
            if (tablero[fila][col] === 0) return false;
        }
    }
    return true;
}

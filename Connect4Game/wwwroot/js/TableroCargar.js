const filas = 6;
const columnas = 7;
//let turno = 1;

let juegoTerminado;
if (estadoPartida === 'EnCurso') {
    juegoTerminado = false; // La partida no ha terminado
} else {
    juegoTerminado = true; // La partida ya ha terminado
}
console.log("Desde TableroCargar.js" + estadoPartida);
//const tablero = Array(filas).fill().map(() => Array(columnas).fill(0));

// Si viene vacío o null → crear nuevo, sino usarlo
const tablero = tableroGuardado && tableroGuardado.length > 0
    ? tableroGuardado
    : Array(filas).fill().map(() => Array(columnas).fill(0));


    console.log(tablero);
//console.log(jugador1.Nombre);
console.log("Desde Tablero.js");
console.log("Jugador 1:", jugador1);
console.log("Jugador 2:", jugador2);
console.log("Turno inicial:", turno);
console.log("Partida ID:", partidaIdcs);

document.addEventListener("DOMContentLoaded", () => {
    renderizarTablero(tablero);
});
function renderizarTablero(tablero) {
    for (let fila = 0; fila < filas; fila++) {
        for (let col = 0; col < columnas; col++) {
            if (tablero[fila][col] !== 0) {
                colocarFicha(fila, col, tablero[fila][col]);
            } else {
                // Celda vacía: ponemos el disco base
                const celda = document.getElementById(`celda-${fila}-${col}`);
                celda.innerHTML = '<div class="disco-base"></div>';
            }
        }
    }
}


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
        partidaId: partidaIdcs,
        tablero: tablero,
        turno: turno,
    })
})
.then(response => response.json())
.then(data => {
    console.log("Respuesta del backend:", data);

    if (data.success) {
        if (data.estado === "Finalizada") {
            juegoTerminado = true;
            if (data.ganadorId) {
                const jugadorGanador = (data.ganadorId === jugador1.id ? jugador1 : jugador2);
                //alert(`¡Ganó ${jugadorGanador.nombre}!`);
            } else {
                //alert("¡Empate!");
            }
        } else {
            // ahora usamos el turno que trae la BD 👇
            turno = data.turno;
            const jugador = turno === 1 ? jugador1 : jugador2;
            document.getElementById("turnoActual").textContent =
                `${jugador.nombre} (${jugador.color})`;
        }
    }
})
.catch(error => {
    console.error("Error al actualizar el tablero:", error);
});




            if (verificarVictoria(fila, columna, turno)) {
                juegoTerminado = true;
                const jugadorGanador = turno === 1 ? jugador1 : jugador2;
                mostrarMensaje("¡Ganó el jugador con ID " + jugadorGanador.nombre + "!", "success");
                //setTimeout(() => alert(`¡Ganó el Jugador ${jugadorGanador.nombre} (${jugadorGanador.color})!`), 100);
                return;
            }

            if (esEmpate()) {
                juegoTerminado = true;
                mostrarMensaje("¡Empate!", "warning");
                //setTimeout(() => alert("¡Empate!"), 100);
                return;
            }

            //cambiarTurno();
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


function mostrarMensaje(mensaje, tipo = "primary") {
    let mensajeDiv = document.getElementById("mensajePartida");

    // Cambia el estilo según el tipo (success, danger, warning, etc.)
    mensajeDiv.className = `alert alert-${tipo} text-center`;
    mensajeDiv.textContent = mensaje;

    // Mostrarlo (quita d-none)
    mensajeDiv.classList.remove("d-none");
}


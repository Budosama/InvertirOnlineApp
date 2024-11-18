$(document).ready(function () {
    
});

async function verDetalle(simbolo, mercado) {
    const detalleTableBody = document.getElementById('detalleTableBody');
    const simboloSeleccionado = document.getElementById('simboloSeleccionado');

    // Limpia la tabla y actualiza el símbolo seleccionado
    detalleTableBody.innerHTML = '';
    simboloSeleccionado.value = simbolo;

    try {
        // Realiza la solicitud al PageModel
        const response = await fetch(`?handler=Detalle&mercado=${mercado}&simbolo=${simbolo}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const detalle = await response.json();

            // Construye las filas de la tabla con los datos obtenidos
            const fila = `
                <tr>
                    <td>${detalle.simbolo || 'N/A'}</td>
                    <td>${detalle.tendencia || 'N/A'}</td>
                    <td>${detalle.cierreAnterior || 'N/A'}</td>
                    <td>${detalle.plazo || 'N/A'}</td>
                </tr>`;
            detalleTableBody.innerHTML = fila;
        } else {
            const errorText = await response.text();
            alert(`Error: ${errorText}`);
        }
    } catch (error) {
        console.error('Error al cargar el detalle:', error);
        alert('Ocurrió un error al intentar cargar el detalle.');
    }
}
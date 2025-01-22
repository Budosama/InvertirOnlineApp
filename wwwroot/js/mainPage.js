$(document).ready(function () {
    // Usar la variable valorizadoPorTipo que se pasa desde el servidor
    if (valorizadoPorTipo && valorizadoPorTipo.length > 0) {
        var labels = valorizadoPorTipo.map(item => item.Tipo || "Sin Tipo");
        var dataValorizado = valorizadoPorTipo.map(item => item.ValorizadoTotal);

        // Crear gráfico de dona
        crearGraficoDona('graficoDonaMainPage', labels, dataValorizado, "Sin Tipo");

        // Lógica para los activos
        for (var i = 0; i < informePorTipo.length; i++) {
            var grupo = informePorTipo[i];
            var tipoActivo = grupo[0].titulo?.tipo;
            var labels = grupo.map(item => item.titulo?.simbolo);
            var dataValorizado = grupo.map(item => item.valorizado);

            // Crear gráfico de dona para este tipo de activo
            crearGraficoDona("graficoDona'" + tipoActivo + "'", labels, dataValorizado, tipoActivo);
        }   
    }

    // Función para establecer el valor de TNA en el input
    $('#banco').on('change', function() {
        var valorSeleccionado = $(this).val(); // Obtener el valor seleccionado
        var tnaClientes = (parseFloat(valorSeleccionado.replace(',', '.')) * 100).toFixed(2); // Convertir a porcentaje y limitar a 2 decimales
        $('#tna').val(tnaClientes); // Rellenar el input de TNA
    });  
    
    document.getElementById('exportarExcel').addEventListener('click', function () {
        // Seleccionar la tabla HTML
        const tabla = document.querySelector('.table');
        
        // Crear una copia de la tabla para trabajar
        const tablaCopia = tabla.cloneNode(true);
        
        // Eliminar la columna "Detalle" (última columna, índice 12)
        tablaCopia.querySelectorAll('tr').forEach((fila) => {
            const celdaDetalle = fila.children[12]; // Índice de la columna Detalle
            if (celdaDetalle) {
                fila.removeChild(celdaDetalle);
            }
        });
        
        // Iterar por cada fila de la tabla para aplicar cambios
        tablaCopia.querySelectorAll('tr').forEach((fila) => {
            fila.querySelectorAll('td').forEach((celda, indice) => {
                // Reemplazar puntos solo en columnas específicas: PPC (6), Ganancia ($) (10), Valorizado (11)
                if ([6, 10, 11].includes(indice)) {
                    celda.textContent = celda.textContent.replace(/\./g, '').replace(/\,/g, '.');
                }
    
                // Arreglar formato de fecha en columna P. Compra (1)
                if (indice === 1) {
                    const fechaOriginal = celda.textContent.trim();
                    const partesFecha = fechaOriginal.match(/(\d{1,2})[\/\-](\d{1,2})[\/\-](\d{4})/);
                    if (partesFecha) {
                        const dia = partesFecha[1].padStart(2, '0');
                        const mes = partesFecha[2].padStart(2, '0');
                        const anio = partesFecha[3];
                        celda.textContent = `${dia}-${mes}-${anio}`;
                    }
                }
            });
        });
    
        // Crear un libro de Excel a partir de la tabla modificada
        const workbook = XLSX.utils.book_new();
        const worksheet = XLSX.utils.table_to_sheet(tablaCopia);
    
        // Agregar la hoja al libro
        XLSX.utils.book_append_sheet(workbook, worksheet, 'Portafolio');
    
        // Exportar el archivo
        XLSX.writeFile(workbook, 'portafolio.xlsx');
    });     
     
});

function enviarSimbolo(simbolo, operacionesJson) {
    var operaciones = JSON.parse(operacionesJson);
    var operacionesFiltradas = operaciones
        .filter(o => o.simbolo === simbolo)
        .sort((a, b) => new Date(a.fechaOrden) - new Date(b.fechaOrden));

    // Establecer el símbolo seleccionado en el campo oculto
    document.getElementById("simboloSeleccionado").value = simbolo;

    const tbody = $('#operacionesTableBody');
    tbody.empty(); // Limpiar el tbody

    let totalInvertido = 0;

    // Llenar la tabla con las operaciones filtradas
    operacionesFiltradas.forEach(operacion => {
        const fechaOrden = operacion.fechaOrden ? 
            new Date(operacion.fechaOrden).toLocaleDateString("es-AR", {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric'
            }).replace(/\//g, '-') : '-'; // Formato dd-MM-yyyy

        if(operacion.monto >= operacion.montoOperado * 100){
            operacion.monto /= 100;
        }

        if (operacion.tipo.toUpperCase() === "COMPRA") {
            totalInvertido += operacion.monto || 0;
        } else if (operacion.tipo.toUpperCase() === "VENTA") {
            totalInvertido -= operacion.montoOperado || 0;
        }   
        
        const row = `<tr>
            <td>${fechaOrden}</td>
            <td>${operacion.tipo ? operacion.tipo.toUpperCase() : '-'}</td>
            <td>${operacion.estado ? operacion.estado.toUpperCase() : '-'}</td>
            <td>${operacion.simbolo || '-'}</td>
            <td>${operacion.cantidadOperada ? operacion.cantidadOperada.toFixed(0) : '-'}</td>
            <td>${operacion.precioOperado ? operacion.precioOperado.toLocaleString("es-AR", { style: 'currency', currency: 'ARS' }) : '-'}</td>
            <td>${(operacion.monto && operacion.tipo == "Compra") ? operacion.monto.toLocaleString("es-AR", { style: 'currency', currency: 'ARS' }) : '-'}</td>
            <td>${operacion.montoOperado ? operacion.montoOperado.toLocaleString("es-AR", { style: 'currency', currency: 'ARS' }) : '-'}</td>
            <td>${(operacion.montoOperado && operacion.tipo == "Compra") ? (operacion.monto - operacion.montoOperado).toLocaleString("es-AR", { style: 'currency', currency: 'ARS' }) : '-'}</td>
        </tr>`;
        tbody.append(row);

        document.getElementById("totalInvertido").textContent = totalInvertido.toLocaleString("es-AR", { style: 'currency', currency: 'ARS' });
    });    
}

function abrirCalculadora(simbolo, cantidad, ppc, ultimoPrecio) {
    document.getElementById('simboloSeleccionado').value = simbolo;
    document.getElementById('cantidadActual').value = cantidad;
    document.getElementById('ppc').value = ppc;
    document.getElementById('ultimoPrecio').value = ultimoPrecio;

    document.getElementById('verSimbolo').textContent = simbolo;
    document.getElementById('verCantidadActual').textContent = cantidad.toString("F0");
    document.getElementById('verPPC').textContent = ppc.toLocaleString('es-AR', { style: 'currency', currency: 'ARS' });
    document.getElementById('verUltimoPrecio').textContent = ultimoPrecio.toLocaleString("es-AR", { style: 'currency', currency: 'ARS' });

    document.getElementById('verSimboloSeleccionado').textContent = simbolo;
}

function calcularAccionesAdicionales() {
    const cantidadActual = parseFloat(document.getElementById('cantidadActual').value);
    const ppc = parseFloat(document.getElementById('ppc').value);
    const ultimoPrecio = parseFloat(document.getElementById('ultimoPrecio').value);
    const gananciaDeseada = parseFloat(document.getElementById('gananciaDeseada').value) / 100;

    // Validación básica de datos ingresados
    if (isNaN(gananciaDeseada)) {
        alert('Por favor, ingrese un valor válido de ganancia deseada.');
        return;
    }

    const costoAnterior = ppc * cantidadActual;

    // Calcular el numerador y el denominador para Q2
    const numerador = (ultimoPrecio * cantidadActual) - costoAnterior - (gananciaDeseada * costoAnterior);
    const denominador = gananciaDeseada * ultimoPrecio;

    // Verificar si el denominador es válido (no cero)
    if (denominador == 0)
    {
        throw new ArgumentException("El denominador no puede ser cero.");
    }

    // Calcular la cantidad adicional de acciones
    let acciones = numerador / denominador;
    acciones = Math.round(Math.abs(acciones));
    const totalAInvertir = acciones * ultimoPrecio;

    // Mostrar resultado
    document.getElementById('accionesAComprar').textContent = Math.round(acciones); // sin decimales
    document.getElementById('verUltimoPrecioAComprar').textContent = ultimoPrecio.toLocaleString('es-AR', { style: 'currency', currency: 'ARS' });
    document.getElementById('totalAInvertir').textContent = totalAInvertir.toLocaleString('es-AR', { style: 'currency', currency: 'ARS' });
    document.getElementById('resultadoCalculo').classList.remove('d-none');
}

function toggleAllColumnsVisibility() {
    const columnClasses = ['cantidad', 'ganancia', 'valorizado'];
    const ojito = document.getElementById('ojito-general');
    const additionalValues = document.querySelectorAll('.ocultable');

    // Ocultar/mostrar valores en las columnas de la tabla
    columnClasses.forEach(className => {
        const cells = document.querySelectorAll(`.${className}`);
        cells.forEach(cell => {
            if (cell.dataset.originalValue === undefined) {
                cell.dataset.originalValue = cell.textContent;
                cell.textContent = '***';
            } else {
                cell.textContent = cell.dataset.originalValue;
                delete cell.dataset.originalValue;
            }
        });
    });

    // Ocultar/mostrar valores adicionales
    additionalValues.forEach(value => {
        if (value.dataset.originalValue === undefined) {
            value.dataset.originalValue = value.textContent;
            value.textContent = '***';
        } else {
            value.textContent = value.dataset.originalValue;
            delete value.dataset.originalValue;
        }
    });

    // Alternar icono del ojito
    ojito.classList.toggle('fa-eye');
    ojito.classList.toggle('fa-eye-slash');
}
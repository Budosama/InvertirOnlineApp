// Función para formatear como moneda (pesos)
function formatCurrency(input) {
    let value = parseFloat(input.value);
    if (!isNaN(value)) {
        input.value = value.toFixed(2); // Formatea con dos decimales
    }
}

// Función para formatear como porcentaje
function formatPercentage(input) {
    let value = parseFloat(input.value);
    if (!isNaN(value)) {
        input.value = value.toFixed(2); // Formatea con dos decimales
    }
}
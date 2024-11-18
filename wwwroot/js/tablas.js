$(document).ready(function () {
    $('th').on('click', function () {
        const table = $(this).parents('table').eq(0);
        const rows = table.find('tr:gt(0)').toArray().sort(comparer($(this).index()));
        this.asc = !this.asc; // alternar entre ascendente y descendente
        if (!this.asc) { rows.reverse(); }
        for (let i = 0; i < rows.length; i++) { table.append(rows[i]); }
    });

    function comparer(index) {
        return function (a, b) {
            const valA = getCellValue(a, index);
            const valB = getCellValue(b, index);
            return $.isNumeric(valA) && $.isNumeric(valB) ? valA - valB : valA.localeCompare(valB);
        };
    }

    function getCellValue(row, index) {
        const cell = $(row).children('td').eq(index).text().trim();

        // Convertir valores con formato de moneda o porcentaje a número entero sin comas ni puntos
        let num = cell.replace(/[^0-9,-]/g, '').replace(',', '.');
        num = parseFloat(num);

        // Multiplicar por 100 si es un número válido para evitar decimales
        return isNaN(num) ? cell : Math.round(num * 100);
    }
});
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

iconoVer = `<svg version="1.1" id="Capa_1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink"  width="24" height="24" x="0px" y="0px"
	 viewBox="0 0 24 24" style="enable-background:new 0 0 24 24;" xml:space="preserve">
<style type="text/css">
	.st5{fill:#4374BA;}
	.st6{fill:none;stroke:#FFFFFF;stroke-miterlimit:10;}
	.st7{fill:#FFFFFF;}
</style>
<path class="st5" d="M1.9,24h20.3c1,0,1.9-0.8,1.9-1.9V1.9c0-1-0.8-1.9-1.9-1.9H1.9C0.8,0,0,0.8,0,1.9v20.3C0,23.2,0.8,24,1.9,24z"
	/>
<g>
	<ellipse class="st6" cx="12.1" cy="11.8" rx="4.8" ry="5.2"/>
	<ellipse class="st7" cx="12.2" cy="11.8" rx="1.9" ry="2.1"/>
	<path class="st6" d="M1.8,11.8C2.1,11.3,5.7,6.2,12,6.2c6.3,0,9.9,5.3,10.2,5.8"/>
	<path class="st6" d="M22.2,11.4C21.9,11.9,18.3,17,12,17c-6.3,0-9.9-5.3-10.2-5.8"/>
</g>
</svg>`;
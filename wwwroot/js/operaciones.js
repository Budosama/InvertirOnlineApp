document.addEventListener('DOMContentLoaded', function () {
    const filtroGenerico = document.getElementById('filtroGenerico');
    if (filtroGenerico) {
        filtroGenerico.addEventListener('input', function () {
            const filter = this.value.toLowerCase();
            const rows = document.querySelectorAll('#tablaOperaciones tbody tr');

            rows.forEach(row => {
                const cells = row.querySelectorAll('td');
                let match = false;
                cells.forEach(cell => {
                    if (cell.textContent.toLowerCase().includes(filter)) {
                        match = true;
                    }
                });
                row.style.display = match ? '' : 'none';
            });
        });
    } else {
        console.error('Elemento con id "filtroGenerico" no encontrado');
    }
});

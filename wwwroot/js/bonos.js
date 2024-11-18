$(document).ready(function () {
    document.getElementById('filterEstado').addEventListener('change', function() {
        const filter = this.value;
        const rows = document.querySelectorAll('#bonosTable tr');
        
        rows.forEach(row => {
          const estado = row.cells[5].textContent.toLowerCase();
          row.style.display = (filter === 'all' || estado === filter) ? '' : 'none';
        });
      });
});
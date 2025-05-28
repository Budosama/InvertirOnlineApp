$(document).ready(function () {

    // Seleccionar el interruptor
    const darkModeToggle = document.getElementById('darkModeToggle');
    const icon = document.getElementById('modeIcon');

    // Elementos que necesitan modo oscuro
    const darkModeElements = document.querySelectorAll('.dark-mode-target, .modal-content, .modal-header, .table, .form-control, .btn-primary');

    // Función para alternar entre modos
    const toggleDarkMode = () => {
        document.body.classList.toggle('dark-mode');
        darkModeElements.forEach(el => el.classList.toggle('dark-mode'));

        const isDark = document.body.classList.contains('dark-mode');
        icon.textContent = isDark ? '🌙' : '🌞';

        localStorage.setItem('darkMode', isDark ? 'enabled' : 'disabled');
    };

    // Al cargar la página, verificar el estado guardado
    window.addEventListener('DOMContentLoaded', () => {
        const savedMode = localStorage.getItem('darkMode');
        const isDark = savedMode === 'enabled';

        document.body.classList.toggle('dark-mode', isDark);
        darkModeElements.forEach(el => el.classList.toggle('dark-mode', isDark));
        icon.textContent = isDark ? '🌙' : '🌞';
        darkModeToggle.checked = isDark;
    });

    // Evento para alternar
    darkModeToggle.addEventListener('change', toggleDarkMode);

    document.querySelectorAll('.generar-png').forEach((boton) => {
        boton.addEventListener('click', () => {
            // Seleccionar el div por ID o clase
            let imagen = document.querySelector('#contenidoImagen');

            if (!imagen) {
                console.error('No se encontró el div con el contenido a convertir en imagen.');
                return;
            }
    
            let nombreArchivo = imagen.getAttribute('data-nombre-img') || 'estadistica';
    
            let fondoActual = getComputedStyle(imagen).backgroundColor;
    
            // Color de fondo según el tema actual
            if (fondoActual === 'rgba(0, 0, 0, 0)' || fondoActual === 'transparent') {
                fondoActual = document.body.classList.contains('dark-mode') ? '#121212' : '#ffffff';
            }
    
            html2canvas(imagen, {
                backgroundColor: fondoActual, // Captura fondo transparente o heredado
                scale: 2, // Mejora la calidad visual
                useCORS: true, // Evita problemas con recursos externos
            }).then(canvas => {
                let link = document.createElement('a');
                link.download = `${nombreArchivo}.png`;
                link.href = canvas.toDataURL('image/png');
                link.click();
            });
        });
    });    
});
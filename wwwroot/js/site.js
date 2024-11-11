$(document).ready(function () {

    // Seleccionar el interruptor
    const darkModeToggle = document.getElementById('darkModeToggle');

    // Elementos que necesitan modo oscuro
    const darkModeElements = document.querySelectorAll('.modal-content, .modal-header, .table, .form-control, .btn-primary');

    // Cargar preferencia desde localStorage al iniciar
    if (localStorage.getItem('darkMode') === 'enabled') {
        document.body.classList.add('dark-mode');
        darkModeElements.forEach(el => el.classList.add('dark-mode'));
        darkModeToggle.checked = true;
    }

    // Función para alternar entre modos
    const toggleDarkMode = () => {
        document.body.classList.toggle('dark-mode');
        darkModeElements.forEach(el => el.classList.toggle('dark-mode'));
        if (document.body.classList.contains('dark-mode')) {
            localStorage.setItem('darkMode', 'enabled');
        } else {
            localStorage.setItem('darkMode', 'disabled');
        }
    };

    // Evento para alternar entre modos
    darkModeToggle.addEventListener('change', toggleDarkMode);

});
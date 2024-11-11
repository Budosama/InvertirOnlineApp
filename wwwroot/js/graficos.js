function crearGraficoDona(canvasId, labels, data) {
    const ctx = document.getElementById(canvasId);
    
    if (ctx) {
        // Crear un array de objetos para poder ordenar los datos y etiquetas juntos
        const datosOrdenados = labels.map((label, index) => ({
            label: label,
            value: data[index]
        }));

        // Ordenar de mayor a menor en función del valor
        datosOrdenados.sort((a, b) => b.value - a.value);

        // Separar de nuevo en arrays de labels y data después de la ordenación
        const labelsOrdenados = datosOrdenados.map(item => item.label);
        const dataOrdenados = datosOrdenados.map(item => item.value);

        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labelsOrdenados,
                datasets: [{
                    label: 'Tipos Activos',
                    data: dataOrdenados,
                    borderWidth: 1,
                    backgroundColor: ['#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF', '#FF9F40'] // Colores opcionales
                }]
            },
            options: {
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: function (tooltipItem) {
                                let total = dataOrdenados.reduce((a, b) => a + b, 0);
                                let porcentaje = ((tooltipItem.raw / total) * 100).toFixed(2);
                                return `${tooltipItem.label}: ${porcentaje}% (${tooltipItem.raw})`;
                            }
                        }
                    },
                    datalabels: {
                        color: '#fff',
                        formatter: function (value, context) {
                            let total = context.chart.data.datasets[0].data.reduce((a, b) => a + b, 0);
                            let porcentaje = ((value / total) * 100).toFixed(2);
                            return `${porcentaje}%`;
                        }
                    }
                }
            }
        });
    } else {
        console.log("No se encontró el contenedor " + canvasId);
    }
}
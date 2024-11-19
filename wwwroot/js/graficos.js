function crearGraficoDona(canvasId, labels, data, tiposActivos) {
    const ctx = document.getElementById(canvasId);

    if (ctx) {
        const datosOrdenados = labels.map((label, index) => ({
            label: label,
            value: data[index],
            tipo: tiposActivos
        }));

        datosOrdenados.sort((a, b) => b.value - a.value);

        const labelsOrdenados = datosOrdenados.map(item => item.label);
        const dataOrdenados = datosOrdenados.map(item => item.value);
        const tiposOrdenados = datosOrdenados.map(item => item.tipo);

        const coloresBase = {
            "BONOS": "#FFD700",
            "ACCIONES": "#00FF7F",
            "LEACAPS": "#FF4500",
            "CEDEARS": "#1E90FF",
            "FCI": "#FF0000",
            "ON": "#FFF000"
        };

        const generarTonos = (baseColor, cantidad) => {
            const tonos = [];
            for (let i = 0; i < cantidad; i++) {
                const porcentaje = i / (cantidad * 2);
                tonos.push(aclararColor(baseColor, porcentaje));
            }
            return tonos;
        };

        const aclararColor = (hexColor, porcentaje) => {
            const num = parseInt(hexColor.slice(1), 16);
            const r = Math.min(255, Math.floor((num >> 16) * (1 + porcentaje)));
            const g = Math.min(255, Math.floor(((num >> 8) & 0x00FF) * (1 + porcentaje)));
            const b = Math.min(255, Math.floor((num & 0x0000FF) * (1 + porcentaje)));

            return `#${((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1)}`;
        };

        const colores = dataOrdenados.map((_, index) => {
            const tipo = tiposOrdenados[index];
            const baseColor = coloresBase[tipo] || "#800080";
            const cantidad = dataOrdenados.filter((_, i) => tiposOrdenados[i] === tipo).length;
            return generarTonos(baseColor, cantidad)[index % cantidad];
        });

        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labelsOrdenados,
                datasets: [{
                    label: 'Tipos Activos',
                    data: dataOrdenados,
                    borderWidth: 1,
                    backgroundColor: colores
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
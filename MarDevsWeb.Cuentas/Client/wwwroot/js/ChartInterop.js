
function datalabelsConfig(canvasID) {

    // Register the plugin to all charts:
    //Chart.register(ChartDataLabels);

    let chart = window.ChartJsInterop.BlazorCharts.get(canvasID);
       
   
 
    //Tooltip decimal separator
    chart.options.tooltips.callbacks.label = function (tooltipItem, data) {
        let dataset = data.datasets[tooltipItem.datasetIndex];

        // return formatted string here
        //return data.labels[tooltipItem.index] + ": " + Number(dataset.data[tooltipItem.index]).toFixed(2).toLocaleString().replace(".", ",");
        return data.labels[tooltipItem.index] + ": " + Number(dataset.data[tooltipItem.index]).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2, currency: 'ARS' });
    }

    //chart.options.plugins.datalabels.font.weight = 'bold';
    chart.options.plugins.datalabels.color = '#ffffff'

    

    //Datalabel decimal separator
    chart.options.plugins.datalabels.formatter = function (value, context) {
        // return formatted string here        //let ds = context.chart.data.datasets[context.datasetIndex];           

        let dsMeta = context.chart.getDatasetMeta(context.datasetIndex)
        var ttl = dsMeta.total;
        return ((value / ttl) * 100).toLocaleString('es-AR', { minimumFractionDigits: 1, maximumFractionDigits: 1 }) + '%';

        //return context.chart.data.labels[context.dataIndex];
    }

    //chart.options.plugins.datalabels.font.weight = 'bold';
    //chart.options.plugins.datalabels.font.color = '#ffffff'
    
    
    chart.update();
}



getTotal = (arr) => {
    let total = 0
    for (let i = 0; i < arr.length; i++) {
        total += arr[i];
    }
    return total
}
// Exportamos la función como un módulo aislado de JavaScript
export function inicializarReceptorVoz(dotNetHelper) {
    // Soporte para Chrome, Edge (webkit) y navegadores modernos
    const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;

    if (!SpeechRecognition) {
        console.error("Este navegador no soporta reconocimiento de voz nativo.");
        alert("Tu navegador no soporta control por voz. Prueba usando Google Chrome o Microsoft Edge.");
        return;
    }

    const recognition = new SpeechRecognition();
    recognition.lang = 'es-ES';        // Configurado en español
    recognition.continuous = false;    // Se detiene automáticamente cuando el usuario deja de hablar
    recognition.interimResults = false; // Solo nos interesa el resultado final procesado

    // Evento que se dispara cuando el navegador entiende la frase
    recognition.onresult = (event) => {
        if (event.results && event.results.length > 0) {
            const textoEscuchado = event.results[0][0].transcript;

            // Enviamos el texto de vuelta al método C# de Blazor
            dotNetHelper.invokeMethodAsync('ProcesarComandoVoz', textoEscuchado);
        }
    };

    // Control de errores (ej. si el usuario deniega el permiso del micrófono)
    recognition.onerror = (event) => {
        console.error("Error en el reconocimiento de voz: ", event.error);
    };

    // Activa el micrófono físicos del dispositivo
    recognition.start();
}

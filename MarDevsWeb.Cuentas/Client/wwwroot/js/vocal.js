let recognition = null;
let dotnetRef = null;
let transcripcionAcumulada = "";
let textoEnTiempoReal = ""; // Guardamos el texto intermedio

export function iniciarGrabacionVoz(dotNetObject) {
    dotnetRef = dotNetObject;
    transcripcionAcumulada = "";
    textoEnTiempoReal = "";

    const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (!SpeechRecognition) {
        alert("Tu navegador no soporta el reconocimiento de voz.");
        return;
    }

    recognition = new SpeechRecognition();
    recognition.lang = 'es-AR'; 
    recognition.continuous = true;
    recognition.interimResults = true;

    recognition.onresult = (event) => {
        textoEnTiempoReal = ""; // Reiniciamos el buffer temporal

        for (let i = event.resultIndex; i < event.results.length; ++i) {
            const transcript = event.results[i][0].transcript;

            if (event.results[i].isFinal) {
                transcripcionAcumulada += transcript + " ";
            } else {
                textoEnTiempoReal += transcript;
            }
        }
    };

    recognition.onerror = (event) => {
        console.error("Error en reconocimiento de voz:", event.error);
    };

    recognition.start();
}

export function detenerGrabacionVoz() {
    if (recognition) {
        recognition.stop();

        // Damos un pequeño margen para que el navegador cierre el stream
        setTimeout(() => {
            if (dotnetRef) {
                // Si transcripcionAcumulada quedó vacía porque no llegó a 'isFinal', 
                // usamos textoEnTiempoReal para no perder la voz dictada.
                let textoFinal = transcripcionAcumulada.trim();

                if (!textoFinal && textoEnTiempoReal) {
                    textoFinal = textoEnTiempoReal.trim();
                }

                dotnetRef.invokeMethodAsync('ProcesarComandoVoz', textoFinal);
            }
        }, 300);
    }
}
let recognition = null;
let dotnetRef = null;
let transcripcionAcumulada = "";
let textoEnTiempoReal = "";

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
    // Para frases cortas de comando/ingreso, false evita que Android abra múltiples sesiones de buffer
    recognition.continuous = false;
    recognition.interimResults = true;

    recognition.onresult = (event) => {
        let finalTemp = "";
        let interimTemp = "";

        for (let i = 0; i < event.results.length; ++i) {
            const transcript = event.results[i][0].transcript;

            if (event.results[i].isFinal) {
                finalTemp += transcript + " ";
            } else {
                interimTemp += transcript + " ";
            }
        }

        transcripcionAcumulada = finalTemp;
        textoEnTiempoReal = interimTemp;
    };

    recognition.onerror = (event) => {
        console.error("Error en reconocimiento de voz:", event.error);
    };

    // Cuando el motor de voz se detiene formalmente (por stop() o por silencio)
    recognition.onend = () => {
        if (dotnetRef) {
            // Combinamos el texto final con el interino para no perder lo dictado al último segundo
            let textoCompleto = (transcripcionAcumulada + " " + textoEnTiempoReal).trim();

            // Limpieza básica de espacios dobles
            textoCompleto = textoCompleto.replace(/\s+/g, ' ');

            if (textoCompleto) {
                dotnetRef.invokeMethodAsync('ProcesarComandoVoz', textoCompleto);
            }

            // Limpiamos referencias
            dotnetRef = null;
        }
    };

    recognition.start();
}

export function detenerGrabacionVoz() {
    if (recognition) {
        // Al llamar a stop(), el motor procesará lo que le quedó en el buffer y disparará 'onend'
        recognition.stop();
    }
}
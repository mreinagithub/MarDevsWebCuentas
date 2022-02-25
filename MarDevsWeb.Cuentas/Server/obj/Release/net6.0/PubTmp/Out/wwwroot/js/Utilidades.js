
function MostrarMensaje(titulo, mensaje) {
    alert(titulo + '\n' + mensaje);
}

function focusElement(id) {
    
    setTimeout(() => {
        const element = document.getElementById(id);
        element.focus()
    },200);
}

//function timerInactivo(dotnetHelper) {

//    var timer;
//    document.onmousemove = resetTimer;
//    document.onkeypress = resetTimer;

//    function resetTimer() {
//        clearTimeout(timer);
//        timer = setTimeout(logout, 1000 * 60 * 10); //10 minutos
//    }

//    function logout() {
//        dotnetHelper.invokeMethodAsync("Logout");
//    }

//}

function mostrarModal(id) {
    
    $('#' + id).modal({
        backdrop: 'static',
        keyboard: false  // to prevent closing with Esc button (if you want this too)})
    }, 'show');
}

function ocultarModal(id) {

    $('#' + id).modal('hide');
}

function initBootstrapSelect(id) {        

    $('#' + id).selectpicker({
        liveSearch: true,
        style: '',
        styleBase: 'form-control',
        //datasize: '5',
        dropupAuto: false
    });     
  
}

function actualizarValorBootstrapSelect(id, valor) {

    $('#' + id).selectpicker('val', valor);
}

function seleccionarSelectItem(id, valor) {
    $("#" + id).val(valor);
    //$('#' + id + ' option["'+valor+'"]').attr("selected", "selected");
    //[value="0"]
}

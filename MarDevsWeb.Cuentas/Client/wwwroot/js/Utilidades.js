
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


function CookiesSetCustom(id, value, expireDays) {
    Cookies.set(id, value, { expires: expireDays });
}

function SetTemaAplication(tema) {

    if (tema == "OSCURO") {
        $('#topmostPageId').addClass("modo-oscuro");
        //$('#app').addClass("modo-oscuro");
    }
    else {
        $('#topmostPageId').removeClass("modo-oscuro");
        //$('#app').addClass("modo-oscuro");
    }    
}

function saveAsFile(filename, bytesBase64){
    var link = document.createElement('a');
    //link.innerText="CLIK ACA"
    link.download = filename;
    link.href = "data:application/octet-stream;base64, " + bytesBase64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function setFooterText() {    
    $('.app-footer').removeAttr("hidden")
    $('#footerId')[0].innerText = "MarDevs® 2021-2024 - Todos los derechos reservados. Para consultas y comentarios enviar un e-mail a: infomardevs@gmail.com"
}
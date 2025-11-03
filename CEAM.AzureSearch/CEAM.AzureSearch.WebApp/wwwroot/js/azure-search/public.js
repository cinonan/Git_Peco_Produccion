//function activarPopUp(fp) {

//    var v_file = '';
//    if ($(fp).data('file')) {
//        v_file = '<a href="https://demopcstorage.blob.core.windows.net/contproveedor/Documentos/Productos/' + $(fp).data('file') + '"><i class="fa fa-download icono color-morado m-r-20"></i></a>';
//    }

//    var v_img = "https://saeusceprod01.blob.core.windows.net/contproveedor/Imagenes/Productos/" + $(fp).data('img');
//    var v_acuerdo = $(fp).data('acuerdo');
//    var v_catalogo = $(fp).data('catalogo');
//    var v_categoria = $(fp).data('categoria');
//    var v_descripcion = $(fp).data('descripcion');
//    var v_caracteristica = $(fp).data('caracteristica');
    
//    console.log(v_caracteristica);

//    var data = '<div class="popup-cabecera fondo-morado color-blanco">Información detallada</div>' +
//        '<div class="popup-imagen"><img class="mx-auto d-block" src="' + v_img + '"></div>' +
//        '<hr>' +
//        '<h2 class="popup-titulo color-morado">' + v_categoria + '</h2>' +
//        '<p class="popup-texto-1 color-gris-5">' + v_catalogo + '</p>' +
//        '<p class="popup-texto-1 color-gris-5"><strong>' + v_acuerdo + '</strong></p>' +
//        '<p class="popup-texto-2 color-gris-4">' + v_descripcion + '</p>' +
//        '<p class="popup-texto-1 color-gris-5"><strong>Características:</strong></p>' +
//        '<div class="contenedor" style="overflow: hidden;"><div class="row">' +
//        '<div class="col-md-12 ml-1">' +
//        '<ul class="popup-lista color-gris-4 row ml-0">' + v_caracteristica + '</ul>' +
//        '</div>' +
//        '</div></div>' +
//        '<hr>' +
//        '<div class="popup-pie color-morado">' +
//        '<i class="fa fa-file icono color-morado m-r-20"></i>' +
//        v_file +
//        //'<a href="' + v_file + '"><i class="fa fa-download icono color-morado m-r-20"></i></a>' +
//        //'<i class="fa fa-download icono color-morado m-r-20"></i>' +
//        '<i class="fa fa-plus icono color-morado"></i>' +
//        '</div>';
//    $('#respuesta-oculta').html(data);
//    $('#boton-oculto').click();
//}

function realizarBusqueda(informacion) {

}

function botonMenuSuperior(objeto) {
    var estadoMenu = $('.contenedor-menu').css('display');
    var anchoMenu = $('.contenedor-menu').css('width');
    $('#boton-menu-superior').css('display', 'none').fadeOut(500);
    if (estadoMenu == 'block') {
        $('#boton-menu-superior').html('<i class="fa fa-bars icono color-blanco"></i>');
        $('.contenedor-menu').css('display', 'none');
    } else {
        if (anchoMenu == '60px') {
            $('.menu-opciones').css('display', 'block');
            $('.contenedor-menu').css('width', '260px');
        }
        $('#boton-menu-superior').html('<i class="fa fa-times icono color-blanco"></i>');
        $('.contenedor-menu').css('display', 'block');
    }
    $('#boton-menu-superior').fadeIn(500);
}

function botonMenuLateral(objeto) {
    var hijo = $(objeto).children('i').attr('class');
    $('#boton-menu-lateral').css('display', 'none').fadeOut(500);
    if (hijo == 'fa fa-times icono color-blanco') {
        $('#boton-menu-lateral').html('<i class="fa fa-bars icono color-blanco"></i>');
        $('.contenedor-menu').animate({ 'width': '60px' }, 500);
        $('.contenedor-principal').animate({ 'padding-left': '60px' }, 500);
        $('.menu-opciones').css('display', 'none').fadeOut(500);
    } else {
        $('#boton-menu-lateral').html('<i class="fa fa-times icono color-blanco"></i>');
        $('.contenedor-menu').animate({ 'width': '260px' }, 500);
        $('.contenedor-principal').animate({ 'padding-left': '260px' }, 500);
        $('.menu-opciones').fadeIn(1000);
    }
    $('#boton-menu-lateral').fadeIn(500);
}

function detectarAncho() {
    var ancho = $(window).width();
    var anchoMenu = $('.contenedor-menu').css('width');
    if (ancho <= 768) {
        $('.contenedor-menu').css('display', 'none');
        $('.contenedor-principal').css('padding-left', '0');
    } else {
        $('.contenedor-menu').css('display', 'block');
        if (anchoMenu == '60px') {
            $('.contenedor-principal').css('padding-left', '60px');
        } else {
            $('.contenedor-principal').css('padding-left', '260px');
        }
    }
}

$(window).resize(function () {
    detectarAncho();
});
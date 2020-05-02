﻿
$.get("/Productos/ListarCards/", function (data) {

    var contenido = "";



    for (var i = 0; i < data.length; i++) {


        contenido += "<div class='card' style='width:300px'>";

        contenido += "<img class='card-img-top' src='" + data[i].UBICACION + "' alt='Card image'>";

        contenido += "<div class='card-body text-center'>";
        //contenido += "<small id='idCodigo'>" + data[i].CODIGO + "</small>";


        contenido += "<h4 class='card-title'>" + data[i].NOMBRE + " - " + data[i].MARCA + "</h4>";

        contenido += "<p class='card-text'>" + data[i].DESCRIPCION + "</p>";
        contenido += "<h3 class='card-title'>" + parsearMoneda(data[i].PRECIO_UNITARIO) + "</h3>";
        contenido += "<button id='btnAgregarAlCarrito' class='btn btn-primary' onclick='agregarAlCarrito(" + data[i].CODIGO + ")' >Agregar al carrito</button>";
      
        contenido += "</div>";
        contenido += "</div>";

    }



    document.getElementById("divRenderizado").innerHTML = contenido;


});

function parsearMoneda(decimal) {

    return new Intl.NumberFormat("es-AR", { style: "currency", currency: "ARS" }).format(decimal);
};


function agregarAlCarrito(codigo) {

    alert(codigo);
}
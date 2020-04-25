﻿$(document).ready(function () {


    //SE EJECUTA DESPUES DE HACER CLIC EN EL BOTON INICIAR SESION
    $("#btnLogin").click(function () {


        var frm = new FormData();

        //colocar en una variable el valor de cada elemento
        var usuario = document.getElementById("txtUsuario").value;
        var clave = document.getElementById("txtClave").value;

        //relacionar el valor de cada elemento con la clase que le corresponde
        frm.append("USUARIO", usuario);
        frm.append("CLAVE", clave);


        $.ajax({
            type: "POST",
            url: "/Usuarios/ValidarLogin/",
            data: frm,
            contentType: false,
            processData: false,
            success: function (data) {

                if (data == 1) {
                    //Declaraciones ejecutadas cuando el resultado de expresión coincide con el valor1
                    alertify.success("Bienvenido " + usuario + "!");

                    location.href = '/Usuarios/Index/';

                } else if (data == 2) {
                    //Declaraciones ejecutadas cuando el resultado de expresión coincide con el valor2
                    alertify.success("Cambiar clave " + usuario + "!");

                    location.href = '/Usuarios/CambiarClave/';

                } else {
                    //Declaraciones ejecutadas cuando ninguno de los valores coincide con el valor de la expresión

                    alertify.error("Usuario y/o Contraseña Incorrectos");

                }






            }

        });



    });


});




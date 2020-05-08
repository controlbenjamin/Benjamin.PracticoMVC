﻿renderizarMenu();
function renderizarMenu() {

    var rol = "CLI";
    var menu = "";

    menu += "<ul class='navbar-nav mr-auto'>";

    if (rol == "ADMIN") {
        menu += "<li class='nav-item'>";
        menu += "<a class='nav-link' href='/Usuarios/ABM'> <i class='fas fa-users'></i> Usuarios</a>";
        menu += "</li>";

        menu += "<li class='nav-item'> ";
        menu += "<a class='nav-link' href='/Productos/ABM'> <i class='fab fa-paypal'></i> Productos(admin)</a>";
        menu += "</li> ";

        menu += "<li class='nav-item'> ";
        menu += "<a class='nav-link'  href='/Usuarios/ABM' > <i class='fas fa-list-ul'></i> Pedidos</a>";
        menu += "</li> ";

        
    }

    if (rol == "CLI") {

        menu += "<li class='nav-item'> ";
        menu += "<a class='nav-link' href='/Productos/Cards/'><i class='fab fa-paypal'></i> Productos(cliente)</a>";
        menu += "</li> ";

        menu += "<li class='nav-item'> ";
        menu += "<a class='nav-link' href='/Productos/Carrito/'><i class='fas fa-cart-plus'></i><label id='idCantidadCarrito'></label> Carrito  </a>";
        menu += "</li> ";

        menu += "<li class='nav-item'> ";
        menu += "<a class='nav-link' href='/Pedidos/MisPedidos/'><i class='fas fa-list-ul'></i>Mis Pedidos</a>";
        menu += "</li> ";

    }

    menu += "</ul>";
 
    document.getElementById("idMenuSegunRol").innerHTML = menu;
}
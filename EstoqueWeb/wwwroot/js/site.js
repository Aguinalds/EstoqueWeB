// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(".custom-file-input").on("change", function () {
    var fileName = $(this).val().split("\\").pop();
    $(this).siblings(".custom-file-label").addClass("selected").html(fileName);
});

$(document).ready(function () {
    $('#tableAll').DataTable({
        "language": {
            "lengthMenu": "Apresentar _MENU_ linhas",
            "zeroRecords": "Não há registros",
            "info": "Mostrando páginas _PAGE_ de _PAGES_",
            "infoEmpty": "Não há registros",
            "infoFiltered": "(filtered from _MAX_ total records)"
        },

        "scrollY": "470px",
        "scrollCollapse": true,
        "paging": false

    });
});

//FORMATAÇÃO PARA CPF
function formataCPF(cpf) {
    const elementoAlvo = cpf
    const cpfAtual = cpf.value

    let cpfAtualizado;

    cpfAtualizado = cpfAtual.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/,
        function (regex, argumento1, argumento2, argumento3, argumento4) {
            return argumento1 + '.' + argumento2 + '.' + argumento3 + '-' + argumento4;
        })
    elementoAlvo.value = cpfAtualizado;
}

//FORMATAÇÃO PARA CELULAR
let campo_celular = document.querySelector('#campo_celular');

campo_celular.addEventListener("blur", function (e) {
    //Remove tudo o que não é dígito
    let celular = this.value.replace(/\D/g, "");

    if (celular.length == 11) {
        celular = celular.replace(/^(\d{2})(\d)/g, "($1) $2");
        resultado_celular = celular.replace(/(\d)(\d{4})$/, "$1-$2");
        document.getElementById('campo_celular').value = resultado_celular;
    } else {
        alert("Digite 11 números.");
    }
})

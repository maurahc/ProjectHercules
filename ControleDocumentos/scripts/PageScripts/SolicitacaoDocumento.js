﻿/// <reference path="Util.js" />
$(document).ready(function () {
    // Util
    bindExclusao();
    bindDatatable();
    bindFormFilter();
    bindFormSubmitExclusao();
    bindCancelar();

    bindCadastroSol();
    bindGetAlunos();
    bindFormSubmit();
});

function bindGetAlunos() {
    $(document).on("change", "#ddlCurso", function () {
        var idCurso = $(this).val();
        if (idCurso > 0) {
            var url = $(this).attr("url");
            $.ajax({
                url: url,
                type: 'post',
                data: { "idCurso": idCurso },
                success: function (data) {
                    var options = $("#ddlAluno");
                    options.empty();
                    $.each(data, function () {
                        options.append($("<option />").val(this.Value).text(this.Text));
                    });
                },
                error: function () {
                    var obj = {
                        Type: "error",
                        Message: "Ocorreu um erro ao realizar esta operação"
                    };
                    showNotification(obj);
                }
            });
        }
        else {
            var options = $("#ddlAluno");
            options.empty();
            options.append($("<option />").val("").text("Selecione um curso"))
        }
    });
}

function bindCadastroSol() {
    $(document).on("click", ".btnCadastro", function () {
        $.ajax({
            url: $(this).attr("url"),
            type: 'GET',
            success: function (data) {
                $(".formSave").html(data);
                $(".divFormSave").show();
                $(".divList").hide();

                $('.datepicker').datepicker({
                    minDate : new Date()
                });
            },
            error: function () {
                var obj = {
                    Type: "error",
                    Message: "Ocorreu um erro ao realizar esta operação"
                }
                showNotification(obj);
            }
        });
    });
}
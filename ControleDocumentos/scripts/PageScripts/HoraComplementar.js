﻿/// <reference path="Util.js" />
$(document).ready(function () {
    // Util
    bindShowConfirmacao();
    bindDatatable();
    bindFormFilter();
    bindCancelar();
    bindCadastro();

    bindSubmitDocumento();
});

function bindSubmitDocumento() {
    $(document).on("submit", '.frm-submit', function () {
        var frm = $('.frm-submit');
        $.ajax({
            url: frm.attr("action"),
            cache: false,
            contentType: false,
            processData: false,
            type: frm.attr("method"),
            data: new FormData(this),
            beforeSend: function () {
                showLoader();
            },
            success: function (result) {
                if (result.Status == true) {
                    showNotificationRefresh(result, false, true);
                }
                else
                    showNotification(result);
            },
            error: function (result) {
                var obj = {
                    Type: "error",
                    Message: "Ocorreu um erro ao realizar esta operação"
                }
                showNotification(obj);
            },
            complete: function () {
                hideLoader();
            },
        });
        return false;
    });
}

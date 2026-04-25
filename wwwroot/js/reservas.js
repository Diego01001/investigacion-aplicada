/**
 * Lógica de jQuery y AJAX para la gestión de reservas
 */

$(document).ready(function () {
    // Carga inicial
    cargarReservas();

    // Evento de guardado
    $('#btnGuardar').on('click', function () {
        if (validarFormulario()) {
            registrarReserva();
        }
    });

    // Filtros
    $('#filtroFecha, #filtroEspacio, #filtroEstado').on('change', function () {
        cargarReservas();
    });

    $('#btnLimpiarFiltros').on('click', function () {
        $('#filtroFecha, #filtroEspacio, #filtroEstado').val('');
        cargarReservas();
    });
});

/**
 * Obtiene las reservas del servidor mediante AJAX
 */
function cargarReservas() {
    const filtros = {
        fecha: $('#filtroFecha').val(),
        espacioId: $('#filtroEspacio').val(),
        estado: $('#filtroEstado').val()
    };

    $('#tableBody').html('<tr><td colspan="8" class="text-center"><i class="fas fa-spinner fa-spin"></i> Cargando...</td></tr>');

    $.ajax({
        url: '/Reservas/GetReservas',
        type: 'GET',
        data: filtros,
        success: function (response) {
            let rows = '';
            // Handle both direct array and wrapped { value: [] } responses
            const data = Array.isArray(response) ? response : (response.value || []);
            
            if (data.length === 0) {
                rows = '<tr><td colspan="8" class="text-center text-muted">No se encontraron reservas con los filtros seleccionados.</td></tr>';
            } else {
                data.forEach(item => {
                    // Check for both casing just in case
                    const estado = item.estado || item.Estado || '';
                    const isVigente = estado.toLowerCase() === 'vigente';
                    const statusBadge = isVigente ? 'bg-success' : 'bg-danger';
                    const canCancel = isVigente;

                    rows += `
                        <tr>
                            <td>${item.id || item.Id}</td>
                            <td>
                                <strong>${item.solicitante || item.Solicitante}</strong><br>
                                <small class="text-muted">${item.correo || item.Correo}</small>
                            </td>
                            <td>${(item.espacio || item.Espacio)?.nombre || (item.espacio || item.Espacio)?.Nombre || 'N/A'}</td>
                            <td>${new Date(item.fecha || item.Fecha).toLocaleDateString()}</td>
                            <td>${(item.horaInicio || item.HoraInicio).substring(0, 5)} - ${(item.horaFin || item.HoraFin).substring(0, 5)}</td>
                            <td><span class="badge ${statusBadge}">${estado}</span></td>
                            <td>${item.motivo || item.Motivo}</td>
                            <td class="text-center">
                                ${canCancel ? 
                                    `<button class="btn btn-outline-danger btn-sm" onclick="confirmarCancelacion(${item.id || item.Id})">
                                        <i class="fas fa-times-circle"></i> Cancelar
                                     </button>` : 
                                    '<span class="text-muted">-</span>'}
                            </td>
                        </tr>
                    `;
                });
            }
            $('#tableBody').html(rows);
        },
        error: function () {
            mostrarAlerta('error', 'No se pudieron cargar las reservas.');
        }
    });
}

/**
 * Registra una nueva reserva
 */
function registrarReserva() {
    const data = {
        Solicitante: $('#Solicitante').val(),
        Correo: $('#Correo').val(),
        EspacioId: $('#EspacioId').val(),
        Fecha: $('#Fecha').val(),
        HoraInicio: $('#HoraInicio').val(),
        HoraFin: $('#HoraFin').val(),
        Motivo: $('#Motivo').val()
    };

    $.ajax({
        url: '/Reservas/Create',
        type: 'POST',
        data: data,
        success: function (res) {
            if (res.success) {
                cerrarModal();
                $('#reservaForm')[0].reset();
                cargarReservas();
                mostrarAlerta('success', res.message);
            } else {
                mostrarAlerta('error', res.message);
            }
        },
        error: function () {
            mostrarAlerta('error', 'Error crítico en el servidor al intentar registrar.');
        }
    });
}

/**
 * Cancela una reserva existente
 */
function confirmarCancelacion(id) {
    if (confirm('¿Está seguro de que desea cancelar esta reserva?')) {
        $.ajax({
            url: '/Reservas/Cancelar',
            type: 'POST',
            data: { id: id },
            success: function (res) {
                if (res.success) {
                    cargarReservas();
                    mostrarAlerta('success', res.message);
                } else {
                    mostrarAlerta('error', res.message);
                }
            }
        });
    }
}

/**
 * Validaciones del lado del cliente
 */
function validarFormulario() {
    let isValid = true;
    $('.form-control').removeClass('is-invalid');

    const campos = ['Solicitante', 'Correo', 'EspacioId', 'Fecha', 'HoraInicio', 'HoraFin', 'Motivo'];
    
    campos.forEach(id => {
        const val = $(`#${id}`).val();
        if (!val || val.trim() === '') {
            $(`#${id}`).addClass('is-invalid');
            isValid = false;
        }
    });

    // Validar sentido de horas
    const h1 = $('#HoraInicio').val();
    const h2 = $('#HoraFin').val();
    if (h1 && h2 && h1 >= h2) {
        $('#HoraFin').addClass('is-invalid');
        alert('La hora de fin debe ser mayor a la de inicio.');
        isValid = false;
    }

    return isValid;
}

function cerrarModal() {
    const modalEl = document.getElementById('modalNuevaReserva');
    const modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
    modal.hide();
}

function mostrarAlerta(tipo, mensaje) {
    // Podría ser un Toast, usamos alert simple pero claro
    const color = tipo === 'success' ? 'alert-success' : 'alert-danger';
    const html = `<div class="alert ${color} alert-dismissible fade show fixed-top m-3 shadow" role="alert" style="z-index: 9999;">
                    ${mensaje}
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                  </div>`;
    $('body').append(html);
    setTimeout(() => { $('.alert').alert('close'); }, 4000);
}

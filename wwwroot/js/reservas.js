/**
 * Lógica de jQuery y AJAX para la gestión de reservas.
 * Este archivo maneja la carga, registro, filtrado, cancelación y eliminación de reservas
 * sin recargar completamente la página.
 */

$(document).ready(function () {
    // Carga las reservas al abrir la página.
    cargarReservas();

    // Evento para registrar una nueva reserva desde el formulario.
    $('#btnGuardar').on('click', function () {
        if (validarFormulario()) {
            registrarReserva();
        }
    });

    // Cada vez que cambia un filtro, se recarga la tabla de reservas.
    $('#filtroFecha, #filtroEspacio, #filtroEstado').on('change', function () {
        cargarReservas();
    });

    // Limpia los filtros y vuelve a mostrar todas las reservas.
    $('#btnLimpiarFiltros').on('click', function () {
        $('#filtroFecha, #filtroEspacio, #filtroEstado').val('');
        cargarReservas();
    });
});

/**
 * Obtiene las reservas desde el servidor mediante AJAX
 * y actualiza dinámicamente la tabla.
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

            // Se adapta la respuesta por si llega como arreglo directo o dentro de una propiedad value.
            const data = Array.isArray(response) ? response : (response.value || []);

            if (data.length === 0) {
                rows = '<tr><td colspan="6" class="text-center py-5 text-muted">No se encontraron reservas con los filtros seleccionados.</td></tr>';
            } else {
                // Construye las filas de la tabla con las reservas recibidas.
                data.forEach(item => {
                    const estado = item.estado || item.Estado || '';
                    const estadoNormalizado = estado.trim().toLowerCase();

                    const isVigente = estadoNormalizado === 'vigente';
                    const isCancelada = estadoNormalizado === 'cancelada';

                    const statusBadge = isVigente ? 'bg-success-light' : 'bg-danger-light';

                    rows += `
                        <tr>
                            <td><span class="fw-bold text-primary">#${item.id || item.Id}</span></td>
                            <td>
                                <div class="d-flex align-items-center">
                                    <div class="rounded-circle bg-light d-flex align-items-center justify-content-center me-3" style="width: 40px; height: 40px;">
                                        <i class="fas fa-user text-muted"></i>
                                    </div>
                                    <div>
                                        <div class="fw-semibold">${item.solicitante || item.Solicitante}</div>
                                        <div class="small text-muted">${item.correo || item.Correo}</div>
                                    </div>
                                </div>
                            </td>
                            <td>
                                <div class="fw-medium">${(item.espacio || item.Espacio)?.nombre || 'N/A'}</div>
                                <div class="small text-muted">${(item.espacio || item.Espacio)?.ubicacion || ''}</div>
                            </td>
                            <td>
                                <div class="d-flex align-items-center">
                                    <i class="far fa-clock text-primary me-2"></i>
                                    <div>
                                        <div>${new Date(item.fecha || item.Fecha).toLocaleDateString()}</div>
                                        <div class="small text-primary fw-semibold">${(item.horaInicio || item.HoraInicio).substring(0, 5)} - ${(item.horaFin || item.HoraFin).substring(0, 5)}</div>
                                    </div>
                                </div>
                            </td>
                            <td><span class="badge ${statusBadge}">${estado}</span></td>
                            <td class="text-center">
                                ${isVigente ?
                            `<button class="btn btn-outline-warning btn-sm" onclick="confirmarCancelacion(${item.id || item.Id})">
                                        Cancelar
                                     </button>` :
                            isCancelada ?
                                `<button class="btn btn-outline-danger btn-sm" onclick="confirmarEliminacion(${item.id || item.Id})">
                                        Eliminar
                                     </button>` :
                                '<span class="text-muted small">Sin acciones</span>'}
                            </td>
                        </tr>
                    `;
                });
            }

            $('#tableBody').hide().html(rows).fadeIn(400);
        },
        error: function () {
            mostrarAlerta('error', 'No se pudieron cargar las reservas.');
        }
    });
}

/**
 * Envía los datos del formulario al servidor para registrar una reserva.
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
 * Solicita confirmación y cancela una reserva vigente mediante AJAX.
 * La reserva no se elimina; solo cambia a estado Cancelada.
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
            },
            error: function () {
                mostrarAlerta('error', 'No se pudo cancelar la reserva.');
            }
        });
    }
}

/**
 * Solicita confirmación y elimina definitivamente una reserva cancelada mediante AJAX.
 */
function confirmarEliminacion(id) {
    if (confirm('¿Está seguro de que desea eliminar definitivamente esta reserva?')) {
        $.ajax({
            url: '/Reservas/Eliminar',
            type: 'POST',
            data: { id: id },
            success: function (res) {
                if (res.success) {
                    cargarReservas();
                    mostrarAlerta('success', res.message);
                } else {
                    mostrarAlerta('error', res.message);
                }
            },
            error: function () {
                mostrarAlerta('error', 'No se pudo eliminar la reserva.');
            }
        });
    }
}

/**
 * Valida los campos del formulario antes de enviar la reserva al servidor.
 */
function validarFormulario() {
    let isValid = true;

    // Limpia marcas de error anteriores.
    $('.form-control').removeClass('is-invalid');

    const campos = ['Solicitante', 'Correo', 'EspacioId', 'Fecha', 'HoraInicio', 'HoraFin', 'Motivo'];

    // Valida que los campos obligatorios no estén vacíos.
    campos.forEach(id => {
        const val = $(`#${id}`).val();

        if (!val || val.trim() === '') {
            $(`#${id}`).addClass('is-invalid');
            isValid = false;
        }
    });

    // Valida que la hora de fin sea mayor que la hora de inicio.
    const h1 = $('#HoraInicio').val();
    const h2 = $('#HoraFin').val();

    if (h1 && h2 && h1 >= h2) {
        $('#HoraFin').addClass('is-invalid');
        alert('La hora de fin debe ser mayor a la de inicio.');
        isValid = false;
    }

    return isValid;
}

/**
 * Cierra el modal donde se registra una nueva reserva.
 */
function cerrarModal() {
    const modalEl = document.getElementById('modalNuevaReserva');
    const modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);

    modal.hide();
}

/**
 * Muestra una alerta visual de éxito o error en la parte superior de la página.
 */
function mostrarAlerta(tipo, mensaje) {
    const color = tipo === 'success' ? 'alert-success' : 'alert-danger';

    const html = `<div class="alert ${color} alert-dismissible fade show fixed-top m-3 shadow" role="alert" style="z-index: 9999;">
                    ${mensaje}
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                  </div>`;

    $('body').append(html);

    // Cierra automáticamente la alerta después de unos segundos.
    setTimeout(() => {
        $('.alert').alert('close');
    }, 4000);
}
$(function () {
    var l = abp.localization.getResource('XYPortal');
    var createModal = new abp.ModalManager(abp.appPath + 'OpenIddict/Applications/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'OpenIddict/Applications/EditModal');

    var dataTable = $('#OpenIddictApplicationsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(function (input) {
                return $.ajax({
                    url: abp.appPath + 'OpenIddict/Applications?handler=List',
                    data: input,
                    type: 'GET',
                    dataType: 'json'
                });
            }),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('XYPortal.OpenIdDictManager.ApplicationManager.Edit'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('XYPortal.OpenIdDictManager.ApplicationManager.Delete'),
                                confirmMessage: function (data) {
                                    return l('ApplicationDeletionConfirmation', data.record.clientId);
                                },
                                action: function (data) {
                                    abp.ajax({
                                        url: abp.appPath + 'OpenIddict/Applications?handler=Delete&id=' + data.record.id,
                                        type: 'POST'
                                    }).then(function () {
                                        abp.notify.info(l('SuccessfullyDeleted'));
                                        dataTable.ajax.reload();
                                    });
                                }
                            }
                        ]
                    }
                },
                {
                    title: l('ClientId'),
                    data: "clientId"
                },
                {
                    title: l('DisplayName'),
                    data: "displayName"
                },
                {
                    title: l('ClientType'),
                    data: "clientType"
                },
                {
                    title: l('ConsentType'),
                    data: "consentType"
                }
            ]
        })
    );

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $('#NewOpenIddictApplicationButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });
});

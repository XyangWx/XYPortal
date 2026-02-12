$(function () {
    var l = abp.localization.getResource('XYPortal');
    var createModal = new abp.ModalManager(abp.appPath + 'OpenIddict/Scopes/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'OpenIddict/Scopes/EditModal');

    var dataTable = $('#OpenIddictScopesTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(function (input) {
                return $.ajax({
                    url: abp.appPath + 'OpenIddict/Scopes?handler=List',
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
                                visible: abp.auth.isGranted('XYPortal.OpenIdDictManager.ScopeManager.Edit'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('XYPortal.OpenIdDictManager.ScopeManager.Delete'),
                                confirmMessage: function (data) {
                                    return l('ScopeDeletionConfirmation', data.record.name);
                                },
                                action: function (data) {
                                    abp.ajax({
                                        url: abp.appPath + 'OpenIddict/Scopes?handler=Delete&id=' + data.record.id,
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
                    title: l('Name'),
                    data: "name"
                },
                {
                    title: l('DisplayName'),
                    data: "displayName"
                },
                {
                    title: l('Description'),
                    data: "description"
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

    $('#NewOpenIddictScopeButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });
});

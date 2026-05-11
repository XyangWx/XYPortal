$(function () {
    var l = abp.localization.getResource('XYPortal');
    var createModal = new abp.ModalManager(abp.appPath + 'LinkBoard/Categories/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'LinkBoard/Categories/EditModal');

    var getStatusText = function (status) {
        switch (status) {
            case 0: return l('LinkBoard:StatusDraft');
            case 1: return l('LinkBoard:StatusPending');
            case 2: return l('LinkBoard:StatusApproved');
            case 3: return l('LinkBoard:StatusRejected');
            default: return status;
        }
    };

    var dataTable = $('#CategoriesTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(function (input) {
                return $.ajax({
                    url: abp.appPath + 'LinkBoard/Categories?handler=List',
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
                                visible: abp.auth.isGranted('LinkBoard.User.LinkCategoryManager.Modify'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('LinkBoard:Submit'),
                                visible: function (data) {
                                    return abp.auth.isGranted('LinkBoard.User.LinkCategoryManager.Modify') &&
                                        (data.status === 0 || data.status === 3);
                                },
                                action: function (data) {
                                    abp.ajax({
                                        url: abp.appPath + 'LinkBoard/Categories?handler=Submit&id=' + data.record.id,
                                        type: 'POST'
                                    }).then(function () {
                                        abp.notify.success(l('SuccessfullyDeleted'));
                                        dataTable.ajax.reload();
                                    });
                                }
                            },
                            {
                                text: l('LinkBoard:Withdraw'),
                                visible: function (data) {
                                    return abp.auth.isGranted('LinkBoard.User.LinkCategoryManager.Modify') &&
                                        data.status === 1;
                                },
                                action: function (data) {
                                    abp.ajax({
                                        url: abp.appPath + 'LinkBoard/Categories?handler=Withdraw&id=' + data.record.id,
                                        type: 'POST'
                                    }).then(function () {
                                        abp.notify.success(l('SuccessfullyDeleted'));
                                        dataTable.ajax.reload();
                                    });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('LinkBoard.User.LinkCategoryManager.Delete'),
                                confirmMessage: function (data) {
                                    return l('LinkBoard:CategoryDeletionConfirmation', data.record.name);
                                },
                                action: function (data) {
                                    abp.ajax({
                                        url: abp.appPath + 'LinkBoard/Categories?handler=Delete&id=' + data.record.id,
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
                    title: l('LinkBoard:CategoryName'),
                    data: "name"
                },
                {
                    title: l('LinkBoard:CategoryDisplayName'),
                    data: "displayName"
                },
                {
                    title: l('LinkBoard:SortOrder'),
                    data: "sortOrder"
                },
                {
                    title: l('LinkBoard:IsPublic'),
                    data: "isPublic",
                    render: function (data) {
                        return data ? '<i class="fa fa-check text-success"></i>' : '<i class="fa fa-times text-muted"></i>';
                    }
                },
                {
                    title: l('LinkBoard:Status'),
                    data: "status",
                    render: function (data) {
                        return getStatusText(data);
                    }
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

    $('#NewCategoryButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });
});

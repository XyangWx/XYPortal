$(function () {
    var l = abp.localization.getResource('XYPortal');
    var createModal = new abp.ModalManager(abp.appPath + 'LinkBoard/Links/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'LinkBoard/Links/EditModal');

    var getStatusText = function (status) {
        switch (status) {
            case 0: return l('LinkBoard:StatusDraft');
            case 1: return l('LinkBoard:StatusPending');
            case 2: return l('LinkBoard:StatusApproved');
            case 3: return l('LinkBoard:StatusRejected');
            default: return status;
        }
    };

    var dataTable = $('#LinksTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(function (input) {
                return $.ajax({
                    url: abp.appPath + 'LinkBoard/Links?handler=List',
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
                                visible: abp.auth.isGranted('LinkBoard.User.LinkManager.Modify'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('LinkBoard:Submit'),
                                visible: function (data) {
                                    return abp.auth.isGranted('LinkBoard.User.LinkManager.Modify') &&
                                        (data.status === 0 || data.status === 3);
                                },
                                action: function (data) {
                                    abp.ajax({
                                        url: abp.appPath + 'LinkBoard/Links?handler=Submit&id=' + data.record.id,
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
                                    return abp.auth.isGranted('LinkBoard.User.LinkManager.Modify') &&
                                        data.status === 1;
                                },
                                action: function (data) {
                                    abp.ajax({
                                        url: abp.appPath + 'LinkBoard/Links?handler=Withdraw&id=' + data.record.id,
                                        type: 'POST'
                                    }).then(function () {
                                        abp.notify.success(l('SuccessfullyDeleted'));
                                        dataTable.ajax.reload();
                                    });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('LinkBoard.User.LinkManager.Delete'),
                                confirmMessage: function (data) {
                                    return l('LinkBoard:LinkDeletionConfirmation', data.record.title);
                                },
                                action: function (data) {
                                    abp.ajax({
                                        url: abp.appPath + 'LinkBoard/Links?handler=Delete&id=' + data.record.id,
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
                    title: l('LinkBoard:LinkTitle'),
                    data: "title"
                },
                {
                    title: l('LinkBoard:LinkUrl'),
                    data: "url",
                    render: function (data) {
                        return '<a href="' + data + '" target="_blank">' + data + '</a>';
                    }
                },
                {
                    title: l('LinkBoard:Category'),
                    data: "categoryName"
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

    $('#NewLinkButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });
});

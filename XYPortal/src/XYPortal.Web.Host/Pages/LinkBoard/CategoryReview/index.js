$(function () {
    var l = abp.localization.getResource('XYPortal');

    var dataTable = $('#CategoryReviewTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(function (input) {
                return $.ajax({
                    url: abp.appPath + 'LinkBoard/CategoryReview?handler=List',
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
                                text: l('LinkBoard:Approve'),
                                visible: abp.auth.isGranted('LinkBoard.Admin.LinkCategoryReview'),
                                action: function (data) {
                                    abp.ajax({
                                        url: abp.appPath + 'LinkBoard/CategoryReview?handler=Approve&id=' + data.record.id,
                                        type: 'POST'
                                    }).then(function () {
                                        abp.notify.success(l('SuccessfullyDeleted'));
                                        dataTable.ajax.reload();
                                    });
                                }
                            },
                            {
                                text: l('LinkBoard:Reject'),
                                visible: abp.auth.isGranted('LinkBoard.Admin.LinkCategoryReview'),
                                action: function (data) {
                                    abp.message.prompt(l('LinkBoard:ReviewComment')).then(function (comment) {
                                        abp.ajax({
                                            url: abp.appPath + 'LinkBoard/CategoryReview?handler=Reject&id=' + data.record.id + '&comment=' + encodeURIComponent(comment || ''),
                                            type: 'POST'
                                        }).then(function () {
                                            abp.notify.success(l('SuccessfullyDeleted'));
                                            dataTable.ajax.reload();
                                        });
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
                    title: l('LinkBoard:CategoryDescription'),
                    data: "description"
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
                }
            ]
        })
    );
});

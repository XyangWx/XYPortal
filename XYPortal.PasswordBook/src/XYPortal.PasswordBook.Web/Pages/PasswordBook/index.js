$(function () {
    window.createPasswordBook = function () {
        var form = document.getElementById('CreateForm');
        var formData = new FormData(form);

        var input = {
            name: formData.get('Name') || form.querySelector('[name="Name"]').value,
            description: formData.get('Description') || form.querySelector('[name="Description"]')?.value || null,
            allowedType: parseInt(form.querySelector('[name="AllowedType"]')?.value || 1),
            minLength: parseInt(form.querySelector('[name="MinLength"]')?.value || 8),
            maxLength: parseInt(form.querySelector('[name="MaxLength"]')?.value || 20)
        };

        fetch('/api/password-book', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(input)
        })
        .then(response => {
            if (response.ok) {
                abp.notify.success('PasswordBook created successfully');
                $('#CreateModal').modal('hide');
                location.reload();
            } else {
                return response.json().then(err => {
                    throw new Error(err.message || 'Failed to create PasswordBook');
                });
            }
        })
        .catch(error => {
            abp.notify.error(error.message);
        });
    };

    window.deletePasswordBook = function (id) {
        abp.message.confirm(
            'Are you sure you want to delete this PasswordBook?',
            'Delete Confirmation',
            function (confirmed) {
                if (confirmed) {
                    fetch('/api/password-book/' + id, {
                        method: 'DELETE'
                    })
                    .then(response => {
                        if (response.ok) {
                            abp.notify.success('PasswordBook deleted successfully');
                            location.reload();
                        } else {
                            throw new Error('Failed to delete PasswordBook');
                        }
                    })
                    .catch(error => {
                        abp.notify.error(error.message);
                    });
                }
            }
        );
    };
});

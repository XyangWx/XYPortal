$(function () {
    window.createPasswordBook = function () {
        var form = document.getElementById('CreateForm');
        var formData = new FormData(form);

        var input = {
            name: formData.get('CreateInput.Name') || form.querySelector('[name="CreateInput.Name"]')?.value,
            description: formData.get('CreateInput.Description') || form.querySelector('[name="CreateInput.Description"]')?.value || null,
            allowedType: parseInt(formData.get('CreateInput.AllowedType') || form.querySelector('[name="CreateInput.AllowedType"]')?.value || 1),
            minLength: parseInt(formData.get('CreateInput.MinLength') || form.querySelector('[name="CreateInput.MinLength"]')?.value || 8),
            maxLength: parseInt(formData.get('CreateInput.MaxLength') || form.querySelector('[name="CreateInput.MaxLength"]')?.value || 20)
        };

        if (!input.name) {
            abp.notify.error('Password book name is required');
            return;
        }

        fetch('/api/password-book', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': abp.security.antiForgery.getToken()
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
                    throw new Error(err.error || err.message || 'Failed to create PasswordBook');
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
                        method: 'DELETE',
                        headers: {
                            'RequestVerificationToken': abp.security.antiForgery.getToken()
                        }
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

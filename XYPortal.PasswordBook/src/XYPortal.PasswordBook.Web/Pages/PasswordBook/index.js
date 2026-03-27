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
                    var errorMessage = 'Failed to create PasswordBook';
                    if (err && err.error) {
                        if (typeof err.error === 'object') {
                            errorMessage = err.error.message || err.error.code || JSON.stringify(err.error);
                        } else {
                            errorMessage = err.error;
                        }
                    } else if (err && err.message) {
                        errorMessage = err.message;
                    }
                    throw new Error(errorMessage);
                });
            }
        })
        .catch(error => {
            abp.notify.error(error.message);
        });
    };

    window.viewPasswordBook = function (id) {
        fetch('/api/password-book/' + id + '/with-entries', {
            method: 'GET',
            headers: {
                'RequestVerificationToken': abp.security.antiForgery.getToken()
            }
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to load PasswordBook');
            }
            return response.json();
        })
        .then(data => {
            document.getElementById('view-book-name').textContent = data.name || '';
            document.getElementById('view-book-description').textContent = data.description || '-';
            document.getElementById('view-book-allowedtype').textContent = data.allowedType === 1 ? 'General' : 'NumericOnly';
            document.getElementById('view-book-creationtime').textContent = new Date(data.creationTime).toLocaleString();

            var tbody = document.getElementById('PasswordEntriesTableBody');
            tbody.innerHTML = '';

            if (data.passwordEntries && data.passwordEntries.length > 0) {
                data.passwordEntries.forEach(function (entry) {
                    if (!entry.isDeleted) {
                        var row = '<tr>' +
                            '<td>' + (entry.title || '') + '</td>' +
                            '<td>' + (entry.username || '-') + '</td>' +
                            '<td>' + (entry.passwordType === 1 ? 'General' : 'NumericOnly') + '</td>' +
                            '<td>' + (entry.weakLevel || '-') + '</td>' +
                            '</tr>';
                        tbody.innerHTML += row;
                    }
                });
            } else {
                tbody.innerHTML = '<tr><td colspan="4" class="text-center">No password entries</td></tr>';
            }

            var modal = new bootstrap.Modal(document.getElementById('ViewModal'));
            modal.show();
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

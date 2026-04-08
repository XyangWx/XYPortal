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
            document.getElementById('view-book-id').textContent = data.id;

            // Set the password book ID for create entry form
            document.getElementById('entry-passwordbook-id').value = data.id;

            var tbody = document.getElementById('PasswordEntriesTableBody');
            tbody.innerHTML = '';

            if (data.passwordEntries && data.passwordEntries.length > 0) {
                data.passwordEntries.forEach(function (entry) {
                    var actionButtons = '';
                    // Show Password 按钮始终显示（不论是否已删除）
                    actionButtons = '<button type="button" class="btn btn-info btn-sm" onclick="showPasswordEntry(\'' + data.id + '\', \'' + entry.id + '\', ' + (entry.isDeleted ? 1 : 0) + ')"><i class="fa fa-eye"></i> ' + window.passwordBookLocales.ShowPassword + '</button>';

                    if (entry.isDeleted) {
                        // 已删除条目：显示恢复按钮
                        actionButtons += ' <button type="button" class="btn btn-success btn-sm" onclick="restorePasswordEntry(\'' + data.id + '\', \'' + entry.id + '\')"><i class="fa fa-undo"></i> ' + window.passwordBookLocales.Restore + '</button>';
                    } else {
                        // 有效条目：显示复制和删除按钮
                        actionButtons += ' <button type="button" class="btn btn-secondary btn-sm" onclick="copyPasswordToClipboard(\'' + data.id + '\', \'' + entry.id + '\')"><i class="fa fa-copy"></i> ' + window.passwordBookLocales.Copy + '</button>'
                            + ' <button type="button" class="btn btn-danger btn-sm" onclick="deletePasswordEntry(\'' + data.id + '\', \'' + entry.id + '\')"><i class="fa fa-trash"></i> ' + window.passwordBookLocales.Delete + '</button>';
                    }
                    
                    var row = '<tr>' +
                        '<td>' + (entry.title || '') + '</td>' +
                        '<td>' + (entry.username || '-') + '</td>' +
                        '<td>' + (entry.passwordType === 1 ? 'General' : 'NumericOnly') + '</td>' +
                        '<td>' + (entry.weakLevel || '-') + '</td>' +
                        '<td>' + (entry.isDeleted ? '<span class="badge bg-secondary">' + window.passwordBookLocales.Voided + '</span>' : '<span class="badge bg-success">' + window.passwordBookLocales.Active + '</span>') + '</td>' +
                        '<td>' + actionButtons + '</td>' +
                        '</tr>';
                    tbody.innerHTML += row;
                });
            } else {
                tbody.innerHTML = '<tr><td colspan="6" class="text-center">No password entries</td></tr>';
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

    // 显示创建密码条目模态框
    window.showCreateEntryModal = function() {
        var modal = new bootstrap.Modal(document.getElementById('CreateEntryModal'));
        modal.show();
    };

    // 创建密码条目
    window.createPasswordEntry = function() {
        var passwordBookId = document.getElementById('entry-passwordbook-id').value;
        
        var passwordTypeValue = document.getElementById('entry-passwordtype')?.value;
        var passwordTypeInt = parseInt(passwordTypeValue);
        if (isNaN(passwordTypeInt) || (passwordTypeInt !== 0 && passwordTypeInt !== 1)) {
            passwordTypeInt = 1; // default to General
        }
        
        var weakLevelValue = document.getElementById('entry-weaklevel')?.value;
        var weakLevel = null;
        if (weakLevelValue) {
            var weakLevelInt = parseInt(weakLevelValue);
            weakLevel = isNaN(weakLevelInt) ? null : weakLevelInt;
        }
        
        var input = {
            title: document.getElementById('entry-title')?.value || '',
            password: document.getElementById('entry-password')?.value || '',
            hasUsername: document.getElementById('entry-hasusername')?.value === 'true',
            username: document.getElementById('entry-username')?.value || null,
            passwordType: passwordTypeInt,
            weakLevel: weakLevel,
            remark: document.getElementById('entry-remark')?.value || null
        };
        
        if (!input.title || !input.password) {
            abp.notify.error('Title and Password are required');
            return;
        }
        
        console.log(input);
        
        fetch('/api/password-book/' + passwordBookId + '/entries', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': abp.security.antiForgery.getToken()
            },
            body: JSON.stringify(input)
        })
        .then(response => {
            if (response.ok) {
                abp.notify.success('Password entry created successfully');
                $('#CreateEntryModal').modal('hide');
                location.reload();
            } else {
                return response.json().then(err => {
                    var errorMessage = 'Failed to create password entry';
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

    // 删除密码条目
    window.deletePasswordEntry = function(passwordBookId, entryId) {
        abp.message.confirm(
            'Are you sure you want to delete this password entry?',
            'Delete Confirmation',
            function(confirmed) {
                if (confirmed) {
                    fetch('/api/password-book/' + passwordBookId + '/entries/' + entryId, {
                        method: 'DELETE',
                        headers: {
                            'RequestVerificationToken': abp.security.antiForgery.getToken()
                        }
                    })
                    .then(response => {
                        if (response.ok) {
                            abp.notify.success('Password entry deleted successfully');
                            location.reload();
                        } else {
                            throw new Error('Failed to delete password entry');
                        }
                    })
                    .catch(error => {
                        abp.notify.error(error.message);
                    });
                }
            }
        );
    };

    // 恢复密码条目
    window.restorePasswordEntry = function(passwordBookId, entryId) {
        abp.message.confirm(
            'Are you sure you want to restore this password entry?',
            'Restore Confirmation',
            function(confirmed) {
                if (confirmed) {
                    fetch('/api/password-book/' + passwordBookId + '/entries/' + entryId + '/restore', {
                        method: 'POST',
                        headers: {
                            'RequestVerificationToken': abp.security.antiForgery.getToken()
                        }
                    })
                    .then(response => {
                        if (response.ok) {
                            abp.notify.success('Password entry restored successfully');
                            location.reload();
                        } else {
                            throw new Error('Failed to restore password entry');
                        }
                    })
                    .catch(error => {
                        abp.notify.error(error.message);
                    });
                }
            }
        );
    };

    // 复制密码到剪贴板
    window.copyPasswordToClipboard = function(passwordBookId, entryId) {
        fetch('/api/password-book/' + passwordBookId + '/entries/' + entryId, {
            method: 'GET',
            headers: {
                'RequestVerificationToken': abp.security.antiForgery.getToken()
            }
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to get password entry');
            }
            return response.json();
        })
        .then(data => {
            if (!data.currentPassword) {
                throw new Error('Password not found');
            }
            navigator.clipboard.writeText(data.currentPassword).then(function() {
                abp.notify.success('Password copied to clipboard');
            }).catch(function(err) {
                abp.notify.error('Failed to copy password: ' + err);
            });
        })
        .catch(error => {
            abp.notify.error(error.message);
        });
    };

    // 显示密码明文（不消失模态框）
    window.showPasswordEntry = function(passwordBookId, entryId, queryKind) {
        queryKind = (typeof queryKind === 'number') ? queryKind : 0;
        fetch('/api/password-book/' + passwordBookId + '/entries/' + entryId + '?queryKind=' + queryKind, {
            method: 'GET',
            headers: {
                'RequestVerificationToken': abp.security.antiForgery.getToken()
            }
        })
        .then(response => {
            if (!response.ok) throw new Error('Failed to load password entry');
            return response.json();
        })
        .then(data => {
            document.getElementById('sp-title').textContent = data.title || '';
            document.getElementById('sp-username').textContent = data.username || '-';
            document.getElementById('sp-password').value = data.currentPassword || '';
            document.getElementById('sp-remark').textContent = data.remark || '-';
            document.getElementById('sp-weaklevel').textContent = data.weakLevel || '-';
            var modal = new bootstrap.Modal(document.getElementById('ShowPasswordModal'));
            modal.show();
        })
        .catch(error => abp.notify.error(error.message));
    };

    // 复制 ShowPasswordModal 中的密码
    window.copySpPassword = function() {
        var pwd = document.getElementById('sp-password').value;
        navigator.clipboard.writeText(pwd).then(function() {
            abp.notify.success('Password copied');
        }).catch(function(err) {
            abp.notify.error('Failed to copy password: ' + err);
        });
    };
});
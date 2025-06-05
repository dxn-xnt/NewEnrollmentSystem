$(document).ready(function () {
    $('#submitButton').click(function (e) {
        $('.input-error').removeClass('input-error');

        // Client-side validation
        if ($('#Password').val() !== $('#ConfirmPassword').val()) {
            Swal.fire('Error', 'Passwords do not match', 'error');
            $('#ConfirmPassword').addClass('input-error').focus();
            return;
        }

        const student = {
            Id: parseInt($('#StudentID').val()),
            LastName: $('#LastName').val(),
            FirstName: $('#FirstName').val(),
            MiddleName: $('#MiddleName').val(),
            Birthdate: $('#Birthdate').val(), 
            Contact: $('#Contact').val(),
            Email: $('#Email').val(),
            HomeAddress: $('#HomeAddress').val(),
            Password: $('#Password').val()
        };

        console.log('Submitting:', student); // Debugging

        $.ajax({
            type: "POST",
            url: '/Account/SignUp',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(student),
            success: function (response) {
                if (response.mess === 1) {
                    Swal.fire({
                        title: 'Success!',
                        text: response.message,
                        icon: 'success',
                        confirmButtonText: 'OK',
                        timer: 2000,
                        timerProgressBar: true,
                        willClose: () => {
                            window.location.href = response.redirectUrl;
                        }
                    });
                } else if (response.mess === 2 || response.mess === 3) {
                    Swal.fire({
                        title: 'Error',
                        text: response.error,
                        icon: 'error',
                        confirmButtonText: 'Try Again'
                    });
                    $('#' + response.field).addClass('input-error').focus();
                } else {
                    Swal.fire({
                        title: 'Error',
                        text: response.earror || "Submission failed.",
                        icon: 'error',
                        confirmButtonText: 'Try Again'
                    });
                }
            },
            error: function (xhr) {
                console.error('Error:', xhr.responseText);
                let errorMsg = "Something went wrong during submission.";
                try {
                    const serverError = JSON.parse(xhr.responseText);
                    errorMsg = serverError.error || serverError.message || errorMsg;
                } catch (e) {}

                Swal.fire('Error', errorMsg, 'error');
            }
        });
    });
});
$(document).ready(function () {
    $('#submitButton').click(function (e) {
        e.preventDefault(); // Add this to prevent form submission
        $('.input-error').removeClass('input-error');

        // Validate Program is selected
        if (!$('#ProgramCode').val()) {
            Swal.fire('Error', 'Please select a program', 'error');
            $('#Program').addClass('input-error').focus();
            return;
        }

        // Reset errors
        $('.error-label').hide();

        // Validate Program selection
        if (!$('#ProgramCode').val()) {
            $('#program-error').show();
            $('#Program').addClass('input-error').focus();
            return;
        }
    
        // Password validation
        if ($('#Password').val() !== $('#ConfirmPassword').val()) {
            Swal.fire('Error', 'Passwords do not match', 'error');
            $('#ConfirmPassword').addClass('input-error').focus();
            return;
        }

        console.log('Program value:', $('#Program').val());
        console.log('Program element:', $('#Program'));
        
        const student = {
            Id: parseInt($('#StudentID').val()) || 0,
            Program: $('#ProgramCode').val(),
            LastName: $('#LastName').val(),
            FirstName: $('#FirstName').val(),
            MiddleName: $('#MiddleName').val(),
            Birthdate: $('#Birthdate').val(),
            Contact: $('#Contact').val(),
            Email: $('#Email').val(),
            HomeAddress: $('#HomeAddress').val(),
            Password: $('#Password').val()
        };

        console.log('Submitting student:', student);

        $.ajax({
            type: "POST",
            url: '/Account/SignUp',
            contentType: 'application/json',
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
                Swal.fire('Error', xhr.responseJSON?.error || "Submission failed", 'error');
            }
        });
    });
});
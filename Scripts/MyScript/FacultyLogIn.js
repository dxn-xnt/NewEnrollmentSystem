$(document).ready(function () {
    $('#submitButton').click(function (e) {
        const facultyAccount = {
            Id: parseInt($('#FacultyID').val()),
            Password: $('#FacultyPassword').val()
        };
        
        const token = $('input[name="__RequestVerificationToken"]').val();

        console.log("Sending login request:", facultyAccount); // Debug output

        $.ajax({
            type: 'POST',
            url: '/Account/Faculty/LogIn',
            data: facultyAccount,
            success: function (response) {
                console.log("Received response:", response); // Debug output
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Welcome!',
                        text: response.message,
                        confirmButtonText: 'Continue',
                    }).then(() => {
                        window.location.href = response.redirectUrl;
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Login Failed',
                        text: response.message,
                        confirmButtonText: 'Try Again'
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error("AJAX error:", status, error); // Debug output
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Unable to process login request.',
                });
            }
        });
    });

    // Input event handlers
    $('#username, #password').on('input', function () {
        $(this).removeClass('is-invalid');
        $('#user-inv').attr('hidden', true);
        $('#pass-inv').attr('hidden', true);
    });
});
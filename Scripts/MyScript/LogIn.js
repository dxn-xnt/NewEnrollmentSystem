$().ready(function () {
    $('#submitButton').click(function (e) {
        const student = {
            Id: parseInt($('#LoginStudentID').val()),
            Password: $('#LoginPassword').val()
        };

        const token = $('input[name="__RequestVerificationToken"]').val();

        console.log(student);
        
        $.ajax({
            type: 'POST',
            url: '/Account/Student/LogIn',
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            data: $.param(student) + "&__RequestVerificationToken=" + encodeURIComponent(token),
            success: function (response) {
                console.log(response);
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
            error: function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Unable to process login request.',
                });
            }
        });
    });
});
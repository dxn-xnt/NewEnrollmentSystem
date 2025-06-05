$(document).ready(function() {
    $('#addCourseForm').submit(function(e) {
        e.preventDefault();

        // Validate units
        const units = parseInt($('#numberOfUnits').val()) || 0;
        const lec = parseInt($('#lecUnits').val()) || 0;
        const lab = parseInt($('#labUnits').val()) || 0;
        
        // Prepare data
        const courseData = {
            Code: $('#courseCategory').val() +' '+ $('#courseCode').val(),
            Title: $('#descriptiveTitle').val(),
            Units: units,
            LecHours: lec,
            LabHours: lab,
            CategoryCode: $('#courseCategory').val(),
            Prerequisites: $('#prerequisite').val() || []
        };

        // Submit via AJAX
        $.ajax({
            url: '/Admin/Course/AddCourse',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(courseData),
            success: function(response) {
                if (response.mess === 1) {
                    Swal.fire({
                        title: 'Success!',
                        text: response.message,
                        icon: 'success',
                        confirmButtonText: 'OK',
                        willClose: () => {
                            window.location.href = response.redirectUrl;
                        }
                    });
                } else {
                    Swal.fire({
                        title: 'Error',
                        text: response.error || 'An error occurred',
                        icon: 'error'
                    });
                    if (response.field) {
                        $('#' + response.field).addClass('is-invalid');
                    }
                }
            },
            error: function(xhr) {
                Swal.fire({
                    title: 'Error',
                    text: xhr.responseJSON?.error || 'An error occurred during submission',
                    icon: 'error'
                });
            }
        });
    });
});
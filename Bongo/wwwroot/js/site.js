/*
*//* Modals *//*

$(document).ready(function () {
    $('#openSignInModalButton').click(function () {
        
        $('#LoginModal').modal('show');
        $('#login-error-modal').modal('show');
    });
});

$(document).ready(function () {
    $('#openRegisterModalButton').click(function () {
        $('#RegisterModal').modal('show');
    });
});


*//*$(document).ready(function () {
    var showModal = $("#showModal").val() === true;
    console.log('hi');
    console.log(showModal);
    if (showModal) {
        $('#LoginModal').modal('show');
    }
});*//*

$('#LoginModal form').submit(function (event) {
    event.preventDefault();
    console.log('Hi fu');
    var form = $(this);
    var url = form.attr('action');
    var method = form.attr('method');
    var formData = new FormData(form[0]);

    $.ajax({
        url: url,
        type: method,
        data: formData,
        processData: false,
        contentType: false,
        success: function (data) {
            if (data.success) {
                // Process successful login
                window.location.href = data.redirectUrl;
            } else {
                console.log('Hi');
                console.log(data);
                var modalContent = $(data).find('#LoginModal').html();

                // Update the modal content inside the login-error-modal
                $('#login-error-modal #LoginModal').html(modalContent);

                // Show the modal
                $('#login-error-modal').modal('show');

                $('#error-message').text(data.message);

                console.log(data);
                // Re-enable client-side validation after updating the form content
                $.validator.unobtrusive.parse('#LoginModal form');
               
            }
        },
        error: function () {
            // Handle error
           *//* $('#modal-error').html(data);
 $('#LoginModal').modal('show');
 console.log(data);*//*
// Re-enable client-side validation after updating the form content
$.validator.unobtrusive.parse('#LoginModal form');
alert('An error occurred during the login process. Please try again.');
}
});
});
*/


/*Alert auto collapse*/

$("document").ready(function () {
    setTimeout(function () {
        $(".alert").hide('medium');
    }, 5000);
});


$(document).ready(function () {
    $('#noticeModal').modal('show');
});












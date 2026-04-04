document.addEventListener('DOMContentLoaded', function () {
    const token = $('input[name="__RequestVerificationToken"]').val();
    let currentStep = 1; // 1 = phone or email, 2 = OTP, 3 = new password
    let resetToken = '';
    let resendTimer;
    let formState = {
        emailOrPhone: '',
        otp: '',
        newPassword: '',
        confirmPassword: ''
    };

    // Toggle password visibility (existing functionality)
    $('#togglePassword').on('click', function () {
        const passwordField = $('#inputPassword');
        const icon = $(this).find('i');

        if (passwordField.attr('type') === 'password') {
            passwordField.attr('type', 'text');
            icon.removeClass('fa-eye').addClass('fa-eye-slash');
        } else {
            passwordField.attr('type', 'password');
            icon.removeClass('fa-eye-slash').addClass('fa-eye');
        }
    });

    // Show email or phone section in modal when user clicks forgot password
    $('#resetPasswordModal').on('show.bs.modal', function () {
        renderTemplate('step1Template');
    });

    // Resend OTP functionality (Delegate the click event for dynamically inserted elements)
    $('#stepsContainer').on('click', '#resendOtpBtn', async function () {
        if ($(this).hasClass('disabled')) return;

        try {
            const response = await sendOtp(formState.emailOrPhone);
            $('#otpStatusMessage').text('OTP Code sent!').show();
            $('#otpStatusMessage').replaceClass('text-danger', 'text-success');
            startTimer(60);
        } catch (error) {
            if (error.message === "OTP_SEND_FAILED") {
                $('#otpStatusMessage').text('Failed to send OTP. Please try again.').show();
                $('#otpStatusMessage').replaceClass('text-success', 'text-danger');
            } else {
                $('#otpStatusMessage').text(error.responseJSON?.message ||
                    'An error occured. Please try again.').show();
                $('#otpStatusMessage').replaceClass('text-success', 'text-danger');
            }
        }
    });

    // Reset modal when closed
    $('#resetPasswordModal').on('hidden.bs.modal', function () {
        resetForgotPasswordModal();
    });

    // Handle continue button clicks
    $(document).on('submit', '#passwordResetForm', async function (e) {
        e.preventDefault();
        const $btn = $('#resetPassContinueBtn');
        $btn.prop('disabled', true)
            .prepend('<span class="spinner-border spinner-border-sm me-1 btn-spinner" role="status" aria-hidden="true"></span>');

        if (currentStep === 1) {
            try {
                // Step 1: Verify phone number
                formState.emailOrPhone = $('#resetPhoneNumber').val().trim();

                const response = await $.ajax({
                    url: '/Account/VerifyUser',
                    method: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ EmailOrPhone: formState.emailOrPhone }),
                    headers: {
                        'RequestVerificationToken': token
                    }
                });

                if (!response.exists) {
                    $('#resetPhoneNumber').addClass('is-invalid');
                    $('#phoneNumberError').text('Please enter a valid phone number').show();
                    return;
                }

                if (response.isLocked) {
                    $('#phoneNumberError').text(
                        `You are temporarily locked out. 
                                Please try again in ${response.remainingLockoutTime}m.`
                    ).show();
                    return;
                }

                await sendOtp(formState.emailOrPhone);

                renderTemplate('step2Template');
                $('#maskedPhone').text(response.maskedContact);
                $('#otpCode').focus();

                currentStep = 2;
                $btn.text('Verify OTP');
                startTimer(60);
            } catch (error) {
                $('#phoneNumberError').text(
                    error.responseJSON?.message || 'Something went wrong. Please try again.'
                ).show();
            } finally {
                $btn.find('.btn-spinner').remove();
                $btn.prop('disabled', false);
            }
        }

        else if (currentStep === 2) {
            try {
                // Step 1: Verify OTP
                const otp = $('#otpCode').val().trim();

                if (!/^\d{6}$/.test(otp)) {
                    $('#otpCode').addClass('is-invalid');
                    $('#otpStatusMessage').text('Please enter a valid 6-digit code.').show();
                    $('#otpStatusMessage').replaceClass('text-success', 'text-danger');
                    $btn.find('.btn-spinner').remove();
                    $btn.prop('disabled', false);

                    return;
                }

                await verifyOtp(formState.emailOrPhone, otp);

                renderTemplate('step3Template');
                currentStep = 3;
                $btn.text('Reset Password');
                $btn.prop('disabled', false);
                $('#resendOtpBtn').prop('disabled', false);
            } catch (error) {
                if (error.status === 423 && error.responseJSON?.isLockedOut) {
                    $('#otpCode').addClass('is-invalid');
                    $('#otpStatusMessage').text(
                        `You are temporarily locked out. Please try again in ${error.responseJSON.lockoutTimeRemaining}m.`
                    ).show();
                    $('#otpStatusMessage').replaceClass('text-success', 'text-danger');
                    clearInterval(resendTimer);
                    $('#resendOtpBtn').prop('disabled', true);
                    $('#resendOtpBtn').text('Resend');
                    $('#otpCode').removeClass('is-invalid');
                    $('#otpCode').val('');

                    $btn.prop('disabled', true);
                }
                else {
                    $('#otpCode').addClass('is-invalid');
                    $('#otpStatusMessage').text(
                        error.responseJSON?.message || 'OTP verification failed. Please try again.'
                    ).show();
                    $('#otpStatusMessage').replaceClass('text-success', 'text-danger');
                    $btn.find('.btn-spinner').remove();
                    $btn.prop('disabled', false);
                    $('#resendOtpBtn').prop('disabled', false);
                }

                return;
            }
        }

        else if (currentStep === 3) {
            try {
                // Step 3: Reset password
                const newPassword = $('#newPassword').val().trim();
                const confirmPassword = $('#confirmPassword').val().trim();

                // Basic client-side validation
                if (newPassword.length < 8) {
                    $('#newPassword').addClass('is-invalid');
                    $('#resetPasswordStatusMessage').text('Password must be at least 8 characters').show();
                    $btn.find('.btn-spinner').remove();
                    $btn.prop('disabled', false);

                    return;
                }

                if (newPassword !== confirmPassword) {
                    $('#newPassword').addClass('is-invalid');
                    $('#confirmPassword').addClass('is-invalid');
                    $('#resetPasswordStatusMessage').text('Passwords do not match').show();
                    $btn.find('.btn-spinner').remove();
                    $btn.prop('disabled', false);

                    return;
                }

                // Submit to server
                const response = await $.ajax({
                    url: '/Account/ResetPassword',
                    method: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({
                        EmailOrPhone: formState.emailOrPhone,
                        // Otp: formState.otp, // Store this during step 2 verification !!!
                        NewPassword: newPassword,
                        ConfirmNewPassword: confirmPassword
                    }),
                    headers: {
                        'RequestVerificationToken': token
                    }
                });

                if (response.success) {
                    // Success - close modal and show message
                    $('#resetPasswordModal').modal('hide');

                    // Show success toast/alert
                    Swal.fire({
                        icon: 'success',
                        title: 'Success!',
                        html: `Your password has been successfully reset! 
                            <a href="/Account/Login">Login</a> to your account
                        `,
                        // timer: 3000
                    });
                } else {
                    // Show error message.s
                    $('#resetPasswordStatusMessage').text(response.message || 'Password reset failed').show();

                    // Check for futur use
                    //if (response.errors && response.errors.length > 0) {
                    //    // Join array errors into a <ul> list for better display
                    //    const errorHtml = '<ul>' + response.errors.map(err => `<li>${err}</li>`).join('') + '</ul>';
                    //    $('#resetPasswordStatusMessage').html(errorHtml).show();
                    //} else {
                    //    $('#resetPasswordStatusMessage').text(response.message || 'Password reset failed').show();
                    //}

                    $btn.find('.btn-spinner').remove();
                    $btn.prop('disabled', false);
                }

            } catch (error) {
                //console.error('Password reset error:', error);

                // Handle validation errors from server
                if (error.status === 400 && error.responseJSON) {
                    if (error.responseJSON.errors && error.responseJSON.errors.length > 0) {
                        const errorHtml = '<ul>' + error.responseJSON.errors
                            .map(err => `<li>${err}</li>`)
                            .join('') + '</ul>';
                        $('#resetPasswordStatusMessage').html(errorHtml).show();
                    } else {
                        const errorMsg = error.responseJSON.message || 'Invalid password requirements';
                        $('#resetPasswordStatusMessage').text(errorMsg).show();
                    }
                } else {
                    $('#resetPasswordStatusMessage').text('An unexpected error occurred').show();
                }

                $btn.find('.btn-spinner').remove();
                $btn.prop('disabled', false);
            }
        }
    });

    //// Functions

    // Auto-send OTP on Step 1 success
    async function sendOtp(emailOrPhone) {
        try {
            const response = await $.ajax({
                url: '/Account/SendOtp',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ EmailOrPhone: emailOrPhone }),
                headers: { 'RequestVerificationToken': token }
            });
            // startTimer(60); // Start 90s countdown

            return response;
        } catch (jqXHR) {
            const error = new Error("OTP_SEND_FAILED");
            error.responseJSON = jqXHR.responseJSON;
            error.status = jqXHR.status;
            throw error;
        }
    }

    // Verify OTP
    async function verifyOtp(emailOrPhone, otp) {
        try {
            const response = await $.ajax({
                url: '/Account/VerifyOtp',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    EmailOrPhone: emailOrPhone,
                    Otp: otp,
                }),
                dataType: 'json',
                headers: { 'RequestVerificationToken': token }
            });

            return response;
        } catch (jqXHR) {
            const error = new Error("OTP_VERIFICATION_FAILED");
            error.responseJSON = jqXHR.responseJSON;
            error.status = jqXHR.status;
            throw error;
        }
    }

    // Countdown Timer
    function startTimer(seconds) {
        $('#resendOtpBtn').addClass('disabled');
        $('#resendText').addClass('d-none');
        $('#countdown').removeClass('d-none');

        resendTimer = setInterval(function () {
            seconds--;
            $('#countdown').text(`(${seconds}s)`);

            if (seconds <= 0) {
                clearInterval(resendTimer);
                $('#resendOtpBtn').removeClass('disabled');
                $('#resendText').removeClass('d-none');
                $('#countdown').addClass('d-none');
            }
        }, 1000);
    }

    // Reset modal
    function resetForgotPasswordModal() {
        clearInterval(resendTimer);
        currentStep = 1;
        resetToken = '';
        Object.keys(formState).forEach(key => formState[key] = '');
        // renderTemplate('step1Template');

        $('#resetPhoneNumber').val('').removeClass('is-invalid');
        $('#otpCode').val('');
        $('#newPassword').val('');
        $('#confirmPassword').val('');
        $('#phoneEntrySection').removeClass('d-none');
        $('#otpSection, #newPasswordSection').addClass('d-none');
        $('#resetPassContinueBtn').text('Continue');
        $('#resendOtpBtn').removeClass('disabled');
        $('#resendText').removeClass('d-none');
        $('#countdown').addClass('d-none');
        $('#resetPasswordModal .text-danger').hide();
        $('#resetPasswordModal .text-success').hide();
        $('#resetPassContinueBtn').prop('disabled', false);
    }

    // Render templates at each step for password reset
    function renderTemplate(templateId) {
        const container = $('#stepsContainer');

        // Preserve form state before switching
        if (currentStep === 1) formState.emailOrPhone = $('#resetPhoneNumber').val();
        if (currentStep === 2) formState.otp = $('#otpCode').val();
        if (currentStep === 3) {
            formState.newPassword = $('#newPassword').val();
            formState.confirmPassword = $('#confirmPassword').val();
        }

        container.empty();

        const template = document.getElementById(templateId);
        if (template) {
            const clone = template.content.cloneNode(true);
            container.append(clone);
        }

        // Restore state after rendering
        switch (templateId) {
            case 'step1Template':
                $('#resetPhoneNumber').val(formState.emailOrPhone);
                break;
            case 'step2Template':
                $('#otpCode').val(formState.otp);
                break;
            case 'step3Template':
                $('#newPassword').val(formState.newPassword);
                $('#confirmPassword').val(formState.confirmPassword);
                break;
        }
    }

    // Replace class custom function
    $.fn.replaceClass = function (oldClass, newClass) {
        return this.removeClass(oldClass).addClass(newClass);
    };
});
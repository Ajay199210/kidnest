document.addEventListener('DOMContentLoaded', function () {
    const maxSizeMB = 5;
    const allowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    $('input[type="file"]').on('change', function () {
        const file = this.files[0];
        if (!file) return;

        const fileName = file.name.toLowerCase();
        const extension = fileName.substring(fileName.lastIndexOf('.'));
        const fileSizeMB = file.size / (1024 * 1024);

        if ($.inArray(extension, allowedExtensions) === -1) {
            alert("Invalid file type. Only JPG, JPEG, PNG, and WEBP are allowed.");
            $(this).val(""); // Clear the file input
            return;
        }

        if (fileSizeMB > maxSizeMB) {
            alert("File too large. Maximum allowed size is " + maxSizeMB + "MB.");
            $(this).val(""); // Clear the file input
        }
    });
});
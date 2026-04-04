// Measure actual header height and expose as CSS variable
(function () {
    function setHeaderHeight() {
        const h = document.querySelector('header');
        if (h) document.documentElement.style.setProperty('--header-h', h.offsetHeight + 'px');
    }
    setHeaderHeight();
    window.addEventListener('resize', setHeaderHeight);
})();

document.addEventListener('DOMContentLoaded', function () {

    // Set up SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/storeHub") // URL to the ProductHub endpoint
        .build();

    // Listen for the "ProductAdded" event from the SignalR hub
    //connection.on("ProductAdded", function (product) {
    //    showBootstrapToast(`A new product has been added! - ${product.name}`);
    //});

    // Listen for the "ProductUpdated" event from the SignalR hub
    //connection.on("ProductUpdated", function (product) {
    //    showBootstrapToast(`A product has been updated! - ${product.name}`);
    //});

    // Listen for the "ContentAdded" event from the SignalR hub
    //connection.on("ContentAdded", function (content) {
    //    showBootstrapToast(`Carousel will be updated! - ${content.Id}`);
    //});

    // Start the connection to the SignalR hub
    connection.start().catch(function (err) {
        return console.error(err.toString());
    });

    // Activate product zoom on selected images
    initProductZoom();

    // Update all max quantites for all product variants (products grid and details page)
    $('.card.product-card, form#addToCartForm').each(function () {
        updateMaxQuantity($(this));
    });

    // Refresh off canvas
    refreshCartOffcanvas();

    // Show floating filter button only on pages with filter sidebar
    const $filterFloatBtn = $('#filterFloatBtn');
    if ($('.filter-sidebar-col').length) {
        $filterFloatBtn.removeClass('d-none');
    }

    // Show/hide back to top button
    const $backtoTopBtn = $('#backToTopBtn');
    $(window).on('scroll', function () {
        if ($(this).scrollTop() > 300) {
            if ($backtoTopBtn.hasClass('d-none')) {
                $backtoTopBtn.removeClass('d-none').hide().fadeIn();
            }
        } else {
            if ($backtoTopBtn.is(':visible')) {
                $backtoTopBtn.fadeOut(function () {
                    $backtoTopBtn.addClass('d-none');
                });
            }
        }
    });

    // Smooth scroll to top
    $backtoTopBtn.on('click', function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });

    // Highlight active nav link in bottom header
    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.header-nav .nav-link').forEach(function (link) {
        const href = link.getAttribute('href');
        if (href && href.toLowerCase() === currentPath) {
            link.classList.add('active');
        }
    });

    // Mobile burger menu toggle
    var toggleBtn = document.getElementById('headerNavToggle');
    var navMenu = document.getElementById('headerNavMenu');
    if (toggleBtn && navMenu) {
        toggleBtn.addEventListener('click', function () {
            navMenu.classList.toggle('show');
            var icon = toggleBtn.querySelector('i');
            icon.classList.toggle('fa-bars');
            icon.classList.toggle('fa-xmark');
        });
    }
});

//// Functions

// Reusable Toast Function
function showBootstrapToast(message) {
    const toastBody = document.querySelector('#liveToast .toast-body');
    toastBody.textContent = message;

    const toastElement = new bootstrap.Toast(document.getElementById("liveToast"),
        {
            autohide: true,
        });

    toastElement.show();
}

// Initiate product zoom
function initProductZoom(selector = '.img-fluid.prod-img') {
    // Check first if the 'elevateZoom' plugin is loaded and registered
    if ($(selector).length && $.fn.elevateZoom) {
        $(selector).elevateZoom({
            zoomType: "window",
            cursor: "crosshair",
            zoomWindowFadeIn: 500,
            zoomWindowFadeOut: 500,
            zoomWindowWidth: 500,
            zoomWindowHeight: 500,
            responsive: true,
            zoomWindowOffetx: 20
            // borderSize: 1,
            // borderColour: "#888"
        });
    }
}

// Calculate total cart price
function calculateCartTotalPrice() {
    var total = 0;

    $("#cartItemsList .item-price").each(function () {
        const price = parseFloat($(this).data("price"));

        const quantitySpan = $(this).closest("li").find(".cart-qty-text span");
        const quantity = parseInt(quantitySpan.text());

        if (!isNaN(price) && !isNaN(quantity)) {
            total += price * quantity;
        }
    });

    $("#cart-total").text(`$${total.toFixed(2)}`);
}

// Update order modal
function updateOrderModal() {
    let orderItemsList = '';
    let totalAmount = 0;
    let totalItemsCount = 0;

    $('#cartItemsList li').each(function () {
        let $item = $(this);

        let productId = $item.data('id');
        let productName = $item.find('.fw-bold').text().trim();
        let productQuantity = parseInt($item.find('.item-quantity').text().trim());
        let productPrice = parseFloat($item.find('.item-price').data('price'));
        let color = $item.find('.selected-color').text().replace('Color:', '').trim() || null;
        let size = $item.find('.selected-size').text().replace('Size:', '').trim() || null;
        let variantId = $item.data('serialized')?.variantId || null;

        let itemTotalPrice = productQuantity * productPrice;
        totalAmount += itemTotalPrice;
        totalItemsCount += productQuantity;

        let variantDetails = [];
        if (color) variantDetails.push(`Color: ${color}`);
        if (size) variantDetails.push(`Size: ${size}`);

        let variantHtml = variantDetails.length > 0
            ? `<div class="text-muted small">${variantDetails.join(', ')}<br>Unit Price: \$${productPrice.toFixed(2)}</div>`
            : `<div class="text-muted small">Unit Price: \$${productPrice.toFixed(2)}</div>`;

        orderItemsList += `
            <li class="list-group-item d-flex justify-content-between align-items-center" 
                data-id="${productId}"
                data-variant-id="${variantId}">
                <div>
                    <span class="product-name">${productName}</span>
                    <span class="badge bg-secondary">x ${productQuantity}</span>
                    ${variantHtml}
                </div>
                <span class="price fw-semibold">\$${itemTotalPrice.toFixed(2)}</span>
            </li>
        `;

        // Attach the serialized data to each <li>
        $item.data('serialized', {
            productId,
            productName,
            quantity: productQuantity,
            price: productPrice,
            color,
            size,
            variantId // fetched also on order checkout via fetchVariantId (see checkout button handler)
        });
    });

    $('#orderItemsList').html(orderItemsList);
    $('#totalItemsCount').text(totalItemsCount);
    $('#modalTotal').text(`\$${totalAmount.toFixed(2)}`);

    // Collect all serialized data from the DOM and set it into the hidden input
    //const serialized = $('#cartItemsList li').map(function () {
    //    return $(this).data('serialized');
    //}).get();

    //$('#OrderItems').val(JSON.stringify(serialized));
}

// Refresh Offcanvas
function refreshCartOffcanvas() {
    $("#cartItemsContainer").html('<div class="text-center p-4"><i class="fas fa-spinner fa-spin"></i> Updating...</div>');

    // This callback function runs AFTER loading is done
    $("#cartItemsContainer").load("/Cart/RenderCartItemsPartial", function () {
        //updateMaxQuantity();
        calculateCartTotalPrice();
        updateCartCount();
        updateOrderModal();
    });
}

// Reset cart UI
function resetCartUI() {
    $('#cartCount').text(0);
    refreshCartOffcanvas();
}

// Update cart count
function updateCartCount() {
    $.ajax({
        url: '/Cart/GetCartCount', // Add a route to return the total item count
        method: 'GET',
        success: function (response) {
            $('#cartCount').text(response.itemCount);  // Update the cart count on the page
        }
    });
}

// Update cart quantity
function updateCartQuantity(productId, quantity, colorId = null, sizeId = null, complete = null) {
    $.ajax({
        url: '/cart/updateQuantity', // Your update endpoint
        method: 'POST',
        data: JSON.stringify({
            productId: productId,
            quantity: quantity,
            colorId: colorId,
            sizeId: sizeId
        }), // Send the data as JSON
        contentType: 'application/json',
        success: function (response) {
            if (response.success) {
                //console.log('Cart updated');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error updating cart');
        },
        complete: function () {
            if (complete) complete();
        }
    });
}

// Update product variant max quantity: WORKS FOR THE DETAILS PAGE
//function updateMaxQuantity() {
//    if (typeof productVariants === 'undefined') return;

//    const selectedColorId = $('input[name="selectedColor"]:checked').data('color-id');
//    const selectedSizeId = $('input[name="selectedSize"]:checked').data('size-id');

//    if (selectedColorId && selectedSizeId) {
//        // Product has both color and size
//        variant = productVariants.find(v =>
//            v.colorId === selectedColorId && v.sizeId === selectedSizeId
//        );
//    } else if (selectedColorId && !selectedSizeId) {
//        // Product has only color
//        variant = productVariants.find(v =>
//            v.colorId === selectedColorId
//        );
//    } else if (!selectedColorId && selectedSizeId) {
//        // Product has only size
//        variant = productVariants.find(v =>
//            v.sizeId === selectedSizeId
//        );
//    }

//    const qtyInput = $('#quantityInput');
//    if (variant) {
//        qtyInput.attr('max', variant.quantity);
//    }
//}

//Update product variant max quantity: WORKS FOR THE PRODUCTS GRID & PRODUCT DETAILS PAGE
//function updateMaxQuantity() {
//    /*console.log(productVariants);*/
//    if (typeof productVariants === 'undefined') return;

//    $('form#addToCartForm, .card.product-card').each(function () {
//        const container = $(this);

//        const colorInput = container.find('input[name^="selectedColor"]:checked');
//        const sizeInput = container.find('input[name^="selectedSize"]:checked');

//        const selectedColorId = colorInput.data('color-id');
//        const selectedSizeId = sizeInput.data('size-id');

//        let variant = null;

//        if (selectedColorId && selectedSizeId) {
//            variant = productVariants.find(v =>
//                v.colorId === selectedColorId && v.sizeId === selectedSizeId
//            );
//        } else if (selectedColorId && !selectedSizeId) {
//            variant = productVariants.find(v =>
//                v.colorId === selectedColorId
//            );
//        } else if (!selectedColorId && selectedSizeId) {
//            variant = productVariants.find(v =>
//                v.sizeId === selectedSizeId
//            );
//        }

//        if (variant) {
//            const qtyInput = container.find('#quantityInput');
//            if (qtyInput.length) {
//                // Details page
//                qtyInput.attr('max', variant.quantity);
//            } else {
//                // Grid product card
//                container.find('.add-to-cart').data('max', variant.quantity);
//            }
//        }
//    });
//}

// Update product variant max quantity: WORKS FOR THE PRODUCTS GRID & PRODUCT DETAILS PAGE
function updateMaxQuantity(container) {
    // For product cards in grid
    const addToCartBtn = container.find('.add-to-cart');
    const productId = addToCartBtn.data('id');

    const colorInput = container.find('input[name^="selectedColor"]:checked');
    const sizeInput = container.find('input[name^="selectedSize"]:checked');

    const selectedColorId = colorInput.length ? colorInput.data('color-id') : null;
    const selectedSizeId = sizeInput.length ? sizeInput.data('size-id') : null;

    function applyMax(variants) {
        let variant = null;

        if (selectedColorId && selectedSizeId) {
            variant = variants.find(v =>
                v.colorId === selectedColorId && v.sizeId === selectedSizeId
            );
        } else if (selectedColorId) {
            variant = variants.find(v => v.colorId === selectedColorId);
        } else if (selectedSizeId) {
            variant = variants.find(v => v.sizeId === selectedSizeId);
        } else {
            // For products with no variants
            variant = { quantity: addToCartBtn.data('max') };
        }

        if (variant) {
            const qtyInput = container.find('#quantityInput');
            if (qtyInput.length) {
                // Details page
                qtyInput.attr('max', variant.quantity);
            } else {
                // Grid product card
                addToCartBtn.data('max', variant.quantity) // Update jQuery data store
                addToCartBtn.attr('data-max', variant.quantity); // Update DOM attribute
                //console.log('Updated max to:', variant.quantity); // Debug log
            }
        }
    }

    // Check if we're on details page with productVariants
    if (typeof productVariants !== 'undefined') {
        applyMax(productVariants);
        return;
    }

    // For grid cards - use AJAX
    $.ajax({
        url: `/Products/GetProductVariants/${productId}`,
        method: 'GET',
        success: function (variants) {
            container.data('variants', variants);
            applyMax(variants);
        },
        error: function () {
            console.warn('Error fetching product variants');
        }
    });
}

// Fetch variant ID by key
function fetchVariantId($item) {
    const productId = $item.data('id');
    const colorId = $item.data('color-id');
    const sizeId = $item.data('size-id');

    const keyParts = [];
    if (productId) keyParts.push(productId);
    if (colorId) keyParts.push(`C${colorId}`);
    if (sizeId) keyParts.push(`S${sizeId}`);
    const variantKey = keyParts.join('');

    return $.get('/Products/GetProductVariantIdByKey', { key: variantKey })
        .then(response => {
            if (response.success) {
                const serialized = $item.data('serialized') || {};
                //console.log(serialized);
                serialized.variantId = response.variantId;
                $item.data('serialized', serialized);
                //console.log($item);
            } else {
                console.warn(response.message || 'Variant not found.');
                throw new Error('Variant not found.');
            }
        });
}

//// Events

// Add to cart with color/size : WORKS FOR THE DETAILS PAGE ONLY
//$(document).on("click", ".add-to-cart", function () {
//    const btn = $(this);
//    const form = btn.closest('form');

//    // Get quantity data
//    const quantityInput = form.find('#quantityInput');
//    const hasQuantityInput = quantityInput.length > 0;
//    let quantity = hasQuantityInput ? parseInt(quantityInput.val()) : 1;
//    const maxQuantity = hasQuantityInput ? parseInt(quantityInput.attr('max')) : parseInt(btn.data('max'));

//    // Get product variants
//    const colorInput = form.find('input[name="selectedColor"]:checked');
//    const sizeInput = form.find('input[name="selectedSize"]:checked');

//    const color = colorInput.length ? colorInput.val() : null;
//    const colorId = colorInput.length ? colorInput.data('color-id') : null;
//    const size = sizeInput.length ? sizeInput.val() : null;
//    const sizeId = sizeInput.length ? sizeInput.data('size-id') : null;

//    // Check existing items with same variants
//    const existingSelector = `#cartItemsList li[data-id="${btn.data("id")}"]` +
//        `[data-color-id="${colorId || ""}"]` +
//        `[data-size-id="${sizeId || ""}"]`;

//    const existingCartItem = $(existingSelector);
//    const alreadyInCartQuantity = existingCartItem.length ?
//        parseInt(existingCartItem.find('.cart-qty-text span').text()) : 0;
//    const availableToAdd = maxQuantity - alreadyInCartQuantity;

//    // Validate quantity
//    if (quantity <= 0 || quantity > availableToAdd) {
//        let errorMessage = "";
//        if (quantity <= 0) {
//            errorMessage = "Please enter at least 1 unit.";
//            quantity = 1;
//        }
//        else if (availableToAdd === 0) {
//            errorMessage = "No more items can be added for this product.";
//        }
//        else {
//            errorMessage = `Only ${availableToAdd} item(s) available.`;
//            quantity = availableToAdd;
//        }

//        if (hasQuantityInput) {
//            form.find('#quantityValidation').text(errorMessage);
//            quantityInput.val(quantity);
//        } else {
//            showBootstrapToast(errorMessage);
//        }
//        return;
//    }

//    // Prepare cart data
//    const cartItem = {
//        productId: btn.data("id"),
//        productName: btn.data("name"),
//        productImage: btn.data("img"),
//        productPrice: btn.data("price"),
//        //variantId: ,
//        quantity: quantity,
//        stockQuantity: maxQuantity,
//        color: color,
//        colorId: colorId,
//        size: size,
//        sizeId: sizeId
//    };

//    // AJAX call
//    $.ajax({
//        url: '/Cart/AddToCart',
//        method: 'POST',
//        contentType: 'application/json',
//        data: JSON.stringify(cartItem),
//        success: function (response) {
//            if (response.success) {
//                $('#cartCount').text(response.itemCount);
//                refreshCartOffcanvas(); // This will reload the partial view with updated data

//                // Visual feedback
//                btn.addClass('added');
//                setTimeout(() => btn.removeClass('added'), 500);
//            } else {
//                form.find('#quantityValidation').text(response.message);
//            }
//        },
//        error: function () {
//            // Generic message in case if something happened
//            let msg = 'An error occurred. Please try again.';
//            form.find('#quantityValidation').text(msg);
//        }
//    });
//});

// Add to cart with color/size: WORKS WITH PRODUCTS GRID AND PRODUCT DETAILS PAGE
$(document).on("click", ".add-to-cart", function () {
    const btn = $(this);
    const isDetailsPage = btn.closest('form').length > 0;
    const cardContainer = isDetailsPage ? btn.closest('form') : btn.closest('.card');

    // Get quantity data
    const quantityInput = isDetailsPage ? cardContainer.find('#quantityInput') : null;
    let quantity = isDetailsPage ? parseInt(quantityInput.val()) : 1;
    const maxQuantity = isDetailsPage ? parseInt(quantityInput.attr('max')) : parseInt(btn.data('max'));

    // Get product variants - handle different naming patterns
    const colorInput = isDetailsPage
        ? cardContainer.find('input[name="selectedColor"]:checked')
        : cardContainer.find(`input[name="selectedColor-${btn.data('id')}"]:checked`);

    const sizeInput = isDetailsPage
        ? cardContainer.find('input[name="selectedSize"]:checked')
        : cardContainer.find(`input[name="selectedSize-${btn.data('id')}"]:checked`);

    const color = colorInput.length ? colorInput.val() : null;
    const colorId = colorInput.length ? colorInput.data('color-id') : null;
    const size = sizeInput.length ? sizeInput.val() : null;
    const sizeId = sizeInput.length ? sizeInput.data('size-id') : null;

    // Check existing items with same variants
    const existingSelector = `#cartItemsList li[data-id="${btn.data("id")}"]` +
        `[data-color-id="${colorId || ""}"]` +
        `[data-size-id="${sizeId || ""}"]`;

    const existingCartItem = $(existingSelector);
    const alreadyInCartQuantity = existingCartItem.length ?
        parseInt(existingCartItem.find('.cart-qty-text span').text()) : 0;
    const availableToAdd = maxQuantity - alreadyInCartQuantity;

    // Validate quantity
    if (quantity <= 0 || quantity > availableToAdd) {
        let errorMessage = "";
        if (quantity <= 0) {
            errorMessage = "Please enter at least 1 unit.";
            quantity = 1;
        }
        else if (availableToAdd === 0) {
            errorMessage = "No more items can be added for this product.";
        }
        else {
            errorMessage = `Only ${availableToAdd} item(s) available.`;
            quantity = availableToAdd;
        }

        if (isDetailsPage) {
            cardContainer.find('#quantityValidation').text(errorMessage);
            if (quantityInput) quantityInput.val(quantity);
        } else {
            showBootstrapToast(errorMessage);
        }
        return;
    }

    // Prepare cart data
    const cartItem = {
        productId: btn.data("id"),
        productName: btn.data("name"),
        productImage: btn.data("img"),
        productPrice: btn.data("price"),
        quantity: quantity,
        stockQuantity: maxQuantity,
        color: color,
        colorId: colorId,
        size: size,
        sizeId: sizeId
    };

    // AJAX call
    $.ajax({
        url: '/Cart/AddToCart',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(cartItem),
        success: function (response) {
            if (response.success) {
                $('#cartCount').text(response.itemCount);
                refreshCartOffcanvas();
                btn.addClass('added');
                setTimeout(() => btn.removeClass('added'), 500);
            } else {
                if (isDetailsPage) {
                    cardContainer.find('#quantityValidation').text(response.message);
                } else {
                    showBootstrapToast(response.message);
                }
            }
        },
        error: function () {
            const msg = 'An error occurred. Please try again.';
            if (isDetailsPage) {
                cardContainer.find('#quantityValidation').text(msg);
            } else {
                showBootstrapToast(msg);
            }
        }
    });
});

// Remove all cart items
$(document).on("click", "#clearCartBtn", function () {
    $.ajax({
        url: '/Cart/Clear',
        method: 'POST',
        success: function (response) {
            if (response.success) {
                resetCartUI();
            }
        }
    });
});

// Handle increment/decrement item count in off canvas
$(document).on('click', '.increment-btn, .decrement-btn', function () {
    const $btn = $(this);
    const isIncrement = $btn.hasClass('increment-btn');
    const productId = $btn.closest('li').data('id');
    const colorId = $btn.closest('li').data('color-id');
    const sizeId = $btn.closest('li').data('size-id');

    const inputGroup = $btn.closest('.input-group');
    const qtyDiv = inputGroup.find('.cart-qty-text');
    const qtySpan = qtyDiv.find('span');
    let quantity = parseInt(qtySpan.text());
    const maxQty = parseInt(qtyDiv.data('max'));

    // Adjust quantity
    if (isIncrement) {
        quantity++;
    } else {
        quantity--;
    }

    // Validate quantity after change
    if (quantity > maxQty) {
        showBootstrapToast(`Sorry, only ${maxQty} items are available in stock.`);
        quantity = maxQty;
    } else if (quantity <= 0) {
        showBootstrapToast('Quantity cannot be less than 1.');
        quantity = 1;
    }

    // Update quantity visually
    qtySpan.text(quantity);

    // Update the displayed "item-quantity" in the price line
    $btn.closest('li').find('.item-quantity').text(quantity);

    // Loading state: disable both buttons, show spinner on clicked one
    const incrementBtn = inputGroup.find('.increment-btn');
    const decrementBtn = inputGroup.find('.decrement-btn');
    const originalHtml = $btn.html();
    incrementBtn.prop('disabled', true);
    decrementBtn.prop('disabled', true);
    $btn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>');

    // Send updated quantity to server, restore buttons on complete
    updateCartQuantity(productId, quantity, colorId, sizeId, function () {
        $btn.html(originalHtml);
        incrementBtn.prop('disabled', quantity >= maxQty);
        decrementBtn.prop('disabled', quantity <= 1);
    });

    // Update total price immediately (no need to wait for refresh)
    calculateCartTotalPrice();

    // Update order modal
    updateOrderModal();

    // Update cart count
    updateCartCount();
});

// Delete an item from the cart
$(document).on('click', '.delete-item-btn', function () {
    const $btn = $(this);
    const productId = $btn.closest('li').data('id');
    const $listItem = $btn.closest('.list-group-item');

    $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i>');

    $.ajax({
        url: '/cart/removeFromCart',
        method: 'POST',
        data: JSON.stringify(productId),
        contentType: 'application/json',
        success: function (response) {
            if (response.success) {
                $listItem.fadeOut(300, function () {
                    $(this).remove();
                });
                refreshCartOffcanvas();
            }
        },
        error: function (xhr, status, error) {
            $btn.prop('disabled', false).html('<i class="fa-solid fa-xmark"></i>');
            console.error('Error removing item from cart:', error);
        }
    });
});

// Handle checkout button click
$(document).on('click', '#checkoutBtn', function () {
    const $cartItems = $('#cartItemsList li');
    const itemCount = $cartItems.length;

    if (itemCount === 0) {
        alert('Your cart is empty!');
        return;
    }

    const requests = [];

    $cartItems.each(function () {
        const $item = $(this);
        const hasColorOrSize = $item.data('color-id') || $item.data('size-id');

        if (hasColorOrSize) {
            const req = fetchVariantId($(this));
            requests.push(req);
        }
    });

    Promise.all(requests).then(function () {
        // All variant IDs are fetched, now update the modal
        updateOrderModal();
        $('#orderConfirmationModal').modal('show');
    }).catch(function () {
        alert('There was a problem processing your items. Please try again.');
    });
});

// Process order
$(document).on('submit', '#orderForm', function (e) {
    e.preventDefault(); // avoid full page reload

    $('#error-message').hide();

    // Disable the buttons and show spinner on "Confirm order" button
    $('#confirmOrderBtn').prop('disabled', true);
    $('#confirmOrderBtn').html(`
    <div class="spinner-border spinner-border-sm" role="status">
    <span class="visually-hidden">Loading...</span></div>`);

    const orderItems = [];
    $('#orderItemsList li').each(function () {
        orderItems.push({
            ProductId: $(this).data('id'),
            ProductName: $(this).find('.product-name').text().trim(),
            Quantity: parseInt($(this).find('.badge.bg-secondary').text().trim().replace('x', '').trim()),
            Price: parseFloat($(this).find('.price').text().trim().replace('$', '').trim()),
            VariantId: $(this).data('variant-id')
        });
    });

    const orderData = {
        UserId: $('#UserId').val(),
        OrderItems: orderItems,
        ShippingAddress: $('#ShippingAddress').val(),
        PaymentMethod: $('#PaymentMethod').val()
    };

    $.ajax({
        url: '/Orders/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(orderData),
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                $('#orderConfirmationModal').modal('hide');

                Swal.fire({
                    title: 'Order Placed!',
                    text: response.message,
                    icon: 'success',
                    confirmButtonText: 'OK'
                }).then((result) => {
                    if (result.isConfirmed) {
                        bootstrap.Offcanvas.getInstance($('#cartOffcanvas'))?.hide();
                        setTimeout(function () {
                            location.reload();
                        }, 1000);
                    }
                });
            } else {
                $('#error-message')
                    .removeClass('d-none')
                    .html(response.message || response.errors?.join('<br />') ||
                        'An error occurred while processing your oder.')
                    .fadeIn();
            }
        },
        error: function (xhr) {
            //console.log(xhr);
            $('#error-message')
                .removeClass('d-none')
                .html(xhr.responseJSON?.message || 'An unexpected error occurred.')
                .fadeIn();
        },
        complete: function () {
            // Re-enable the button and revert the spinner
            $('#confirmOrderBtn').prop('disabled', false);
            $('#confirmOrderBtn').html('Submit');
        }
    });
});

// Update selected category id for the search bar component
$(document).on('click', '.category-option', function (e) {
    const $categoryIdInput = $("#categoryIdInput");
    const $categoryLabel = $(".search-category-btn .category-label");
    e.preventDefault();

    const id = $(this).data("id") ?? "";
    const name = $(this).text().trim();

    $categoryIdInput.val(id);
    $categoryLabel.text(name || "All");
});

// Handle buy now button click
$(document).on('click', '#btnBuyNow', function () {
    let totalAmount = 0;
    let totalItemsCount = 0;

    const btn = $(this);

    const productId = btn.data("id");
    const productName = btn.data('name').trim();
    const productPrice = parseFloat(btn.data('price'));

    const form = btn.closest('form');
    const quantityInput = form.find('#quantityInput');
    const hasQuantityInput = quantityInput.length > 0;

    let quantity = hasQuantityInput ? parseInt(quantityInput.val()) : 1;
    let maxQuantity = hasQuantityInput ? parseInt(quantityInput.attr('max')) : parseInt(btn.data('max'));

    // Get selected color and size
    const selectedColorInput = form.find('input[name="selectedColor"]:checked');
    const selectedColor = selectedColorInput.val() || '';
    const selectedColorId = selectedColorInput.data('color-id') || null;

    const selectedSizeInput = form.find('input[name="selectedSize"]:checked');
    const selectedSize = selectedSizeInput.val() || '';
    const selectedSizeId = selectedSizeInput.data('size-id') || null;

    // Generate a unique key based on product + color + size
    const variantKey = `${productId}-${selectedColorId ?? 'x'}-${selectedSizeId ?? 'x'}`;

    // Find how many units of this specific variant are already in cart
    const existingCartItem = $(`#cartItemsList li[data-key="${variantKey}"]`);
    const alreadyInCartQuantity = existingCartItem.length ? parseInt(existingCartItem.find('.cart-qty-text span').text()) : 0;
    const availableToAdd = maxQuantity - alreadyInCartQuantity;

    if (hasQuantityInput) {
        form.find('#quantityValidation').text('');
    }

    if (quantity <= 0 || quantity > availableToAdd) {
        let errorMessage = '';

        if (quantity <= 0) {
            errorMessage = "Please enter at least 1 unit if you want to add to the cart.";
            quantity = 1;
        }
        else if (availableToAdd === 0) {
            errorMessage = "Sorry, no more items can be added for this product variant.";
        }
        else {
            errorMessage = `Sorry, only ${availableToAdd} item(s) of this variant can be added.`;
            quantity = availableToAdd;
        }

        if (hasQuantityInput) {
            form.find('#quantityValidation').text(errorMessage);
            quantityInput.val(quantity);
        } else {
            showBootstrapToast(errorMessage);
        }

        return;
    }

    // Build variant details
    const variantDetails = [];
    if (selectedColor) variantDetails.push(`Color: ${selectedColor}`);
    if (selectedSize) variantDetails.push(`Size: ${selectedSize}`);

    // Build variantHtml section
    const variantHtml = variantDetails.length > 0
        ? `<div class="text-muted small">${variantDetails.join(', ')}
        <br>
        Unit Price: \$${productPrice.toFixed(2)}</div>`
        : `<div class="text-muted small">Unit Price: \$${productPrice.toFixed(2)}</div>`;

    let itemTotal = quantity * productPrice;
    totalAmount += itemTotal;
    totalItemsCount += quantity;

    // Update modal with variant-specific product
    const orderItem = `
        <li class="list-group-item d-flex justify-content-between align-items-center" data-key="${variantKey}">
            <div>
                <span class="product-name">${productName}</span>
                <span class="badge bg-secondary">x ${quantity}</span>
                ${variantHtml}
            </div>
            <span class="price fw-semibold">\$${itemTotal.toFixed(2)}</span>
        </li>
    `;

    $('#orderItemsList').html(orderItem);
    $('#totalItemsCount').text(totalItemsCount);
    $('#modalTotal').text(`\$${totalAmount.toFixed(2)}`);

    $('#orderConfirmationModal').modal('show');
});

// Handle color/size selection change event
$(document).on('change', 'input[name^="selectedColor"], input[name^="selectedSize"]', function () {
    const container = $(this).closest('form#addToCartForm, .card.product-card');
    updateMaxQuantity(container);
});


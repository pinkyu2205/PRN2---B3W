"use strict";

document.addEventListener("DOMContentLoaded", function () {
    // Check if SignalR is loaded
    if (typeof signalR === 'undefined') {
        console.warn("SignalR library is not loaded.");
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notification")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // Utility: Create HTML for a Bootstrap Toast
    function showToast(title, message, type = "info") {
        let toastContainer = document.getElementById("toast-container");
        if (!toastContainer) {
            toastContainer = document.createElement("div");
            toastContainer.id = "toast-container";
            toastContainer.className = "toast-container position-fixed bottom-0 end-0 p-3";
            toastContainer.style.zIndex = "1090";
            document.body.appendChild(toastContainer);
        }

        // Map type to Bootstrap color class
        let bgClass = "text-bg-primary";
        let icon = "fa-info-circle";
        if (type === "success") { bgClass = "text-bg-success"; icon = "fa-check-circle"; }
        if (type === "danger" || type === "error") { bgClass = "text-bg-danger"; icon = "fa-exclamation-circle"; }
        if (type === "warning") { bgClass = "text-bg-warning"; icon = "fa-exclamation-triangle"; }

        const toastId = "toast_" + new Date().getTime();
        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center ${bgClass} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <strong><i class="fas ${icon} me-2"></i>${title}</strong><br/>
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;
        toastContainer.insertAdjacentHTML("beforeend", toastHtml);
        
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement, { autohide: true, delay: 5000 });
        toast.show();

        // Cleanup
        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    }

    // 1. General User Notification
    connection.on("ReceiveNotification", function (title, message, type) {
        showToast(title, message, type);
    });

    // 2. Order Status Changed for Customer
    connection.on("ReceiveOverviewUpdate", function (orderId, newStatus) {
        showToast("Trạng thái đơn hàng", `Đơn hàng #${orderId} của bạn đã chuyển sang trạng thái: ${newStatus}`, "info");
        // Optionally trigger a custom event that views can listen to (to update grids without refresh)
        window.dispatchEvent(new CustomEvent('OrderUpdatedEvent', { detail: { orderId, newStatus } }));
    });

    // 3. New Order Notification for Shop Vendor
    connection.on("ReceiveOrderNotification", function (orderId, message) {
        showToast("Đơn hàng mới!", message, "success");
        // Audio blip could be added here
    });

    async function start() {
        try {
            await connection.start();
            console.log("Notification Hub Connected.");
        } catch (err) {
            console.log(err);
            setTimeout(start, 5000); // Retry logic
        }
    }

    connection.onclose(async () => {
        await start();
    });

    // Start connection
    start();
});

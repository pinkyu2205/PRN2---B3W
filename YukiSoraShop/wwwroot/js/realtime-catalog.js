"use strict";

document.addEventListener("DOMContentLoaded", function () {
    if (typeof signalR === 'undefined') {
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/catalog")
        .withAutomaticReconnect()
        .build();

    connection.on("StockUpdated", function (productId, currentStock) {
        console.log(`[CatalogHub] Product ${productId} stock updated to ${currentStock}.`);
        
        // Dispatch Custom Event for specific views to handle gracefully (e.g. Catalog grid, Product Details page)
        window.dispatchEvent(new CustomEvent('ProductStockUpdated', { 
            detail: { productId, currentStock } 
        }));
    });

    async function start() {
        try {
            await connection.start();
            console.log("Catalog Hub Connected.");
        } catch (err) {
            console.log(err);
            setTimeout(start, 5000);
        }
    }

    connection.onclose(async () => {
        await start();
    });

    start();
});



function showLoading(message) {
    loadingOverlay = document.getElementById("loadingOverlay");
    loadingOverlay?.classList.add("active");
    loadingOverlay?.querySelector("#spinner")?.classList.add("active");
    loadingOverlay?.querySelector("#closeButton")?.classList.remove("active");
    loadingOverlay?.querySelector("#error")?.classList.remove("active");
    loadingOverlay?.querySelector("#success")?.classList.remove("active");
    const messageElement = loadingOverlay?.querySelector("#message");
    if (messageElement) {
        messageElement.textContent = message;
        messageElement.classList.add("active");
    }
}

function hideLoading() {
    document.getElementById("loadingOverlay")?.classList.remove("active");
}

function showError(message) {
    loadingOverlay = document.getElementById("loadingOverlay");
    loadingOverlay?.classList.add("active");
    loadingOverlay?.querySelector("#iconMessage")?.classList.add("active");
    loadingOverlay?.querySelector("#iconMessage")?.classList.add("error-message");
    loadingOverlay?.querySelector("#spinner")?.classList.remove("active");
    loadingOverlay?.querySelector("#success")?.classList.remove("active");
    loadingOverlay?.querySelector("#closeButton")?.classList.remove("active");
    loadingOverlay?.querySelector("#error")?.classList.add("active");
    messageElement = loadingOverlay?.querySelector("#message");
    if (messageElement) {
        messageElement.textContent = message;
        messageElement.classList.add("active");
    }
}

function showSuccess(message) {
    loadingOverlay = document.getElementById("loadingOverlay");
    loadingOverlay?.classList.add("active");
    loadingOverlay?.querySelector("#iconMessage")?.classList.add("success-message");
    loadingOverlay?.querySelector("#spinner")?.classList.remove("active");
    loadingOverlay?.querySelector("#success")?.classList.add("active");
    loadingOverlay?.querySelector("#closeButton")?.classList.remove("active");
    loadingOverlay?.querySelector("#error")?.classList.remove("active");
    messageElement = loadingOverlay?.querySelector("#message");
    if (messageElement) {
        messageElement.textContent = message;
        messageElement.classList.add("active");
    }
}




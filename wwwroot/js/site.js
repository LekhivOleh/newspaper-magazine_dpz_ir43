document.addEventListener("click", async event => {
    const button = event.target.closest("[data-copy-url]");
    if (!button) return;

    const original = button.textContent;
    try {
        await navigator.clipboard.writeText(button.dataset.copyUrl);
        button.textContent = "Скопійовано ✓";
    } catch {
        button.textContent = "Не вдалося скопіювати";
    }
    setTimeout(() => button.textContent = original, 2000);
});

(() => {
    const root = document.getElementById("content-editor");
    if (!root) return;

    const list = root.querySelector("[data-blocks]");
    const empty = root.querySelector("[data-empty]");
    const status = root.querySelector("[data-status]");
    const saveButton = root.querySelector("[data-save]");
    const saveUrl = root.dataset.saveUrl;

    let blocks = JSON.parse(document.getElementById("initial-blocks").textContent);
    let dirty = false;

    const templates = {
        text: () => ({ type: "text", heading: "", body: "" }),
        image: () => ({ type: "image", url: "", caption: "" }),
        video: () => ({ type: "video", embedUrl: "", caption: "" }),
        gallery: () => ({ type: "gallery", title: "", images: [{ url: "", caption: "" }] })
    };

    const titles = { text: "Текст", image: "Фото", video: "Відео", gallery: "Галерея" };

    function field(label, value, onInput, { multiline = false, placeholder = "" } = {}) {
        const wrapper = document.createElement("label");
        wrapper.className = "editor-field";
        wrapper.innerHTML = `<span>${label}</span>`;

        const input = document.createElement(multiline ? "textarea" : "input");
        input.className = "form-control form-control-sm";
        input.value = value ?? "";
        input.placeholder = placeholder;
        if (multiline) input.rows = 6;
        input.addEventListener("input", () => {
            onInput(input.value);
            markDirty();
        });

        wrapper.append(input);
        return wrapper;
    }

    function iconButton(text, title, onClick, disabled = false) {
        const button = document.createElement("button");
        button.type = "button";
        button.className = "btn btn-sm btn-light";
        button.textContent = text;
        button.title = title;
        button.disabled = disabled;
        button.addEventListener("click", onClick);
        return button;
    }

    function renderBlockFields(block, body) {
        switch (block.type) {
            case "text":
                body.append(
                    field("Підзаголовок", block.heading, v => block.heading = v),
                    field("Текст (новий абзац — з нового рядка)", block.body, v => block.body = v, { multiline: true }));
                break;
            case "image":
                body.append(
                    field("Посилання на фото", block.url, v => block.url = v, { placeholder: "https://…" }),
                    field("Підпис", block.caption, v => block.caption = v));
                break;
            case "video":
                body.append(
                    field("Embed-посилання", block.embedUrl, v => block.embedUrl = v, { placeholder: "https://www.youtube.com/embed/…" }),
                    field("Підпис", block.caption, v => block.caption = v));
                break;
            case "gallery":
                body.append(field("Назва галереї", block.title, v => block.title = v));
                block.images.forEach((image, i) => {
                    const row = document.createElement("div");
                    row.className = "editor-gallery-row";
                    row.append(
                        field(`Фото ${i + 1}`, image.url, v => image.url = v, { placeholder: "https://…" }),
                        field("Підпис", image.caption, v => image.caption = v),
                        iconButton("✕", "Прибрати фото", () => {
                            block.images.splice(i, 1);
                            markDirty();
                            render();
                        }, block.images.length === 1));
                    body.append(row);
                });
                body.append(iconButton("+ фото", "Додати фото в галерею", () => {
                    block.images.push({ url: "", caption: "" });
                    markDirty();
                    render();
                }));
                break;
        }
    }

    function render() {
        list.replaceChildren();
        empty.hidden = blocks.length > 0;

        blocks.forEach((block, index) => {
            const card = document.createElement("section");
            card.className = `editor-block editor-block-${block.type}`;

            const header = document.createElement("header");
            header.innerHTML = `<strong>${index + 1}. ${titles[block.type]}</strong>`;
            const actions = document.createElement("div");
            actions.append(
                iconButton("↑", "Вище", () => move(index, -1), index === 0),
                iconButton("↓", "Нижче", () => move(index, 1), index === blocks.length - 1),
                iconButton("🗑", "Видалити блок", () => {
                    blocks.splice(index, 1);
                    markDirty();
                    render();
                }));
            header.append(actions);

            const body = document.createElement("div");
            body.className = "editor-block-body";
            renderBlockFields(block, body);

            card.append(header, body);
            list.append(card);
        });
    }

    function move(index, delta) {
        const [block] = blocks.splice(index, 1);
        blocks.splice(index + delta, 0, block);
        markDirty();
        render();
    }

    function markDirty() {
        dirty = true;
        setStatus("Є незбережені зміни", "warning");
    }

    function setStatus(text, kind) {
        status.textContent = text;
        status.dataset.kind = kind ?? "";
    }

    function emptyToNull(value) {
        return typeof value === "string" && value.trim() === "" ? null : value;
    }

    function payload() {
        return {
            blocks: blocks.map(block => {
                const copy = Object.fromEntries(Object.entries(block).map(([k, v]) => [k, emptyToNull(v)]));
                if (block.type === "gallery")
                    copy.images = block.images.map(img => ({ url: emptyToNull(img.url), caption: emptyToNull(img.caption) }));
                return copy;
            })
        };
    }

    async function save() {
        saveButton.disabled = true;
        setStatus("Зберігаємо…");
        try {
            const response = await fetch(saveUrl, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                credentials: "same-origin",
                body: JSON.stringify(payload())
            });

            if (response.ok) {
                blocks = (await response.json()).blocks;
                dirty = false;
                render();
                setStatus("Збережено ✓", "success");
                return true;
            }

            const problem = await response.json().catch(() => null);
            const details = problem?.detail
                || Object.values(problem?.errors ?? {}).flat().join(" ")
                || `HTTP ${response.status}`;
            setStatus(`Помилка: ${details}`, "danger");
            return false;
        } catch {
            setStatus("Немає зв'язку з сервером.", "danger");
            return false;
        } finally {
            saveButton.disabled = false;
        }
    }

    root.querySelectorAll("[data-add]").forEach(button =>
        button.addEventListener("click", () => {
            blocks.push(templates[button.dataset.add]());
            markDirty();
            render();
            list.lastElementChild?.querySelector("input, textarea")?.focus();
        }));

    saveButton.addEventListener("click", save);

    document.querySelectorAll("[data-requires-saved]").forEach(button =>
        button.closest("form").addEventListener("submit", async event => {
            if (!dirty) return;
            event.preventDefault();
            if (await save()) event.target.submit();
        }));

    window.addEventListener("beforeunload", event => {
        if (dirty) event.preventDefault();
    });

    render();
})();

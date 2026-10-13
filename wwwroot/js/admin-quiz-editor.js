(() => {
    const root = document.getElementById("quiz-editor");
    if (!root) return;

    const locked = root.dataset.locked === "true";
    const modeSelect = root.querySelector("[data-mode]");
    const modeHint = root.querySelector("[data-mode-hint]");
    const outcomesList = root.querySelector("[data-outcomes]");
    const questionsList = root.querySelector("[data-questions]");
    const status = root.querySelector("[data-status]");
    const saveButton = root.querySelector("[data-save]");

    let state = JSON.parse(document.getElementById("initial-definition").textContent);
    let dirty = false;

    const hints = {
        CorrectAnswers: "Позначте одну правильну відповідь у кожному питанні. Поріг результату — у відсотках (потрібен рівень з порогом 0).",
        Personality: "Кожна відповідь «голосує» за один із результатів. Перемагає той, за який голосів найбільше.",
        Points: "Кожна відповідь має бали. Результат обирається за сумою: найвищий поріг, якого досягнуто."
    };

    const el = (tag, props = {}, ...children) => {
        const node = Object.assign(document.createElement(tag), props);
        node.append(...children);
        return node;
    };

    function input(value, onInput, props = {}) {
        const node = el("input", { className: "form-control form-control-sm", value: value ?? "", disabled: locked, ...props });
        node.addEventListener("input", () => {
            onInput(props.type === "number" ? Number(node.value) : node.value);
            markDirty();
        });
        return node;
    }

    function button(text, title, onClick, disabled = false) {
        const node = el("button", { type: "button", className: "btn btn-sm btn-light", textContent: text, title, disabled: disabled || locked });
        node.addEventListener("click", () => {
            onClick();
            markDirty();
            render();
        });
        return node;
    }

    function field(label, control) {
        return el("label", { className: "editor-field" }, el("span", { textContent: label }), control);
    }

    function renderOutcomes() {
        outcomesList.replaceChildren();
        const usesThreshold = state.scoringMode !== "Personality";

        state.outcomes.forEach((outcome, index) => {
            const body = el("div", { className: "editor-block-body" },
                el("div", { className: "editor-gallery-row" },
                    field("Ключ", input(outcome.key, v => outcome.key = v, { placeholder: "laptop" })),
                    field("Назва", input(outcome.title, v => outcome.title = v)),
                    usesThreshold
                        ? field(state.scoringMode === "Points" ? "Від балів" : "Від %", input(outcome.threshold, v => outcome.threshold = v, { type: "number", min: 0 }))
                        : ""),
                field("Опис", input(outcome.description, v => outcome.description = v)));

            outcomesList.append(el("section", { className: "editor-block editor-block-gallery" },
                el("header", {},
                    el("strong", { textContent: `Результат ${index + 1}` }),
                    el("div", {}, button("🗑", "Видалити", () => state.outcomes.splice(index, 1)))),
                body));
        });
    }

    function optionControl(question, option) {
        switch (state.scoringMode) {
            case "CorrectAnswers": {
                const radio = el("input", { type: "radio", className: "form-check-input", checked: option.isCorrect, disabled: locked, title: "Правильна відповідь" });
                radio.addEventListener("change", () => {
                    question.options.forEach(o => o.isCorrect = o === option);
                    markDirty();
                });
                return el("label", { className: "d-flex align-items-center gap-1 small text-nowrap" }, radio, "правильна");
            }
            case "Personality": {
                const select = el("select", { className: "form-select form-select-sm", disabled: locked });
                select.append(el("option", { value: "", textContent: "— результат —" }));
                state.outcomes.forEach(o => select.append(el("option", { value: o.key, textContent: o.title || o.key, selected: o.key === option.outcomeKey })));
                select.addEventListener("change", () => {
                    option.outcomeKey = select.value || null;
                    markDirty();
                });
                return select;
            }
            default:
                return el("div", { style: "max-width: 6rem" }, input(option.points, v => option.points = v, { type: "number", min: 0, title: "Бали" }));
        }
    }

    function renderQuestions() {
        questionsList.replaceChildren();

        state.questions.forEach((question, qIndex) => {
            const body = el("div", { className: "editor-block-body" },
                field("Питання", input(question.text, v => question.text = v)));

            question.options.forEach((option, oIndex) => {
                body.append(el("div", { className: "editor-gallery-row align-items-center" },
                    input(option.text, v => option.text = v, { placeholder: `Варіант ${oIndex + 1}` }),
                    optionControl(question, option),
                    button("✕", "Прибрати варіант", () => question.options.splice(oIndex, 1), question.options.length <= 2)));
            });

            body.append(el("div", {}, button("+ варіант", "Додати варіант", () =>
                question.options.push({ text: "", isCorrect: false, outcomeKey: null, points: 0 }))));

            questionsList.append(el("section", { className: "editor-block editor-block-text" },
                el("header", {},
                    el("strong", { textContent: `${qIndex + 1}. Питання` }),
                    el("div", {},
                        button("↑", "Вище", () => move(state.questions, qIndex, -1), qIndex === 0),
                        button("↓", "Нижче", () => move(state.questions, qIndex, 1), qIndex === state.questions.length - 1),
                        button("🗑", "Видалити", () => state.questions.splice(qIndex, 1)))),
                body));
        });
    }

    function move(list, index, delta) {
        const [item] = list.splice(index, 1);
        list.splice(index + delta, 0, item);
    }

    function render() {
        modeSelect.value = state.scoringMode;
        modeSelect.disabled = locked;
        modeHint.textContent = hints[state.scoringMode];
        renderOutcomes();
        renderQuestions();
        root.querySelectorAll("[data-add-outcome], [data-add-question]").forEach(b => b.disabled = locked);
        saveButton.disabled = locked;
    }

    function markDirty() {
        dirty = true;
        status.textContent = "Є незбережені зміни";
        status.dataset.kind = "warning";
    }

    async function save() {
        saveButton.disabled = true;
        status.textContent = "Зберігаємо…";
        status.dataset.kind = "";
        try {
            const response = await fetch(root.dataset.saveUrl, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                credentials: "same-origin",
                body: JSON.stringify(state)
            });
            if (response.ok) {
                state = await response.json();
                dirty = false;
                render();
                status.textContent = "Збережено ✓";
                status.dataset.kind = "success";
                return;
            }
            const problem = await response.json().catch(() => null);
            status.textContent = "Помилка: " + (problem?.detail
                || Object.values(problem?.errors ?? {}).flat().join(" ")
                || `HTTP ${response.status}`);
            status.dataset.kind = "danger";
        } catch {
            status.textContent = "Немає зв'язку з сервером.";
            status.dataset.kind = "danger";
        } finally {
            saveButton.disabled = locked;
        }
    }

    modeSelect.addEventListener("change", () => {
        state.scoringMode = modeSelect.value;
        markDirty();
        render();
    });

    root.querySelector("[data-add-outcome]").addEventListener("click", () => {
        const n = state.outcomes.length + 1;
        state.outcomes.push({ key: `result${n}`, title: "", description: "", threshold: 0 });
        markDirty();
        render();
    });

    root.querySelector("[data-add-question]").addEventListener("click", () => {
        state.questions.push({ text: "", options: [
            { text: "", isCorrect: false, outcomeKey: null, points: 0 },
            { text: "", isCorrect: false, outcomeKey: null, points: 0 }
        ] });
        markDirty();
        render();
    });

    // Outcome keys/titles feed the personality dropdowns — refresh them once an edit is committed.
    outcomesList.addEventListener("change", renderQuestions);

    saveButton.addEventListener("click", save);

    window.addEventListener("beforeunload", event => {
        if (dirty) event.preventDefault();
    });

    render();
})();

"use strict";

document.querySelectorAll("[data-password-targets]").forEach((toggle) => {
    const ids = (toggle.dataset.passwordTargets || "").split(/\s+/).filter(Boolean);
    const fields = ids.map((id) => document.getElementById(id)).filter((field) => field?.tagName === "INPUT");
    if (fields.length === 0) return;

    toggle.addEventListener("change", () => {
        const type = toggle.checked ? "text" : "password";
        fields.forEach((field) => { field.type = type; });
    });
});
"use strict";

// These are example values for a local coursework demo. Passwords are never filled.
const presets = {
    organiser: {
        Input_FirstName: "Demo",
        Input_LastName: "Organiser",
        Input_Role: "Organiser",
        Input_OrganiserName: "Durban Demo Race Club",
        Input_DateOfBirth: "",
        Input_Email: "organiser@raceday.test"
    },
    participant: {
        Input_FirstName: "Demo",
        Input_LastName: "Participant",
        Input_Role: "Participant",
        Input_OrganiserName: "",
        Input_DateOfBirth: "2004-06-12",
        Input_Email: "participant@raceday.test"
    },
    event: {
        EventName: "Durban Spring 5K and 10K Demo",
        Description: "Sample race for my PROG6212 Part 2 demonstration. Test data only.",
        Location: "Durban, KwaZulu-Natal",
        EventType: "Run",
        Status: "Open",
        EventDate: "2026-11-21",
        ClosingDate: "2026-11-14"
    },
    "event-close": { Status: "Closed" },
    "category-5": {
        CategoryName: "5 km Fun Run",
        DistanceKm: "5",
        EntryFee: "50.00",
        MaxParticipants: "30"
    },
    "category-10": {
        CategoryName: "10 km Road Run",
        DistanceKm: "10",
        EntryFee: "100.00",
        MaxParticipants: "20"
    },
    result: {
        ResultStatus: "Finished",
        StartTime: "08:00",
        FinishTime: "08:34",
        OverallPosition: "1"
    }
};

for (const button of document.querySelectorAll("button[data-demo-preset]")) {
    button.addEventListener("click", () => {
        const sample = presets[button.dataset.demoPreset];
        const form = button.closest("form");
        if (!sample || !form) return;

        for (const [id, value] of Object.entries(sample)) {
            const field = form.querySelector(`[id="${id}"]`);
            if (!field) continue;
            field.value = value;
            field.dispatchEvent(new Event("input", { bubbles: true }));
            field.dispatchEvent(new Event("change", { bubbles: true }));
        }
        const feedback = form.querySelector("[data-demo-feedback]");
        if (feedback) {
            feedback.textContent = button.dataset.demoPreset === "event-close"
                ? "Status set to Closed. Review the event and click Save event."
                : "Example details filled. Review everything and submit the form yourself. Passwords are not filled.";
        }
    });
}

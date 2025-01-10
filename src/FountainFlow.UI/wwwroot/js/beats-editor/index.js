// beats-editor/index.js
import { BeatState } from './state.js';
import { Renderers } from './renderers.js';
import { EventHandlers } from './event-handlers.js';
import { BeatActions } from './beat-actions.js';

// Create the public API
const BeatEditor = {
    initialize(beats) {
        BeatState.initialize(beats);
        Renderers.renderBeats();
        EventHandlers.initialize();
    },

    // Public methods that match your existing API
    addBeat() {
        BeatActions.addBeat();
    },

    hasUnsavedChanges() {
        return BeatState.hasUnsavedChanges();
    },

    setDirty(isDirty) {
        BeatState.setDirty(isDirty);
    },

    getChanges() {
        return BeatState.getChanges();
    }
};

// Export for global access
window.BeatEditor = BeatEditor;
// beat-actions.js
import { BeatState } from './state.js';
import { Renderers } from './renderers.js';

export const BeatActions = {
    addBeat() {
        const newBeat = {
            id: 'temp_' + Date.now(),
            name: 'New Beat',
            description: 'Beat description',
            sequence: BeatState.getCurrentState().length + 1,
            percentOfStory: 0,
            archetypeId: window.archetypeId // Use from window
        };

        BeatState.addBeat(newBeat);
        Renderers.renderBeats();
        BeatState.setSelectedBeat(newBeat.id);
    },

    updateBeat(beatId, updates) {
        BeatState.updateBeat(beatId, updates);
        Renderers.renderBeats();
    },

    deleteBeat(beatId) {
        BeatState.deleteBeat(beatId);
        Renderers.renderBeats();
    }
};
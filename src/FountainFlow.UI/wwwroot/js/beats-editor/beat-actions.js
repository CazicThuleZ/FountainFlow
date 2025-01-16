// beat-actions.js
import { BeatState } from './state.js';
import { Renderers } from './renderers.js';

export const BeatActions = {
    addBeat() {
        const newBeat = {
            id: 'temp_' + Date.now(),
            name: 'New Beat',
            description: '',
            prompt: '',  // New field
            parentSequence: BeatState.getNextParentSequence(),
            childSequence: null,  // Start as top-level beat
            grandchildSequence: null,  // Start as top-level beat
            percentOfStory: 0,
            archetypeId: window.archetypeId
        };

        BeatState.addBeat(newBeat);
        Renderers.renderBeats();
        BeatState.setSelectedBeat(newBeat.id);
    },

    updateBeat(beatId, updates) {
        try {
            // Ensure we're not breaking hierarchy rules
            if (updates.hasOwnProperty('childSequence') || 
                updates.hasOwnProperty('grandchildSequence')) {
                this.validateHierarchyUpdate(beatId, updates);
            }

            const updatedBeat = BeatState.updateBeat(beatId, updates);
            if (updatedBeat) {
                this.updateDependentBeats(beatId, updates);
                Renderers.renderBeats();
            }
        } catch (error) {
            console.error('Failed to update beat:', error);
            throw error;
        }
    },

    validateHierarchyUpdate(beatId, updates) {
        const beat = BeatState.getBeatById(beatId);
        if (!beat) return;

        // Check if we're trying to nest a parent with children
        if (updates.childSequence !== null || updates.grandchildSequence !== null) {
            const hasChildren = BeatState.getCurrentState().some(b => 
                b.parentSequence === beat.parentSequence && 
                b.childSequence !== null
            );
            
            if (hasChildren) {
                throw new Error('Cannot nest a beat that has children');
            }
        }

        // Check maximum nesting depth
        if (updates.grandchildSequence !== null && updates.childSequence === null) {
            throw new Error('Cannot create grandchild without child sequence');
        }
    },
    
    updateDependentBeats(beatId, updates) {
        const beat = BeatState.getBeatById(beatId);
        if (!beat) return;

        const state = BeatState.getCurrentState();

        // If this is a parent beat being moved
        if (updates.hasOwnProperty('parentSequence')) {
            // Update all child beats
            state.forEach(b => {
                if (b.parentSequence === beat.parentSequence && b.id !== beatId) {
                    BeatState.updateBeat(b.id, { parentSequence: updates.parentSequence });
                }
            });
        }

        // If this is a child beat being moved
        if (updates.hasOwnProperty('childSequence')) {
            // Update all grandchild beats
            state.forEach(b => {
                if (b.parentSequence === beat.parentSequence && 
                    b.childSequence === beat.childSequence && 
                    b.id !== beatId) {
                    BeatState.updateBeat(b.id, { childSequence: updates.childSequence });
                }
            });
        }
    },    

    deleteBeat(beatId) {
        try {
            const beat = BeatState.getBeatById(beatId);
            if (!beat) return;

            // First, delete any descendant beats
            this.deleteDescendants(beat);

            // Then delete the beat itself
            BeatState.deleteBeat(beatId);
            Renderers.renderBeats();

            // If this was the selected beat, clear selection
            if (BeatState.getSelectedBeat()?.id === beatId) {
                BeatState.clearSelection();
            }
        } catch (error) {
            console.error('Failed to delete beat:', error);
            throw error;
        }
    },

    deleteDescendants(beat) {
        const state = BeatState.getCurrentState();

        // If this is a parent beat, delete all children first
        if (!beat.childSequence) {
            const children = state.filter(b => 
                b.parentSequence === beat.parentSequence && 
                b.childSequence !== null
            );
            
            children.forEach(child => this.deleteBeat(child.id));
        }
        // If this is a child beat, delete all grandchildren
        else if (!beat.grandchildSequence) {
            const grandchildren = state.filter(b => 
                b.parentSequence === beat.parentSequence && 
                b.childSequence === beat.childSequence && 
                b.grandchildSequence !== null
            );
            
            grandchildren.forEach(grandchild => BeatState.deleteBeat(grandchild.id));
        }
    },
    moveBeat(beatId, targetParentId, targetChildId = null) {
        const beat = BeatState.getBeatById(beatId);
        const targetParent = BeatState.getBeatById(targetParentId);
        
        if (!beat || !targetParent) return;

        try {
            const updates = {
                parentSequence: targetParent.parentSequence,
                childSequence: targetChildId ? BeatState.getNextChildSequence(targetParent.parentSequence) : null,
                grandchildSequence: null
            };

            this.updateBeat(beatId, updates);
        } catch (error) {
            console.error('Failed to move beat:', error);
            throw error;
        }
    }        
};
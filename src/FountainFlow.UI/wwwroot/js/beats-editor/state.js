// state.js

/**
 * Constants for state management
 */
const STATE_DEFAULTS = {
    MIN_PERCENT: 0,
    MAX_PERCENT: 100,
    MAX_DEPTH: 3
};

/**
 * BeatState manages the application state for the beat editor
 */
export const BeatState = {
    // Core state properties remain the same
    originalState: [],
    currentState: [],
    isDirty: false,
    selectedBeatId: null,

    // Initialization remains largely the same
    initialize(beats) {
        if (!Array.isArray(beats)) {
            throw new Error('Beats must be an array');
        }
        this.originalState = JSON.parse(JSON.stringify(beats));
        this.currentState = JSON.parse(JSON.stringify(beats));
        this.isDirty = false;
        this.selectedBeatId = null;
    },

    /**
     * Get the current state
     * @returns {Array} Current beats state
     */
    getCurrentState() {
        return this.currentState;
    },

    /**
     * Get a beat by ID
     * @param {string} beatId - ID of the beat to retrieve
     * @returns {Object|null} Beat object or null if not found
     */
    getBeatById(beatId) {
        return this.currentState.find(beat => beat.id === beatId) || null;
    },

    /**
     * Get the selected beat
     * @returns {Object|null} Currently selected beat or null
     */
    getSelectedBeat() {
        return this.currentState.find(beat => beat.id === this.selectedBeatId) || null;
    },

    /**
     * Update dirty state and trigger UI updates
     * @param {boolean} isDirty - New dirty state
     */
    setDirty(isDirty) {
        this.isDirty = isDirty;
        this.notifyDirtyStateChange();
    },

    /**
     * Check if there are unsaved changes
     * @returns {boolean} True if there are unsaved changes
     */
    hasUnsavedChanges() {
        return this.isDirty;
    },

    /**
     * Add a new beat to the state
     * @param {Object} beat - Beat object to add
     * @returns {Object} Added beat
     */
    addBeat(beat) {
        if (!this.validateBeat(beat)) {
            throw new Error('Invalid beat object');
        }

        // Set default sequence values
        beat.parentSequence = this.getNextParentSequence();
        beat.childSequence = null;
        beat.grandchildSequence = null;

        this.currentState.push(beat);
        this.setDirty(true);
        return beat;
    },
    getNextParentSequence() {

        const currentBeats = this.currentState;

        // Increment all existing beats' parent sequences
        currentBeats.forEach(beat => {
            if (!beat.childSequence) {  // Only adjust parent beats
                beat.parentSequence += 1;
            }
        });

        return 1;  // New beat will always be sequence 1
    },
    getNextChildSequence(parentSequence) {
        const siblings = this.currentState.filter(
            b => b.parentSequence === parentSequence && b.childSequence !== null
        );
        return siblings.length > 0
            ? Math.max(...siblings.map(b => b.childSequence)) + 1
            : 1;
    },
    getNextGrandchildSequence(parentSequence, childSequence) {
        const siblings = this.currentState.filter(
            b => b.parentSequence === parentSequence &&
                b.childSequence === childSequence &&
                b.grandchildSequence !== null
        );
        return siblings.length > 0
            ? Math.max(...siblings.map(b => b.grandchildSequence)) + 1
            : 1;
    },
    /**
     * Update an existing beat
     * @param {string} beatId - ID of beat to update
     * @param {Object} updates - Properties to update
     * @returns {Object|null} Updated beat or null if not found
     */
    updateBeat(beatId, updates) {
        const index = this.currentState.findIndex(b => b.id === beatId);
        if (index === -1) return null;

        const updatedBeat = {
            ...this.currentState[index],
            ...updates,
            percentOfStory: this.validatePercent(updates.percentOfStory)
        };

        if (!this.validateBeat(updatedBeat)) {
            throw new Error('Invalid beat update');
        }

        this.currentState[index] = updatedBeat;
        this.setDirty(true);
        return updatedBeat;
    },

    /**
     * Delete a beat from the state
     * @param {string} beatId - ID of beat to delete
     * @returns {boolean} True if beat was deleted
     */
    deleteBeat(beatId) {
        const index = this.currentState.findIndex(b => b.id === beatId);
        if (index === -1) return false;

        this.currentState.splice(index, 1);
        this.updateSequences();
        this.setDirty(true);

        if (this.selectedBeatId === beatId) {
            this.clearSelection();
        }

        return true;
    },

    /**
     * Update the sequence numbers for all beats
     */
    updateSequences() {
        this.currentState.forEach((beat, index) => {
            beat.sequence = index + 1;
        });
    },

    /**
     * Reorder beats based on new sequence
     * @param {Array} newOrder - Array of beat IDs in new order
     */
    reorderBeats(nestableData) {
        // Recursively process nestable data and update sequences
        this.updateSequencesFromNestable(nestableData);
        this.setDirty(true);
    },

    updateSequencesFromNestable(items, parentSequence = null, childSequence = null) {
        console.log('Processing items:', items, 'parentSeq:', parentSequence, 'childSeq:', childSequence);  // Debug log

        items.forEach((item, index) => {
            const beat = this.getBeatById(item.id);
            if (!beat) return;

            if (!parentSequence) {
                // Top level
                beat.parentSequence = index + 1;
                beat.childSequence = null;
                beat.grandchildSequence = null;
            } else if (!childSequence) {
                // Second level
                beat.parentSequence = parentSequence;
                beat.childSequence = index + 1;
                beat.grandchildSequence = null;
            } else {
                // Third level
                beat.parentSequence = parentSequence;
                beat.childSequence = childSequence;
                beat.grandchildSequence = index + 1;
            }

            console.log('Updated beat:', { ...beat });  // Debug log

            // Process children if they exist
            if (item.children && item.children.length > 0) {
                if (!parentSequence) {
                    // If we're at top level, process children as second level
                    this.updateSequencesFromNestable(item.children, beat.parentSequence);
                } else if (!childSequence) {
                    // If we're at second level, process children as third level
                    this.updateSequencesFromNestable(
                        item.children,
                        beat.parentSequence,
                        beat.childSequence
                    );
                }
                // No need to process children at third level as we don't allow a fourth level
            }
        });
    },

    /**
     * Set the selected beat
     * @param {string|null} beatId - ID of beat to select, or null to clear
     */
    setSelectedBeat(beatId) {
        this.selectedBeatId = beatId;
        this.notifySelectionChange();

        // Log the selected beat details for debugging
        const selectedBeat = this.getSelectedBeat();
        console.log('Selected Beat:', selectedBeat);
    },

    /**
     * Clear the current beat selection
     */
    clearSelection() {
        this.selectedBeatId = null;
        this.notifySelectionChange();
    },

    /**
     * Validate a beat object
     * @param {Object} beat - Beat to validate
     * @returns {boolean} True if beat is valid
     */
    validateBeat(beat) {
        if (!beat) return false;

        const basicValidation = (
            typeof beat.id === 'string' &&
            typeof beat.name === 'string' &&
            typeof beat.description === 'string' &&
            typeof beat.percentOfStory === 'number' &&
            beat.percentOfStory >= STATE_DEFAULTS.MIN_PERCENT &&
            beat.percentOfStory <= STATE_DEFAULTS.MAX_PERCENT
        );

        if (!basicValidation) return false;

        // Validate sequence hierarchy
        if (typeof beat.parentSequence !== 'number' || beat.parentSequence < 1) {
            return false;
        }

        if (beat.childSequence !== null &&
            (typeof beat.childSequence !== 'number' || beat.childSequence < 1)) {
            return false;
        }

        if (beat.grandchildSequence !== null &&
            (typeof beat.grandchildSequence !== 'number' || beat.grandchildSequence < 1)) {
            return false;
        }

        return true;
    },

    /**
     * Validate and constrain a percent value
     * @param {number} percent - Percent value to validate
     * @returns {number} Validated and constrained percent value
     */
    validatePercent(percent) {
        const num = parseInt(percent) || 0;
        return Math.min(Math.max(num, STATE_DEFAULTS.MIN_PERCENT), STATE_DEFAULTS.MAX_PERCENT);
    },

    /**
     * Get changes for saving
     * @returns {Object} Object containing changes to save
     */
    getChanges() {
        return {
            archetypeId: window.archetypeId,
            beats: this.currentState.map(beat => ({
                id: beat.id,
                name: beat.name,
                description: beat.description,
                prompt: beat.prompt,
                parentSequence: beat.parentSequence,
                childSequence: beat.childSequence,
                grandchildSequence: beat.grandchildSequence,
                percentOfStory: beat.percentOfStory
            }))
        };
    },
    /**
     * Reset state to original
     */
    reset() {
        this.currentState = JSON.parse(JSON.stringify(this.originalState));
        this.isDirty = false;
        this.selectedBeatId = null;
    },

    // Event notification methods
    notifyDirtyStateChange() {
        const saveBtn = $('#saveChangesBtn');
        if (this.isDirty) {
            saveBtn.prop('disabled', false)
                .removeClass('btn-secondary')
                .addClass('btn-primary');
        } else {
            saveBtn.prop('disabled', true)
                .removeClass('btn-primary')
                .addClass('btn-secondary');
        }
    },

    notifySelectionChange() {
        const beat = this.getSelectedBeat();
        if (beat) {
            $('#noBeatSelected').addClass('d-none');
            $('#beatDetails').removeClass('d-none');
            $('#sequenceBadge').removeClass('d-none');

            $('#beatDetailsTitle').text(beat.name);
            $('#beatSequence').text(beat.sequence);
            $('#beatDescription').val(beat.description);
            $('#beatPercent').val(beat.percentOfStory);

            $('.dd-item').removeClass('selected');
            $(`.dd-item[data-id="${beat.id}"]`).addClass('selected');
        } else {
            $('#beatDetails').addClass('d-none');
            $('#noBeatSelected').removeClass('d-none');
            $('#sequenceBadge').addClass('d-none');
            $('#beatDetailsTitle').text('Beat Details');
            $('.dd-item').removeClass('selected');
        }
    }
};
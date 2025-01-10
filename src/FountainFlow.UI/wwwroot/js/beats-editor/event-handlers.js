// event-handlers.js
import { BeatState } from './state.js';
import { Renderers } from './renderers.js';

/**
 * Event handler configurations
 */
const HANDLERS = {
    KEYCODE: {
        ENTER: 13,
        ESCAPE: 27
    },
    
    SELECTORS: {
        DELETE_BEAT: '.delete-beat',
        EDIT_NAME: '.edit-name',
        BEAT_ITEM: '.dd-item',
        BEAT_DESCRIPTION: '#beatDescription',
        BEAT_PERCENT: '#beatPercent'
    }
};

/**
 * Event Handler module for managing all UI interactions
 */
export const EventHandlers = {
    /**
     * Initialize all event handlers
     */
    initialize() {
        this.attachDeleteHandlers();
        this.attachEditHandlers();
        this.attachSelectionHandlers();
        this.attachDetailPanelHandlers();
        this.attachNavigationHandlers();
    },

    /**
     * Attach delete button handlers
     */
    attachDeleteHandlers() {
        $(document).on('click', HANDLERS.SELECTORS.DELETE_BEAT, (e) => {
            e.stopPropagation();
            const beatId = $(e.currentTarget).data('beat-id');

            if (confirm('Are you sure you want to delete this beat?')) {
                try {
                    BeatState.deleteBeat(beatId);
                    Renderers.renderBeats();
                } catch (error) {
                    console.error('Failed to delete beat:', error);
                    Renderers.showError('Failed to delete beat. Please try again.');
                }
            }
        });
    },

    /**
     * Attach name edit handlers
     */
    attachEditHandlers() {
        $(document).on('click', HANDLERS.SELECTORS.EDIT_NAME, (e) => {
            console.log("Edit button clicked");
            e.stopPropagation();
            const beatId = $(e.currentTarget).data('beat-id');
            const beatNameElement = $(e.currentTarget).siblings('.beat-name');
            const currentValue = beatNameElement.text();

            // Render edit controls
            Renderers.renderNameEditControls(beatNameElement, currentValue);
            
            const editControlsDiv = beatNameElement.next('.edit-name-controls');
            const input = editControlsDiv.find('input');
            const saveBtn = editControlsDiv.find('.save-name');
            const cancelBtn = editControlsDiv.find('.cancel-name');

            const handleSave = () => {
                const newValue = input.val().trim();
                if (newValue) {
                    try {
                        BeatState.updateBeat(beatId, { name: newValue });
                        Renderers.renderBeats();
                    } catch (error) {
                        console.error('Failed to update beat name:', error);
                        Renderers.showError('Failed to update beat name. Please try again.');
                    }
                }
            };

            saveBtn.on('click', handleSave);
            cancelBtn.on('click', () => Renderers.renderBeats());

            input.on('keydown', (e) => {
                if (e.keyCode === HANDLERS.KEYCODE.ENTER) {
                    e.preventDefault();
                    handleSave();
                } else if (e.keyCode === HANDLERS.KEYCODE.ESCAPE) {
                    Renderers.renderBeats();
                }
            });

            input.on('blur', (e) => {
                if (!$(e.relatedTarget).is(saveBtn) && !$(e.relatedTarget).is(cancelBtn)) {
                    Renderers.renderBeats();
                }
            });

            $(e.currentTarget).hide();
        });
    },

    /**
     * Attach beat selection handlers
     */
    attachSelectionHandlers() {
        $(document).on('click', HANDLERS.SELECTORS.BEAT_ITEM, function() {
            const beatId = $(this).data('id');

            try {
                BeatState.setSelectedBeat(beatId);
                const selectedBeat = BeatState.getSelectedBeat();
                if (selectedBeat) {
                    Renderers.renderDetailPanel(selectedBeat);
                }
            } catch (error) {
                console.error('Failed to select beat:', error);
                Renderers.showError('Failed to select beat. Please try again.');
            }
        });
    },

    /**
     * Attach detail panel input handlers
     */
    attachDetailPanelHandlers() {
        $(document).on('change', HANDLERS.SELECTORS.BEAT_DESCRIPTION, (e) => {
            const selectedBeat = BeatState.getSelectedBeat();
            if (selectedBeat) {
                try {
                    BeatState.updateBeat(selectedBeat.id, {
                        description: $(e.target).val()
                    });
                } catch (error) {
                    console.error('Failed to update description:', error);
                    Renderers.showError('Failed to update description. Please try again.');
                }
            }
        });

        $(document).on('change', HANDLERS.SELECTORS.BEAT_PERCENT, (e) => {
            const selectedBeat = BeatState.getSelectedBeat();
            if (selectedBeat) {
                try {
                    const value = BeatState.validatePercent($(e.target).val());
                    $(e.target).val(value);

                    BeatState.updateBeat(selectedBeat.id, {
                        percentOfStory: value
                    });
                } catch (error) {
                    console.error('Failed to update percent:', error);
                    Renderers.showError('Failed to update percent. Please try again.');
                }
            }
        });
    },

    /**
     * Attach navigation warning handlers
     */
    attachNavigationHandlers() {
        window.addEventListener('beforeunload', (e) => {
            if (BeatState.hasUnsavedChanges()) {
                e.preventDefault();
                e.returnValue = '';
            }
        });

        $(document).on('click', 'a', (e) => {
            if (BeatState.hasUnsavedChanges()) {
                e.preventDefault();
                const href = $(e.currentTarget).attr('href');

                if (confirm('You have unsaved changes. Are you sure you want to leave?')) {
                    window.location.href = href;
                }
            }
        });
    },

    /**
     * Remove all event handlers (cleanup)
     */
    cleanup() {
        $(document).off('click', HANDLERS.SELECTORS.DELETE_BEAT);
        $(document).off('click', HANDLERS.SELECTORS.EDIT_NAME);
        $(document).off('click', HANDLERS.SELECTORS.BEAT_ITEM);
        $(document).off('change', HANDLERS.SELECTORS.BEAT_DESCRIPTION);
        $(document).off('change', HANDLERS.SELECTORS.BEAT_PERCENT);
        window.removeEventListener('beforeunload', this.attachNavigationHandlers);
    }
};

// Export default event handlers object
export default EventHandlers;

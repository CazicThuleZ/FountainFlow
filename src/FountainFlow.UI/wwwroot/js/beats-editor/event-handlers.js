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
        BEAT_PROMPT: '#beatPrompt',
        BEAT_PERCENT: '#beatPercent',
        DISTRIBUTE_COVERAGE_BTN: '#distributeCoverageBtn'
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
        this.attachDistributeCoverageHandler();
        this.attachNestableHandlers();
    },

    attachNestableHandlers() {
        $('.dd').on('change', (e) => {
            const nestableData = $('.dd').nestable('serialize');
            BeatState.reorderBeats(nestableData);
            Renderers.refreshUI();
        });

        // Prevent nesting beyond maximum depth
        $('.dd').on('dragStart', (e, item, source, destination, position) => {
            const currentDepth = $(item).parents('.dd-list').length;
            if (currentDepth >= 3) {
                e.preventDefault();
                showToast('Warning', 'Maximum nesting depth reached', 'warning');
                return false;
            }
        });
    },    

    /**
     * Attach delete button handlers
     */
    attachDeleteHandlers() {
        $(document).on('click', HANDLERS.SELECTORS.DELETE_BEAT, (e) => {
            e.stopPropagation();
            const beatId = $(e.currentTarget).data('beat-id');
            const beat = BeatState.getBeatById(beatId);

            if (!beat) return;

            // Customize confirmation message based on beat's position
            let message = 'Are you sure you want to delete this beat?';
            if (beat.childSequence === null) {
                // Parent beat
                const hasChildren = BeatState.getCurrentState()
                    .some(b => b.parentSequence === beat.parentSequence && b.childSequence !== null);
                if (hasChildren) {
                    message = 'This will also delete all nested beats. Are you sure?';
                }
            }

            if (confirm(message)) {
                try {
                    this.deleteBeatAndDescendants(beatId);
                    Renderers.renderBeats();
                } catch (error) {
                    console.error('Failed to delete beat:', error);
                    Renderers.showError('Failed to delete beat. Please try again.');
                }
            }
        });
    },
    deleteBeatAndDescendants(beatId) {
        const beat = BeatState.getBeatById(beatId);
        if (!beat) return;

        // If this is a parent beat, delete all children first
        if (beat.childSequence === null) {
            const children = BeatState.getCurrentState()
                .filter(b => b.parentSequence === beat.parentSequence && b.childSequence !== null);
            
            children.forEach(child => this.deleteBeatAndDescendants(child.id));
        }
        // If this is a child beat, delete all grandchildren first
        else if (beat.grandchildSequence === null) {
            const grandchildren = BeatState.getCurrentState()
                .filter(b => 
                    b.parentSequence === beat.parentSequence && 
                    b.childSequence === beat.childSequence && 
                    b.grandchildSequence !== null
                );
            
            grandchildren.forEach(grandchild => BeatState.deleteBeat(grandchild.id));
        }

        // Delete the beat itself
        BeatState.deleteBeat(beatId);
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
        $(document).on('click', '.dd-item', function(e) {
            // Stop event from bubbling up to parent beats
            e.stopPropagation();
            
            const beatId = $(this).data('id');
            const currentTarget = $(this);
    
            // Don't trigger selection if clicking handle or delete button
            if ($(e.target).closest('.dd-handle, .delete-beat').length) {
                return;
            }
    
            try {
                // Remove selected state from all beats
                $('.dd-item').removeClass('selected');
                
                // Add selected state to clicked beat
                currentTarget.addClass('selected');
                
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
                  BeatState.setDirty(true);                    
                } catch (error) {
                    console.error('Failed to update description:', error);
                    Renderers.showError('Failed to update description. Please try again.');
                }
            }
        });

        // Add handler for prompt field
        $(document).on('change', HANDLERS.SELECTORS.BEAT_PROMPT, (e) => {
            const selectedBeat = BeatState.getSelectedBeat();
            if (selectedBeat) {
                try {
                    BeatState.updateBeat(selectedBeat.id, {
                        prompt: $(e.target).val()
                    });
                } catch (error) {
                    console.error('Failed to update prompt:', error);
                    Renderers.showError('Failed to update prompt. Please try again.');
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
    attachDistributeCoverageHandler() {
        $(document).on('click', HANDLERS.SELECTORS.DISTRIBUTE_COVERAGE_BTN, () => {
            const beats = BeatState.getCurrentState();
            if (!beats || beats.length === 0) {
                console.log("No beats available to distribute coverage.");
                return;
            }

            // Calculate distribution by level
            const distributeByLevel = (levelBeats) => {
                const evenValue = Math.floor(100 / levelBeats.length);
                const remainder = 100 % levelBeats.length;
                
                levelBeats.forEach((beat, index) => {
                    const value = index === levelBeats.length - 1 ? 
                        evenValue + remainder : 
                        evenValue;
                    
                    BeatState.updateBeat(beat.id, { percentOfStory: value });
                });
            };

            // Get top-level beats
            const topLevelBeats = beats.filter(b => !b.childSequence);
            distributeByLevel(topLevelBeats);

            BeatState.setDirty(true);
            Renderers.refreshUI();
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
        $(document).off('change', HANDLERS.SELECTORS.BEAT_PROMPT);
        $(document).off('change', HANDLERS.SELECTORS.BEAT_PERCENT);
        $('.dd').off('change');
        $('.dd').off('dragStart');
        window.removeEventListener('beforeunload', this.attachNavigationHandlers);
    }
};

// Export default event handlers object
export default EventHandlers;

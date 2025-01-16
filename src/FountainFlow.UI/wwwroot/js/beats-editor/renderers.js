// renderers.js
import { BeatState } from './state.js';

/**
 * Constants for rendering
 */
const TEMPLATES = {
    beatItem: (beat, isSelected) => `
        <li class="dd-item ${isSelected ? 'selected' : ''}" data-id="${beat.id}">
            <div class="dd-handle">
                <i class="fas fa-grip-vertical"></i>
            </div>
            <div class="dd-content d-flex justify-content-between align-items-center">
                <div class="d-flex align-items-center">
                    <div class="beat-name">${beat.name}</div>
                    <button class="btn btn-link btn-xs edit-name ms-2" data-beat-id="${beat.id}">
                        <i class="fa fa-pencil"></i>
                    </button>
                </div>
                <div class="d-flex align-items-center justify-content-end" style="gap: 10px; min-width: 70px;">
                    <button class="btn btn-danger btn-xs delete-beat" data-beat-id="${beat.id}">
                        <i class="fa fa-trash"></i>
                    </button>
                    <span id="beatPercentValue-${beat.id}" class="font-bold text-end">${beat.percentOfStory || 0}%</span>
                </div>
            </div>
        </li>
    `,

    editNameControls: (currentValue) => `
        <div class="d-flex align-items-center edit-name-controls">
            <input type="text" 
                   class="form-control form-control-sm" 
                   value="${currentValue}">
            <button class="btn btn-primary btn-xs ms-2 save-name">
                <i class="fa fa-check"></i>
            </button>
            <button class="btn btn-secondary btn-xs ms-1 cancel-name">
                <i class="fa fa-times"></i>
            </button>
        </div>
    `,

    detailPanel: (beat) => `
        <div class="form-group">
            <label class="font-bold">Description</label>
            <textarea id="beatDescription" class="form-control" rows="3">${beat.description || ''}</textarea>
        </div>
        <div class="form-group mt-3">
            <label class="font-bold">Prompt</label>
            <textarea id="beatPrompt" class="form-control" rows="3">${beat.prompt || ''}</textarea>
        </div>
        <div class="form-group mt-3">
            <label class="font-bold">Percent of Story</label>
            <input type="text" id="beatPercentKnob" value="${beat.percentOfStory || 0}" class="dial">
            <input type="hidden" id="beatPercent" name="beatPercent" value="${beat.percentOfStory || 0}">
        </div>
    `
};

/**
 * Main rendering functions
 */
export const Renderers = {
    /**
     * Render the complete beats list
     */
    renderBeats() {
        const container = $('.beats-container');
        container.empty();
    
        const nestableList = this.createNestableList();
        container.append(nestableList);
    
        this.initializeNestable();
    
        // If there's a newly added beat (it will have a temp_ id)
        const newBeat = BeatState.getCurrentState().find(b => b.id.startsWith('temp_'));
        if (newBeat) {
            // Scroll container to top
            container.scrollTop(0);
            
            // Optional: highlight the new beat briefly
            const newBeatElement = $(`.dd-item[data-id="${newBeat.id}"]`);
            newBeatElement.addClass('highlight-new');
            setTimeout(() => {
                newBeatElement.removeClass('highlight-new');
            }, 1500);
        }
    },

    /**
     * Create the nestable list structure
     * @returns {jQuery} Nestable list jQuery object
     */
    createNestableList() {
        const nestableList = $('<div>')
            .addClass('dd')
            .attr('id', 'beats-list');

        const beatsList = this.createBeatsHierarchy();
        return nestableList.append(beatsList);
    },

    createBeatsHierarchy() {
        const rootList = $('<ol>').addClass('dd-list');
        const beats = BeatState.getCurrentState();

        // Get parent beats (no child sequence)
        const parentBeats = beats
            .filter(b => !b.childSequence)
            .sort((a, b) => a.parentSequence - b.parentSequence);

        parentBeats.forEach(parentBeat => {
            const parentItem = $(TEMPLATES.beatItem(parentBeat, parentBeat.id === BeatState.selectedBeatId));

            // Find and add child beats
            const childBeats = beats
                .filter(b => b.parentSequence === parentBeat.parentSequence && b.childSequence && !b.grandchildSequence)
                .sort((a, b) => a.childSequence - b.childSequence);

            if (childBeats.length > 0) {
                const childList = $('<ol>').addClass('dd-list');
                childBeats.forEach(childBeat => {
                    const childItem = $(TEMPLATES.beatItem(childBeat, childBeat.id === BeatState.selectedBeatId));

                    // Find and add grandchild beats
                    const grandchildBeats = beats
                        .filter(b => 
                            b.parentSequence === parentBeat.parentSequence && 
                            b.childSequence === childBeat.childSequence && 
                            b.grandchildSequence
                        )
                        .sort((a, b) => a.grandchildSequence - b.grandchildSequence);

                    if (grandchildBeats.length > 0) {
                        const grandchildList = $('<ol>').addClass('dd-list');
                        grandchildBeats.forEach(grandchildBeat => {
                            grandchildList.append(TEMPLATES.beatItem(grandchildBeat, grandchildBeat.id === BeatState.selectedBeatId));
                        });
                        childItem.append(grandchildList);
                    }

                    childList.append(childItem);
                });
                parentItem.append(childList);
            }

            rootList.append(parentItem);
        });

        return rootList;
    },    

    /**
     * Initialize the Nestable2 plugin
     */
    initializeNestable() {
        $('#beats-list').nestable({
            maxDepth: 3,
            group: 1,
            handleClass: 'dd-handle',
            callback: (l, e) => {
                const items = $('#beats-list').nestable('serialize');
                console.log('Nestable structure before processing:', items);
                BeatState.reorderBeats(items);
                console.log('Current state after reorder:', BeatState.getCurrentState());
                this.renderBeats();
            },
            beforeDragStop: (l, e, p) => {
                // Log information about the drop
                console.log('Drop target:', p);
                return true; // Allow the drop
            }
        });
    },

    /**
     * Render the name editing controls
     * @param {jQuery} beatNameElement - Element containing the beat name
     * @param {string} currentValue - Current name value
     */
    renderNameEditControls(beatNameElement, currentValue) {
        const controls = $(TEMPLATES.editNameControls(currentValue));
        beatNameElement.hide().after(controls);

        // Focus the input and select its content
        const input = controls.find('input');
        input.focus().select();
    },

    /**
     * Render the detail panel for a beat
     * @param {Object} beat - Beat object to render details for
     */
    renderDetailPanel(beat) {
        if (!beat) {
            this.renderEmptyDetailPanel();
            return;
        }

        this.updateDetailPanelHeader(beat);
        
        const detailsContainer = $('#beatDetails');
        detailsContainer.html(TEMPLATES.detailPanel(beat));

        $('#beatPercentKnob').knob({
            'min': 0,
            'max': 100,
            'width': 120,
            'height': 120,
            'thickness': 0.3,
            'fgColor': "#1AB394",
            'bgColor': "#EEEEEE",
            'cursor': true,
            'release': (value) => {
                $('#beatPercent').val(value);
                BeatState.updateBeat(beat.id, { percentOfStory: value });
                $(`#beatPercentValue-${beat.id}`).text(`${value}%`);
                this.updateDynamicKnobLabel();
            }
        });

        this.updateDynamicKnobLabel();
        $('#noBeatSelected').addClass('d-none');
        detailsContainer.removeClass('d-none');
    },

    /**
     * Update the detail panel header
     * @param {Object} beat - Beat object
     */
    updateDetailPanelHeader(beat) {
        $('#beatDetailsTitle').text(beat.name);

        let sequenceText = `${beat.parentSequence}`;
        if (beat.childSequence) {
            sequenceText += `.${beat.childSequence}`;
            if (beat.grandchildSequence) {
                sequenceText += `.${beat.grandchildSequence}`;
            }
        }

        const sequenceBadge = $('#sequenceBadge');
        $('#beatSequence').text(sequenceText);
        sequenceBadge.removeClass('d-none');
    },
    updateDynamicKnobLabel() {
        const beats = BeatState.getCurrentState(); // Retrieve all beats
        const totalPercent = beats.reduce((sum, beat) => sum + (beat.percentOfStory || 0), 0);

        const label = $('#dynamicKnobLabel');
        label.text(`${totalPercent}%`);

        // Update label styling based on the totalPercent
        if (totalPercent === 100) {
            label.removeClass('text-danger').addClass('text-primary'); // Green
        } else {
            label.removeClass('text-primary').addClass('text-danger'); // Red
        }
    },
    /**
     * Render empty detail panel
     */
    renderEmptyDetailPanel() {
        $('#beatDetails').addClass('d-none');
        $('#sequenceBadge').addClass('d-none');
        $('#noBeatSelected').removeClass('d-none');
        $('#beatDetailsTitle').text('Beat Details');
    },

    /**
     * Show loading state
     * @param {jQuery} element - Element to show loading state on
     */
    showLoading(element) {
        element.prop('disabled', true)
            .html('<i class="fas fa-spinner fa-spin me-1"></i> Loading...');
    },

    /**
     * Hide loading state
     * @param {jQuery} element - Element to hide loading state on
     * @param {string} originalHtml - Original HTML to restore
     */
    hideLoading(element, originalHtml) {
        element.prop('disabled', false)
            .html(originalHtml);
    },

    /**
     * Show an error message
     * @param {string} message - Error message to display
     */
    showError(message) {
        console.error(message);
    },

    /**
     * Refresh the entire UI
     */
    refreshUI() {
        this.renderBeats();
        const selectedBeat = BeatState.getSelectedBeat();
        if (selectedBeat) {
            this.renderDetailPanel(selectedBeat);
        } else {
            this.renderEmptyDetailPanel();
        }

        this.updateDynamicKnobLabel();
    }
};

// Export default renderers object
export default Renderers;
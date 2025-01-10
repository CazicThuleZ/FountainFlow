// renderers.js
import { BeatState } from './state.js';
import { EventHandlers } from './event-handlers.js';

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
`
    ,

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
    <div class="form-group d-flex align-items-start">
        <!-- Beat Description: 80% width -->
        <div class="flex-grow-1 me-3" style="width: 80%;">
            <label class="font-bold">Prompt</label>
            <textarea id="beatDescription" 
                      class="form-control" 
                      rows="10">${beat.description || ''}</textarea>
        </div>
        
        <!-- Knob and Label: 20% width -->
        <div style="width: 20%; text-align: center;">
            <label for="beatPercentKnob" class="d-block font-bold mt-2">Percent of Story</label>        
            <input type="text" id="beatPercentKnob" value="${beat.percentOfStory || 0}" class="dial">
            <label for="beatPercentKnob" class="d-block font-bold mt-2">Coverage</label>
            <span id="dynamicKnobLabel" class="d-block mt-2 font-bold" style="font-size: 3em; line-height: 1;">0%</span>
            <input type="hidden" id="beatPercent" name="beatPercent" value="${beat.percentOfStory || 0}">
        </div>
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
    },

    /**
     * Create the nestable list structure
     * @returns {jQuery} Nestable list jQuery object
     */
    createNestableList() {
        const nestableList = $('<div>')
            .addClass('dd')
            .attr('id', 'beats-list');

        const beatsList = $('<ol>').addClass('dd-list');

        BeatState.getCurrentState()
            .sort((a, b) => a.sequence - b.sequence)
            .forEach(beat => {
                const isSelected = beat.id === BeatState.selectedBeatId;
                const beatItem = $(TEMPLATES.beatItem(beat, isSelected));
                beatsList.append(beatItem);
            });

        return nestableList.append(beatsList);
    },

    /**
     * Initialize the Nestable2 plugin
     */
    initializeNestable() {
        $('#beats-list').nestable({
            maxDepth: 1,
            handleClass: 'dd-handle',
            callback: (l, e) => {
                const items = $('#beats-list').nestable('serialize');
                const newOrder = items.map(item => item.id);
                BeatState.reorderBeats(newOrder);
                this.renderBeats();
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

        // Update title and sequence badge
        this.updateDetailPanelHeader(beat);

        // Update main content
        const detailsContainer = $('#beatDetails');
        detailsContainer.html(TEMPLATES.detailPanel(beat));

        // Initialize the knob for #beatPercentKnob
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
                console.log("Knob value: " + value);
                $('#beatPercent').val(value); // Update the hidden field
                const selectedBeat = BeatState.getSelectedBeat();
                if (selectedBeat) {
                    BeatState.updateBeat(selectedBeat.id, { percentOfStory: value });
                    $(`#beatPercentValue-${selectedBeat.id}`).text(`${value}%`); // Update the list
                }

                this.updateDynamicKnobLabel();
            }
        });

        this.updateDynamicKnobLabel();

        // Show/hide appropriate sections
        $('#noBeatSelected').addClass('d-none');
        detailsContainer.removeClass('d-none');
    },

    /**
     * Update the detail panel header
     * @param {Object} beat - Beat object
     */
    updateDetailPanelHeader(beat) {
        $('#beatDetailsTitle').text(beat.name);

        const sequenceBadge = $('#sequenceBadge');
        $('#beatSequence').text(beat.sequence);
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
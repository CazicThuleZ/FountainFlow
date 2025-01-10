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
                <button class="btn btn-danger btn-xs delete-beat" data-beat-id="${beat.id}">
                    <i class="fa fa-trash"></i>
                </button>
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
            <textarea id="beatDescription" 
                      class="form-control" 
                      rows="3">${beat.description || ''}</textarea>
        </div>
        <div class="form-group mt-3">
            <label class="font-bold">Percent of Story</label>
            <input type="number" 
                   id="beatPercent" 
                   class="form-control" 
                   min="0" 
                   max="100" 
                   value="${beat.percentOfStory || 0}">
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
    }
};

// Export default renderers object
export default Renderers;
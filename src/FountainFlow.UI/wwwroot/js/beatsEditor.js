// beatsEditor.js
const BeatEditor = {
    originalState: [], // Store initial state
    currentState: [], // Store current state
    isDirty: false,   // Track if changes exist

    initialize(beats) {
        // Deep clone the initial beats array to both states
        this.originalState = JSON.parse(JSON.stringify(beats));
        this.currentState = JSON.parse(JSON.stringify(beats));
        this.isDirty = false;
        this.renderBeats();
    },

    // Check if there are unsaved changes
    hasUnsavedChanges() {
        return this.isDirty;
    },

    // Mark state as dirty and update save button
    setDirty(isDirty) {
        this.isDirty = isDirty;
        const saveBtn = $('#saveChangesBtn');
        if (isDirty) {
            saveBtn.prop('disabled', false)
                .removeClass('btn-secondary')
                .addClass('btn-primary');
        } else {
            saveBtn.prop('disabled', true)
                .removeClass('btn-primary')
                .addClass('btn-secondary');
        }
    },

    // Add a new beat
    addBeat() {
        const newBeat = {
            id: 'temp_' + Date.now(), // Temporary ID for new beats
            name: 'New Beat',
            description: 'Beat description',
            sequence: this.currentState.length + 1,
            percentOfStory: 0,
            archetypeId: archetypeId // This should be available from your view
        };

        this.currentState.push(newBeat);
        this.setDirty(true);
        this.renderBeats();
    },

    // Update a beat
    updateBeat(beatId, updates) {
        const index = this.currentState.findIndex(b => b.id === beatId);
        if (index !== -1) {
            this.currentState[index] = { ...this.currentState[index], ...updates };
            this.setDirty(true);
            this.renderBeats();
        }
    },

    // Delete a beat
    deleteBeat(beatId) {
        const index = this.currentState.findIndex(b => b.id === beatId);
        if (index !== -1) {
            this.currentState.splice(index, 1);
            // Update sequences for remaining beats
            this.currentState.forEach((beat, idx) => {
                beat.sequence = idx + 1;
            });
            this.setDirty(true);
            this.renderBeats();
        }
    },

    renderBeats() {
        const container = $('.beats-container');
        container.empty();

        // Create the nestable list structure
        const nestableList = $('<div>')
            .addClass('dd')
            .attr('id', 'beats-list');

        const beatsList = $('<ol>').addClass('dd-list');

        this.currentState
            .sort((a, b) => a.sequence - b.sequence)
            .forEach(beat => {
                const beatItem = $(`
                    <li class="dd-item" data-id="${beat.id}">
                        <div class="dd-handle">
                            <i class="fas fa-grip-vertical"></i>
                        </div>
                        <div class="dd-content">
                            <div class="ibox mb-0" style="min-width: 1400px;">
                                <div class="ibox-title">
                                    <div class="row align-items-center">
                                        <div class="col">
                                            <div class="editable-field">
                                                <h5 class="m-0 d-flex align-items-center">
                                                    <span class="editable-text" data-field="name">${beat.name}</span>
                                                    <button class="btn btn-link btn-xs edit-field ms-2" data-field="name">
                                                        <i class="fa fa-pencil"></i>
                                                    </button>
                                                </h5>
                                            </div>
                                        </div>
                                        <div class="col-auto">
                                            <button class="btn btn-danger btn-xs delete-beat" data-beat-id="${beat.id}">
                                                <i class="fa fa-trash"></i> Delete
                                            </button>
                                        </div>
                                    </div>
                                </div>
                                <div class="ibox-content">
                                    <div class="editable-field mb-3">
                                        <p class="editable-text mb-0" data-field="description">${beat.description}</p>
                                        <button class="btn btn-link btn-xs edit-field" data-field="description">
                                            <i class="fa fa-pencil"></i>
                                        </button>
                                    </div>
                                    <div class="row text-muted">
                                        <div class="col-6">
                                            <small>Sequence: ${beat.sequence}</small>
                                        </div>
                                        <div class="col-6">
                                            <div class="editable-field text-end">
                                                <small class="editable-text" data-field="percentOfStory">${beat.percentOfStory}% of Story</small>
                                                <button class="btn btn-link btn-xs edit-field" data-field="percentOfStory">
                                                    <i class="fa fa-pencil"></i>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </li>
                `);
                beatsList.append(beatItem);
            });

        nestableList.append(beatsList);
        container.append(nestableList);

        // Initialize Nestable2
        $('#beats-list').nestable({
            maxDepth: 1,
            handleClass: 'dd-handle',
            callback: (l, e) => {
                const items = $('#beats-list').nestable('serialize');
                this.updateSequences(items);
            }
        });

        this.attachEventHandlers();
    },

    // Attach event handlers
    attachEventHandlers() {
        // Delete beat handler
        $('.delete-beat').on('click', (e) => {
            const beatId = $(e.currentTarget).data('beat-id');

            // Confirm deletion
            if (confirm('Are you sure you want to delete this beat?')) {
                this.deleteBeat(beatId);
            }
        });

        // Edit field handler
        $('.edit-field').on('click', (e) => {
            e.preventDefault();
            const button = $(e.currentTarget);
            const field = button.data('field');
            const beatItem = button.closest('.dd-item');
            const beatId = beatItem.data('id');
            const textElement = button.closest('.editable-field').find('.editable-text');
            const currentValue = this.currentState.find(b => b.id === beatId)[field];

            // Create input based on field type
            let input;
            if (field === 'description') {
                input = $('<textarea>')
                    .addClass('form-control')
                    .val(currentValue)
                    .css('min-height', '100px');
            } else if (field === 'percentOfStory') {
                input = $('<input>')
                    .attr('type', 'number')
                    .addClass('form-control')
                    .attr('min', '0')
                    .attr('max', '100')
                    .val(currentValue);
            } else {
                input = $('<input>')
                    .attr('type', 'text')
                    .addClass('form-control')
                    .val(currentValue);
            }

            // Add save and cancel buttons
            const controls = $('<div>')
                .addClass('mt-2')
                .append(
                    $('<button>')
                        .addClass('btn btn-primary btn-sm me-2 save-edit')
                        .html('<i class="fa fa-check"></i> Save'),
                    $('<button>')
                        .addClass('btn btn-default btn-sm cancel-edit')
                        .html('<i class="fa fa-times"></i> Cancel')
                );

            // Replace text with input and controls
            textElement.hide()
                .after(input)
                .after(controls);
            button.hide();

            // Handle save
            controls.find('.save-edit').on('click', () => {
                const newValue = input.val();
                let processedValue = newValue;

                if (field === 'percentOfStory') {
                    processedValue = Math.min(Math.max(parseInt(newValue) || 0, 0), 100);
                }

                this.updateBeat(beatId, { [field]: processedValue });
            });

            // Handle cancel
            controls.find('.cancel-edit').on('click', () => {
                this.renderBeats();
            });

            // Focus the input
            input.focus();
        });
    },

    // Get changes for saving
    getChanges() {
        return {
            archetypeId: archetypeId,
            beats: this.currentState
        };
    },

    // Update sequences after drag and drop
    updateSequences(items) {

        const newOrder = items.map(item => item.id);
        this.currentState.sort((a, b) => {
            return newOrder.indexOf(a.id) - newOrder.indexOf(b.id);
        });
        
        this.currentState.forEach((beat, index) => {
            beat.sequence = index + 1;
        });

        this.setDirty(true);
        this.renderBeats();
    }
};

// Export the BeatEditor object
window.BeatEditor = BeatEditor;
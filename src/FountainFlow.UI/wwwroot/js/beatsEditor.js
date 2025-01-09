// beatsEditor.js
const BeatEditor = {
    originalState: [],
    currentState: [],
    isDirty: false,
    selectedBeatId: null,

    initialize(beats) {
        this.originalState = JSON.parse(JSON.stringify(beats));
        this.currentState = JSON.parse(JSON.stringify(beats));
        this.isDirty = false;
        this.selectedBeatId = null;
        this.renderBeats();
        this.initializeDetailHandlers();
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
        this.selectBeat(newBeat.id); // Select the newly added beat
    },

    // Update a beat
    updateBeat(beatId, updates) {
        const index = this.currentState.findIndex(b => b.id === beatId);
        if (index !== -1) {
            this.currentState[index] = { ...this.currentState[index], ...updates };
            this.setDirty(true);
            this.renderBeats();

            // If this beat is currently selected, refresh the detail panel
            if (this.selectedBeatId === beatId) {
                this.selectBeat(beatId);
            }
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
            if (this.selectedBeatId === beatId) {
                this.clearSelection();
            }
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
                    <li class="dd-item ${beat.id === this.selectedBeatId ? 'selected' : ''}" data-id="${beat.id}">
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

    selectBeat(beatId) {
        this.selectedBeatId = beatId;
        const beat = this.currentState.find(b => b.id === beatId);
        
        if (beat) {
            $('#noBeatSelected').addClass('d-none');
            $('#beatDetails').removeClass('d-none');
            $('#sequenceBadge').removeClass('d-none');
            
            // Update the panel title and sequence
            $('#beatDetailsTitle').text(beat.name);
            $('#beatSequence').text(beat.sequence);
            
            // Update the detail fields
            $('#beatDescription').val(beat.description);
            $('#beatPercent').val(beat.percentOfStory);
            
            // Update visual selection
            $('.dd-item').removeClass('selected');
            $(`.dd-item[data-id="${beatId}"]`).addClass('selected');
        } else {
            this.clearSelection();
        }
    },
    
    clearSelection() {
        this.selectedBeatId = null;
        $('#beatDetails').addClass('d-none');
        $('#noBeatSelected').removeClass('d-none');
        $('#sequenceBadge').addClass('d-none');
        $('.dd-item').removeClass('selected');
        $('#beatDetailsTitle').text('Beat Details');
    },

    initializeDetailHandlers() {
        // Handle Description changes
        $('#beatDescription').on('change', (e) => {
            if (this.selectedBeatId) {
                this.updateBeat(this.selectedBeatId, {
                    description: $(e.target).val()
                });
            }
        });

        // Handle Percent changes
        $('#beatPercent').on('change', (e) => {
            if (this.selectedBeatId) {
                const value = Math.min(Math.max(parseInt($(e.target).val()) || 0, 0), 100);
                $(e.target).val(value); // Update input with constrained value
                this.updateBeat(this.selectedBeatId, {
                    percentOfStory: value
                });
            }
        });
    },

    // Attach event handlers
    attachEventHandlers() {
        // Delete beat handler
        $('.delete-beat').on('click', (e) => {
            e.stopPropagation(); // Prevent beat selection when deleting
            const beatId = $(e.currentTarget).data('beat-id');

            if (confirm('Are you sure you want to delete this beat?')) {
                this.deleteBeat(beatId);
            }
        });

        // Edit name handler
        $('.edit-name').on('click', (e) => {
            e.stopPropagation(); // Prevent beat selection
            const beatId = $(e.currentTarget).data('beat-id');
            const beatNameElement = $(e.currentTarget).siblings('.beat-name');
            const currentValue = beatNameElement.text();

            const input = $('<input>')
                .attr('type', 'text')
                .addClass('form-control form-control-sm')
                .val(currentValue);

            const saveBtn = $('<button>')
                .addClass('btn btn-primary btn-xs ms-2')
                .html('<i class="fa fa-check"></i>');

            const cancelBtn = $('<button>')
                .addClass('btn btn-secondary btn-xs ms-1')
                .html('<i class="fa fa-times"></i>');

            const buttonGroup = $('<div>')
                .addClass('d-flex align-items-center')
                .append(input)
                .append(saveBtn)
                .append(cancelBtn);

            beatNameElement.hide()
                .after(buttonGroup);
            $(e.currentTarget).hide();
            input.focus();

            // Handle save
            saveBtn.on('click', () => {
                const newValue = input.val().trim();
                if (newValue) {
                    this.updateBeat(beatId, { name: newValue });
                }
            });

            // Handle cancel
            cancelBtn.on('click', () => {
                this.renderBeats();
            });

            // Handle enter key
            input.on('keypress', (e) => {
                if (e.which === 13) {
                    e.preventDefault();
                    const newValue = input.val().trim();
                    if (newValue) {
                        this.updateBeat(beatId, { name: newValue });
                    }
                }
            });

            // Handle escape key
            input.on('keyup', (e) => {
                if (e.which === 27) {
                    this.renderBeats();
                }
            });

            // Handle blur if clicking outside
            input.on('blur', (e) => {
                // Only trigger if not clicking the save or cancel buttons
                if (!$(e.relatedTarget).is(saveBtn) && !$(e.relatedTarget).is(cancelBtn)) {
                    this.renderBeats();
                }
            });
        });

        // Beat selection handler
        $('.dd-item').on('click', (e) => {
            const beatId = $(e.currentTarget).data('id');
            this.selectBeat(beatId);
        });
    },

    // Get changes for saving
    getChanges() {
        return {
            archetypeId: archetypeId,
            beats: this.currentState.map(beat => ({
                id: beat.id,
                name: beat.name,
                description: beat.description,
                sequence: beat.sequence,
                percentOfStory: beat.percentOfStory
            }))
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

        // If a beat is selected, update its sequence display
        if (this.selectedBeatId) {
            const selectedBeat = this.currentState.find(b => b.id === this.selectedBeatId);
            if (selectedBeat) {
                $('#beatSequence').text(selectedBeat.sequence);
            }
        }
    }
};

// Export the BeatEditor object
window.BeatEditor = BeatEditor;
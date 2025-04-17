$(document).ready(function () {
    // --- Configuration & Constants ---
    const archetypesTable = $('#archetypes-datatable');
    const tableContainer = archetypesTable.closest('.table-container');
    const importModal = $('#importModal');
    const importSubmitBtn = document.querySelector("#import-submit-btn");
    const defaultImagePath = '/images/default-archetype.png'; // Configurable default image

    // URLs from data attributes
    const urls = {
        getArchetypes: archetypesTable.data('get-archetypes-url'),
        getArchetype: archetypesTable.data('get-archetype-url'),
        deleteArchetype: archetypesTable.data('delete-archetype-url'),
        deleteMultiple: archetypesTable.data('delete-multiple-url'),
        exportArchetypes: archetypesTable.data('export-url'),
        importArchetypes: archetypesTable.data('import-url')
    };

    // Check if URLs are defined
    for (const key in urls) {
        if (!urls[key]) {
            console.error(`Missing URL for ${key}. Check data attributes on #archetypes-datatable.`);
            // Optionally disable functionality or show an error message to the user
            // return; 
        }
    }

    let selectedArchetypeId = null;
    let dataTableInstance = null;

    // --- Function Definitions ---

    /**
     * Shows a Bootstrap toast notification.
     * @param {('success'|'error'|'info')} type - The type of toast.
     * @param {string} title - The title of the toast.
     * @param {string} message - The message body of the toast.
     */
    function showToast(type, title, message) {
        const toastEl = document.getElementById('importToast'); // Using ID for consistency
        if (!toastEl) {
            console.error("Toast element #importToast not found.");
            return;
        }

        const toastIcon = toastEl.querySelector('#toast-icon');
        const toastTitle = toastEl.querySelector('#toast-title');
        const toastMessage = toastEl.querySelector('#toast-message');
        const toastTime = toastEl.querySelector('#toast-time');

        // Set icon and color based on type
        if (toastIcon) {
            toastIcon.className = 'font-18 me-1 mdi '; // Reset classes
            switch (type) {
                case 'success':
                    toastIcon.classList.add('mdi-check-circle', 'text-success');
                    break;
                case 'error':
                    toastIcon.classList.add('mdi-alert-circle', 'text-danger');
                    break;
                default: // 'info' or other
                    toastIcon.classList.add('mdi-information', 'text-info');
                    break;
            }
        }

        // Set title and message
        if (toastTitle) toastTitle.textContent = title;
        if (toastMessage) toastMessage.textContent = message;

        // Set time
        if (toastTime) {
            const now = new Date();
            toastTime.textContent = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        }

        // Show the toast
        const bsToast = bootstrap.Toast.getOrCreateInstance(toastEl);
        bsToast.show();
    }

    /**
     * Shows or hides the loading overlay.
     * @param {boolean} show - True to show, false to hide.
     */
    function toggleLoadingOverlay(show) {
        const overlay = tableContainer.find('.overlay');
        if (show) {
            if (overlay.length === 0) {
                tableContainer.append('<div class="overlay"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');
            }
        } else {
            overlay.remove();
        }
    }

    /**
     * Clears the archetype details panel.
     */
    function clearArchetypeDetails() {
        $('#archetype-image').attr('src', defaultImagePath);
        $('#archetype-title').text('No Archetype Selected');
        $('#archetype-date').text('');
        $('#archetype-description').text('');
        $('#beats-timeline').empty().append('<div class="text-center p-3">No archetype selected.</div>');
        $('#genres-content').empty().append('<div class="text-center p-3">No archetype selected.</div>');
        selectedArchetypeId = null;
        // Ensure no row is marked active in the table
        if (dataTableInstance) {
             dataTableInstance.rows().nodes().to$().removeClass('table-active');
        }
    }

    /**
     * Updates the details panel with data for the selected archetype.
     * @param {object} archetype - The archetype data object.
     */
    function updateArchetypeDetails(archetype) {
        if (!archetype) {
            clearArchetypeDetails();
            return;
        }
        const imagePath = archetype.icon || defaultImagePath;
        $('#archetype-image').attr('src', imagePath);
        $('#archetype-title').text(archetype.domain || 'N/A');

        // --- Display Creation Date ---
        let formattedDate = 'Date not available';
        if (archetype.createdDate) { // Assuming 'createdDate' property exists
            try {
                const date = new Date(archetype.createdDate);
                 // Check if date is valid before formatting
                if (!isNaN(date.getTime())) {
                    formattedDate = date.toLocaleDateString([], { year: 'numeric', month: 'long', day: 'numeric' });
                }
            } catch (e) {
                console.error("Error formatting creation date:", e);
            }
        }
        $('#archetype-date').text(`Added: ${formattedDate}`);
        // --- End Date Display ---

        $('#archetype-description').text(archetype.description || 'No description available.');
    }

    /**
     * Updates the beats timeline.
     * @param {Array} beats - Array of beat objects.
     */
    function updateBeats(beats) {
        const beatsTimeline = $('#beats-timeline');
        beatsTimeline.empty();

        if (beats && beats.length > 0) {
            // Sort beats (assuming parentSequence exists)
            beats.sort((a, b) => (a.parentSequence || 0) - (b.parentSequence || 0));

            beats.forEach(beat => {
                // Consider using a template literal or a more robust templating method
                const beatItem = `
                    <div class="timeline-item">
                        <i class="mdi mdi-upload bg-info-lighten text-info timeline-icon"></i>
                        <div class="timeline-item-info">
                            <a href="#" class="text-info fw-bold mb-1 d-block">${beat.name || 'Unnamed Beat'}</a>
                            <small>${beat.description || ''}</small>
                            <p class="mb-0 pb-2">
                                <small class="text-muted">Page ${beat.percentOfStory || 'N/A'}</small>
                            </p>
                        </div>
                    </div>
                `;
                beatsTimeline.append(beatItem);
            });
        } else {
            beatsTimeline.append('<div class="text-center p-3">No beats available for this archetype.</div>');
        }
    }

    /**
     * Updates the genres display.
     * @param {Array} genres - Array of genre objects.
     */
    function updateGenres(genres) {
        const genresContent = $('#genres-content');
        genresContent.empty();

        if (genres && genres.length > 0) {
            genres.forEach(genre => {
                // TODO: Replace hardcoded example/hybrid counts (10, 50) with actual data if available from backend
                const exampleCount = genre.exampleCount || 0; // Placeholder
                const hybridCount = genre.hybridCount || 0;   // Placeholder

                const genreCard = `
                    <div class="card d-block mb-3">
                        <div class="card-body">
                            <div class="dropdown card-widgets">
                                <a href="#" class="dropdown-toggle arrow-none" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="ri-more-fill"></i>
                                </a>
                                <div class="dropdown-menu dropdown-menu-end">
                                    <a href="javascript:void(0);" class="dropdown-item"><i class="mdi mdi-pencil me-1"></i>Edit</a>
                                    <a href="javascript:void(0);" class="dropdown-item"><i class="mdi mdi-delete me-1"></i>Delete</a>
                                    <a href="javascript:void(0);" class="dropdown-item"><i class="mdi mdi-email-outline me-1"></i>Invite</a>
                                    <a href="javascript:void(0);" class="dropdown-item"><i class="mdi mdi-exit-to-app me-1"></i>Leave</a>
                                </div>
                            </div>
                            <h4 class="mt-0">
                                <a href="javascript:void(0);" class="text-title">${genre.name || 'Unnamed Genre'}</a>
                            </h4>
                            <p class="text-muted font-13 my-3">${genre.description || 'No description.'}<a href="javascript:void(0);" class="fw-bold text-muted"> view more</a></p>
                            <p class="mb-1">
                                <span class="pe-2 text-nowrap mb-2 d-inline-block">
                                    <i class="mdi mdi-format-list-bulleted-type text-muted"></i>
                                    <b>${exampleCount}</b> Examples
                                </span>
                                <span class="text-nowrap mb-2 d-inline-block">
                                    <i class="mdi mdi-comment-multiple-outline text-muted"></i>
                                    <b>${hybridCount}</b> Hybrids
                                </span>
                            </p>
                        </div>
                    </div>
                `;
                genresContent.append(genreCard);
            });
        } else {
            genresContent.append('<div class="text-center p-3">No genres available for this archetype.</div>');
        }
    }

    /**
     * Fetches and displays details for a specific archetype.
     * @param {string} archetypeId - The ID of the archetype to select.
     */
    function selectArchetype(archetypeId) {
        if (!archetypeId) return;
        selectedArchetypeId = archetypeId;

        // Highlight row in DataTable
        if (dataTableInstance) {
            dataTableInstance.rows().nodes().to$().removeClass('table-active');
            dataTableInstance.row(`[data-archetype-id="${archetypeId}"]`).nodes().to$().addClass('table-active');
        }

        // Fetch details
        toggleLoadingOverlay(true); // Show loading on details panel? Maybe add overlay there too.
        $.ajax({
            url: `${urls.getArchetype}?id=${archetypeId}`,
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                updateArchetypeDetails(data);
                updateBeats(data.beats);
                updateGenres(data.genres);
            },
            error: function (error) {
                console.error('Error loading archetype details:', error);
                showToast('error', 'Load Failed', 'Could not load archetype details.');
                clearArchetypeDetails(); // Clear details on error
            },
            complete: function () {
                toggleLoadingOverlay(false);
            }
        });
    }

    /**
     * Handles the deletion of multiple selected archetypes.
     */
    function handleDeleteSelected() {
        if (!dataTableInstance) {
            console.error("DataTable instance not available in handleDeleteSelected.");
            return;
        }

        // Manually find checked checkboxes within the table body and get their row IDs
        const checkedIds = [];
        dataTableInstance.$('input.archetype-checkbox:checked').each(function() {
            const rowNode = $(this).closest('tr');
            const rowData = dataTableInstance.row(rowNode).data();
            if (rowData && rowData.id) {
                checkedIds.push(rowData.id);
            } else {
                console.warn("Could not find row data or ID for a checked checkbox in row:", rowNode);
            }
        });

        if (checkedIds.length === 0) {
            showToast('info', 'No Selection', 'Please select at least one archetype to delete.');
            return;
        }

        // Confirmation dialog (optional but recommended)
        if (!confirm(`Are you sure you want to delete ${checkedIds.length} selected archetype(s)?`)) {
            return;
        }

        toggleLoadingOverlay(true);
        $.ajax({
            url: urls.deleteMultiple,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(checkedIds),
            success: function (response) {
                if (response.success) {
                    showToast('success', 'Delete Successful', `Successfully deleted ${response.deletedCount || checkedIds.length} archetype(s).`);
                    dataTableInstance.ajax.reload(null, false); // Reload data, keep pagination
                    // If the currently selected archetype was deleted, clear details
                    if (selectedArchetypeId && checkedIds.includes(selectedArchetypeId)) {
                        clearArchetypeDetails();
                    }
                } else {
                    showToast('error', 'Delete Failed', response.message || 'Failed to delete selected archetypes.');
                }
            },
            error: function (error) {
                console.error('Error deleting multiple archetypes:', error);
                showToast('error', 'Delete Failed', 'An error occurred while deleting archetypes.');
            },
            complete: function () {
                toggleLoadingOverlay(false);
            }
        });
    }

    /**
     * Handles the deletion of a single archetype via action button.
     * @param {string} archetypeId - The ID of the archetype to delete.
     */
    function deleteSingleArchetype(archetypeId) {
        if (!archetypeId) {
            showToast('error', 'Delete Failed', 'Invalid archetype ID provided.');
            return;
        }

        // Confirmation dialog (optional but recommended)
        if (!confirm(`Are you sure you want to delete this archetype?`)) {
            return;
        }

        toggleLoadingOverlay(true);
        $.ajax({
            url: `${urls.deleteArchetype}?id=${archetypeId}`,
            type: 'DELETE',
            success: function (response) {
                // Assuming success means it was deleted, even if response format varies
                showToast('success', 'Delete Successful', 'Archetype deleted successfully.');
                dataTableInstance.ajax.reload(null, false); // Reload data, keep pagination
                 // If the currently selected archetype was deleted, clear details
                if (selectedArchetypeId === archetypeId) {
                    clearArchetypeDetails();
                }
            },
            error: function (error) {
                console.error('Error deleting archetype:', error);
                showToast('error', 'Delete Failed', 'An error occurred while deleting the archetype.');
            },
            complete: function () {
                toggleLoadingOverlay(false);
            }
        });
    }

    /**
     * Handles the export of selected archetypes.
     */
    function handleExport() {
         if (!dataTableInstance) return;

        const checkedIds = dataTableInstance.rows({ selected: true }).data().pluck('id').toArray();

        if (checkedIds.length === 0) {
            showToast('info', 'No Selection', 'Please select at least one archetype to export.');
            return;
        }

        toggleLoadingOverlay(true); // Show loading indicator
        $.ajax({
            url: urls.exportArchetypes,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(checkedIds),
            success: function (response) {
                if (response.success && response.data) {
                    try {
                        const jsonData = JSON.stringify(response.data, null, 2);
                        const blob = new Blob([jsonData], { type: 'application/json' });
                        const link = document.createElement('a');
                        link.href = URL.createObjectURL(blob);
                        link.download = `archetypes_export_${new Date().toISOString().split('T')[0]}.json`; // Dynamic filename
                        link.click();
                        URL.revokeObjectURL(link.href);
                        showToast('success', 'Export Successful', `Exported ${checkedIds.length} archetype(s).`);
                    } catch (e) {
                         console.error("Error processing export data:", e);
                         showToast('error', 'Export Failed', 'Error processing data for download.');
                    }
                } else {
                    showToast('error', 'Export Failed', response.message || 'Failed to retrieve data for export.');
                }
            },
            error: function (error) {
                console.error('Error exporting archetypes:', error);
                showToast('error', 'Export Failed', 'An error occurred during export.');
            },
            complete: function() {
                toggleLoadingOverlay(false); // Hide loading indicator
            }
        });
    }


    // --- DataTable Initialization ---
    function initializeDataTable() {
        if (!urls.getArchetypes) {
             showToast('error', 'Initialization Failed', 'Cannot load archetypes table. Configuration missing.');
             return; // Don't initialize if URL is missing
        }
        
        dataTableInstance = archetypesTable.DataTable({
            responsive: true,
            processing: true, // Show DataTable's processing indicator
            serverSide: false, // Assuming data is loaded client-side initially or via ajax reload
            ajax: {
                url: urls.getArchetypes,
                type: 'GET',
                dataType: 'json',
                dataSrc: '', // Directly use the array returned
                error: function (xhr, error, thrown) {
                    console.error("DataTable AJAX error:", error, thrown);
                    showToast('error', 'Load Failed', 'Failed to load archetype data for the table.');
                    toggleLoadingOverlay(false); // Ensure overlay is removed on error
                }
            },
            columns: [
                {
                    data: null, // Checkbox column
                    orderable: false,
                    searchable: false,
                    width: "20px",
                    className: 'select-checkbox', // Use DataTables Select extension class if using it
                     render: function (data, type, row, meta) {
                         // Use row ID for unique checkbox ID
                         return `<div class="form-check">
                                     <input type="checkbox" class="form-check-input archetype-checkbox" id="check_${row.id}">
                                     <label class="form-check-label" for="check_${row.id}">&nbsp;</label>
                                 </div>`;
                     }
                },
                {
                    data: 'domain', // Domain/Architect column
                    render: function (data, type, row) {
                        // Combine Domain and Architect
                        const domain = row.domain || 'N/A';
                        const architect = row.architect || 'N/A';
                        // Link is handled by row click now, but keep structure
                        return `<div class="d-flex align-items-center">
                                    <div>
                                        <span class="text-body fw-semibold">${domain}</span>
                                        <p class="text-muted mb-0">${architect}</p>
                                    </div>
                                </div>`;
                    }
                },
                {
                    data: 'rank', // Rank column
                    width: "50px" // Adjust width as needed
                },
                {
                    data: null, // Action column
                    orderable: false,
                    searchable: false,
                    width: "85px",
                    className: 'table-action',
                    render: function (data, type, row) {
                        // Use row.id for actions
                        return `
                            <a href="javascript:void(0);" class="action-icon view-archetype" title="View Details"> <i class="mdi mdi-eye"></i></a>
                            <a href="javascript:void(0);" class="action-icon edit-archetype" title="Edit Archetype"> <i class="mdi mdi-square-edit-outline"></i></a>
                            <a href="javascript:void(0);" class="action-icon delete-archetype" title="Delete Archetype"> <i class="mdi mdi-delete"></i></a>
                        `;
                    }
                }
            ],
            order: [[2, "asc"]], // Default sort by Rank (index 2)
            select: { // Enable row selection (requires DataTables Select extension)
                 style: 'multi', // Allow multiple row selection via checkboxes
                 selector: 'td:first-child input[type="checkbox"]' // Select rows via checkbox click
            },
             // Handle initial load and subsequent draws
            drawCallback: function(settings) {
                toggleLoadingOverlay(false); // Hide loading overlay after table draw
                // If an archetype was selected, re-highlight its row
                if (selectedArchetypeId) {
                     dataTableInstance.rows().nodes().to$().removeClass('table-active'); // Clear previous
                     const selectedRow = dataTableInstance.row(`[data-archetype-id="${selectedArchetypeId}"]`);
                     if (selectedRow.any()) {
                         selectedRow.nodes().to$().addClass('table-active');
                     } else {
                         // If selected archetype is no longer in the table, clear details
                         clearArchetypeDetails();
                     }
                }
            },
            // Select first row after initial load if none selected
            initComplete: function(settings, json) {
                 if (!selectedArchetypeId && dataTableInstance.rows().count() > 0) {
                     const firstRowData = dataTableInstance.row(0).data();
                     if (firstRowData && firstRowData.id) {
                         selectArchetype(firstRowData.id);
                     }
                 } else if (dataTableInstance.rows().count() === 0) {
                     clearArchetypeDetails(); // Clear details if table is empty
                 }
            }
        });

        // Add listener for DataTables Select extension events (if used)
        // dataTableInstance.on('select deselect', function () {
        //     // Update delete button state, etc. based on selection
        //     const selectedCount = dataTableInstance.rows({ selected: true }).count();
        //     // Example: $('#delete-selected-btn').prop('disabled', selectedCount === 0);
        // });
    }


    // --- Dropzone Initialization ---
    function initializeDropzone() {
        if (!urls.importArchetypes) {
             showToast('error', 'Initialization Failed', 'Cannot initialize import. Configuration missing.');
             return; // Don't initialize if URL is missing
        }
        
        Dropzone.autoDiscover = false;
        let importDropzoneInstance = new Dropzone("#importDropzone", {
            url: urls.importArchetypes,
            maxFilesize: 10, // MB
            maxFiles: 1,
            acceptedFiles: "application/json,.json",
            autoProcessQueue: false,
            addRemoveLinks: true,
            paramName: "files", // Match controller parameter
            init: function () {
                let myDropzone = this;

                this.on("addedfile", function (file) {
                    console.log("File added:", file.name);
                    if (importSubmitBtn) importSubmitBtn.removeAttribute("disabled");
                });

                this.on("removedfile", function (file) {
                    console.log("File removed:", file.name);
                    if (myDropzone.files.length === 0 && importSubmitBtn) {
                        importSubmitBtn.setAttribute("disabled", "disabled");
                    }
                });

                this.on("processing", function (file) {
                    console.log("Processing file:", file.name);
                    // Optionally show spinner on submit button
                    if (importSubmitBtn) {
                        importSubmitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Importing...';
                        importSubmitBtn.setAttribute("disabled", "disabled");
                    }
                });

                this.on("success", function (file, response) {
                    console.log("Upload successful:", response);
                    importModal.modal('hide'); // Close modal
                    showToast('success', 'Import Successful', response.message || 'Archetypes imported successfully.');
                    if (dataTableInstance) {
                        dataTableInstance.ajax.reload(null, false); // Reload table data
                    }
                    myDropzone.removeAllFiles(true); // Clear dropzone
                });

                this.on("error", function (file, errorMessage, xhr) {
                    console.error("Upload error:", errorMessage, xhr);
                    let message = "An error occurred during upload.";
                    if (typeof errorMessage === 'string') {
                        message = errorMessage;
                    } else if (xhr && xhr.responseText) {
                        try {
                            const responseJson = JSON.parse(xhr.responseText);
                            message = responseJson.message || xhr.statusText || message;
                        } catch (e) {
                            message = xhr.statusText || message;
                        }
                    } else if (typeof errorMessage === 'object' && errorMessage.message) {
                         message = errorMessage.message;
                    }
                    
                    showToast('error', 'Import Failed', message);
                    // Optionally remove the failed file: myDropzone.removeFile(file);
                });

                this.on("complete", function(file) {
                     // Reset submit button state after completion (success or error)
                     if (importSubmitBtn) {
                         importSubmitBtn.innerHTML = 'Import';
                         if (myDropzone.files.length > 0) {
                             importSubmitBtn.removeAttribute("disabled");
                         } else {
                              importSubmitBtn.setAttribute("disabled", "disabled");
                         }
                     }
                });

                // Manual trigger for processing queue
                if (importSubmitBtn) {
                    importSubmitBtn.addEventListener("click", function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        if (myDropzone.getQueuedFiles().length > 0) {
                            myDropzone.processQueue();
                        } else {
                             showToast('info', 'No File', 'Please select a file to import.');
                        }
                    });
                }
            }
        });
    }

    // --- Event Listeners ---

    // Static button clicks
    $('#export-btn').on('click', handleExport);
    $('#import-btn').on('click', () => importModal.modal('show'));
    $('#delete-selected-btn').on('click', handleDeleteSelected);

    // Delegated event listeners for DataTable content
    archetypesTable.on('click', 'tbody tr', function (e) {
        // Prevent row selection if clicking on checkbox or action icons
        if ($(e.target).is('input[type="checkbox"]') || $(e.target).closest('.table-action').length) {
            return;
        }
        const row = dataTableInstance.row(this);
        const data = row.data();
        if (data && data.id) {
            selectArchetype(data.id);
        }
    });

    archetypesTable.on('click', '.action-icon.view-archetype', function (e) {
        e.stopPropagation(); // Prevent row click handler
        const row = dataTableInstance.row($(this).closest('tr'));
        const data = row.data();
        if (data && data.id) {
            selectArchetype(data.id);
        }
    });

     archetypesTable.on('click', '.action-icon.edit-archetype', function (e) {
        e.stopPropagation(); // Prevent row click handler
        const row = dataTableInstance.row($(this).closest('tr'));
        const data = row.data();
        if (data && data.id) {
            // TODO: Implement edit functionality (e.g., open modal, redirect)
            showToast('info', 'Not Implemented', `Edit action for Archetype ID: ${data.id}`);
            console.log("Edit archetype:", data.id);
        }
    });

    archetypesTable.on('click', '.action-icon.delete-archetype', function (e) {
        e.stopPropagation(); // Prevent row click handler
        const row = dataTableInstance.row($(this).closest('tr'));
        const data = row.data();
        if (data && data.id) {
            deleteSingleArchetype(data.id);
        }
    });

    // --- Initialization ---
    toggleLoadingOverlay(true); // Show initial loading overlay
    initializeDataTable();
    initializeDropzone();
    clearArchetypeDetails(); // Start with a clean details panel

});
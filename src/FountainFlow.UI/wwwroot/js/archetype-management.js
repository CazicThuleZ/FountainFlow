$(document).ready(function () {
    // console.log("archetype-management.js: Document ready."); // DEBUG REMOVED
    // --- Configuration & Constants ---
    const archetypesTable = $('#archetypes-datatable');
    const tableContainer = archetypesTable.closest('.table-container');
    const importModal = $('#importModal');
    const importSubmitBtn = document.querySelector("#import-submit-btn");
    const addArchetypeModal = $('#addArchetypeModal'); // Added
    const addArchetypeForm = $('#addArchetypeForm'); // Added
    const saveArchetypeBtn = $('#save-archetype-btn'); // Added
    const addArchetypeBtn = $('#add-archetype-btn'); // Added
    // console.log("archetype-management.js: Add button found?", addArchetypeBtn.length); // DEBUG REMOVED
    // console.log("archetype-management.js: Add modal found?", addArchetypeModal.length); // DEBUG REMOVED
    const defaultImagePath = '/images/Thumbnails/archetyp_default.png'; // Default image path
    // URLs from data attributes
    const urls = {
        getArchetypes: archetypesTable.data('get-archetypes-url'),
        getArchetype: archetypesTable.data('get-archetype-url'),
        deleteArchetype: archetypesTable.data('delete-archetype-url'),
        deleteMultiple: archetypesTable.data('delete-multiple-url'),
        exportArchetypes: archetypesTable.data('export-url'),
        importArchetypes: archetypesTable.data('import-url'),
        createArchetype: '/Archetypes/CreateArchetype' // Added - Assuming standard URL pattern
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
    let archetypeIconDropzoneInstance = null; // Added for Dropzone instance

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
    } // End of handleExport function

    /**
     * Handles saving a new archetype from the modal form.
     */
    function handleSaveArchetype() {
        // Basic Validation
        if (!addArchetypeForm[0].checkValidity()) {
            addArchetypeForm[0].reportValidity();
            return;
        }

        // Use FormData to include the file
        let formData = new FormData(addArchetypeForm[0]); // Get form fields

        // Append other fields manually if they don't have 'name' attributes or need specific formatting
        formData.append('domain', $('#archetype-domain').val());
        formData.append('architect', $('#archetype-architect').val());
        formData.append('description', $('#addArchetypeForm #archetype-description').val());
        formData.append('rank', parseInt($('#archetype-rank').val(), 10));
        formData.append('externalLink', $('#archetype-externalLink').val());

        // Get the file from Dropzone
        if (archetypeIconDropzoneInstance) {
            const files = archetypeIconDropzoneInstance.getAcceptedFiles();
            if (files.length > 0) {
                // Use 'iconFile' to match the expected parameter name in the controller
                formData.append('iconFile', files[0], files[0].name);
            }
            // Note: If no file is added, 'iconFile' will not be appended,
            // and the controller will receive null for the IFormFile parameter.
        } else {
             console.error("Archetype Icon Dropzone instance not found.");
             // Optionally prevent submission or show an error
             // return;
        }


        saveArchetypeBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>Saving...');
        toggleLoadingOverlay(true);

        $.ajax({
            url: urls.createArchetype,
            type: 'POST',
            data: formData, // Send FormData
            processData: false, // Prevent jQuery from processing the data
            contentType: false, // Prevent jQuery from setting contentType
            success: function (response) {
                addArchetypeModal.modal('hide'); // Hide modal on success
                showToast('success', 'Archetype Added', `Successfully added archetype: ${response.domain || formData.get('domain')}.`); // Use response data if available
                if (dataTableInstance) {
                    dataTableInstance.ajax.reload(function() {
                        if (response && response.id) {
                            selectArchetype(response.id);
                        }
                    }, false);
                }
                // Dropzone is reset via the 'hidden.bs.modal' event handler added below
            },
            error: function (error) {
                console.error('Error creating archetype:', error);
                let errorMsg = 'Failed to add archetype.';
                if (error.responseJSON && error.responseJSON.title) {
                    errorMsg += ` ${error.responseJSON.title}`;
                } else if (error.responseText) {
                     // Try to parse responseText if it's JSON, otherwise show raw text
                    try {
                        const errData = JSON.parse(error.responseText);
                        errorMsg += ` ${errData.message || errData.title || ''}`;
                    } catch (e) {
                        // Limit length of raw error text
                        errorMsg += ` ${error.responseText.substring(0, 200)}`;
                    }
                }
                showToast('error', 'Save Failed', errorMsg);
            },
            complete: function () {
                saveArchetypeBtn.prop('disabled', false).html('Save Archetype');
                toggleLoadingOverlay(false);
            }
        });
    } // End of handleSaveArchetype function


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
                     const selectedRow = dataTableInstance.row((idx, data, node) => data.id === selectedArchetypeId); // Find row by ID
                     if (selectedRow.any()) {
                         selectedRow.nodes().to$().addClass('table-active');
                     } else {
                         // If selected archetype is no longer in the table, clear details
                         clearArchetypeDetails();
                     }
                }
                // Re-attach tooltips if using Bootstrap tooltips
                $('[data-bs-toggle="tooltip"]').tooltip();
            },
            rowCallback: function(row, data) {
                // Add data attribute for easier selection later
                $(row).attr('data-archetype-id', data.id);
            },
            initComplete: function(settings, json) {
                // Optional: Select the first archetype by default after initial load
                // if (json && json.length > 0) {
                //     selectArchetype(json[0].id);
                // } else {
                //     clearArchetypeDetails(); // Ensure details are clear if table is empty
                // }
            }
        });
    } // End of initializeDataTable

    // --- Dropzone Initialization ---
    function initializeDropzone() {
        if (!urls.importArchetypes) {
             showToast('error', 'Initialization Failed', 'Cannot initialize import. Configuration missing.');
             return; // Don't initialize if URL is missing
        }

        // Check if Dropzone is defined
        if (typeof Dropzone === 'undefined') {
            console.error("Dropzone library not loaded.");
            return;
        }

        Dropzone.autoDiscover = false; // Prevent Dropzone from automatically attaching

        let importDropzoneInstance = new Dropzone("#importDropzone", {
            url: urls.importArchetypes,
            autoProcessQueue: false, // Don't upload immediately
            uploadMultiple: false, // Upload one file at a time
            acceptedFiles: 'application/json', // Only accept JSON
            maxFiles: 1, // Only allow one file
            addRemoveLinks: true,
            dictDefaultMessage: "Drop JSON file here or click to upload",
            dictRemoveFile: "Remove file",
            dictMaxFilesExceeded: "You can only upload one file.",
            dictInvalidFileType: "You can only upload JSON files.",
            previewsContainer: "#file-previews", // Define the container to display the previews
            previewTemplate: document.querySelector('#uploadPreviewTemplate').innerHTML, // Define the preview template
            init: function () {
                this.on("addedfile", function (file) {
                    // Only one file allowed, remove others
                    if (this.files.length > 1) {
                        this.removeFile(this.files[0]);
                    }
                    // Enable submit button when a valid file is added
                    if (this.files.length === 1 && this.files[0].status === Dropzone.ADDED) {
                         importSubmitBtn.disabled = false;
                    }
                });

                this.on("removedfile", function (file) {
                    // Disable submit button if no files are present
                    if (this.files.length === 0) {
                        importSubmitBtn.disabled = true;
                    }
                });

                this.on("error", function (file, message) {
                    console.error("Dropzone error:", message);
                    // Show error using toast
                    showToast('error', 'Upload Error', typeof message === 'string' ? message : 'An error occurred during upload.');
                    // Optionally remove the file with error
                    this.removeFile(file);
                    importSubmitBtn.disabled = true; // Ensure button is disabled on error
                });

                this.on("sending", function(file, xhr, formData) {
                    // Add loading state to button
                    importSubmitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span> Importing...';
                    importSubmitBtn.disabled = true;
                });

                this.on("success", function (file, response) {
                    // Handle success from server
                    if (response.success) {
                        showToast('success', 'Import Successful', response.message || 'Archetypes imported successfully.');
                        importModal.modal('hide'); // Close modal on success
                        if (dataTableInstance) {
                            dataTableInstance.ajax.reload(); // Reload DataTable
                        }
                    } else {
                        showToast('error', 'Import Failed', response.message || 'Failed to import archetypes.');
                    }
                    this.removeFile(file); // Remove file preview on success
                });

                this.on("complete", function(file) {
                    // Restore button state regardless of success/error
                    importSubmitBtn.innerHTML = 'Import';
                    // Re-enable button only if there's a valid file remaining (shouldn't happen with maxFiles: 1 and auto-removal)
                    importSubmitBtn.disabled = this.files.length === 0;
                });

                // Manual trigger for upload
                importSubmitBtn.addEventListener('click', () => {
                    if (this.files.length > 0) {
                        this.processQueue(); // Start upload
                    } else {
                        showToast('info', 'No File', 'Please select a file to import.');
                    }
                });

                // Clear dropzone when modal is hidden
                importModal.on('hidden.bs.modal', () => {
                    this.removeAllFiles(true); // true to cancel uploads
                    importSubmitBtn.disabled = true; // Reset button state
                });
            }
        });
    } // End of initializeDropzone

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

    // --- Add Archetype Modal Event Listeners ---
    // console.log("archetype-management.js: Attaching click listener to add button."); // DEBUG REMOVED
    addArchetypeBtn.on('click', function() {
        // console.log("archetype-management.js: Add archetype button clicked."); // DEBUG REMOVED
        addArchetypeForm[0].reset(); // Clear form before showing
        // console.log("archetype-management.js: Attempting to show modal:", addArchetypeModal); // DEBUG REMOVED
        addArchetypeModal.modal('show');
    });

    saveArchetypeBtn.on('click', handleSaveArchetype);

    // Optional: Clear form when modal is hidden
    // Moved the reset logic to the new combined listener below

    // --- Initialization ---

    /**
     * Initializes the Dropzone instance for the archetype icon upload.
     */
    function initializeArchetypeIconDropzone() {
        // Ensure Dropzone is auto-discovered or manually attach if needed
        // Dropzone.autoDiscover = false; // Uncomment if you face issues with auto-discovery

        if ($("#archetypeIconDropzone").length > 0 && !archetypeIconDropzoneInstance) {
            try {
                archetypeIconDropzoneInstance = new Dropzone("#archetypeIconDropzone", {
                    url: urls.createArchetype, // Will be overridden by AJAX call, but good practice to set
                    autoProcessQueue: false,   // We trigger upload manually via AJAX
                    uploadMultiple: false,     // Only one file
                    maxFiles: 1,               // Enforce single file
                    acceptedFiles: 'image/*',  // Accept only images
                    addRemoveLinks: true,      // Show remove links
                    previewsContainer: "#archetype-icon-preview", // Set custom preview container
                    // Simple preview template (adjust as needed)
                    previewTemplate: `
                        <div class="dz-preview dz-file-preview">
                          <div class="dz-image"><img data-dz-thumbnail /></div>
                          <div class="dz-details">
                            <div class="dz-size"><span data-dz-size></span></div>
                            <div class="dz-filename"><span data-dz-name></span></div>
                          </div>
                          <div class="dz-progress"><span class="dz-upload" data-dz-uploadprogress></span></div>
                          <div class="dz-error-message"><span data-dz-errormessage></span></div>
                          <a class="dz-remove" href="javascript:undefined;" data-dz-remove>Remove file</a>
                        </div>
                    `,
                    init: function () {
                        this.on("addedfile", function (file) {
                            // If maxFiles is 1, remove previous file when a new one is added
                            if (this.files.length > this.options.maxFiles) {
                                this.removeFile(this.files[0]);
                            }
                            // Optional: Show preview container if hidden
                            $('#archetype-icon-preview').show();
                        });
                        this.on("removedfile", function (file) {
                            // Optional: Hide preview container if empty
                            if (this.files.length === 0) {
                                $('#archetype-icon-preview').hide();
                            }
                        });
                        this.on("maxfilesexceeded", function (file) {
                            this.removeAllFiles();
                            this.addFile(file); // Add the new file after removing others
                        });
                        this.on("error", function(file, message) {
                            // Handle errors (e.g., invalid file type)
                            console.error("Dropzone error:", message);
                            showToast('error', 'Upload Error', typeof message === 'string' ? message : 'Invalid file.');
                            this.removeFile(file); // Remove the invalid file
                        });
                    }
                });
                 // Initially hide preview container if empty
                if (archetypeIconDropzoneInstance.files.length === 0) {
                    $('#archetype-icon-preview').hide();
                }
            } catch (e) {
                console.error("Failed to initialize Archetype Icon Dropzone:", e);
                showToast('error', 'Initialization Error', 'Could not initialize image upload.');
            }
        } else if ($("#archetypeIconDropzone").length === 0) {
             console.warn("Archetype Icon Dropzone element (#archetypeIconDropzone) not found.");
        }
    }


    // --- Event Listeners & Initial Calls ---

    // Reset form AND Dropzone when the Add/Edit modal is hidden
    addArchetypeModal.on('hidden.bs.modal', function () {
        addArchetypeForm[0].reset(); // Reset standard form fields
        // Remove any validation states if necessary
        $('.is-invalid').removeClass('is-invalid'); // Example: remove Bootstrap validation state

        if (archetypeIconDropzoneInstance) {
            archetypeIconDropzoneInstance.removeAllFiles(true); // true = silent removal without events
        }
         $('#archetype-icon-preview').hide(); // Hide preview on modal close
    });

    // Initial setup calls
    toggleLoadingOverlay(true); // Show initial loading overlay
    initializeDataTable();
    initializeDropzone(); // For import modal
    initializeArchetypeIconDropzone(); // For archetype icon
    clearArchetypeDetails(); // Start with a clean details panel

});
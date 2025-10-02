$(document).ready(function () {
    const jwtToken = getCookie("JwtToken");
    console.log(jwtToken);

    if (!jwtToken) {
        alert('Authorization token not found.');
        return;
    }

    // Function to load subjects and display them in the table
    function loadSubjects() {
        $.ajax({
            url: 'http://localhost:5224/api/Subjects/viewallsubject',
            method: 'GET',
            beforeSend: function (xhr) {
                xhr.setRequestHeader('Authorization', `Bearer ${jwtToken}`);
            },
            success: function (data) {
                const tableBody = $('#subjects-table tbody');
                tableBody.empty(); // Clear the table before appending new data
                data.forEach(subject => {
                    tableBody.append(`
                        <tr data-id="${subject.subjectId}">
                            <td>${subject.subjectId}</td>
                            <td>${subject.subjectName}</td>
                            <td>
                                <button class="btn btn-warning btn-sm edit-btn">Edit</button>
                            </td>
                        </tr>
                    `);
                });
            },
            error: function (xhr, status, error) {
                console.error("Error with API call: ", error);
                if (xhr.status === 401) {
                    alert('You do not have the required permissions to access this resource.');
                } else {
                    alert('Failed to load subjects.');
                }
            }
        });
    }

    // Load subjects on page load
    loadSubjects();

    // Handle the submission of the Add/Edit form
    $('#subject-form').on('submit', function (e) {
        e.preventDefault();

        const subjectId = $('#subjectId').val();
        const subjectName = $('#subjectName').val().trim();

        if (!subjectName) {
            alert("Subject name is required.");
            return;
        }

        // Determine the API endpoint and method based on whether we're adding or editing
        const url = subjectId ? `http://localhost:5224/api/Subjects/editsubject?id=${subjectId}` : `http://localhost:5224/api/Subjects/addsubject`;
        const method = subjectId ? 'PUT' : 'POST';

        // Prepare the data to be sent
        const requestData = JSON.stringify(subjectName);

        $.ajax({
            url: url,
            method: method,
            contentType: 'application/json',
            beforeSend: function (xhr) {
                xhr.setRequestHeader('Authorization', `Bearer ${jwtToken}`);
            },
            data: requestData,
            success: function () {
                $('#subject-form')[0].reset(); // Clear the form
                $('#subjectId').val(''); // Clear hidden subject ID
                $('#form-title').text('Add Subject'); // Reset form title
                loadSubjects(); // Reload the subject list
            },
            error: function (xhr) {
                let errorMessage = 'Failed to save subject.';
                if (xhr.status === 409) { // Conflict status code for duplicate subject
                    errorMessage = 'A subject with this name already exists.';
                } else if (xhr.status === 401) { // Unauthorized
                    errorMessage = 'You do not have the required permissions to perform this action.';
                } else if (xhr.status === 400) { // Bad Request
                    errorMessage = 'Bad Request: ' + xhr.responseText;
                } else if (xhr.status === 404) { // Not Found
                    errorMessage = 'The subject was not found.';
                }
                alert(errorMessage);
            }
        });
    });

    // Handle the Edit button click
    $('#subjects-table').on('click', '.edit-btn', function () {
        const row = $(this).closest('tr');
        const subjectId = row.data('id');
        const subjectName = row.find('td:eq(1)').text();

        // Populate the form for editing
        $('#subjectId').val(subjectId);
        $('#subjectName').val(subjectName);
        $('#form-title').text('Edit Subject');
    });
});

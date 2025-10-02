document.addEventListener("DOMContentLoaded", function () {
    const userTableBody = document.querySelector("#userTable tbody");

    // Fetch user data and populate the table
    function fetchUsers() {
        const jwtToken = getCookie("JwtToken");
        fetch('http://localhost:5224/api/Admin/viewactive', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${jwtToken}`,
                'Content-Type': 'application/json'
            }
        })
            .then(response => response.json())
            .then(users => {
                userTableBody.innerHTML = ''; // Clear existing rows
                users.forEach(user => {
                    const row = document.createElement('tr');

                    const usernameCell = document.createElement('td');
                    usernameCell.textContent = user.username;
                    row.appendChild(usernameCell);

                    const activeCell = document.createElement('td');
                    activeCell.textContent = user.active;
                    row.appendChild(activeCell);

                    const actionCell = document.createElement('td');

                    const editButton = document.createElement('button');
                    editButton.textContent = 'Edit';
                    editButton.addEventListener('click', function () {
                        showEditControls(user, row, activeCell);
                    });
                    actionCell.appendChild(editButton);
                    row.appendChild(actionCell);

                    userTableBody.appendChild(row);
                });
            })
            .catch(error => console.error('Error fetching users:', error));
    }

    // Show edit controls (select and save button)
    function showEditControls(user, row, activeCell) {
        activeCell.innerHTML = '';

        const select = document.createElement('select');
        const trueOption = document.createElement('option');
        trueOption.value = 'true';
        trueOption.textContent = 'True';
        const falseOption = document.createElement('option');
        falseOption.value = 'false';
        falseOption.textContent = 'False';

        select.appendChild(trueOption);
        select.appendChild(falseOption);

        // Set the current active status as selected option
        select.value = user.active.toString();

        const saveButton = document.createElement('button');
        saveButton.textContent = 'Save';
        saveButton.addEventListener('click', function () {
            updateUserActiveStatus(user.username, select.value === 'true');
        });

        activeCell.appendChild(select);
        activeCell.appendChild(saveButton);
    }

    // Update user's active status
    function updateUserActiveStatus(username, isActive) {
        const url = `http://localhost:5224/api/Admin/editactive?username=${encodeURIComponent(username)}&isActive=${isActive}`;
        const jwtToken = getCookie("JwtToken");

        fetch(url, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${jwtToken}`,
                'Content-Type': 'application/json'
            }
        })
            .then(response => {
                if (response.ok) {
                    fetchUsers(); // Refresh the table after updating
                } else {
                    console.error('Failed to update user active status');
                }
            })
            .catch(error => console.error('Error updating user active status:', error));
    }

    // Initial fetch to populate the table
    fetchUsers();
});

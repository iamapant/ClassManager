const apiBaseUrl = 'http://localhost:5224/api/Students';

// Function to get JWT token from cookies
function getJwtToken() {
    const match = document.cookie.match(new RegExp('(^| )JwtToken=([^;]+)'));
    return match ? match[2] : null;
}

window.addEventListener('load', () => {
    showStudentTable();
});

function showStudentTable() {
    const jwtToken = getJwtToken();
    if (!jwtToken) {
        alert('Authorization token not found.');
        return;
    }

    fetch(`${apiBaseUrl}/viewstudentdetail`, {
        headers: {
            'Authorization': `Bearer ${jwtToken}`
        }
    })
        .then(response => response.json())
        .then(data => {
            const contentDiv = document.getElementById('content');
            contentDiv.innerHTML = `
            <button onclick="showAddStudentForm()">Add New Student</button>
            <table>
                <thead>
                    <tr>
                        <th>Student ID</th>
                        <th>Name</th>
                        <th>Age</th>
                        <th>Is Regular</th>
                        <th>Address</th>
                        <th>Additional Information</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody id="studentTableBody"></tbody>
            </table>
        `;
            const tableBody = document.getElementById('studentTableBody');
            tableBody.innerHTML = '';

            data.forEach(student => {
                const row = `
                <tr>
                    <td>${student.studentId}</td>
                    <td>${student.name}</td>
                    <td>${student.age}</td>
                    <td>${student.isRegularStudent ? 'Yes' : 'No'}</td>
                    <td>${student.address}</td>
                    <td>${student.additionalInformation}</td>
                    <td>
                        <button onclick="showEditStudentForm(${student.studentId})">Edit</button>
                    </td>
                </tr>
            `;
                tableBody.innerHTML += row;
            });
        })
        .catch(error => console.error('Error fetching students:', error));
}

function showAddStudentForm() {
    const contentDiv = document.getElementById('content');
    contentDiv.innerHTML = `
        <h2>Add New Student</h2>
        <form id="studentForm">
            <label>Name: <input type="text" id="name" required></label><br>
            <label>Age: <input type="number" id="age" required></label><br>
            <label>Is Regular: <input type="checkbox" id="isRegularStudent"></label><br>
            <label>Address: <input type="text" id="address" required></label><br>
            <label>Additional Information: <input type="text" id="additionalInformation" required></label><br>
            <button type="submit">Add Student</button>
        </form>
        <button onclick="showStudentTable()">Cancel</button>
    `;

    document.getElementById('studentForm').addEventListener('submit', function (event) {
        event.preventDefault();
        addStudent();
    });
}

function addStudent() {
    const newStudent = {
        name: document.getElementById('name').value,
        age: parseInt(document.getElementById('age').value, 10),
        isRegularStudent: document.getElementById('isRegularStudent').checked,
        studentDetails: [
            {
                address: document.getElementById('address').value || null,
                additionalInformation: document.getElementById('additionalInformation').value || null
            }
        ]
    };

    console.log('Adding student:', newStudent);

    const jwtToken = getJwtToken();
    if (!jwtToken) {
        alert('Authorization token not found.');
        return;
    }

    fetch(`${apiBaseUrl}/createstudent`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${jwtToken}`
        },
        body: JSON.stringify(newStudent)
    })
        .then(response => response.json())
        .then(data => {
            alert('Student added successfully');
            showStudentTable();
        })
        .catch(error => console.error('Error adding student:', error));
}

function showEditStudentForm(studentId) {
    const jwtToken = getJwtToken();
    if (!jwtToken) {
        alert('Authorization token not found.');
        return;
    }

    fetch(`${apiBaseUrl}/viewstudentdetail`, {
        headers: {
            'Authorization': `Bearer ${jwtToken}`
        }
    })
        .then(response => response.json())
        .then(data => {
            const student = data.find(s => s.studentId === studentId);

            if (student) {
                const contentDiv = document.getElementById('content');
                contentDiv.innerHTML = `
                <h2>Edit Student</h2>
                <form id="editStudentForm">
                    <label>Name: <input type="text" id="name" value="${student.name}" required></label><br>
                    <label>Age: <input type="number" id="age" value="${student.age}" required></label><br>
                    <label>Is Regular: <input type="checkbox" id="isRegularStudent" ${student.isRegularStudent ? 'checked' : ''}></label><br>
                    <label>Address: <input type="text" id="address" value="${student.address}" required></label><br>
                    <label>Additional Information: <input type="text" id="additionalInformation" value="${student.additionalInformation}" required></label><br>
                    <button type="submit">Save Changes</button>
                </form>
                <button onclick="showStudentTable()">Cancel</button>
            `;

                document.getElementById('editStudentForm').addEventListener('submit', function (event) {
                    event.preventDefault();
                    editStudent(studentId);
                });
            }
        })
        .catch(error => console.error('Error fetching student details:', error));
}

function editStudent(studentId) {
    const updatedStudent = {
        name: document.getElementById('name').value,
        age: parseInt(document.getElementById('age').value, 10),
        isRegularStudent: document.getElementById('isRegularStudent').checked,
        address: document.getElementById('address').value,
        additionalInformation: document.getElementById('additionalInformation').value
    };

    const jwtToken = getJwtToken();
    if (!jwtToken) {
        alert('Authorization token not found.');
        return;
    }

    fetch(`${apiBaseUrl}/editstudent?id=${studentId}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${jwtToken}`
        },
        body: JSON.stringify(updatedStudent)
    })
        .then(response => {
            if (response.ok) {
                alert('Student updated successfully');
                showStudentTable();
            } else {
                alert('Failed to update student');
            }
        })
        .catch(error => console.error('Error updating student:', error));
}
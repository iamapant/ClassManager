# Class Manager API

Class Manager is a **.NET 6 Web API** project built using the **Database-First approach** with Entity Framework Core.  
It is designed to serve as a **backend API service** that connects to a SQL Server database and exposes endpoints for data access, management, and reporting.  

The API includes authentication via **JWT tokens**, supports **OData queries** for flexible data filtering and paging, and integrates with external utilities like **Excel file handling** and **email sending**.

---

## Features

- **Entity Framework Core (DB-First)** for seamless database interaction.
- **JWT Authentication** to secure API endpoints.
- **OData query support** (`$filter`, `$select`, `$expand`, `$orderby`) for flexible client queries.
- **Excel import/export** powered by EPPlus.
- **Email sending** support via SMTP using MailKit.
- Clean object mapping with **AutoMapper**.

---

## Requirements

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- SQL Server (or a compatible database)
- Visual Studio 2022 / VS Code with C# extensions

---

## Getting Started

### 1. Clone the repository
```bash
git clone https://github.com/yourusername/DataBridge.git
cd DataBridge/DBfirst
```

### 2. Configure
Update **`appsettings.json`** with your SMPT credentials:
```json
"MailSettings": {
    "DefaultSender": "YourEmailAddress",
    "Password": "YourEmailPassword",
        "DisplayName": "AspNetCore.WebApi",
        "Provider": "smtp.gmail.com",
        "Port": 587
}
```
Create a unique JWT key and save it to **`appsettings.json`**:
```json
    "JwtConfig": {
        "Secret": "YourJWTSecret",
        "ExpiryTimeFrame": "00:01:00"
    }
```
Update **`appsettings.json`** with your database connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=YourDatabase;Trusted_Connection=True;TrustServerCertificate=True"
}
```
### 3. Run database migrations
```bash
dotnet ef database update
```
### 4. *(Optional)* Seed database
If an `.sql` is included in the project folder, you can follow the steps below to populate the database.
1. Locate the included `.sql` file in the project folder (e.g., `database.sql`).  
2. Open **SQL Server Management Studio (SSMS)** or use the `sqlcmd` command-line tool.  
3. Run the script against your SQL Server instance to create the database and its tables.
   Example using SSMS:
   - Open the `.sql` file in a new query window.
   - Select your target server/instance.
   - Execute the script (F5).

   Example using `sqlcmd`:
   ```bash
   sqlcmd -S .\SQLEXPRESS -d master -i "full\path\to\script.sql"
   ```
### 5. Run the project
```bash
dotnet run
```
The API will start at:
`https://localhost:5001` or `http://localhost:5000`


## Usage
Each controller represents a user role, and each role has access to a different set of endpoints.

- **Authentication**  
  Handles user login, registration, and token generation.  
  - Obtain a JWT token for secure access  
  - Refresh tokens or manage authentication state  
  - Access: `/api/Authentication/*`
  - Functions:
    - `POST /api/Authentication/Register` → Create new user  
    - `POST /api/Authentication/Verify` → Verify user's email  
    - `GET /api/Authentication/forget-password` → Send verification email and reset password
    - `POST /api/Authentication/reset-password` → Reset user's password
    - `POST /api/Authentication/Login` → Sign in user
    - `POST /api/Authentication/RefreshToken` → Invalidate current and send new sign in token


- **Admin**  
  Manages the overall system.  
  - Create and manage user accounts  
  - Assign roles and permissions  
  - Access: `/api/Admin/*`
  - Functions:
    - `GET /api/Admin/viewactive` → Retrieve all active users  
    - `PUT /api/Admin/editactive` → Set user's active status  

- **Teacher**  
  Manages teacher's data.  
  - Create and update courses  
  - Assign homework and manage submissions  
  - Access: `/api/Teachers/*`
  - Functions:
    - `GET /api/Teachers` → **[Admin]** Get all teachers
    - `GET /api/Teachers/create` → **[Admin]** Create new teacher account
    - `PUT /api/Teachers/edit/{id}` → **[Admin]** Edit teacher's info
    - `DELETE /api/Teachers/delete/{id}` → **[Admin]** Delete teacher account
    - `POST /api/Teachers/ChangeSubject` → **[Teacher]** Change teaching subject


- **Student**  
  Interacts with course content and personal progress.  
  - View enrolled courses  
  - Submit assignments and check grades  
  - Access: `/api/Students/*`
  - Functions:
    - `GET /api/Students` → Get all students
    - `GET /api/Students/{studentId}` → Get student by id
    - `PUT /api/Students/{id}` → Update student info 
    - `GET /api/Students/{id}/scores` → Get student's scores
    - `GET /api/Students/viewstudentdetail` → **[Admin]** Get basic details of all students  
    - `POST /api/Students/createstudent` → **[Admin]** Create new student account
    - `PUT /api/Students/editstudent` → **[Admin]** Update student info
    - `GET /api/Students/{id}/classes` → View student's classes
    - `POST /api/Students/AddStudentClass` → **[Admin]** Assign student to a class
    - `POST /api/Students/feedback/{id}/{classId}` → Create feedback to a class


- **Class**  
  Represents a collection of students grouped together.  
  - Create and manage classes  
  - Assign teachers and students to classes  
  - Access: `/api/Class/*`
  - Functions:
    - `GET /api/Class` → **[Admin]** Get all classes
    - `POST /api/Class` → **[Admin]** Create new class


- **Subject**  
  Defines the subjects that are taught within classes.  
  - Add new subjects (e.g., Math, Science)  
  - Link subjects to classes or teachers  
  - Access: `/api/Subjects/*`
  - Functions:
    - `GET /api/Subjects` → **[Admin]** Get all subject
    - `GET /api/Subjects/viewallsubject` → **[Admin, Teacher]** View all subjects id and name
    - `POST /api/Subjects/addsubject` → **[Admin]** Create subject
    - `PUT /api/Subjects/editsubject` → **[Admin]** Update subject

- **Evaluation**  
  Manages assessments, grading, and performance reviews.  
  - Record test/exam scores  
  - Generate reports and performance evaluations  
  - Access: `/api/Evaluations/*`
  - Functions:
    - `GET /api/Evaluations` → **[Teacher]** Get all evaluations
    - `GET /api/SendGrade` → **[Teacher]** Send all evaluation via email
    - `GET /api/Evaluations/{id}` → **[Teacher]** Get all evaluation from a teacher
    - `POST /api/Evaluations` → **[Teacher]** Create new evaluation
    - `PUT /api/Evaluations/{id}` → **[Teacher]** Update an evaluation
    - `DELETE /api/Evaluations/{id}` → **[Teacher]** Delete evaluation   


- **Dashboard**  
  Provides an overview of key system metrics.  
  - Display statistics (e.g., number of students, courses, or assignments)  
  - Summarize performance and usage trends  
  - Access: `/api/Dashboard/*`
  - Functions:
    - `GET /api/Dashboard/subject_analysis` → **[Admin]** Get subject analysis
    - `GET /api/Dashboard/stuent_analysis` → **[Admin]** Get students analysis
    - `GET /api/Dashboard/export_subject_analysis` → **[Admin]** Download subject analysis as an excel sheet


## License
This project is licensed under the MIT License.

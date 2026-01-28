using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMSS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed Users
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Name", "HashedPassword", "Email", "GithubUsername", "GithubEmail", "Role", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("53FDB14C-A240-48B0-C32E-08DE5E7881C3"), "System Administrator", "Y2yVzgU1fUASUxv4bMx7tPtOVjGHfszAU/VHuBilbBUyFZ5gJXtdRkeuLPGSkjCH", "admin@pmss.com", "adminpmss", "admin@pmss.com", 0, new DateTime(2026, 1, 28, 21, 21, 19, 934), new DateTime(2026, 1, 28, 21, 21, 19, 934) },
                    { new Guid("4ECF5DAD-27C5-45FB-C32F-08DE5E7881C3"), "Đoàn Đức Hải", "bT90hWTIa0u1xCGPwP74oAR+xeXW9LD8W0fDzEZdYrWHg1EhHvMktB87BOxwpv/o", "beu2901@gmail.com", "haibeu2901", "beu2901@gmail.com", 2, new DateTime(2026, 1, 28, 21, 22, 47, 962), new DateTime(2026, 1, 28, 21, 22, 47, 962) },
                    { new Guid("90D28D4B-AAE3-4DE1-C330-08DE5E7881C3"), "Lâm Hữu Khánh Phương", "7FZI33MF05hu0sR6FzWVvlRNaul1tnZ/wfHM8aWoawXvO6MkRbYTx8ufwYKxPY7F", "phuonglhk@fe.edu.vn", "phuonglhk", "phuonglhk@fe.edu.vn", 1, new DateTime(2026, 1, 28, 21, 26, 32, 472), new DateTime(2026, 1, 28, 21, 26, 32, 472) },
                    { new Guid("E3C7BA0E-CF15-42FE-C331-08DE5E7881C3"), "Trần Ngọc Như Quỳnh", "pid4NUucjHv3CLW21pG9TCtkxixl6S31QRCS5TXdIZugu/rzvzeEAIzPpb0l1z0m", "QuynhTNN4@fe.edu.vn", "QuynhTNN4", "QuynhTNN4@fe.edu.vn", 1, new DateTime(2026, 1, 28, 21, 27, 14, 26), new DateTime(2026, 1, 28, 21, 27, 14, 26) },
                    { new Guid("700A9DE6-4A1C-4312-C332-08DE5E7881C3"), "Phan Minh Tâm", "e5QaN3y60ut1NsIoMP6TgiF1xoCK1P1hfPPuvApJbYoPVDDeZmogjSLEWqxzZGFN", "TamPM@fe.edu.vn", "TamPM", "TamPM@fe.edu.vn", 1, new DateTime(2026, 1, 28, 21, 28, 35, 609), new DateTime(2026, 1, 28, 21, 28, 35, 609) },
                    { new Guid("96296C3A-F9EA-4920-C333-08DE5E7881C3"), "Nguyễn Ngọc Lâm", "hUmumuyiJB+LXq8jGyhP7YU1M2sUjJR4mStnjwkinapCh/yDgL06WFm8rol6ZqMl", "lamnn15@fe.edu.vn", "lamnn15", "lamnn15@fe.edu.vn", 1, new DateTime(2026, 1, 28, 21, 29, 10, 556), new DateTime(2026, 1, 28, 21, 29, 10, 556) }
                });

            // Seed Semesters
            migrationBuilder.InsertData(
                table: "Semesters",
                columns: new[] { "SemesterId", "Name", "StartDate", "EndDate", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("25A82196-DE13-4954-40E0-08DE5E77EBB2"), "Spring 2026", new DateTime(2026, 1, 5, 14, 16, 47, 660), new DateTime(2026, 4, 28, 14, 16, 47, 660), new DateTime(2026, 1, 28, 21, 17, 8, 125), new DateTime(2026, 1, 28, 21, 17, 8, 125) },
                    { new Guid("F371E31B-1098-4E23-40E1-08DE5E77EBB2"), "Fall 2025", new DateTime(2025, 9, 16, 14, 16, 47, 660), new DateTime(2025, 12, 24, 14, 16, 47, 660), new DateTime(2026, 1, 28, 21, 18, 19, 145), new DateTime(2026, 1, 28, 21, 18, 19, 145) },
                    { new Guid("D5B1CED5-975E-4F13-40E2-08DE5E77EBB2"), "Summer 2025", new DateTime(2025, 5, 13, 14, 16, 47, 660), new DateTime(2025, 8, 28, 14, 16, 47, 660), new DateTime(2026, 1, 28, 21, 24, 38, 13), new DateTime(2026, 1, 28, 21, 24, 38, 13) }
                });

            // Seed Courses
            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "CourseId", "Code", "Name", "Description", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("EC2EEF7D-0A5B-4F29-F285-08DE5E783025"), "PRN212", "Basis Cross-Platform Application Programming With .NET", @"Upon completion of this course students should:
1. Understand the followings:
• C# language for developing .NET applications;
• Fundamental concepts of .NET Platform
• Basic knowledge of Windows Presentation Foundation in .NET
2. Be able to:
• Develop Cross-platform Desktop applications and support for user experience ( UI & UX )
3. Be able to work in a team and present group's results", new DateTime(2026, 1, 28, 21, 19, 3, 4), new DateTime(2026, 1, 28, 21, 19, 3, 4) },
                    { new Guid("EBD6FF6B-370C-4BA1-F286-08DE5E783025"), "PRN222", "Advanced Cross-Platform Application Programming With .NET", @"Upon completion of this course students should:
1. Understand the followings:
• Apply C# language for developing tranditional and realtime communication Web applications;
• Fundamental concepts of .NET Core Platform
• Basic knowledge of ASP.NET Core with Realtime Communication using SignalR
• Basic knowledge of ASP.NET Core MVC, Razor Pages, Blazor with tranditional Web App and apply into ASP.NET Core application
• Basic knowledge of Asynchronous and Parallel Programming in .NET Core application
• Basic knowledge of Dependency Injection apply into .NET Core applications
• Basic knowledge of Worker Service and apply to implement Background Tasks
2. Be able to:
• Develop Cross - platform Web applications by ASP.NET Core MVC, Razor Pages, Blazor
• Implement Real-time applications by Signal R and ASP.NET Core
• Implement Background Tasks with Worker Service
3. Be able to work in a team and present group's results", new DateTime(2026, 1, 28, 21, 19, 19, 306), new DateTime(2026, 1, 28, 21, 19, 19, 306) },
                    { new Guid("FBD956CA-4F71-4965-F287-08DE5E783025"), "PRN232", "Building Cross-Platform Back-End Application With .NET", @"Upon completion of this course students shoud:
1. Understand the followings:
• Apply C# language for develop ASP.NET WEB API ( RESTful Service applications )
• Fundamental concepts of .NET Core Platform
• Basic knowledge of ASP.NET WEB API on .NET Core
• Basic knowledge of RESTful Service and Microservice architecture
2. Be able to:
• Develop Cross - platform Back-end application can be used by Desktop or Web applications (Cross - platform)
• Develop RESTful Service applications by ASP.NET Web API and Windows Communication Foundation (WCF)
• Implement security in the ASP.NET WEB API
• Implement sending Ajax request to ASP.NET WEB API
• Implement security JWT in ASP.NET Core Web API
• Develop Distributed applications based on Microservice architecture
3. Be able to work in team and present group's results", new DateTime(2026, 1, 28, 21, 19, 36, 329), new DateTime(2026, 1, 28, 21, 19, 36, 329) },
                    { new Guid("748F7562-CA20-44B1-F288-08DE5E783025"), "SWP391", "Software Development Project", "This course guides students through the full Software Development Life Cycle (SDLC) by working on a real-world team project, with an emphasis on applying AI responsibly in requirement analysis, design, implementation, and testing. Students will practice user story writing, system design, coding using MVC and OOP, workflow development, testing, and reporting. AI tools are integrated into each phase to enhance productivity and quality.", new DateTime(2026, 1, 28, 21, 19, 50, 63), new DateTime(2026, 1, 28, 21, 19, 50, 63) },
                    { new Guid("807D2098-B609-497A-F289-08DE5E783025"), "SWD392", "Software Architecture and Design", "This is a course in concepts and methods for the architectural design of software systems of sufficient size and complexity to require the effort of several people for many months. Fundamental design concepts and design notations are introduced. Several design methods are presented and compared, with examples of their use. Students will undertake a term project working in small groups addressing the design of a relatively complex software system.", new DateTime(2026, 1, 28, 21, 20, 3, 803), new DateTime(2026, 1, 28, 21, 20, 3, 803) }
                });

            // Seed Classes
            migrationBuilder.InsertData(
                table: "Classes",
                columns: new[] { "ClassId", "SemesterId", "CourseId", "ClassCode", "TeacherId", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("5E6AECF9-3DFE-4C5D-BA31-08DE5E845A69"), new Guid("D5B1CED5-975E-4F13-40E2-08DE5E77EBB2"), new Guid("EC2EEF7D-0A5B-4F29-F285-08DE5E783025"), "SE1853", new Guid("96296C3A-F9EA-4920-C333-08DE5E7881C3"), new DateTime(2026, 1, 28, 22, 46, 7, 846), new DateTime(2026, 1, 28, 22, 46, 7, 847) },
                    { new Guid("78EEA6B5-9222-41BC-BA32-08DE5E845A69"), new Guid("D5B1CED5-975E-4F13-40E2-08DE5E77EBB2"), new Guid("748F7562-CA20-44B1-F288-08DE5E783025"), "SE1853", new Guid("E3C7BA0E-CF15-42FE-C331-08DE5E7881C3"), new DateTime(2026, 1, 28, 22, 46, 48, 826), new DateTime(2026, 1, 28, 22, 46, 48, 826) },
                    { new Guid("420D7891-3135-40AB-BA33-08DE5E845A69"), new Guid("25A82196-DE13-4954-40E0-08DE5E77EBB2"), new Guid("807D2098-B609-497A-F289-08DE5E783025"), "SE1808", new Guid("90D28D4B-AAE3-4DE1-C330-08DE5E7881C3"), new DateTime(2026, 1, 28, 22, 48, 18, 213), new DateTime(2026, 1, 28, 22, 48, 18, 213) },
                    { new Guid("10CC372F-7474-4565-BA34-08DE5E845A69"), new Guid("25A82196-DE13-4954-40E0-08DE5E77EBB2"), new Guid("EBD6FF6B-370C-4BA1-F286-08DE5E783025"), "SE1818", new Guid("700A9DE6-4A1C-4312-C332-08DE5E7881C3"), new DateTime(2026, 1, 28, 22, 49, 21, 133), new DateTime(2026, 1, 28, 22, 49, 21, 133) }
                });

            // Seed ClassEnrollments
            migrationBuilder.InsertData(
                table: "ClassEnrollments",
                columns: new[] { "ClassId", "UserId", "CourseId", "EnrolledAt" },
                values: new object[,]
                {
                    { new Guid("5E6AECF9-3DFE-4C5D-BA31-08DE5E845A69"), new Guid("4ECF5DAD-27C5-45FB-C32F-08DE5E7881C3"), new Guid("EC2EEF7D-0A5B-4F29-F285-08DE5E783025"), new DateTime(2026, 1, 28, 22, 50, 40, 895) },
                    { new Guid("78EEA6B5-9222-41BC-BA32-08DE5E845A69"), new Guid("4ECF5DAD-27C5-45FB-C32F-08DE5E7881C3"), new Guid("748F7562-CA20-44B1-F288-08DE5E783025"), new DateTime(2026, 1, 28, 22, 53, 15, 326) },
                    { new Guid("420D7891-3135-40AB-BA33-08DE5E845A69"), new Guid("4ECF5DAD-27C5-45FB-C32F-08DE5E7881C3"), new Guid("807D2098-B609-497A-F289-08DE5E783025"), new DateTime(2026, 1, 28, 22, 54, 37, 921) },
                    { new Guid("10CC372F-7474-4565-BA34-08DE5E845A69"), new Guid("4ECF5DAD-27C5-45FB-C32F-08DE5E7881C3"), new Guid("EBD6FF6B-370C-4BA1-F286-08DE5E783025"), new DateTime(2026, 1, 28, 22, 56, 5, 266) }
                });

            // Seed Projects
            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "ProjectId", "ClassId", "Name", "Description", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("E305AC16-C8DD-41FD-60CD-08DE5E853DE4"), new Guid("5E6AECF9-3DFE-4C5D-BA31-08DE5E845A69"), "Farm Products Management System", "A comprehensive farm products management system built with WPF and .NET 8. This desktop application provides complete management of farm operations with role-based access control and inventory tracking.", new DateTime(2026, 1, 28, 22, 52, 29, 520), new DateTime(2026, 1, 28, 22, 52, 29, 520) },
                    { new Guid("31795272-5296-4199-60CE-08DE5E853DE4"), new Guid("78EEA6B5-9222-41BC-BA32-08DE5E845A69"), "HIV Treatment Services System", "A robust, modular, and secure RESTful API built with .NET 8 and C# 12 for managing HIV patient care, medical records, appointments, ARV regimens, notifications, and more. The system is designed for healthcare environments, supporting multiple user roles with fine-grained access control and extensible business logic.", new DateTime(2026, 1, 28, 22, 54, 11, 670), new DateTime(2026, 1, 28, 22, 54, 11, 670) },
                    { new Guid("D04C9AF3-2AA7-401A-60CF-08DE5E853DE4"), new Guid("420D7891-3135-40AB-BA33-08DE5E845A69"), "Project Management Support System", "A full-stack application for managing academic projects with GitHub and Jira integration. Built with ASP.NET Core 10 (backend) and React (frontend), following Clean Architecture principles and modern development best practices.", new DateTime(2026, 1, 28, 22, 55, 43, 368), new DateTime(2026, 1, 28, 22, 55, 43, 368) },
                    { new Guid("D4A8F045-BFD7-4A7D-60D0-08DE5E853DE4"), new Guid("10CC372F-7474-4565-BA34-08DE5E845A69"), "Meal Prep Service System", "A comprehensive ASP.NET Core MVC web application for meal preparation and delivery services, built with a clean 3-layer architecture.", new DateTime(2026, 1, 28, 22, 56, 46, 791), new DateTime(2026, 1, 28, 22, 56, 46, 791) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete Projects
            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "ProjectId",
                keyValue: new Guid("E305AC16-C8DD-41FD-60CD-08DE5E853DE4"));

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "ProjectId",
                keyValue: new Guid("31795272-5296-4199-60CE-08DE5E853DE4"));

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "ProjectId",
                keyValue: new Guid("D04C9AF3-2AA7-401A-60CF-08DE5E853DE4"));

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "ProjectId",
                keyValue: new Guid("D4A8F045-BFD7-4A7D-60D0-08DE5E853DE4"));

            // Delete ClassEnrollments
            migrationBuilder.DeleteData(
                table: "ClassEnrollments",
                keyColumns: new[] { "ClassId", "UserId" },
                keyValues: new object[] { new Guid("5E6AECF9-3DFE-4C5D-BA31-08DE5E845A69"), new Guid("4ECF5DAD-27C5-45FB-C32F-08DE5E7881C3") });

            migrationBuilder.DeleteData(
                table: "ClassEnrollments",
                keyColumns: new[] { "ClassId", "UserId" },
                keyValues: new object[] { new Guid("78EEA6B5-9222-41BC-BA32-08DE5E845A69"), new Guid("4ECF5DAD-27C5-45FB-C32F-08DE5E7881C3") });

            migrationBuilder.DeleteData(
                table: "ClassEnrollments",
                keyColumns: new[] { "ClassId", "UserId" },
                keyValues: new object[] { new Guid("420D7891-3135-40AB-BA33-08DE5E845A69"), new Guid("4ECF5DAD-27C5-45FB-C32F-08DE5E7881C3") });

            migrationBuilder.DeleteData(
                table: "ClassEnrollments",
                keyColumns: new[] { "ClassId", "UserId" },
                keyValues: new object[] { new Guid("10CC372F-7474-4565-BA34-08DE5E845A69"), new Guid("4ECF5DAD-27C5-45FB-C32F-08DE5E7881C3") });

            // Delete Classes
            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "ClassId",
                keyValue: new Guid("5E6AECF9-3DFE-4C5D-BA31-08DE5E845A69"));

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "ClassId",
                keyValue: new Guid("78EEA6B5-9222-41BC-BA32-08DE5E845A69"));

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "ClassId",
                keyValue: new Guid("420D7891-3135-40AB-BA33-08DE5E845A69"));

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "ClassId",
                keyValue: new Guid("10CC372F-7474-4565-BA34-08DE5E845A69"));

            // Delete Courses
            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: new Guid("EC2EEF7D-0A5B-4F29-F285-08DE5E783025"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: new Guid("EBD6FF6B-370C-4BA1-F286-08DE5E783025"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: new Guid("FBD956CA-4F71-4965-F287-08DE5E783025"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: new Guid("748F7562-CA20-44B1-F288-08DE5E783025"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: new Guid("807D2098-B609-497A-F289-08DE5E783025"));

            // Delete Semesters
            migrationBuilder.DeleteData(
                table: "Semesters",
                keyColumn: "SemesterId",
                keyValue: new Guid("25A82196-DE13-4954-40E0-08DE5E77EBB2"));

            migrationBuilder.DeleteData(
                table: "Semesters",
                keyColumn: "SemesterId",
                keyValue: new Guid("F371E31B-1098-4E23-40E1-08DE5E77EBB2"));

            migrationBuilder.DeleteData(
                table: "Semesters",
                keyColumn: "SemesterId",
                keyValue: new Guid("D5B1CED5-975E-4F13-40E2-08DE5E77EBB2"));

            // Delete Users
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("53FDB14C-A240-48B0-C32E-08DE5E7881C3"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("4ECF5DAD-27C5-45FB-C32F-08DE5E7881C3"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("90D28D4B-AAE3-4DE1-C330-08DE5E7881C3"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("E3C7BA0E-CF15-42FE-C331-08DE5E7881C3"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("700A9DE6-4A1C-4312-C332-08DE5E7881C3"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("96296C3A-F9EA-4920-C333-08DE5E7881C3"));
        }
    }
}

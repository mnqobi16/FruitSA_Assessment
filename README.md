Fruit SA Web Application


This Web Application is for a user to manage the List of products and List Categories, it built using ASP.NET MVC, CSS with Bootstrap, Entity Framework 6, and SQL Server. 


Overview

This project is a web application built for managing products and categories. It adheres to the guidelines outlined in the assessment and includes features such as user 
registration, login, product and category management, pagination, product code generation, image uploads, and Excel file handling.


Technologies Used


Language: C#
Frameworks:
Backend: ASP.NET Core Web API
Frontend: ASP.NET MVC Core with Razor Pages
Frontend Libraries:
JavaScript/TypeScript
Bootstrap (for responsive design)
Database: SQL Server or MySQL (configurable)
ORM: Entity Framework Core
Version Control: GitHub (Repository link)
Middleware: ASP.NET Core Web API
Authentication: ASP.NET Core Identity (for user registration and login)

Getting Started

To get started with this project, follow these steps:
1.	Clone the Repository: Clone this repository to your local machine using the following command:
https://github.com/mnqobi16/FruitSA_Assessment.git

2.	Set Up the Database: Set up a SQL Server database and configure the connection string in the appsettings.json file.

3.	Add Migrations:

Open Package Manager Console in Visual Studio, and make sure to choose FruitSA.DataAccess as the Default Project and run the following command: 
•	add-migration "First Migrations" 
•	update-database
4.	Build and Run the Application: Build the solution using Visual Studio. Run the application .

5.	Register on the system by clicking the register link on the top menu and putting your details and the Application should log you in .


6.	Upload Products using Excel

7.	
a. go to the Files of this Application repo and find excel document named Products.xlsx
b. Login to the application And Click Admin Action > Product and upload the Excel file and press the Upload button at the bottom of the page.
c. then go to home page to seen uploaded Product from excel
d. You Can also download products to excel format by clicking the Button at the bottom of the page 

8.	You can Create, Update and Delete products on Product Page 

9.	You can also access the Category page to create, Delete and  Update the Categories
Please contact me if you need more clarity or you encounter any issue.

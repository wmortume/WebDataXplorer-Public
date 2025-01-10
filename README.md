# WebDataXplorer-Public
**WebDataXplorer** is a powerful tool for searching, filtering, and managing data through an interactive data grid.
This project features a **.NET Core** backend with **Entity Framework Core** and a **React** frontend utilizing
the **PrimeReact** library and **TanStack React Query**. It's optimized to support **Azure SQL Database**, **Azure Key Vault**,
**Azure Data Factory**, **Azure Blob Storage**, and **BrightData APIs** for data transformation.
> A production version of this application is deployed to Azure App Services (Web App) and maintained in a private GitHub repository.

Sample data is included in `appsettings.json` to demonstrate the tool's functionality. You can customize it along with other files
such as `SqldbWebDataXplorerContext.cs` and `InventoryRepository.cs` to enable full cloud integration and suit your specific requirements.

## Demo
![image](https://github.com/user-attachments/assets/258cc2d4-f2df-4bec-8035-7d37f5678824)
![image](https://github.com/user-attachments/assets/5c6108f6-a463-4d1b-9978-853d09d2992b)

## ADF Design & Demo
The Azure Data Factory pipelines automate data ingestion and transformation, retrieving Azure Key Vault secrets for secure HTTP requests and mapping JSON blob result files to SQL tables.

![image](https://github.com/user-attachments/assets/a2c81158-6b72-45dd-8860-399e30752f7a)
![image](https://github.com/user-attachments/assets/20a28f34-ac11-4b83-a3af-f9f9875ce7ef)
![image](https://github.com/user-attachments/assets/43892fd0-57c1-490d-9629-5b2f03c2ce7c)

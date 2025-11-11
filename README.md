# Document Move - Folder Hierarchy Application

This Windows Forms application reads documents from SQL Server and organizes them into a date-based folder structure (YYYY/MM).

## Features

- **Connection String Validation**: Test SQL Server connection before processing
- **Library Selection**: Select specific library to process from dropdown
- **Chunked Processing**: Processes documents in chunks of 5000 for better performance
- **Date-Based Organization**: Organizes files into year/month folder structure based on upload date
- **Error Handling**: Comprehensive error handling with logging
- **Progress Tracking**: Real-time status updates during processing

## Requirements

- .NET 6.0 or higher
- SQL Server database with the required schema
- Windows OS

## How to Use

1. **Enter Connection String**: Paste your SQL Server connection string in the provided textbox
   - Example: `Server=myserver;Database=mydb;User Id=myuser;Password=mypass;`

2. **Test Connection**: Click "Test Connection" to validate the connection string
   - If successful, libraries will be automatically loaded into the dropdown

3. **Select Library**: Choose the library you want to process from the dropdown

4. **Select Destination**: Enter or browse to the destination folder where organized files will be copied

5. **Process Documents**: Click "Process Documents" to start the operation
   - Documents are processed in chunks of 5000
   - Files are copied to: `[Destination]\YYYY\MM\filename.ext`
   - Progress and errors are displayed in real-time

## Logging

All operations are logged to `log.txt` in the application directory, including:
- Successful file copies
- Errors (missing files, access issues, etc.)
- Processing statistics

## Build Instructions

```powershell
dotnet build
```

## Run Instructions

```powershell
dotnet run
```

Or build and run the executable:

```powershell
dotnet publish -c Release
```

Then run the .exe from the publish folder.

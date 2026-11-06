GitHub source copy for the Car Wash Management System.

Open CarWashManagementSystem.sln in Visual Studio. Restore NuGet packages from packages.config when prompted; the packages/ folder is intentionally not included and is ignored by .gitignore.

The LocalDB files DBCarWash.mdf and DBCarWash_log.ldf were excluded because they contain application data and are not needed to build the Visual Studio solution or inspect its source and GUI resources. Running database-backed workflows requires a separate sanitized LocalDB database.
